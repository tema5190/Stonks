using Stonks.GRPCBackend.Models.Domain;

namespace Stonks.GRPCBackend.Services;

public interface IStockService
{
    Task<StockPriceRecord> GetLatestStockPriceRecord(string stockSymbol);
}