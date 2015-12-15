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

namespace Test.OneDriveSdk.Requests
{
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class OneDriveClientExtensionsTests : RequestTestBase
    {
        [TestMethod]
        public async Task GetAuthenticatedMicrosoftAccountClient_NoSecret()
        {
            var appId = "appId";
            var returnUrl = "returnUrl";
            var scopes = new string[] { "scope" };

            this.SetupServiceInfoProviderForMicrosoftAccount(appId, /* clientSecret */ null, returnUrl, scopes);

            var client = await OneDriveClient.GetAuthenticatedMicrosoftAccountClient(
                appId,
                returnUrl,
                scopes,
                this.serviceInfoProvider.Object,
                this.credentialCache.Object,
                this.httpProvider.Object);

            this.authenticationProvider.Verify(provider => provider.AuthenticateAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetAuthenticatedMicrosoftAccountClient_WithSecret()
        {
            var appId = "appId";
            var clientSecret = "secret";
            var returnUrl = "returnUrl";
            var scopes = new string[] { "scope" };

            this.SetupServiceInfoProviderForMicrosoftAccount(appId, clientSecret, returnUrl, scopes);

            var client = await OneDriveClient.GetAuthenticatedMicrosoftAccountClient(
                appId,
                returnUrl,
                scopes,
                clientSecret,
                this.serviceInfoProvider.Object,
                this.credentialCache.Object,
                this.httpProvider.Object);

            this.authenticationProvider.Verify(provider => provider.AuthenticateAsync(), Times.Once);
        }

        [TestMethod]
        public void GetMicrosoftAccountClient_NoSecret_InitializeDefaults()
        {
            var appId = "appId";
            var returnUrl = "returnUrl";
            var scopes = new string[] { "scope" };

            var client = OneDriveClient.GetMicrosoftAccountClient(
                appId,
                returnUrl,
                scopes) as OneDriveClient;
            
            Assert.IsNotNull(client.credentialCache, "Cache not initialized.");

            var initializedServiceInfoProvider = client.serviceInfoProvider as ServiceInfoProvider;
            Assert.IsNotNull(initializedServiceInfoProvider, "Service info provider not correctly initialized.");

            var initializedHttpProvider = client.HttpProvider as HttpProvider;
            Assert.IsNotNull(initializedHttpProvider, "HTTP provider not correctly initialized.");

            Assert.AreEqual(appId, client.appConfig.MicrosoftAccountAppId, "Incorrect app ID set.");
            Assert.IsNull(client.appConfig.MicrosoftAccountClientSecret, "Client secret set.");
            Assert.AreEqual(returnUrl, client.appConfig.MicrosoftAccountReturnUrl, "Incorrect return URL set.");
            Assert.AreEqual(scopes, client.appConfig.MicrosoftAccountScopes, "Incorrect scopes set.");
        }

        [TestMethod]
        public void GetMicrosoftAccountClient_WithSecret_InitializeDefaults()
        {
            var appId = "appId";
            var clientSecret = "secret";
            var returnUrl = "returnUrl";
            var scopes = new string[] { "scope" };

            var client = OneDriveClient.GetMicrosoftAccountClient(
                appId,
                returnUrl,
                scopes,
                clientSecret) as OneDriveClient;

            Assert.IsNotNull(client.credentialCache, "Cache not initialized.");

            var initializedServiceInfoProvider = client.serviceInfoProvider as ServiceInfoProvider;
            Assert.IsNotNull(initializedServiceInfoProvider, "Service info provider not correctly initialized.");

            var initializedHttpProvider = client.HttpProvider as HttpProvider;
            Assert.IsNotNull(initializedHttpProvider, "HTTP provider not correctly initialized.");

            Assert.AreEqual(appId, client.appConfig.MicrosoftAccountAppId, "Incorrect app ID set.");
            Assert.AreEqual(clientSecret, client.appConfig.MicrosoftAccountClientSecret, "Incorrect client secret set.");
            Assert.AreEqual(returnUrl, client.appConfig.MicrosoftAccountReturnUrl, "Incorrect return URL set.");
            Assert.AreEqual(scopes, client.appConfig.MicrosoftAccountScopes, "Incorrect scopes set.");
        }

        [TestMethod]
        public async Task GetSilentlyAuthenticatedMicrosoftAccountClient_NoSecret()
        {
            var appId = "appId";
            var refreshToken = "refresh";
            var returnUrl = "returnUrl";
            var scopes = new string[] { "scope" };

            this.authenticationProvider.SetupSet(provider => provider.CurrentAccountSession = It.IsAny<AccountSession>()).Verifiable();

            this.SetupServiceInfoProviderForMicrosoftAccount(appId, /* clientSecret */ null, returnUrl, scopes);

            var client = await OneDriveClient.GetSilentlyAuthenticatedMicrosoftAccountClient(
                appId,
                returnUrl,
                scopes,
                refreshToken,
                this.serviceInfoProvider.Object,
                this.credentialCache.Object,
                this.httpProvider.Object);

            this.authenticationProvider.VerifySet(
                provider => provider.CurrentAccountSession = It.Is<AccountSession>(session => refreshToken.Equals(session.RefreshToken)),
                Times.Once);

            this.authenticationProvider.Verify(provider => provider.AuthenticateAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetSilentlyAuthenticatedMicrosoftAccountClient_WithSecret()
        {
            var appId = "appId";
            var clientSecret = "secret";
            var refreshToken = "refresh";
            var returnUrl = "returnUrl";
            var scopes = new string[] { "scope" };

            this.authenticationProvider.SetupSet(provider => provider.CurrentAccountSession = It.IsAny<AccountSession>()).Verifiable();

            this.SetupServiceInfoProviderForMicrosoftAccount(appId, clientSecret, returnUrl, scopes);

            var client = await OneDriveClient.GetSilentlyAuthenticatedMicrosoftAccountClient(
                appId,
                returnUrl,
                scopes,
                clientSecret,
                refreshToken,
                this.serviceInfoProvider.Object,
                this.credentialCache.Object,
                this.httpProvider.Object);

            this.authenticationProvider.VerifySet(
                provider => provider.CurrentAccountSession = It.Is<AccountSession>(session => refreshToken.Equals(session.RefreshToken)),
                Times.Once);

            this.authenticationProvider.Verify(provider => provider.AuthenticateAsync(), Times.Once);
        }

        private void SetupServiceInfoProviderForMicrosoftAccount(string appId, string clientSecret, string returnUrl, string[] scopes)
        {
            this.serviceInfoProvider.Setup(provider => provider.GetServiceInfo(It.Is<AppConfig>(
                        config => config.MicrosoftAccountAppId.Equals(appId)
                            && config.MicrosoftAccountReturnUrl.Equals(returnUrl)
                            && config.MicrosoftAccountScopes == scopes
                            && string.Equals(config.MicrosoftAccountClientSecret, clientSecret)),
                    this.credentialCache.Object,
                    this.httpProvider.Object,
                    ClientType.Consumer))
                .Returns(Task.FromResult<ServiceInfo>(
                    new MicrosoftAccountServiceInfo
                    {
                        AuthenticationProvider = this.authenticationProvider.Object
                    }));
        }
    }
}
