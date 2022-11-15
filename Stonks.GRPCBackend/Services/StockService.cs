using Stonks.GRPCBackend.Models.Domain;

namespace Stonks.GRPCBackend.Services;

public class StockService : IStockService
{
    public async Task<StockPriceRecord> GetLatestStockPriceRecord(string stockSymbol)
    {
        var random = new Random();

        var price = stockSymbol switch
        {
            "MSFT" => random.Next(25000, 29000),
            "AAPL" => random.Next(15000, 16500),
            "AMZN" => random.Next(12000, 13000)
        };

        return new StockPriceRecord
        {
            StockSymbol = stockSymbol,
            Price = (uint) price,
            DateTime = DateTime.Now.ToUniversalTime()
        };
    }
}