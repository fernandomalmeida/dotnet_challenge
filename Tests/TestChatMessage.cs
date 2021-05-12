using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

using ChatServer.Models;

namespace Tests
{
    public class TestChatMessage
    {
        [Fact]
        public async Task TestChatMessageSerialize()
        {
            var cm = new ChatMessage("author", "text");
            var cm2 = new ChatMessage(cm.AsArraySegment());

            Assert.Equal(cm.author, cm2.author);
            Assert.Equal(cm.text, cm2.text);
        }
    }
}
