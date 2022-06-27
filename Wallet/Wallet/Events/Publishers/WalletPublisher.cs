using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;

namespace Wallet.Events.Publishers
{
    public interface IOrderPlacedPublisher
    {
        void Publish(OrderPlacedEventData orderEvent);
    }

    public class WalletPublisher : IOrderPlacedPublisher
    {
        private const string QueueName = "price-moved";

        public void Publish(OrderPlacedEventData orderEvent)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672")
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(QueueName,
                
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var cartItemAddedEvent = JsonConvert.SerializeObject(orderEvent);

            var body = Encoding.UTF8.GetBytes(cartItemAddedEvent);

            channel.BasicPublish("", QueueName, null, body);
        }
    }

    public class OrderPlacedEventData
    {
        public long Id { get; set; }
        public string Symbol { get; set; }
        public string Transaction_Type { get; set; }
    }
}
