// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace OneDrivePhotoBrowser
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

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
        private const string AadClientId = "client id";
        private const string AadResource = "resource";
        private const string AadTenantId = "tenant id";
        private const string AadReturnUrl = "return url";

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
