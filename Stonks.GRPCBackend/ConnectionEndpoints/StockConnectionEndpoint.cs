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

    public override async Task GetStockStream(StockDataRequest stockDataRequest, IServerStreamWriter<StockData> responseStream,
        ServerCallContext context)
    {
        while (!context.CancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000); // 1 second delay

            var latestStockPriceRecord = await _stockService.GetLatestStockPriceRecord(stockDataRequest.RequestedStockSymbol);

            var stockData = new StockData
            {
                StockSymbol = latestStockPriceRecord.StockSymbol,
                DateTimeStamp = Timestamp.FromDateTime(latestStockPriceRecord.DateTime),
                CurrentPrice = latestStockPriceRecord.Price
            };

            // Cancellation request might be called during prior processing
            if (!context.CancellationToken.IsCancellationRequested)
            {
                await responseStream.WriteAsync(stockData);
            }
        }
    }
}