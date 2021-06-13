using System;
using System.Text;
using System.Threading.Tasks;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using StockBot.Services;

namespace StockBot
{
    class Program
    {
        static private string rabbitMQHostName = "rabbitmq";
        static private string consumeQueue = "stock_queue";
        static private string produceQueue = "msgs_queue";
        static async Task Main(string[] args)
        {
            var factory = new ConnectionFactory() {
                HostName = rabbitMQHostName
            };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.QueueDeclare(
                queue: consumeQueue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            // channel.BasicQos(0, 1, false);
            
            var consumer = new EventingBasicConsumer(channel);

            channel.BasicConsume(
                consumer: consumer,
                queue: consumeQueue,
                autoAck: true
            );

            consumer.Received += (model, ea) =>
            {
                Console.WriteLine("Message Get " + ea.DeliveryTag);
                string response = null;

                var body = ea.Body.ToArray();
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {
                    var message = Encoding.UTF8.GetString(body);
                    var t = StockService.GetStock(message);
                    t.Wait();
                    
                    response = t.Result;
                }
                catch (Exception e)
                {
                    response = "";
                }
                finally
                {
                    Console.WriteLine("Reply to: " + produceQueue);
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    channel.BasicPublish(
                        exchange: "",
                        routingKey: produceQueue,
                        body: responseBytes
                    );
                }
            };
            
            Console.WriteLine(" Awaiting stock requests");

            while(true)
            {
                // To keep the bot running
            }

        }

    }
}
