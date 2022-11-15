namespace Stonks.GRPCBackend.Models.Domain;

/// <summary>
/// Entity that represent stock price at some point of time
/// </summary>
public class StockPriceRecord
{
    public string StockSymbol { get; init; }
    public uint Price { get; init; }
    public DateTime DateTime { get; init; }
}