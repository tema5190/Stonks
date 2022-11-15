using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using gRPCStockServiceContracts;

namespace Stonks.GRPCBackend.Services;

public class StockConnectionEndpoint: gRPCStockServiceContracts.StockService.StockServiceBase
{
    private readonly ILogger<StockConnectionEndpoint> _logger;

    public StockConnectionEndpoint(ILogger<StockConnectionEndpoint> logger)
    {
        _logger = logger;
    }
    
    public override async Task GetStockStream(Empty _, IServerStreamWriter<StockData> responseStream, ServerCallContext context)
    {
        var rng = new Random();
        var i = 500;
        while (!context.CancellationToken.IsCancellationRequested && i > 0)
        {
            await Task.Delay(1000);
        
            var forecast = new StockData
            {
                StockSymbol = "MSFT",
                CurrentPrice = rng.Next(100,200),
                DateTimeStamp = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
            };

            
            _logger.LogInformation($"Sending stockdata response + {Thread.CurrentThread.ManagedThreadId}");

            i--;

            if (!context.CancellationToken.IsCancellationRequested) // might be requested during the calculation so check twice.
            {
                await responseStream.WriteAsync(forecast);    
            }
        }
    }  
}