using Stonks.GRPCBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<StockConnectionEndpoint>();
app.MapGet("/",
    () =>
        "This is gRPC stonks client");

app.Run();