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

namespace Test.OneDriveSdk.WindowsForms.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Moq;
    using OneDriveSdk.Mocks;

    [TestClass]
    public class AdalAuthenticationProviderTests : AdalAuthenticationProviderTestBase
    {
        private AdalAuthenticationProvider authenticationProvider;
        private AdalServiceInfo adalServiceInfo;

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();

            this.adalServiceInfo = new AdalServiceInfo();
            this.adalServiceInfo.CopyFrom(this.serviceInfo);

            this.authenticationProvider = new AdalAuthenticationProvider(this.adalServiceInfo);
        }

        [TestMethod]
        public async Task AppendAuthenticationHeader()
        {
            var cachedAccountSession = new AccountSession
            {
                AccessToken = "token",
                ExpiresOnUtc = DateTimeOffset.UtcNow.AddHours(1),
            };

            this.authenticationProvider.CurrentAccountSession = cachedAccountSession;

            using (var httpRequestMessage = new HttpRequestMessage())
            {
                await this.authenticationProvider.AppendAuthHeaderAsync(httpRequestMessage);
                Assert.AreEqual(
                    string.Format("{0} {1}", Constants.Headers.Bearer, cachedAccountSession.AccessToken),
                    httpRequestMessage.Headers.Authorization.ToString(),
                    "Unexpected authorization header set.");
            }
        }

        [TestMethod]
        public async Task AppendAuthenticationHeaderDifferentType()
        {
            var cachedAccountSession = new AccountSession
            {
                AccessToken = "token",
                AccessTokenType = "test",
                ExpiresOnUtc = DateTimeOffset.UtcNow.AddHours(1),
            };

            this.authenticationProvider.CurrentAccountSession = cachedAccountSession;

            using (var httpRequestMessage = new HttpRequestMessage())
            {
                await this.authenticationProvider.AppendAuthHeaderAsync(httpRequestMessage);
                Assert.AreEqual(
                    string.Format("{0} {1}", cachedAccountSession.AccessTokenType, cachedAccountSession.AccessToken),
                    httpRequestMessage.Headers.Authorization.ToString(),
                    "Unexpected authorization header set.");
            }
        }

        [TestMethod]
        public async Task AuthenticateAsync_AuthenticateSilentlyWithClientCertificate()
        {
            this.adalServiceInfo.ServiceResource = serviceResourceId;
            this.adalServiceInfo.BaseUrl = "https://localhost";

            this.adalServiceInfo.ClientCertificate = new X509Certificate2(@"Certs\testwebapplication.pfx", "password");

            var mockAuthenticationResult = new MockAuthenticationResult();
            mockAuthenticationResult.SetupGet(result => result.AccessToken).Returns("token");
            mockAuthenticationResult.SetupGet(result => result.AccessTokenType).Returns("type");
            mockAuthenticationResult.SetupGet(result => result.ExpiresOn).Returns(DateTimeOffset.UtcNow);

            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId)))).Throws(new Exception());

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<ClientAssertionCertificate>(certificate =>
                    certificate.Certificate == this.adalServiceInfo.ClientCertificate &&
                    certificate.ClientId == this.adalServiceInfo.AppId),
                UserIdentifier.AnyUser)).Returns(Task.FromResult(mockAuthenticationResult.Object));

            await this.AuthenticateAsync_AuthenticateWithoutDiscoveryService(
                mockAuthenticationContextWrapper.Object,
                mockAuthenticationResult.Object);
        }

        [TestMethod]
        public async Task AuthenticateAsync_AuthenticateSilentlyWithClientCredential()
        {
            this.adalServiceInfo.ServiceResource = serviceResourceId;
            this.adalServiceInfo.BaseUrl = "https://localhost";

            this.adalServiceInfo.ClientSecret = "clientSecret";

            var mockAuthenticationResult = new MockAuthenticationResult();
            mockAuthenticationResult.SetupGet(result => result.AccessToken).Returns("token");
            mockAuthenticationResult.SetupGet(result => result.AccessTokenType).Returns("type");
            mockAuthenticationResult.SetupGet(result => result.ExpiresOn).Returns(DateTimeOffset.UtcNow);

            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId)))).Throws(new Exception());

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<ClientCredential>(credential => credential.ClientId.Equals(this.adalServiceInfo.AppId)),
                UserIdentifier.AnyUser)).Returns(Task.FromResult(mockAuthenticationResult.Object));

            await this.AuthenticateAsync_AuthenticateWithoutDiscoveryService(
                mockAuthenticationContextWrapper.Object,
                mockAuthenticationResult.Object);
        }

        [TestMethod]
        public async Task AuthenticateAsync_AuthenticateSilentlyWithDiscoveryService()
        {
            var mockAuthenticationResult = new MockAuthenticationResult();
            mockAuthenticationResult.SetupGet(result => result.AccessToken).Returns("token");
            mockAuthenticationResult.SetupGet(result => result.AccessTokenType).Returns("type");
            mockAuthenticationResult.SetupGet(result => result.ExpiresOn).Returns(DateTimeOffset.UtcNow);

            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();
            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId))))
                .Returns(Task.FromResult(mockAuthenticationResult.Object));

            await this.AuthenticateAsync_AuthenticateWithDiscoveryService(mockAuthenticationContextWrapper, mockAuthenticationResult.Object);
        }
        
        [TestMethod]
        public async Task AuthenticateAsync_AuthenticateSilentlyWithoutDiscoveryService()
        {
            var mockAuthenticationResult = new MockAuthenticationResult();
            mockAuthenticationResult.SetupGet(result => result.AccessToken).Returns("token");
            mockAuthenticationResult.SetupGet(result => result.AccessTokenType).Returns("type");
            mockAuthenticationResult.SetupGet(result => result.ExpiresOn).Returns(DateTimeOffset.UtcNow);

            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();
            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId))))
                .Returns(Task.FromResult(mockAuthenticationResult.Object));

            await this.AuthenticateAsync_AuthenticateWithoutDiscoveryService(
                mockAuthenticationContextWrapper.Object,
                mockAuthenticationResult.Object);
        }

        [TestMethod]
        public async Task AuthenticateAsync_AuthenticateWithClientCertificate()
        {
            this.adalServiceInfo.ServiceResource = serviceResourceId;
            this.adalServiceInfo.BaseUrl = "https://localhost";

            this.adalServiceInfo.ClientCertificate = new X509Certificate2(@"Certs\testwebapplication.pfx", "password");

            var mockAuthenticationResult = new MockAuthenticationResult();
            mockAuthenticationResult.SetupGet(result => result.AccessToken).Returns("token");
            mockAuthenticationResult.SetupGet(result => result.AccessTokenType).Returns("type");
            mockAuthenticationResult.SetupGet(result => result.ExpiresOn).Returns(DateTimeOffset.UtcNow);

            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId)))).Throws(new Exception());

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenByAuthorizationCodeAsync(
                It.Is<string>(code => code.Equals(Constants.Authentication.CodeKeyName)),
                It.Is<Uri>(returnUri => returnUri.ToString().Equals(this.adalServiceInfo.ReturnUrl)),
                It.Is<ClientAssertionCertificate>(certificate =>
                    certificate.Certificate == this.adalServiceInfo.ClientCertificate &&
                    certificate.ClientId == this.adalServiceInfo.AppId),
                It.Is<string>(resource => resource.Equals(serviceResourceId))))
                .Returns(Task.FromResult(mockAuthenticationResult.Object));

            var webAuthenticationUi = new MockWebAuthenticationUi(
                new Dictionary<string, string>
                {
                    { Constants.Authentication.CodeKeyName, Constants.Authentication.CodeKeyName }
                });

            this.adalServiceInfo.WebAuthenticationUi = webAuthenticationUi.Object;
            
            await this.AuthenticateAsync_AuthenticateWithoutDiscoveryService(
                mockAuthenticationContextWrapper.Object,
                mockAuthenticationResult.Object);
        }

        [TestMethod]
        public async Task AuthenticateAsync_AuthenticateWithClientCredential()
        {
            this.adalServiceInfo.ServiceResource = serviceResourceId;
            this.adalServiceInfo.BaseUrl = "https://localhost";

            this.adalServiceInfo.ClientSecret = "clientSecret";

            var mockAuthenticationResult = new MockAuthenticationResult();
            mockAuthenticationResult.SetupGet(result => result.AccessToken).Returns("token");
            mockAuthenticationResult.SetupGet(result => result.AccessTokenType).Returns("type");
            mockAuthenticationResult.SetupGet(result => result.ExpiresOn).Returns(DateTimeOffset.UtcNow);

            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId)))).Throws(new Exception());

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenByAuthorizationCodeAsync(
                It.Is<string>(code => code.Equals(Constants.Authentication.CodeKeyName)),
                It.Is<Uri>(returnUri => returnUri.ToString().Equals(this.adalServiceInfo.ReturnUrl)),
                It.Is<ClientCredential>(credential => credential.ClientId.Equals(this.adalServiceInfo.AppId)),
                It.Is<string>(resource => resource.Equals(serviceResourceId))))
                .Returns(Task.FromResult(mockAuthenticationResult.Object));

            var webAuthenticationUi = new MockWebAuthenticationUi(
                new Dictionary<string, string>
                {
                    { Constants.Authentication.CodeKeyName, Constants.Authentication.CodeKeyName }
                });

            this.adalServiceInfo.WebAuthenticationUi = webAuthenticationUi.Object;

            await this.AuthenticateAsync_AuthenticateWithoutDiscoveryService(
                mockAuthenticationContextWrapper.Object,
                mockAuthenticationResult.Object);
        }

        [TestMethod]
        public async Task AuthenticateAsync_AuthenticateWithDiscoveryService()
        {
            var mockAuthenticationResult = new MockAuthenticationResult();
            mockAuthenticationResult.SetupGet(result => result.AccessToken).Returns("token");
            mockAuthenticationResult.SetupGet(result => result.AccessTokenType).Returns("type");
            mockAuthenticationResult.SetupGet(result => result.ExpiresOn).Returns(DateTimeOffset.UtcNow);

            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId)))).Throws(new Exception());

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireToken(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId)),
                It.Is<Uri>(returnUri => returnUri.ToString().Equals(this.adalServiceInfo.ReturnUrl)),
                PromptBehavior.Auto,
                UserIdentifier.AnyUser)).Returns(mockAuthenticationResult.Object);

            await this.AuthenticateAsync_AuthenticateWithDiscoveryService(
                mockAuthenticationContextWrapper,
                mockAuthenticationResult.Object);
        }

        [TestMethod]
        public async Task AuthenticateAsync_AuthenticateWithoutDiscoveryService()
        {
            this.adalServiceInfo.ServiceResource = serviceResourceId;
            this.adalServiceInfo.BaseUrl = "https://localhost";

            var mockAuthenticationResult = new MockAuthenticationResult();
            mockAuthenticationResult.SetupGet(result => result.AccessToken).Returns("token");
            mockAuthenticationResult.SetupGet(result => result.AccessTokenType).Returns("type");
            mockAuthenticationResult.SetupGet(result => result.ExpiresOn).Returns(DateTimeOffset.UtcNow);

            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId)))).Throws(new Exception());

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireToken(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId)),
                It.Is<Uri>(returnUri => returnUri.ToString().Equals(this.adalServiceInfo.ReturnUrl)),
                PromptBehavior.Auto,
                UserIdentifier.AnyUser)).Returns(mockAuthenticationResult.Object);

            await this.AuthenticateAsync_AuthenticateWithoutDiscoveryService(
                mockAuthenticationContextWrapper.Object,
                mockAuthenticationResult.Object);
        }

        

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task AuthenticateAsync_AuthenticationError()
        {
            var innerException = new Exception();

            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();
            
            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId)))).Throws(new Exception());

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireToken(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId)),
                It.Is<Uri>(returnUri => returnUri.ToString().Equals(this.adalServiceInfo.ReturnUrl)),
                PromptBehavior.Auto,
                UserIdentifier.AnyUser)).Throws(innerException);
            
            try
            {
                await this.AuthenticateWithDiscoveryService(mockAuthenticationContextWrapper);
            }
            catch (OneDriveException exception)
            {
                Assert.IsNotNull(exception.Error, "Error not set in exception.");
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error code returned.");
                Assert.AreEqual("An error occurred during active directory authentication.",
                    exception.Error.Message,
                    "Unexpected error message returned.");
                Assert.AreEqual(innerException, exception.InnerException, "Unexpected inner exception.");

                throw;
            }
        }

        [TestMethod]
        public async Task AuthenticateAsync_CachedCurrentAccountSession()
        {
            var cachedAccountSession = new AccountSession
            {
                AccessToken = "token",
                ExpiresOnUtc = DateTimeOffset.UtcNow.AddMinutes(10),
            };

            this.authenticationProvider.CurrentAccountSession = cachedAccountSession;

            var accountSession = await this.authenticationProvider.AuthenticateAsync();

            Assert.IsNotNull(accountSession, "No account session returned.");
            Assert.AreEqual(cachedAccountSession.AccessToken, accountSession.AccessToken, "Unexpected access token returned.");
            Assert.AreEqual(cachedAccountSession.ExpiresOnUtc, accountSession.ExpiresOnUtc, "Unexpected expiration returned.");
        }

        [TestMethod]
        public async Task AuthenticateAsync_CachedCurrentAccountSessionExpiring()
        {
            var cachedAccountSession = new AccountSession
            {
                AccessToken = "expiredToken",
                ExpiresOnUtc = DateTimeOffset.UtcNow,
            };

            this.authenticationProvider.CurrentAccountSession = cachedAccountSession;
            
            var mockAuthenticationResult = new MockAuthenticationResult();
            mockAuthenticationResult.SetupGet(result => result.AccessToken).Returns("token");
            mockAuthenticationResult.SetupGet(result => result.AccessTokenType).Returns("type");
            mockAuthenticationResult.SetupGet(result => result.ExpiresOn).Returns(DateTimeOffset.UtcNow.AddHours(1));

            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();
            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId))))
                .Returns(Task.FromResult(mockAuthenticationResult.Object));

            await this.AuthenticateAsync_AuthenticateWithoutDiscoveryService(
                mockAuthenticationContextWrapper.Object,
                mockAuthenticationResult.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task AuthenticateAsync_DiscoveryServiceMyFilesCapabilityNotFound()
        {
            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();
            try
            {
                await this.AuthenticateWithDiscoveryService(
                    mockAuthenticationContextWrapper,
                    new DiscoveryServiceResponse
                    {
                        Value = new List<DiscoveryService>
                        {
                            new DiscoveryService
                            {
                                Capability = "Mail",
                                ServiceApiVersion = "v2.0"
                            }
                        }
                    });
            }
            catch (OneDriveException exception)
            {
                Assert.IsNotNull(exception.Error, "Error not set in exception.");
                Assert.AreEqual(OneDriveErrorCode.MyFilesCapabilityNotFound.ToString(), exception.Error.Code, "Unexpected error code returned.");
                Assert.AreEqual(
                    string.Format(
                        "{0} capability with version {1} not found for the current user.",
                        Constants.Authentication.MyFilesCapability,
                        this.adalServiceInfo.OneDriveServiceEndpointVersion),
                    exception.Error.Message,
                    "Unexpected error message returned.");

                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task AuthenticateAsync_DiscoveryServiceMyFilesVersionNotFound()
        {
            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();
            try
            {
                await this.AuthenticateWithDiscoveryService(
                    mockAuthenticationContextWrapper,
                    new DiscoveryServiceResponse
                    {
                        Value = new List<DiscoveryService>
                        {
                            new DiscoveryService
                            {
                                Capability = Constants.Authentication.MyFilesCapability,
                                ServiceApiVersion = "v1.0"
                            }
                        }
                    });
            }
            catch (OneDriveException exception)
            {
                Assert.IsNotNull(exception.Error, "Error not set in exception.");
                Assert.AreEqual(OneDriveErrorCode.MyFilesCapabilityNotFound.ToString(), exception.Error.Code, "Unexpected error code returned.");
                Assert.AreEqual(
                    string.Format(
                        "{0} capability with version {1} not found for the current user.",
                        Constants.Authentication.MyFilesCapability,
                        this.adalServiceInfo.OneDriveServiceEndpointVersion),
                    exception.Error.Message,
                    "Unexpected error message returned.");

                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task AuthenticateAsync_DiscoveryServiceResponseValueNull()
        {
            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();
            try
            {
                await this.AuthenticateWithDiscoveryService(
                    mockAuthenticationContextWrapper,
                    new DiscoveryServiceResponse());
            }
            catch (OneDriveException exception)
            {
                Assert.IsNotNull(exception.Error, "Error not set in exception.");
                Assert.AreEqual(OneDriveErrorCode.MyFilesCapabilityNotFound.ToString(), exception.Error.Code, "Unexpected error code returned.");
                Assert.AreEqual(
                    "MyFiles capability not found for the current user.",
                    exception.Error.Message,
                    "Unexpected error message returned.");

                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task AuthenticateAsync_NullAuthenticationResult()
        {
            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();
            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId)))).Throws(new Exception());

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireToken(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId)),
                It.Is<Uri>(returnUri => returnUri.ToString().Equals(this.adalServiceInfo.ReturnUrl)),
                PromptBehavior.Auto,
                UserIdentifier.AnyUser)).Returns((IAuthenticationResult)null);

            try
            {
                await this.AuthenticateWithDiscoveryService(mockAuthenticationContextWrapper);
            }
            catch (OneDriveException exception)
            {
                Assert.IsNotNull(exception.Error, "Error not set in exception.");
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error code returned.");
                Assert.AreEqual(
                    "An error occurred during active directory authentication.",
                    exception.Error.Message,
                    "Unexpected error message returned.");

                throw;
            }
        }

        [TestMethod]
        public void GetAuthenticationException_Cancelled()
        {
            var innerException = new Exception();
            var oneDriveException = this.authenticationProvider.GetAuthenticationException(true, innerException);

            Assert.IsNotNull(oneDriveException.Error, "Error not set in exception.");
            Assert.AreEqual(OneDriveErrorCode.AuthenticationCancelled.ToString(), oneDriveException.Error.Code, "Unexpected error code returned.");
            Assert.AreEqual("User cancelled authentication.", oneDriveException.Error.Message, "Unexpected error message returned.");
            Assert.AreEqual(innerException, oneDriveException.InnerException, "Unexpected inner exception.");
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public void ServiceInfo_IncorrectCredentialCacheType()
        {
            this.adalServiceInfo.CredentialCache = new MockCredentialCache().Object;

            try
            {
                this.authenticationProvider.ServiceInfo = this.adalServiceInfo;
            }
            catch (OneDriveException exception)
            {
                Assert.IsNotNull(exception.Error, "Error not set in exception.");
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error code returned.");
                Assert.AreEqual(
                    "Invalid credential cache type for authentication using ADAL.",
                    exception.Error.Message,
                    "Unexpected error message returned.");

                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public void ServiceInfo_NullAuthenticationServiceUrl()
        {
            try
            {
                this.authenticationProvider.ServiceInfo = new ServiceInfo();
            }
            catch (OneDriveException exception)
            {
                Assert.IsNotNull(exception.Error, "Error not set in exception.");
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error code returned.");
                Assert.AreEqual(
                    "Invalid service info for authentication.",
                    exception.Error.Message,
                    "Unexpected error message returned.");

                throw;
            }
        }

        [TestMethod]
        public void ServiceInfo_Set()
        {
            var newServiceInfo = new ServiceInfo { AuthenticationServiceUrl = "https://login.live.com/authenticate" };
            this.authenticationProvider.authenticationContextWrapper = null;
            this.authenticationProvider.ServiceInfo = newServiceInfo;

            Assert.AreEqual(newServiceInfo, this.authenticationProvider.ServiceInfo, "Service info not correctly initialized.");
            Assert.IsNotNull(this.authenticationProvider.authenticationContextWrapper, "Authentication context wrapper not correctly initialized.");
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public void ServiceInfo_SetNull()
        {
            try
            {
                this.authenticationProvider.ServiceInfo = null;
            }
            catch (OneDriveException exception)
            {
                Assert.IsNotNull(exception.Error, "Error not set in exception.");
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error code returned.");
                Assert.AreEqual(
                    "Invalid service info for authentication.",
                    exception.Error.Message,
                    "Unexpected error message returned.");

                throw;
            }
        }

        [TestMethod]
        public async Task SignOutAsync()
        {
            var accountSession = new AccountSession
            {
                AccessToken = "accessToken",
                CanSignOut = true,
                ClientId = "12345",
            };

            this.authenticationProvider.CurrentAccountSession = accountSession;

            await this.authenticationProvider.SignOutAsync();

            this.httpProvider.Verify(
                provider => provider.SendAsync(
                    It.Is<HttpRequestMessage>(message => message.RequestUri.ToString().Equals(this.adalServiceInfo.SignOutUrl))),
                Times.Once);

            Assert.IsNull(this.authenticationProvider.CurrentAccountSession, "Current account session not cleared.");

            this.credentialCache.Verify(cache => cache.OnDeleteFromCache(), Times.Once);
        }

        public async Task AuthenticateAsync_AuthenticateWithoutDiscoveryService(
    IAuthenticationContextWrapper authenticationContextWrapper,
    IAuthenticationResult authenticationResult)
        {
            this.adalServiceInfo.BaseUrl = "https://localhost";
            this.adalServiceInfo.ServiceResource = serviceResourceId;

            this.authenticationProvider.authenticationContextWrapper = authenticationContextWrapper;

            var accountSession = await this.authenticationProvider.AuthenticateAsync();

            Assert.AreEqual(accountSession, this.authenticationProvider.CurrentAccountSession, "Account session not cached correctly.");
            Assert.AreEqual(authenticationResult.AccessToken, accountSession.AccessToken, "Unexpected access token set.");
            Assert.AreEqual(authenticationResult.AccessTokenType, accountSession.AccessTokenType, "Unexpected access token type set.");
            Assert.AreEqual(AccountType.ActiveDirectory, accountSession.AccountType, "Unexpected account type set.");
            Assert.IsTrue(accountSession.CanSignOut, "CanSignOut set to false.");
            Assert.AreEqual(this.adalServiceInfo.AppId, accountSession.ClientId, "Unexpected client ID set.");
            Assert.AreEqual(authenticationResult.ExpiresOn, accountSession.ExpiresOnUtc, "Unexpected expiration set.");
            Assert.IsNull(accountSession.UserId, "Unexpected user ID set.");
        }

        public async Task AuthenticateAsync_AuthenticateWithDiscoveryService(
            MockAuthenticationContextWrapper mockAuthenticationContextWrapper,
            IAuthenticationResult authenticationResult)
        {
            var accountSession = await this.AuthenticateWithDiscoveryService(mockAuthenticationContextWrapper);

            Assert.AreEqual(accountSession, this.authenticationProvider.CurrentAccountSession, "Account session not cached correctly.");
            Assert.AreEqual(serviceEndpointUri, this.adalServiceInfo.BaseUrl, "Base URL not set.");
            Assert.AreEqual(serviceResourceId, this.adalServiceInfo.ServiceResource, "Service resource not set.");
            Assert.AreEqual(authenticationResult.AccessToken, accountSession.AccessToken, "Unexpected access token set.");
            Assert.AreEqual(authenticationResult.AccessTokenType, accountSession.AccessTokenType, "Unexpected access token type set.");
            Assert.AreEqual(AccountType.ActiveDirectory, accountSession.AccountType, "Unexpected account type set.");
            Assert.IsTrue(accountSession.CanSignOut, "CanSignOut set to false.");
            Assert.AreEqual(this.adalServiceInfo.AppId, accountSession.ClientId, "Unexpected client ID set.");
            Assert.AreEqual(authenticationResult.ExpiresOn, accountSession.ExpiresOnUtc, "Unexpected expiration set.");
            Assert.IsNull(accountSession.UserId, "Unexpected user ID set.");
        }

        public async Task<AccountSession> AuthenticateWithDiscoveryService(
            MockAuthenticationContextWrapper mockAuthenticationContextWrapper,
            DiscoveryServiceResponse discoveryServiceResponse = null)
        {
            var mockAuthenticationResult = new MockAuthenticationResult();
            mockAuthenticationResult.SetupGet(result => result.AccessToken).Returns("discoveryResource");

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(Constants.Authentication.ActiveDirectoryDiscoveryResource)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId)))).Throws(new Exception());

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireToken(
                It.Is<string>(resource => resource.Equals(Constants.Authentication.ActiveDirectoryDiscoveryResource)),
                It.Is<string>(clientId => clientId.Equals(this.adalServiceInfo.AppId)),
                It.Is<Uri>(returnUri => returnUri.ToString().Equals(this.adalServiceInfo.ReturnUrl)),
                PromptBehavior.Auto,
                UserIdentifier.AnyUser)).Returns(mockAuthenticationResult.Object);

            if (discoveryServiceResponse == null)
            {
                discoveryServiceResponse = new DiscoveryServiceResponse
                {
                    Value = new List<DiscoveryService>
                    {
                        new DiscoveryService
                        {
                            Capability = Constants.Authentication.MyFilesCapability,
                            ServiceApiVersion = this.adalServiceInfo.OneDriveServiceEndpointVersion,
                            ServiceEndpointUri = serviceEndpointUri,
                            ServiceResourceId = serviceResourceId,
                        }
                    }
                };
            }

            var requestBodyString = this.serializer.SerializeObject(discoveryServiceResponse);

            AccountSession accountSession;

            using (var stringContent = new StringContent(requestBodyString))
            {
                this.httpResponseMessage.Content = stringContent;
                this.authenticationProvider.authenticationContextWrapper = mockAuthenticationContextWrapper.Object;

                accountSession = await this.authenticationProvider.AuthenticateAsync();
            }

            return accountSession;
        }
    }
}
