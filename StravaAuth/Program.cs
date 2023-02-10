using System;
using System.Windows.Forms;
using de.schumacher_bw.Strava;
using de.schumacher_bw.Strava.Model;

namespace StravaAuth
{
    internal static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // define the strava client specific info
            int clientId = 77496;
            string clientSecret = "e7ec2036f1893fddf960bbe36d7d1d01904682a9";
            // define the text file that holds the user authentication info
            string stravaAuth = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.ExecutablePath), "stravaApi.txt");
            string serializedApi = System.IO.File.Exists(stravaAuth) ? System.IO.File.ReadAllText(stravaAuth) : null;
            string callbackUrl = "http://localhost/doesNotExist/";

            // create an instance of the api and reload the local stored auth info
            var api = new StravaApiV3Sharp(clientId, clientSecret, serializedApi);
            // add a delegate to the event of the refreshToken or authToken been updated
            api.SerializedObjectChanged += (s, e) => System.IO.File.WriteAllText(stravaAuth, api.Serialize());

            // create a winform that contains a browser 
            var form = new Form() { Width = 1000, Height = 1000 };
            var webView = new Microsoft.Toolkit.Forms.UI.Controls.WebView();
            ((System.ComponentModel.ISupportInitialize)webView).BeginInit();
            webView.Dock = DockStyle.Fill;
            form.Controls.Add(webView);
            ((System.ComponentModel.ISupportInitialize)webView).EndInit();


            // in case the app is not yet connected to the api => start the authentication prozedure
            if (api.Authentication.Scope == Scopes.None_Unknown)
            {
                // ensure to be called again once the authentication is done. 
                // We will be forewared to a not existing url and catch this event
                webView.NavigationStarting += (s, e) =>
                {
                    if (e.Uri?.AbsoluteUri.StartsWith(callbackUrl) ?? false) // in case we are forewarded to the callback URL
                    {
                        api.Authentication.DoTokenExchange(e.Uri); // do the token exchange with the stava api
                        webView.NavigateToString("<h1>Token saved.</h1>");
                    }
                };

                // navigate to the strava auth page to get write access 
                webView.Navigate(api.Authentication.GetAuthUrl(new Uri(callbackUrl), Scopes.ActivityWrite));
            }
            else // the api is allready connected and the information have been loaded from the stravaAuth-file
            {
                webView.NavigateToString("<h1>Token already exists.</h1>");
            }

            form.ShowDialog();
            form.Dispose();
        }
    }
}
