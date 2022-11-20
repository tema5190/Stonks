using Grpc.Core;
using Grpc.Net.Client;

namespace Stonks.MAUI
{
    public partial class MainPage : ContentPage
    {
        CancellationTokenSource cancellationTokenSource;
        bool loadingStarted;
        List<CancellationToken> loadTasksCancellationToken;
        string stockServiceBaseAddress;

        public MainPage()
        {
            InitializeComponent();
            cancellationTokenSource = new CancellationTokenSource();
            loadTasksCancellationToken= new List<CancellationToken>();
            stockServiceBaseAddress = "http://localhost:5094";
        }

        private void StartButtonClicked(object sender, EventArgs e)
        {
            loadingStarted = !loadingStarted;

            if(!loadingStarted)
            {
                StartButton.Text = "Start";
                cancellationTokenSource.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
                MainTextOutput.Text += "Cancelled.";
            } else
            {
                StartButton.Text = "Stop";

                var requestedStockList = new List<StockDataRequest>();
                if (MSFTCheckbox.IsChecked == true)
                {
                    requestedStockList.Add(new() { RequestedStockSymbol = "MSFT" });
                }
                if (AAPLCheckbox.IsChecked == true)
                {
                    requestedStockList.Add(new() { RequestedStockSymbol = "AAPL" });
                }
                if (AMZNCheckbox.IsChecked == true)
                {
                    requestedStockList.Add(new() { RequestedStockSymbol = "AMZN" });
                }

                if(!string.IsNullOrEmpty(AddressInputEntry.Text) && Uri.TryCreate(AddressInputEntry.Text, UriKind.Absolute, out _))
                {
                    stockServiceBaseAddress = AddressInputEntry.Text;
                }

                using var channel = GrpcChannel.ForAddress(stockServiceBaseAddress);
                var client = new StockServiceClientCommon(channel);

                foreach (var dataRequest in requestedStockList)
                {
                    var task = Task.Run(async () =>
                    {
                    using var streamingCall = client.GetStockStream(dataRequest, cancellationToken: cancellationTokenSource.Token);

                    try
                    {
                        await foreach (var stockData in streamingCall.ResponseStream.ReadAllAsync(cancellationToken: cancellationTokenSource.Token))
                        {
                            MainTextOutput.Text += $"{stockData.DateTimeStamp.ToDateTime():s} | {stockData.StockSymbol} | {stockData.CurrentPrice}$\n";
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            throw;
                        }
                    });
                }

            }


            SemanticScreenReader.Announce(StartButton.Text);
        }
    }
}