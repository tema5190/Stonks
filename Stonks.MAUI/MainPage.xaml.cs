namespace Stonks.MAUI
{
    public partial class MainPage : ContentPage
    {
        CancellationTokenSource source;
        bool loadingStarted;
        List<CancellationToken> loadTasksCancellationToken;

        public MainPage()
        {
            InitializeComponent();
            source = new CancellationTokenSource();
            loadTasksCancellationToken= new List<CancellationToken>();
        }

        private void StartButtonClicked(object sender, EventArgs e)
        {
            var cancellationTolen = source.Token;

            loadingStarted = !loadingStarted;

            if(!loadingStarted)
            {
                StartButton.Text = "Start";
                source.Cancel();
                source = new CancellationTokenSource();
                MainTextOutput.Text += "Cancelled.";
            } else
            {
                StartButton.Text = "Stop";
            }


            SemanticScreenReader.Announce(StartButton.Text);
        }
    }
}