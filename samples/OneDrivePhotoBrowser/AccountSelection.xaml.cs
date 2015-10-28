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
    using System.Diagnostics;

    using Microsoft.OneDrive.Sdk;
    using Models;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountSelection : Page
    {
        private readonly string[] scopes = new string[] { "onedrive.readonly", "wl.offline_access", "wl.signin" };

        // Set these values to your app's ID and return URL.
        private readonly string oneDriveForBusinessAppId = "67b8454b-58df-4e6d-a688-c769bd327052";
        private readonly string oneDriveForBusinessReturnUrl = "https://localhost:777";

        public AccountSelection()
        {
            this.InitializeComponent();
            this.Loaded += AccountSelection_Loaded;
        }

        private async void AccountSelection_Loaded(object sender, RoutedEventArgs e)
        {
            if (((App)Application.Current).OneDriveClient != null)
            {
                await ((App)Application.Current).OneDriveClient.SignOutAsync();

                var client = ((App)Application.Current).OneDriveClient as OneDriveClient;
                if (client != null)
                {
                    client.Dispose();
                }

                ((App)Application.Current).OneDriveClient = null;
            }

            // Don't show AAD login if the required AAD auth values aren't set
            if (string.IsNullOrEmpty(oneDriveForBusinessAppId) || string.IsNullOrEmpty(oneDriveForBusinessReturnUrl))
            {
                AadButton.Visibility = Visibility.Collapsed;
            }
        }

        private void AadButton_Click(object sender, RoutedEventArgs e)
        {
            this.InitializeClient(ClientType.Business, e);
        }

        private void MsaButton_Click(object sender, RoutedEventArgs e)
        {
            this.InitializeClient(ClientType.Consumer, e);
        }

        private async void InitializeClient(ClientType clientType, RoutedEventArgs e)
        {
            if (((App)Application.Current).OneDriveClient == null)
            {
                var client = clientType == ClientType.Consumer
                    ? OneDriveClientExtensions.GetUniversalClient(this.scopes) as OneDriveClient
                    : BusinessClientExtensions.GetActiveDirectoryClient(
                        oneDriveForBusinessAppId,
                        oneDriveForBusinessReturnUrl) as OneDriveClient;

                try
                {
                    await client.AuthenticateAsync();
                    ((App)Application.Current).OneDriveClient = client;
                    ((App)Application.Current).NavigationStack.Add(new ItemModel(new Item()));
                    Frame.Navigate(typeof(MainPage), e);
                }
                catch (OneDriveException exception)
                {
                    // Swallow the auth exception but write message for debugging.
                    Debug.WriteLine(exception.Error.Message);
                    client.Dispose();
                }
            }
            else
            {
                Frame.Navigate(typeof(MainPage), e);
            }
        }
    }
}
