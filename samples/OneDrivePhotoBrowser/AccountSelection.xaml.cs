// ------------------------------------------------------------------------------
//  Copyright (c) 2015 Microsoft Corporation
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
// ------------------------------------------------------------------------------

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace OneDrivePhotoBrowser
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Diagnostics;

    using Microsoft.Graph;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.OneDrive.Sdk;
    using Models;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountSelection : Page
    {
        private readonly string[] scopes = new string[] { "onedrive.readonly", "wl.signin" };

        // Set these values to your app's ID and return URL.
        private const string AadAuthenticationEndpoint = "https://login.microsoftonline.com/";
        private const string AadClientId = "5a51bad1-557c-42d0-9b12-72eff58ba798";
        private const string AadResource = "https://ginach-my.sharepoint.com/";
        private const string AadTenantId = "5f09b637-8b81-4912-b800-3260b3b1437d";
        private const string AadReturnUrl = "https://localhost:777";

        public AccountSelection()
        {
            this.InitializeComponent();
            this.Loaded += AccountSelection_Loaded;
        }

        private void AccountSelection_Loaded(object sender, RoutedEventArgs e)
        {
            var authenticationContext = ((App)Application.Current).AuthenticationContext;
            if (authenticationContext != null)
            {
                authenticationContext.TokenCache.Clear();
            }

            var client = ((App)Application.Current).OneDriveClient;
            if (client != null)
            {
                client.HttpProvider.Dispose();

                ((App)Application.Current).OneDriveClient = null;
            }

            // Don't show AAD login if the required AAD auth values aren't set
            if (string.IsNullOrEmpty(AccountSelection.AadClientId) ||
                string.IsNullOrEmpty(AccountSelection.AadReturnUrl) ||
                string.IsNullOrEmpty(AccountSelection.AadResource) ||
                string.IsNullOrEmpty(AccountSelection.AadTenantId))
            {
                AadButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void AadButton_Click(object sender, RoutedEventArgs e)
        {
            var authenticationContext = ((App)Application.Current).AuthenticationContext;
            if (authenticationContext == null)
            {
                authenticationContext = new AuthenticationContext(
                    string.Concat(AccountSelection.AadAuthenticationEndpoint, AccountSelection.AadTenantId),
                    false);

                authenticationContext.TokenCache.Clear();

                ((App)Application.Current).AuthenticationContext = authenticationContext;
            }

            var authenticationResult = await this.AuthenticateUserAsync();

            ((App)Application.Current).AuthenticationProvider = new DelegateAuthenticationProvider(
                async (HttpRequestMessage requestMessage) =>
                {
                    var silentAuthenticationResult = await authenticationContext.AcquireTokenSilentAsync(
                        AccountSelection.AadResource,
                        AccountSelection.AadClientId);

                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                        silentAuthenticationResult.AccessTokenType,
                        silentAuthenticationResult.AccessToken);
                });

            this.InitializeClient(true, e);
        }

        private void MsaButton_Click(object sender, RoutedEventArgs e)
        {
            this.InitializeClient(false, e);
        }

        private void InitializeClient(bool isBusiness, RoutedEventArgs e)
        {
            if (((App)Application.Current).OneDriveClient == null)
            {
                string baseUrl = isBusiness ? string.Format("{0}_api/v2.0", AccountSelection.AadResource) : "https://api.onedrive.com/v1.0";

                ((App)Application.Current).OneDriveClient = new OneDriveClient(baseUrl, ((App)Application.Current).AuthenticationProvider);

                ((App)Application.Current).NavigationStack.Add(new ItemModel(new Item()));
                Frame.Navigate(typeof(MainPage), e);
            }
            else
            {
                Frame.Navigate(typeof(MainPage), e);
            }
        }

        private async Task<AuthenticationResult> AuthenticateUserAsync()
        {
            AuthenticationResult authenticationResult = null;
            var authenticationContext = ((App)Application.Current).AuthenticationContext;

            try
            {
                authenticationResult = await authenticationContext.AcquireTokenSilentAsync(AccountSelection.AadResource, AccountSelection.AadClientId);
            }
            catch (Exception)
            {
                // If an exception happens during silent authentication try interactive authentication.
            }

            if (authenticationResult != null && authenticationResult.Status == AuthenticationStatus.Success)
            {
                return authenticationResult;
            }

            authenticationResult = await authenticationContext.AcquireTokenAsync(
                AccountSelection.AadResource,
                AccountSelection.AadClientId,
                new Uri(AccountSelection.AadReturnUrl),
                PromptBehavior.Auto);

            return authenticationResult;
        }
    }
}
