using Grpc.Core;
using Grpc.Net.Client;
using gRPCStockServiceContracts;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static async Task Main(string[] args)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();
        
        var stockServiceBaseAddress = 
        config.GetSection("StockServerAddress").Value;

        using var channel = GrpcChannel.ForAddress(stockServiceBaseAddress);
        var client = new StockService.StockServiceClient(channel);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(50));
        
        var requestedStockList = new List<StockDataRequest>()
        {
            new() {RequestedStockSymbol = "AMZN"},
            new() {RequestedStockSymbol = "AAPL"},
            new() {RequestedStockSymbol = "AMZN"}
        };

        var tasks = new List<Task>();

        foreach (var dataRequest in requestedStockList)
        {
            var task = Task.Run(async () =>
            {
                using var streamingCall = client.GetStockStream(dataRequest, cancellationToken: cts.Token);

                try
                {
                    await foreach (var stockData in streamingCall.ResponseStream.ReadAllAsync(cancellationToken: cts.Token))
                    {
                        Console.WriteLine($"{stockData.DateTimeStamp.ToDateTime():s} | {stockData.StockSymbol} | {stockData.CurrentPrice}$");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            
            tasks.Add(task);
        }

        var resultTask = Task.WhenAll(tasks);

        try
        {
            resultTask.Wait();
        }
        catch
        {
        }
    }
}