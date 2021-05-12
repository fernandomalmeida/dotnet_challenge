using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Net.WebSockets;

using Confluent.Kafka;

using ChatServer.Models;

namespace ChatServer.Hubs
{
    public class ChatHub
    {
        // private IModel _channel;
        private IProducer<Null, string> _producer;

        private Dictionary<string, WebSocket> webSockets;

        private string bootstrapServers = "localhost:9092";
        private string topicProduced = "chat.stock";
        private string topicConsumed = "chat.msgs";

        public ChatHub()
        {
            webSockets = new Dictionary<string, WebSocket>();

            Task.Run(() => SetupKafka());
        }

        private void SetupKafka()
        {
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

            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();

            try
            {
                using (var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build())
                {
                    consumer.Subscribe(topicConsumed);

                    try
                    {
                        while (true)
                        {
                            var cr = consumer.Consume(cts.Token);

                            SendMessage(new ChatMessage(
                                author: "Bot",
                                text: cr.Message.Value
                            ), WebSocketMessageType.Text, true);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        consumer.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção: {ex.GetType().FullName} | " +
                    $"Mensagem: {ex.Message}");
            }
        }

        public async void SendMessage(ChatMessage msg, WebSocketMessageType messageType, bool endOfMessage)
        {
            var match = Regex.Match(msg.text, @"\/stock\=(.*)");
            if (match.Success)
            {
                var result = await _producer.ProduceAsync(
                    topicProduced,
                    new Message<Null, string>
                    {
                        Value = match.Groups[1].Value
                    }
                );
                return;
            }

            foreach (KeyValuePair<string, WebSocket> ws in webSockets)
            {
                await ws.Value.SendAsync(msg.AsArraySegment(), messageType, endOfMessage, CancellationToken.None);
            }
        }

        public void AddWebSocket(string user, WebSocket ws)
        {
            webSockets.Add(user, ws);
        }

        public void RemoveWebSocket(string user)
        {
            webSockets.Remove(user);
        }
    }
}