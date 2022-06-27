using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;

namespace Exchange.Events.Publishers
{
    public interface IPriceMovedPublisher
    {
        void Publish(PriceMovedEventData priceMovedEvent);
    }

    public class PriceMovedPublisher : IPriceMovedPublisher
    {
        //queue name
        private const string QueueName = "price-moved";

        public void Publish(PriceMovedEventData priceMovedEvent)
        {
            // Connect to RabbitMQ in container
            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672")
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Connect to the "order-placed" queue on RabbitMQ
            channel.QueueDeclare(
                QueueName,
               
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Serialize the PriceMovedEventData object into a json string
            var cartItemAddedEvent = JsonConvert.SerializeObject(priceMovedEvent);

            // Encode the json string into UTF8
            var body = Encoding.UTF8.GetBytes(cartItemAddedEvent);

            // Publish the message to the queue
            channel.BasicPublish("", QueueName, null, body);
        }
    }

    public class PriceMovedEventData
    {
        public string symbol { get; set; }
        public string movement { get; set; }
        public float Qty { get; set; }
    }
}
