using System;
using System.Threading.Tasks;
using TrashBank.Domain.Core.Bus;
using TrashBank.Domain.Core.Commands;
using TrashBank.Domain.Core.Events;
using RabbitMQ.Client;
using Newtonsoft.Json;
using System.Text;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace TrashBank.Infra.Bus
{
    public class RabbitMQEventBus : IEventBus
    {
        private readonly IServiceScopeFactory _serviceFactory;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IMediator _bus;

        // Events + handlers
        private List<string> _events;
        private Dictionary<string, List<Type>> _eventHandlers;

        // Create lists to hold all the subs

        public RabbitMQEventBus(IMediator mediator, IServiceScopeFactory serviceFactory)
        {
            _connectionFactory = new ConnectionFactory() // Configure connection with message brocker
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "llasapg",
                Password = "Gavras123321",
                ContinuationTimeout = TimeSpan.FromSeconds(60) // it shoud be removed and injected using DI
            };
            _connection = _connectionFactory.CreateConnection(); // Open connection
            _bus = mediator;
            _serviceFactory = serviceFactory;
            // stuff connected to sub and unSub
            _events = new List<string>();
            _eventHandlers = new Dictionary<string, List<Type>>();
        }

        public void Publish<T>(T @event) where T : IIntegrationEvent
        {
            using (var channel = _connection.CreateModel()) // Creates new channel to publish message
            {
                var queueName = @event.GetType().Name;
                channel.QueueDeclare(queueName, true, false, false, null);
                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish("", queueName, null, body);
            }
        }

        public async Task SendCommand<T>(T command) where T : ICommand
        {
            await _bus.Publish(command);
        }

        public void Subscribe<T, TH>()
            where T : IIntegrationEvent
            where TH : IEventHandler<T>
        {
            // Chech that we have already added this event + handler
            var eventName = typeof(T).Name;
            var handlerName = typeof(TH).Name;

            // Chech that we have this type of event
            if(!_events.Any(x => x == eventName))
            {
                _events.Add(eventName);
            }

            // Chech that we have this type of event injected in handlers list
            if(!_eventHandlers.Any(x => x.Key == eventName))
            {
                _eventHandlers.Add(eventName, new List<Type>());
            }

            // Chech that we dont have already added event type to the list of handlers

            if(_eventHandlers[eventName].Any(x => x.GetType() == handlerName.GetType())) // so we already have declared this pair of hanlder + event
            {
                throw new ArgumentException("You already have defined this pair of event + handler");
            }

            _eventHandlers[eventName].Add(handlerName.GetType());

            // Chech + consume all the messages ( because proccess of subs will be done at the begining of the microservice start )

            BasicConsume<T>();
        }

        public void UnSubscribe<T, TH>()
            where T : IIntegrationEvent
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            if (_eventHandlers.Any(x => x.Key == eventName && x.Value.Contains(handlerType)))
            {
                var handler = _eventHandlers.Where(x => x.Key == typeof(T).Name).Select(x => x.Value.Where(x => x.GetType() == handlerType).Select(x => x).First()).First();

                _eventHandlers[eventName].Remove(handler);

                Trace.WriteLine($"Handler was removed");
            }
            else
            {
                Trace.WriteLine($"This handler is not sub to this type of event");
            }
        }

        private void BasicConsume<T>() where T : IIntegrationEvent
        {
            var queueName = typeof(T).Name;

            using (var channel = _connection.CreateModel()) // create channle to consume messages
            {
                channel.QueueDeclare(queueName, true, false, false, null);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (sender, e) => await ConsumerReceived(sender, e);
                consumer.Model.BasicConsume(queueName, true, consumer);
            }
        }

        private async Task ConsumerReceived(object sender, BasicDeliverEventArgs e)
        {
            // get message body
            var message = Encoding.UTF8.GetString(e.Body.ToArray());
            var eventName = e.RoutingKey;
            // create handler and handle it
            try
            {
                await ProcessEvent(eventName, message);
            }
            catch(Exception ex)
            {
                Trace.WriteLine($"Error occured :((((, {ex.Message}");
            }
        }

        private async Task ProcessEvent(string eventName, string message) 
        {
            Trace.WriteLine($"Event handling, event name - {eventName}");

            if(_eventHandlers.ContainsKey(eventName))
            {
                var handlers = _eventHandlers[eventName];
                var body = JsonConvert.DeserializeObject(message, eventName.GetType());

                foreach (var handler in handlers)
                {
                    // Somehow get the handler type, create an instace and call method handle

                    using (var scope = _serviceFactory.CreateScope())
                    {
                        var handlerObject = scope.ServiceProvider.GetService(handler);

                        var handlerType = typeof(IEventHandler<>).MakeGenericType(eventName.GetType()); // creates type of handler with spec event type

                        await (Task)handlerType.GetMethod("Handler").Invoke(handlerObject, new object[] { body });
                    }
                }
            }
        }
    }
}
