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

namespace Test.OneDriveSdk.WinRT
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
    using Mocks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    [TestClass]
    public class AdalAuthenticationProviderTests
    {
        private AdalAuthenticationProvider authenticationProvider;
        private MockAdalCredentialCache credentialCache;
        private MockHttpProvider httpProvider;
        private HttpResponseMessage httpResponseMessage;
        private ISerializer serializer;
        private ServiceInfo serviceInfo;
        private MockWebAuthenticationUi webAuthenticationUi;

        [TestInitialize]
        public void Setup()
        {
            this.credentialCache = new MockAdalCredentialCache();
            this.httpResponseMessage = new HttpResponseMessage();
            this.serializer = new Serializer();
            this.httpProvider = new MockHttpProvider(this.httpResponseMessage, this.serializer);
            this.webAuthenticationUi = new MockWebAuthenticationUi();

            this.serviceInfo = new ActiveDirectoryServiceInfo
            {
                AppId = "12345",
                AuthenticationServiceUrl = "https://login.live.com/authenticate",
                CredentialCache = this.credentialCache,
                HttpProvider = this.httpProvider,
                ReturnUrl = "https://login.live.com/return",
                SignOutUrl = "https://login.live.com/signout",
                TokenServiceUrl = "https://login.live.com/token",
                WebAuthenticationUi = this.webAuthenticationUi
            };

            this.authenticationProvider = new AdalAuthenticationProvider(this.serviceInfo);
        }

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
        public async Task AuthenticateAsync_AuthenticateSilentlyWithDiscoveryService()
        {
            const string serviceResourceId = "https://localhost/resource/";

            var authenticationResult = new MockAuthenticationResult
            {
                AccessToken = "token",
                AccessTokenType = "type",
                ExpiresOn = DateTimeOffset.UtcNow,
                Status = AuthenticationStatus.Success,
            };

            await this.AuthenticateAsync_AuthenticateWithDiscoveryService(
                authenticationResult,
                (string resource, string clientId, Uri redirectUri) =>
                {
                    switch (resource)
                    {
                        case (Constants.Authentication.ActiveDirectoryDiscoveryResource):
                            return new MockAuthenticationResult { AccessToken = "discoveryServiceToken" };
                        default:
                            return null;
                    }
                },
                (string resource, string clientId) =>
                {
                    switch (resource)
                    {
                        case (serviceResourceId):
                            return authenticationResult;
                        case (Constants.Authentication.ActiveDirectoryDiscoveryResource):
                            throw new Exception();
                        default:
                            return null;
                    }
                });
        }

        [TestMethod]
        public async Task AuthenticateAsync_AuthenticateWithDiscoveryService()
        {
            const string serviceResourceId = "https://localhost/resource/";

            var authenticationResult = new MockAuthenticationResult
            {
                AccessToken = "token",
                AccessTokenType = "type",
                ExpiresOn = DateTimeOffset.UtcNow,
                Status = AuthenticationStatus.Success,
            };

            await this.AuthenticateAsync_AuthenticateWithDiscoveryService(
                authenticationResult,
                (string resource, string clientId, Uri redirectUri) =>
                    {
                        switch (resource)
                        {
                            case (serviceResourceId):
                                return authenticationResult;
                            case (Constants.Authentication.ActiveDirectoryDiscoveryResource):
                                return new MockAuthenticationResult { AccessToken = "discoveryServiceToken" };
                            default:
                                return null;
                        }
                    },
                (string resource, string clientId) =>
                    {
                        throw new Exception();
                    });
        }

        [TestMethod]
        public async Task AuthenticateAsync_AuthenticateSilentlyWithoutDiscoveryService()
        {
            var silentAuthenticationResult = new MockAuthenticationResult
            {
                AccessToken = "token",
                AccessTokenType = "type",
                ExpiresOn = DateTimeOffset.UtcNow,
                Status = AuthenticationStatus.Success,
            };

            await this.AuthenticateAsync_AuthenticateWithoutDiscoveryService(
                silentAuthenticationResult,
                null,
                (string resource, string clientId) =>
                {
                    return silentAuthenticationResult;
                });
        }

        [TestMethod]
        public async Task AuthenticateAsync_AuthenticateWithoutDiscoveryService()
        {
            this.serviceInfo.ServiceResource = "https://resource/";
            this.serviceInfo.BaseUrl = "https://localhost";

            var authenticationResult = new MockAuthenticationResult
            {
                AccessToken = "token",
                AccessTokenType = "type",
                ExpiresOn = DateTimeOffset.UtcNow,
                Status = AuthenticationStatus.Success,
            };

            await this.AuthenticateAsync_AuthenticateWithoutDiscoveryService(
                authenticationResult,
                (string resource, string clientId, Uri redirectUri) =>
                {
                    return authenticationResult;
                },
                (string resource, string clientId) =>
                {
                    throw new Exception();
                });
        }

        [TestMethod]
        public async Task AuthenticateAsync_AuthenticationError()
        {
            var authenticationResult = new MockAuthenticationResult
            {
                Error = "error",
                ErrorDescription = "errorDescription",
                Status = AuthenticationStatus.ServiceError,
            };

            bool oneDriveExceptionThrown = false;

            try
            {
                await this.AuthenticateWithDiscoveryService(
                    (string resource, string clientId, Uri redirectUri) =>
                    {
                        return authenticationResult;
                    },
                    (string resource, string clientId) =>
                    {
                        throw new Exception();
                    });
            }
            catch (OneDriveException exception)
            {
                Assert.IsNotNull(exception.Error, "Error not set in exception.");
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error code returned.");
                Assert.AreEqual(
                    string.Format("An error occured during active directory authentication. Error: {0}. Description: {1}",
                            authenticationResult.Error,
                            authenticationResult.ErrorDescription),
                    exception.Error.Message,
                    "Unexpected error message returned.");

                oneDriveExceptionThrown = true;
            }

            Assert.IsTrue(oneDriveExceptionThrown, "OneDriveException not thrown.");
        }

        [TestMethod]
        public async Task AuthenticateAsync_DiscoveryServiceMyFilesCapabilityNotFound()
        {
            bool oneDriveExceptionThrown = false;
            try
            {
                await this.AuthenticateWithDiscoveryService(
                    (string resource, string clientId, Uri redirectUri) =>
                    {
                        switch (resource)
                        {
                            case (Constants.Authentication.ActiveDirectoryDiscoveryResource):
                                return new MockAuthenticationResult { AccessToken = "discoveryServiceToken" };
                            default:
                                return null;
                        }
                    },
                    (string resource, string clientId) =>
                    {
                        throw new Exception();
                    },
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

                oneDriveExceptionThrown = true;
            }

            Assert.IsTrue(oneDriveExceptionThrown, "OneDriveException not thrown.");
        }

        [TestMethod]
        public async Task AuthenticateAsync_DiscoveryServiceMyFilesVersionNotFound()
        {
            bool oneDriveExceptionThrown = false;
            try
            {
                await this.AuthenticateWithDiscoveryService(
                    (string resource, string clientId, Uri redirectUri) =>
                    {
                        switch (resource)
                        {
                            case (Constants.Authentication.ActiveDirectoryDiscoveryResource):
                                return new MockAuthenticationResult { AccessToken = "discoveryServiceToken" };
                            default:
                                return null;
                        }
                    },
                    (string resource, string clientId) =>
                    {
                        throw new Exception();
                    },
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

                oneDriveExceptionThrown = true;
            }

            Assert.IsTrue(oneDriveExceptionThrown, "OneDriveException not thrown.");
        }

        [TestMethod]
        public async Task AuthenticateAsync_DiscoveryServiceResponseValueNull()
        {
            bool oneDriveExceptionThrown = false;
            try
            {
                await this.AuthenticateWithDiscoveryService(
                    (string resource, string clientId, Uri redirectUri) =>
                    {
                        switch (resource)
                        {
                            case (Constants.Authentication.ActiveDirectoryDiscoveryResource):
                                return new MockAuthenticationResult { AccessToken = "discoveryServiceToken" };
                            default:
                                return null;
                        }
                    },
                    (string resource, string clientId) =>
                    {
                        throw new Exception();
                    },
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

                oneDriveExceptionThrown = true;
            }

            Assert.IsTrue(oneDriveExceptionThrown, "OneDriveException not thrown.");
        }

        [TestMethod]
        public async Task AuthenticateAsync_NullAuthenticationResult()
        {
            bool oneDriveExceptionThrown = false;
            try
            {
                await this.AuthenticateWithDiscoveryService(
                    (string resource, string clientId, Uri redirectUri) =>
                    {
                        return null;
                    },
                    (string resource, string clientId) =>
                    {
                        throw new Exception();
                    });
            }
            catch (OneDriveException exception)
            {
                Assert.IsNotNull(exception.Error, "Error not set in exception.");
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error code returned.");
                Assert.AreEqual(
                    "An error occured during active directory authentication.",
                    exception.Error.Message,
                    "Unexpected error message returned.");

                oneDriveExceptionThrown = true;
            }

            Assert.IsTrue(oneDriveExceptionThrown, "OneDriveException not thrown.");
        }
        
        [TestMethod]
        public void ServiceInfo_IncorrectCredentialCacheType()
        {
            this.serviceInfo.CredentialCache = new MockCredentialCache();

            Assert.ThrowsException<OneDriveException>(() =>
            {
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
            });
        }

        [TestMethod]
        public void ServiceInfo_NullAuthenticationServiceUrl()
        {
            Assert.ThrowsException<OneDriveException>(() =>
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
            });
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
        public void ServiceInfo_SetNull()
        {
            Assert.ThrowsException<OneDriveException>(() =>
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
            });
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
            this.webAuthenticationUi.OnAuthenticateAsync = this.OnAuthenticateAsync_SignOut;

            await this.authenticationProvider.SignOutAsync();

            Assert.IsNull(this.authenticationProvider.CurrentAccountSession, "Current account session not cleared.");
            Assert.IsTrue(this.credentialCache.DeleteFromCacheCalled, "DeleteFromCache not called.");
        }

        public async Task AuthenticateAsync_AuthenticateWithoutDiscoveryService(
            IAuthenticationResult authenticationResult,
            MockAuthenticationContextWrapper.AuthenticationResultCallback authenticationResultCallback,
            MockAuthenticationContextWrapper.AuthenticationResultSilentCallback authenticationResultSilentCallback)
        {
            this.serviceInfo.BaseUrl = "https://localhost";
            this.serviceInfo.ServiceResource = "https://resource/";

            this.authenticationProvider.authenticationContextWrapper = new MockAuthenticationContextWrapper
            {
                AcquireTokenAsyncCallback = authenticationResultCallback,
                AcquireTokenSilentAsyncCallback = authenticationResultSilentCallback,
            };

            var accountSession = await this.authenticationProvider.AuthenticateAsync();

            Assert.AreEqual(authenticationResult.AccessToken, accountSession.AccessToken, "Unexpected access token set.");
            Assert.AreEqual(authenticationResult.AccessTokenType, accountSession.AccessTokenType, "Unexpected access token type set.");
            Assert.AreEqual(AccountType.ActiveDirectory, accountSession.AccountType, "Unexpected account type set.");
            Assert.IsTrue(accountSession.CanSignOut, "CanSignOut set to false.");
            Assert.AreEqual(this.serviceInfo.AppId, accountSession.ClientId, "Unexpected client ID set.");
            Assert.AreEqual(authenticationResult.ExpiresOn, accountSession.ExpiresOnUtc, "Unexpected expiration set.");
            Assert.IsNull(accountSession.UserId, "Unexpected user ID set.");
        }

        public async Task AuthenticateAsync_AuthenticateWithDiscoveryService(
            IAuthenticationResult authenticationResult,
            MockAuthenticationContextWrapper.AuthenticationResultCallback authenticationResultCallback,
            MockAuthenticationContextWrapper.AuthenticationResultSilentCallback authenticationResultSilentCallback)
        {
            const string serviceEndpointUri = "https://localhost";
            const string serviceResourceId = "https://localhost/resource/";

            var discoveryServiceResponse = new DiscoveryServiceResponse
            {
                Value = new List<DiscoveryService>
                {
                    new DiscoveryService
                    {
                        Capability = Constants.Authentication.MyFilesCapability,
                        ServiceApiVersion = this.serviceInfo.OneDriveServiceEndpointVersion,
                        ServiceEndpointUri = serviceEndpointUri,
                        ServiceResourceId = serviceResourceId,
                    }
                }
            };

            var accountSession = await this.AuthenticateWithDiscoveryService(
                authenticationResultCallback,
                authenticationResultSilentCallback);

            Assert.AreEqual(serviceEndpointUri, this.serviceInfo.BaseUrl, "Base URL not set.");
            Assert.AreEqual(serviceResourceId, this.serviceInfo.ServiceResource, "Service resource not set.");
            Assert.AreEqual(authenticationResult.AccessToken, accountSession.AccessToken, "Unexpected access token set.");
            Assert.AreEqual(authenticationResult.AccessTokenType, accountSession.AccessTokenType, "Unexpected access token type set.");
            Assert.AreEqual(AccountType.ActiveDirectory, accountSession.AccountType, "Unexpected account type set.");
            Assert.IsTrue(accountSession.CanSignOut, "CanSignOut set to false.");
            Assert.AreEqual(this.serviceInfo.AppId, accountSession.ClientId, "Unexpected client ID set.");
            Assert.AreEqual(authenticationResult.ExpiresOn, accountSession.ExpiresOnUtc, "Unexpected expiration set.");
            Assert.IsNull(accountSession.UserId, "Unexpected user ID set.");
        }

        public async Task<AccountSession> AuthenticateWithDiscoveryService(
            MockAuthenticationContextWrapper.AuthenticationResultCallback authenticationResultCallback,
            MockAuthenticationContextWrapper.AuthenticationResultSilentCallback authenticationResultSilentCallback,
            DiscoveryServiceResponse discoveryServiceResponse = null)
        {
            const string serviceEndpointUri = "https://localhost";
            const string serviceResourceId = "https://localhost/resource/";

            if (discoveryServiceResponse == null)
            {
                discoveryServiceResponse = new DiscoveryServiceResponse
                {
                    Value = new List<DiscoveryService>
                    {
                        new DiscoveryService
                        {
                            Capability = Constants.Authentication.MyFilesCapability,
                            ServiceApiVersion = this.serviceInfo.OneDriveServiceEndpointVersion,
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
                this.authenticationProvider.authenticationContextWrapper = new MockAuthenticationContextWrapper
                {
                    AcquireTokenAsyncCallback = authenticationResultCallback,
                    AcquireTokenSilentAsyncCallback = authenticationResultSilentCallback,
                };

                accountSession = await this.authenticationProvider.AuthenticateAsync();
            }

            return accountSession;
        }

        private void OnAuthenticateAsync_SignOut(Uri requestUri, Uri callbackUri)
        {
            Assert.IsNull(callbackUri, "Unexpected callbackUri set.");

            Assert.IsTrue(requestUri.ToString().Equals(this.serviceInfo.SignOutUrl), "Unexpected request URI.");
            Assert.IsNull(callbackUri, "Unexpected callback URI.");
        }
    }
}
