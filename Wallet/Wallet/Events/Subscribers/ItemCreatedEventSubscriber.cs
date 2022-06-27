using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Wallet.Database;
using Wallet.DTO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wallet.Domain.Order.Handlers;

namespace Wallet.Events.Subscribers
{
    public class ItemCreatedEventSubscriber : BackgroundService
    {
        private ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOrderCreateHandler _orderCreateHandler;

        private const string QueueName = "price-moved";

        public ItemCreatedEventSubscriber(
            IServiceScopeFactory parmScopeFactory,
            IOrderCreateHandler parmOrderCreateHandler)
        {
            _scopeFactory = parmScopeFactory;
            _orderCreateHandler = parmOrderCreateHandler;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _connectionFactory = new ConnectionFactory
            {
                UserName = "guest",
                Password = "guest"
            };
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(QueueName,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            _channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var timer = new Timer(ConsumeEvent, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private void ConsumeEvent(object state)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, e) =>
            {
                var body = e.Body.ToArray();
                var cartItemAddedEvent = JsonConvert.DeserializeObject<CartItemAddedEvent>(Encoding.UTF8.GetString(body));
                var dto = new OrderDto
                {
                    Symbol = cartItemAddedEvent.symbol,
                    Transaction_Type = cartItemAddedEvent.movement,
                    Qty = cartItemAddedEvent.Qty

                };
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetService<WalletDbContext>();

                _orderCreateHandler.Handle(context, dto);
            };

            _channel.BasicConsume(QueueName, true, consumer);
        }

        public class CartItemAddedEvent
        {
            public string symbol { get; set; }
            public string movement { get; set; }
            public float Qty { get; set; }
        }
    }
}
