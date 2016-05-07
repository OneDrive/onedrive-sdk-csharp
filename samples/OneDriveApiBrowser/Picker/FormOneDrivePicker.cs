using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneDriveSamples.Picker
{
    using Microsoft.Graph;
    using Microsoft.OneDrive.Sdk;

    public partial class FormOneDrivePicker : Form
    {
        public const string OAuthDesktopEndPoint = "https://login.live.com/oauth20_desktop.srf";

        #region Properties
        public string StartUrl { get; private set; }
        public string EndUrl { get; private set; }
        public PickerResult PickerResult { get; private set; }
        
        #endregion

        #region Constructor
        private FormOneDrivePicker(string startUrl, string endUrl)
        {
            InitializeComponent();

            this.StartUrl = startUrl;
            this.EndUrl = endUrl;
            this.FormClosing += FormMicrosoftAccountAuth_FormClosing;
        }
        #endregion

        #region Private Methods
        private void FormMicrosoftAccountAuth_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void FormMicrosoftAccountAuth_Load(object sender, EventArgs e)
        {
            this.webBrowser.CanGoBackChanged += webBrowser_CanGoBackChanged;
            this.webBrowser.CanGoForwardChanged += webBrowser_CanGoBackChanged;
            FixUpNavigationButtons();

            this.webBrowser.Navigated += webBrowser_Navigated;

            System.Diagnostics.Debug.WriteLine("Navigating to start URL: " + this.StartUrl);
            this.webBrowser.Navigate(this.StartUrl);
        }

        void webBrowser_CanGoBackChanged(object sender, EventArgs e)
        {
            FixUpNavigationButtons();
        }

        private void webBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Navigated to: " + webBrowser.Url.AbsoluteUri.ToString());

            this.Text = webBrowser.DocumentTitle;

            if (this.webBrowser.Url.AbsoluteUri.StartsWith(EndUrl))
            {
                this.PickerResult = new PickerResult(this.webBrowser.Url);
                CloseWindow();
            }
        }

        private void CloseWindow()
        {
            const int interval = 100;
            var t = new System.Threading.Timer(new System.Threading.TimerCallback((state) => 
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.BeginInvoke(new MethodInvoker(() => this.Close()));
            }), null, interval, System.Threading.Timeout.Infinite);
        }

        private void FixUpNavigationButtons()
        {
            toolStripBackButton.Enabled = webBrowser.CanGoBack;
            toolStripForwardButton.Enabled = webBrowser.CanGoForward;
        }
        #endregion


        public Task<DialogResult> ShowDialogAsync(IWin32Window owner = null)
        {
            TaskCompletionSource<DialogResult> tcs = new TaskCompletionSource<DialogResult>();
            this.FormClosed += (s, e) =>
            {
                tcs.SetResult(this.DialogResult);
            };
            if (owner == null)
                this.Show();
            else
                this.Show(owner);

            return tcs.Task;
        }

        #region Static Methods

        private static string GenerateScopeString(string[] scopes)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var scope in scopes)
            {
                if (sb.Length > 0)
                    sb.Append(" ");
                sb.Append(scope);
            }
            return sb.ToString();
        }

        private static string BuildUriWithParameters(string baseUri, Dictionary<string, string> queryStringParameters)
        {
            var sb = new StringBuilder();
            sb.Append(baseUri);
            sb.Append("?");
            foreach (var param in queryStringParameters)
            {
                if (sb[sb.Length - 1] != '?')
                    sb.Append("&");
                sb.Append(param.Key);
                sb.Append("=");
                sb.Append(Uri.EscapeDataString(param.Value));
            }
            return sb.ToString();
        }

        public static async Task<PickerResult> OpenFileAsync(string clientId, bool multiSelect, IWin32Window owner = null)
        {
            const string msaAuthUrl = "https://login.live.com/oauth20_authorize.srf";
            const string msaDesktopUrl = "https://login.live.com/oauth20_desktop.srf";
            string startUrl, completeUrl;

            var scopes = multiSelect ? "onedrive_onetime.access:readfile|multi" : "onedrive_onetime.access:readfile|single";

            Dictionary<string, string> urlParam = new Dictionary<string,string>();
            urlParam.Add(Constants.Authentication.ClientIdKeyName, clientId);
            urlParam.Add(Constants.Authentication.ScopeKeyName, scopes);
            urlParam.Add(Constants.Authentication.RedirectUriKeyName, msaDesktopUrl);
            urlParam.Add(Constants.Authentication.ResponseTypeKeyName, "token");

            startUrl = BuildUriWithParameters(msaAuthUrl, urlParam);
            completeUrl = msaDesktopUrl;

            FormOneDrivePicker authForm = new FormOneDrivePicker(startUrl, completeUrl);
            DialogResult result = await authForm.ShowDialogAsync(owner);
            if (DialogResult.OK == result)
            {
                return authForm.PickerResult;
            }
            return null;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            webBrowser.GoBack();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            webBrowser.GoForward();
        }
        #endregion
    }

    public class PickerResult
    {
        public PickerResult(Uri resultUri)
        {
            Console.WriteLine(resultUri.ToString());
            string[] queryParams = null;
            int accessTokenIndex = resultUri.AbsoluteUri.IndexOf("#" + Constants.Authentication.AccessTokenKeyName);
            if (accessTokenIndex > 0)
            {
                queryParams = resultUri.AbsoluteUri.Substring(accessTokenIndex + 1).Split('&');
            }
            else
            {
                queryParams = resultUri.Query.TrimStart('?').Split('&');
            }

            foreach (string param in queryParams)
            {
                Console.WriteLine("Found parameter: " + param);
                string[] kvp = param.Split('=');
                switch (kvp[0])
                {
                    case Constants.Authentication.AccessTokenKeyName:
                        this.AccessToken = kvp[1];
                        break;
                    case Constants.Authentication.TokenTypeKeyName:
                        this.TokenType = kvp[1];
                        break;
                    case Constants.Authentication.ExpiresInKeyName:
                        this.AccessTokenExpiresIn = new TimeSpan(0, 0, int.Parse(kvp[1]));
                        break;
                    case Constants.Authentication.ScopeKeyName:
                        var scopeValues = kvp[1].Split(new string[] {":"}, StringSplitOptions.RemoveEmptyEntries);
                        this.SelectionId = scopeValues[2].Replace('_', '.');
                        break;

                    case Constants.Authentication.ErrorKeyName:
                        this.ErrorCode = kvp[1];
                        break;
                    case Constants.Authentication.ErrorDescriptionKeyName:
                        this.ErrorDescription = Uri.UnescapeDataString(kvp[1]);
                        break;
                }
            }
        }

        #region Properties

        public string ErrorCode { get; private set; }
        public string ErrorDescription { get; private set; }
        public string AccessToken { get; private set; }
        public string TokenType { get; private set; }
        public TimeSpan AccessTokenExpiresIn { get; private set; }
        public string SelectionId { get; private set; }
        #endregion

        public async Task<Item> GetItemsFromSelectionAsync(IOneDriveClient oneDriveClient = null)
        {
            const string msa_client_id = "0000000044128B55";
            var offers = new string[] { "onedrive.readwrite", "wl.signin" };

            if (oneDriveClient == null)
            {
                /*oneDriveClient = OneDriveClient.GetMicrosoftAccountClient(
                    msa_client_id,
                    "https://login.live.com/oauth20_desktop.srf",
                    offers,
                    webAuthenticationUi: new FormsWebAuthenticationUi());

                await oneDriveClient.AuthenticateAsync();*/
            }

            var parts = this.SelectionId.Split('.');
            var bundleID = parts[2];
            return
                await
                    oneDriveClient.Drive.Items[bundleID].Request().Expand("thumbnails,children(expand=thumbnails)").GetAsync();
        }
    }

    public class SelectionResponse
    {
        public PickedFile[] data { get; set; }
    }

    public class PickedFile
    {
        public string id { get; set; }
        public string name { get; set; }

        public string parent_id { get; set; }
        public DateTime client_update_time { get; set; }
        public DateTime created_time { get; set; }
        public string link { get; set; }
        public long size { get; set; }
        public string source { get; set; }
        public string type { get; set; }
        public DateTime updated_time{ get; set; }
        public string upload_location { get; set; }
    }
}