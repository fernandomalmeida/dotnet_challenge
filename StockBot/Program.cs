using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Http;
using System.Threading.Tasks;

using StockBot.Models;

namespace StockBot
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
            };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: "msgs", type: "topic");
                    var queueName = channel.QueueDeclare().QueueName;

                    channel.QueueBind(
                        queue: queueName,
                        exchange: "msgs",
                        routingKey: "chat.stock"
                    );

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += async (models, ea) =>
                    {
                        var message = Encoding.UTF8.GetString(ea.Body.ToArray());

                        var stock_code = message;
                        var url = $"https://stooq.com/q/l/?s={stock_code}&f=sd2t2ohlcv&h&e=csv";

                        client.DefaultRequestHeaders.Accept.Clear();

                        string msg = await client.GetStringAsync(url);

                        string[] lines = msg.Split("\n");
                        string[] data = lines[1].Split(",");

                        var stock = new Stock();
                        stock.symbol = data[0];
                        stock.close = data[6];


                        Console.WriteLine(stock.symbol);
                        Console.WriteLine(stock.close);
                        Console.WriteLine($"{stock.symbol} quote is ${stock.close} per share");

                        channel.BasicPublish(
                            exchange: "msgs",
                            routingKey: "chat.msgs",
                            basicProperties: null,
                            body: Encoding.UTF8.GetBytes($"{stock.symbol} quote is ${stock.close} per share")
                        );
                    };

                    channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();


                }
            }

        }
    }
}
