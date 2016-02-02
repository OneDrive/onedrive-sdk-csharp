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
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Moq;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Test.OneDriveSdk.Mocks;

    [TestClass]
    public class AdalAuthenticationByCodeAuthenticationProviderTests : AdalAuthenticationProviderTestBase
    {
        [TestMethod]
        public async Task AppendAuthenticationHeader()
        {
            var cachedAccountSession = new AccountSession
            {
                AccessToken = "token",
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
        public async Task AuthenticateAsync_AuthenticateSilentlyWithClientCredential()
        {
            this.serviceInfo.ServiceResource = serviceResourceId;
            this.serviceInfo.BaseUrl = "https://localhost";

            this.serviceInfo.ClientSecret = "clientSecret";

            var mockAuthenticationResult = new MockAuthenticationResult();
            mockAuthenticationResult.SetupGet(result => result.AccessToken).Returns("token");
            mockAuthenticationResult.SetupGet(result => result.AccessTokenType).Returns("type");
            mockAuthenticationResult.SetupGet(result => result.ExpiresOn).Returns(DateTimeOffset.UtcNow);

            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.serviceInfo.AppId)))).Throws(new Exception());

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<ClientCredential>(credential => credential.ClientId.Equals(this.serviceInfo.AppId)),
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
                It.Is<string>(clientId => clientId.Equals(this.serviceInfo.AppId))))
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
                It.Is<string>(clientId => clientId.Equals(this.serviceInfo.AppId))))
                .Returns(Task.FromResult(mockAuthenticationResult.Object));

            await this.AuthenticateAsync_AuthenticateWithoutDiscoveryService(
                mockAuthenticationContextWrapper.Object,
                mockAuthenticationResult.Object);
        }

        [TestMethod]
        public async Task AuthenticateAsync_AuthenticateWithClientCredential()
        {
            this.serviceInfo.ServiceResource = serviceResourceId;
            this.serviceInfo.BaseUrl = "https://localhost";

            this.serviceInfo.ClientSecret = "clientSecret";

            var mockAuthenticationResult = new MockAuthenticationResult();
            mockAuthenticationResult.SetupGet(result => result.AccessToken).Returns("token");
            mockAuthenticationResult.SetupGet(result => result.AccessTokenType).Returns("type");
            mockAuthenticationResult.SetupGet(result => result.ExpiresOn).Returns(DateTimeOffset.UtcNow);

            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.serviceInfo.AppId)))).Throws(new Exception());

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenByAuthorizationCodeAsync(
                It.Is<string>(code => code.Equals(Constants.Authentication.CodeKeyName)),
                It.Is<Uri>(returnUri => returnUri.ToString().Equals(this.serviceInfo.ReturnUrl)),
                It.Is<ClientCredential>(credential => credential.ClientId.Equals(this.serviceInfo.AppId)),
                It.Is<string>(resource => resource.Equals(serviceResourceId))))
                .Returns(Task.FromResult(mockAuthenticationResult.Object));

            var webAuthenticationUi = new MockWebAuthenticationUi(
                new Dictionary<string, string>
                {
                    { Constants.Authentication.CodeKeyName, Constants.Authentication.CodeKeyName }
                });

            this.serviceInfo.WebAuthenticationUi = webAuthenticationUi.Object;
            
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
                It.Is<string>(clientId => clientId.Equals(this.serviceInfo.AppId)))).Throws(new Exception());

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireToken(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.serviceInfo.AppId)),
                It.Is<Uri>(returnUri => returnUri.ToString().Equals(this.serviceInfo.ReturnUrl)),
                PromptBehavior.Auto,
                UserIdentifier.AnyUser)).Returns(mockAuthenticationResult.Object);

            await this.AuthenticateAsync_AuthenticateWithDiscoveryService(
                mockAuthenticationContextWrapper,
                mockAuthenticationResult.Object);
        }

        [TestMethod]
        public async Task AuthenticateAsync_AuthenticateWithoutDiscoveryService()
        {
            this.serviceInfo.ServiceResource = serviceResourceId;
            this.serviceInfo.BaseUrl = "https://localhost";

            var mockAuthenticationResult = new MockAuthenticationResult();
            mockAuthenticationResult.SetupGet(result => result.AccessToken).Returns("token");
            mockAuthenticationResult.SetupGet(result => result.AccessTokenType).Returns("type");
            mockAuthenticationResult.SetupGet(result => result.ExpiresOn).Returns(DateTimeOffset.UtcNow);

            var mockAuthenticationContextWrapper = new MockAuthenticationContextWrapper();

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireTokenSilentAsync(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.serviceInfo.AppId)))).Throws(new Exception());

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireToken(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.serviceInfo.AppId)),
                It.Is<Uri>(returnUri => returnUri.ToString().Equals(this.serviceInfo.ReturnUrl)),
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
                It.Is<string>(clientId => clientId.Equals(this.serviceInfo.AppId)))).Throws(new Exception());

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireToken(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.serviceInfo.AppId)),
                It.Is<Uri>(returnUri => returnUri.ToString().Equals(this.serviceInfo.ReturnUrl)),
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
                It.Is<string>(clientId => clientId.Equals(this.serviceInfo.AppId))))
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
                        this.serviceInfo.OneDriveServiceEndpointVersion),
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
                        this.serviceInfo.OneDriveServiceEndpointVersion),
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
                It.Is<string>(clientId => clientId.Equals(this.serviceInfo.AppId)))).Throws(new Exception());

            mockAuthenticationContextWrapper.Setup(wrapper => wrapper.AcquireToken(
                It.Is<string>(resource => resource.Equals(serviceResourceId)),
                It.Is<string>(clientId => clientId.Equals(this.serviceInfo.AppId)),
                It.Is<Uri>(returnUri => returnUri.ToString().Equals(this.serviceInfo.ReturnUrl)),
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
            this.serviceInfo.CredentialCache = new MockCredentialCache().Object;

            try
            {
                this.authenticationProvider.ServiceInfo = this.serviceInfo;
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
                    It.Is<HttpRequestMessage>(message => message.RequestUri.ToString().Equals(this.serviceInfo.SignOutUrl))),
                Times.Once);

            Assert.IsNull(this.authenticationProvider.CurrentAccountSession, "Current account session not cleared.");

            this.credentialCache.Verify(cache => cache.OnDeleteFromCache(), Times.Once);
        }
    }
}
