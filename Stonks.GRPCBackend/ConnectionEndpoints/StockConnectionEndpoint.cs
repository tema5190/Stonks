using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using gRPCStockServiceContracts;

namespace Stonks.GRPCBackend.Services;

public class StockConnectionEndpoint : gRPCStockServiceContracts.StockService.StockServiceBase
{
    private readonly ILogger<StockConnectionEndpoint> _logger;
    private readonly IStockService _stockService;

    public StockConnectionEndpoint(ILogger<StockConnectionEndpoint> logger, IStockService stockService)
    {
        _logger = logger;
        _stockService = stockService;
    }

    public override async Task GetStockStream(Empty _, IServerStreamWriter<StockData> responseStream,
        ServerCallContext context)
    {
        while (!context.CancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000);

            var latestStockPriceRecord = await _stockService.GetLatestStockPriceRecord("MSFT");

            var stockData = new StockData()
            {
                StockSymbol = latestStockPriceRecord.StockSymbol,
                DateTimeStamp = Timestamp.FromDateTime(latestStockPriceRecord.DateTime),
                CurrentPrice = latestStockPriceRecord.Price
            };
            
            if (!context.CancellationToken
                    .IsCancellationRequested) // might be requested during the calculation so check twice.
            {
                await responseStream.WriteAsync(stockData);
            }
        }
    }
}