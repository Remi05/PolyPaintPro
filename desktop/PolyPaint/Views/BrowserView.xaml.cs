using PolyPaint.Services;
using PolyPaint.Utils;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Navigation;

namespace PolyPaint.Views
{
    /// <summary>
    /// Interaction logic for BrowserView.xaml
    /// </summary>
    public partial class BrowserView : Window
    {
        public event EventHandler FacebookConnected;

        public event EventHandler GoogleConnected;

        public BrowserView(string Uri)
        {
            InitializeComponent();
            ConnectionBrowser.Navigated += NavigatedHandler;
            ConnectionBrowser.Navigate(Uri);
        }

        public async void NavigatedHandler(object sender, NavigationEventArgs e)
        {
            if (e.Uri.Host.Equals("www.facebook.com") && e.Uri.Fragment.Contains("access_token="))
            {
                Visibility = Visibility.Hidden;
                FacebookConnected?.Invoke(this, new ConnectedEventArgs(ParseGetParameter(e.Uri.Fragment, "access_token")));
                return;
            }
            if (e.Uri.Host.Equals("accounts.google.com"))
            {
                dynamic doc = ConnectionBrowser.Document;
                var htmlText = doc.documentElement.InnerHtml;
                Regex codeRegex = new Regex(@"<title>Success\scode=[a-zA-Z\d_\-\/~\.]{57}<\/title>");
                Match codeMatch = codeRegex.Match(htmlText);
                if (codeMatch.Success)
                {
                    Visibility = Visibility.Hidden;
                    string code = codeMatch.Value.Substring(20, 57);
                    string token = await GoogleAPI.GetGoogleAccessToken(code);
                    GoogleConnected?.Invoke(this, new ConnectedEventArgs(token));
                }
            }
        }

        private string ParseGetParameter(string args, string parameterName)
        {
            string token = "";
            string[] parameters = args.Replace("#", "").Replace("?", "").Split('&');

            foreach (string parameter in parameters)
            {
                if (parameter.StartsWith(parameterName))
                {
                    token = parameter.Split('=')[1];
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        return token;
                    }
                }
            }
            return string.Empty;
        }


    }
}
