using System;
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
        private IModel _channel;

        private Dictionary<string, WebSocket> webSockets;

        public ChatHub()
        {
            webSockets = new Dictionary<string, WebSocket>();

            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
            };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "msgs", type: "topic");
            var queueName = _channel.QueueDeclare().QueueName;

            _channel.QueueBind(
                queue: queueName,
                exchange: "msgs",
                routingKey: "chat.msgs"
            );


            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                this.SendMessage(new ChatMessage(
                    "Bot",
                    message
                ), WebSocketMessageType.Text, false);
            };

            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }

        public async void SendMessage(ChatMessage msg, WebSocketMessageType messageType, bool endOfMessage)
        {
            var match = Regex.Match(msg.text, @"\/stock\=(.*)");
            if (match.Success)
            {
                _channel.BasicPublish(
                    exchange: "msgs",
                    routingKey: "chat.stock",
                    basicProperties: null,
                    body: Encoding.UTF8.GetBytes(match.Groups[1].Value)
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

        public async void RemoveWebSocket(string user)
        {
            webSockets.Remove(user);
        }
    }
}