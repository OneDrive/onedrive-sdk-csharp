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
    using Microsoft.OneDrive.Sdk.Authentication;
    using Models;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountSelection : Page
    {
        private readonly string[] scopes = new string[] { "onedrive.readonly" };

        private const string AadClientId = "Insert your AAD app ID here";
        private const string AadResource = "Insert your AAD service resource ID here";
        private const string AadReturnUrl = "Insert your AAD return URL here";

        private const string MsaClientId = "Insert your MSA app ID here";
        private const string MsaReturnUrl = "https://login.live.com/oauth20_desktop.srf";

        public AccountSelection()
        {
            this.InitializeComponent();
            this.Loaded += AccountSelection_Loaded;
        }

        private async void AccountSelection_Loaded(object sender, RoutedEventArgs e)
        {
            var adalAuthenticationProvider = ((App)Application.Current).AuthenticationProvider as AdalAuthenticationProvider;
            var msaAuthenticationProvider = ((App)Application.Current).AuthenticationProvider as MsaAuthenticationProvider;
            if (adalAuthenticationProvider != null)
            {
                adalAuthenticationProvider.SignOut();
            }
            else if (msaAuthenticationProvider != null)
            {
                await msaAuthenticationProvider.SignOutAsync();
            }

            ((App)Application.Current).AuthenticationProvider = null;

            var client = ((App)Application.Current).OneDriveClient;
            if (client != null)
            {
                client.HttpProvider.Dispose();

                ((App)Application.Current).OneDriveClient = null;
            }
        }

        private async void AadButton_Click(object sender, RoutedEventArgs e)
        {
            var adalAuthenticationProvider = new AdalAuthenticationProvider(AccountSelection.AadClientId, AccountSelection.AadReturnUrl);

            var discoveryServiceHelper = new DiscoveryServiceHelper(adalAuthenticationProvider);
            var userInfo = await discoveryServiceHelper.DiscoverFilesEndpointForUserAsync();

            await adalAuthenticationProvider.AuthenticateUserAsync(userInfo.ServiceResourceId);

            ((App)Application.Current).AuthenticationProvider = adalAuthenticationProvider;

            this.InitializeClient(e, userInfo);
        }

        private async void MsaButton_Click(object sender, RoutedEventArgs e)
        {
            var msaAuthenticationProvider = new MsaAuthenticationProvider(AccountSelection.MsaClientId, AccountSelection.MsaReturnUrl, this.scopes);

            await msaAuthenticationProvider.AuthenticateUserAsync();

            ((App)Application.Current).AuthenticationProvider = msaAuthenticationProvider;

            this.InitializeClient(e);
        }

        private void InitializeClient(RoutedEventArgs e, BusinessServiceInfo userInfo = null)
        {
            if (((App)Application.Current).OneDriveClient == null)
            {
                string baseUrl = userInfo == null ? "https://api.onedrive.com/v1.0" : userInfo.ServiceEndpointBaseUrl;

                ((App)Application.Current).OneDriveClient = new OneDriveClient(baseUrl, ((App)Application.Current).AuthenticationProvider);

                ((App)Application.Current).NavigationStack.Add(new ItemModel(new Item()));
                Frame.Navigate(typeof(MainPage), e);
            }
            else
            {
                Frame.Navigate(typeof(MainPage), e);
            }
        }
    }
}
