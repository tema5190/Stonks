
using Google.Protobuf.WellKnownTypes;
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
        config.GetSection("StockServerAddress").ToString();
        
        
        using var channel = GrpcChannel.ForAddress(stockServiceBaseAddress);
        var client = new StockService.StockServiceClient(channel);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(50));

        using var streamingCall = client.GetStockStream(new Empty(), cancellationToken: cts.Token);

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
    }
}