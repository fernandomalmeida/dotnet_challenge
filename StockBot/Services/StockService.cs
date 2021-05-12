using System;
using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;

using StockBot.Models;

namespace StockBot.Services
{
    public class StockService
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> GetStock(string stock_code)
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