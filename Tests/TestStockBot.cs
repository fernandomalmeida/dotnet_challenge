using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

using StockBot.Services;

namespace Tests
{
    public class TestStockBot
    {
        [Fact]
        public async Task TestStockBotGetStock()
        {
            string message = await StockService.GetStock("aapl.us");
            Assert.True(message.StartsWith("AAPL.US quote is "), "result should start with stock name in uppercase");
        }
    }
}
