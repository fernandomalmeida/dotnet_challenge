using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;
using System.Net.WebSockets;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;


using ChatServer.Models;

namespace ChatServer.Hubs
{
    public class ChatHub
    {
        private Dictionary<string, WebSocket> webSockets;

        private string rabbitMQHostName = "rabbitmq";
        private readonly IConnection connection;
        private readonly IModel channel;
        private string consumeQueue = "msgs_queue";
        private string produceQueue = "stock_queue";



        public ChatHub()
        {
            webSockets = new Dictionary<string, WebSocket>();

            var factory = new ConnectionFactory()
            {
                HostName = rabbitMQHostName
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.QueueDeclare(
                queue: consumeQueue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new EventingBasicConsumer(channel);

            channel.BasicConsume(
                consumer: consumer,
                queue: consumeQueue,
                autoAck: true
            );

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var response = Encoding.UTF8.GetString(body);

                SendMessage(new ChatMessage(
                    author: "Bot",
                    text: response
                ), WebSocketMessageType.Text, true);
            };
        }

        public async void SendMessage(ChatMessage msg, WebSocketMessageType messageType, bool endOfMessage)
        {
            var match = Regex.Match(msg.text, @"\/stock\=(.*)");
            if (match.Success)
            {
                var message = match.Groups[1].Value;
                var messageBytes = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(
                    exchange: "",
                    routingKey: produceQueue,
                    body: messageBytes
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