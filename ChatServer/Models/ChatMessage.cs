using System;
using System.Text;
using System.Text.Json;

namespace ChatServer.Models
{
    public class ChatMessage
    {
        public string Id { get; set; }
        public string author { get; set; }
        public string text { get; set; }
        public DateTime dateTime { get; set; }
        public ChatMessage()
        { }
        public ChatMessage(string author, string text)
        {
            var g = Guid.NewGuid();
            this.Id = g.ToString();

            this.author = author;
            this.text = text;

            this.dateTime = DateTime.Now;
        }

        public ChatMessage(ArraySegment<byte> buffer)
        {
            var encoded = Encoding.UTF8.GetString(buffer);
            var msg = JsonSerializer.Deserialize<ChatMessage>(encoded);

            var g = Guid.NewGuid();
            this.Id = g.ToString();

            this.author = msg.author;
            this.text = msg.text;

            this.dateTime = DateTime.Now;
        }

        public ArraySegment<byte> AsArraySegment()
        {
            var data = JsonSerializer.Serialize<ChatMessage>(this);
            var encoded = Encoding.UTF8.GetBytes(data);
            var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);

            return buffer;
        }
    }

}
