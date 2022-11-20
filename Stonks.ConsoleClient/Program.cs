using Grpc.Core;
using Grpc.Net.Client;
using gRPCStockServiceContracts;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

internal class Program
{
    private const string EXIT = "x";

    private static async Task Main(string[] args)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();
        
        var stockServiceBaseAddress = 
        config.GetSection("StockServerAddress").Value;

        string command;
        do
        {
            PrintMenuInConsole();
            command = Console.ReadLine();

            if (command == EXIT)
            {
                break;
            }

            if(command.StartsWith("e"))
            {
                var rawUrl = command.Split(" ")[1];
                var isValid = Uri.TryCreate(rawUrl, new UriCreationOptions(), out var uri);
                if(!isValid)
                {
                    Console.WriteLine("Uri is not valid");
                    continue;
                }

                stockServiceBaseAddress = uri.ToString();
                Console.WriteLine($"Now listening {stockServiceBaseAddress}");
                continue;
            }

            if (!Regex.IsMatch(command, @"^[1-3]+$"))
            {
                Console.WriteLine("Command is not valid");
                continue;
            }

            var requestedStockList = GetStockDataRequestListFromCommand(command);

            var tasks = new List<Task>();
            
            using var channel = GrpcChannel.ForAddress(stockServiceBaseAddress);
            var client = new StockService.StockServiceClient(channel);
            
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(50)); // for simplification works only 50 seconds

            foreach (var dataRequest in requestedStockList)
            {
                var task = Task.Run(async () =>
                {
                    using var streamingCall = client.GetStockStream(dataRequest, cancellationToken: cts.Token);

                    try
                    {
                        await foreach (var stockData in streamingCall.ResponseStream.ReadAllAsync(cancellationToken: cts.Token))
                        {
                            var decimalConsoleValue = (decimal) stockData.CurrentPrice / 100;
                            Console.WriteLine($"{stockData.DateTimeStamp.ToDateTime():s} | {stockData.StockSymbol} | {decimalConsoleValue}$");
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


        } while (true);
        
        Console.WriteLine("Exiting...");
    }

    public static List<StockDataRequest> GetStockDataRequestListFromCommand(string command)
    {
        var listResult = new List<StockDataRequest>();
        if(command.Contains("1"))
        {
            listResult.Add(new() { RequestedStockSymbol = "AMZN" });
        }
        if (command.Contains("2"))
        {
            listResult.Add(new() { RequestedStockSymbol = "AAPL" });
        }
        if (command.Contains("3"))
        {
            listResult.Add(new() { RequestedStockSymbol = "MSFT" });
        }
        return listResult;
    }

    public static void PrintMenuInConsole()
    {
        Console.WriteLine("This application is using couple of commands:");
        Console.WriteLine("You can enter a number for stocks subscriptions:");
        Console.WriteLine("AMZN - 1, AAPL - 2, MSFT - 3");
        Console.WriteLine("'x' to exit");
        Console.WriteLine("For example for all items you can enter '123'");
        Console.WriteLine("For advanced user only enter 'e https://address.com:port' for changing listening server");
    }
}