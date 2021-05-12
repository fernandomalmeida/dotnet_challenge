﻿using System;
using System.Text;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using Confluent.Kafka;

using StockBot.Models;

namespace StockBot
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {
            string bootstrapServers = "localhost:9092";
            string topicConsumed = "chat.stock";
            string topicProduced = "chat.msgs";

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = $"{topicConsumed}-group-0",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                using (var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build())
                {
                    using (var producer = new ProducerBuilder<Null, string>(producerConfig).Build())
                    {
                        consumer.Subscribe(topicConsumed);

                        try
                        {
                            while (true)
                            {
                                var cr = consumer.Consume(cts.Token);

                                var msg = await GetStock(cr.Message.Value);

                                var result = await producer.ProduceAsync(
                                    topicProduced,
                                    new Message<Null, string>
                                    {
                                        Value = msg
                                    }
                                );
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            consumer.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção: {ex.GetType().FullName} | " +
                    $"Mensagem: {ex.Message}");
            }
        }

        private static async Task<string> GetStock(string stock_code)
        {
            var url = $"https://stooq.com/q/l/?s={stock_code}&f=sd2t2ohlcv&h&e=csv";

            client.DefaultRequestHeaders.Accept.Clear();

            string msg = await client.GetStringAsync(url);

            string[] lines = msg.Split("\n");
            string[] data = lines[1].Split(",");

            var stock = new Stock();
            stock.symbol = data[0];
            stock.close = data[6];

            return $"{stock.symbol} quote is ${stock.close} per share";
        }
    }
}
