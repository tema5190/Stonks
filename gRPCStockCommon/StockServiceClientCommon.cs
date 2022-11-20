using Grpc.Net.Client;
using gRPCStockServiceContracts;

namespace gRPCStockCommon;


public class StockServiceClientCommon : StockService.StockServiceClient
{
    public StockServiceClientCommon(GrpcChannel channel) : base(channel)
    {
    }
}