using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

using ChatServer.Models;
using ChatServer.Data;
using ChatServer.Hubs;


namespace ChatServer.Controllers
{
    [ApiController]
    [Route("")]
    public class ChatController : Controller
    {
        private readonly ILogger<ChatController> _logger;
        private ApplicationDbContext _context;
        private ChatHub _hub;

        public ChatController(ILogger<ChatController> logger, ApplicationDbContext context, ChatHub hub)
        {
            _logger = logger;
            _context = context;
            _hub = hub;
        }

        [HttpGet("/messages")]
        public async Task<IActionResult> GetMessages()
        {
            var dbMessages = await _context.ChatMessages.OrderByDescending(cm => cm.dateTime).Take(50).Reverse().ToListAsync();
            return Ok(dbMessages);
        }

        [HttpGet("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _logger.Log(LogLevel.Information, "WebSocket connection established");
                await Chat(webSocket);
            }
        }

        private async Task Chat(WebSocket webSocket)
        {
            _hub.AddWebSocket(this.User.Identity.Name, webSocket);

            var buffer = new ArraySegment<byte>(new byte[1024 * 4]);
            var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
            _logger.Log(LogLevel.Information, "Message received from Client");

            while (!result.CloseStatus.HasValue)
            {
                var msg = new ChatMessage(buffer.Slice(0, result.Count));
                _context.ChatMessages.Add(msg);
                _context.SaveChanges();

                _hub.SendMessage(msg, result.MessageType, result.EndOfMessage);
                // await webSocket.SendAsync(msg.AsArraySegment(), result.MessageType, result.EndOfMessage, CancellationToken.None);
                _logger.Log(LogLevel.Information, "Message sent to Client");

                result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                _logger.Log(LogLevel.Information, "Message received from Client");
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _hub.RemoveWebSocket(this.User.Identity.Name);
            _logger.Log(LogLevel.Information, "WebSocket connection closed");
        }

    }
}