using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Wallet.Database.Entities;
using Wallet.Database;
using Wallet.DTO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Wallet.Events.Subscribers
{
    public class WithdrawProcessor : BackgroundService
    {
        private ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;  
        private readonly IServiceScopeFactory _scopeFactory;
        private const string QueueName = "WithdrawCurrency";

        
        public WithdrawProcessor(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a connection to the  queue on RabbitMQ
           
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

            // Call CheckMessages() every 5 seconds
            var timer = new Timer(CheckMessages, null, TimeSpan.Zero,
                                    TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private void CheckMessages(object state)
        {
            // Create a consumer, that will process messages published
            // in the  queue on RabbitMQ
            var consumer = new EventingBasicConsumer(_channel);

           
            consumer.Received += (sender, evnt) =>
            {
                // Extract the body of the event into a object
               
                var body = evnt.Body.ToArray();
                // Convert the body's data to a object
                var cartItem = JsonConvert.DeserializeObject<DepositDto>(Encoding.UTF8.GetString(body));

                // Save the data to the DB
               
                // Convert the object into a model that EF can use
                TransactionRecord order = new TransactionRecord
                {
                   
                    Qty = -1*(cartItem.Qty),
                    Transaction_Type = "Withdraw",
                    Symbol = cartItem.Symbol
                };

             
                using var scope = _scopeFactory.CreateScope();

               
                
             
                var dbContext = scope.ServiceProvider.GetService<WalletDbContext>();

                // Save the data to the DB 
                dbContext.Add(order);
                dbContext.SaveChanges();

                // Publish the  data to the message queue
               
                // Create a separate connection to the queue on RabbitMQ
                string QueueName = "Withdraw-placed";
                var factory = new ConnectionFactory
                {
                    
                    //connect to RabbitMQ
                    Uri = new Uri("amqp://guest:guest@localhost:5672")
                };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();
                channel.QueueDeclare(
                    QueueName,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                // Serialize the object into a text
                var serializedData = JsonConvert.SerializeObject(order);
                // Encode the serialized  string 
                var utf8Data = Encoding.UTF8.GetBytes(serializedData);
                // Publish message to RabbitMQ
                channel.BasicPublish("", QueueName, null, utf8Data);
            };

           
            // method of RabbitMQ to remove the message
            _channel.BasicConsume(QueueName, true, consumer);
        }

    }
}
