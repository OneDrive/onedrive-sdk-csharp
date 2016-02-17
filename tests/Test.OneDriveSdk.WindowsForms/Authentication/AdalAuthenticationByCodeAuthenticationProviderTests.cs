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
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Moq;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;


    [TestClass]
    public class AdalAuthenticationByCodeAuthenticationProviderTests : AdalAuthenticationProviderTestBase
    {
        private const string authenticationCode = "code";

        private MockAuthenticationContextWrapper authenticationContextWrapper;
        private TestAdalAuthenticationByCodeAuthenticationProvider authenticationProvider;
        private AdalServiceInfo adalServiceInfo;

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();

            this.adalServiceInfo = new AdalServiceInfo();
            this.adalServiceInfo.CopyFrom(this.serviceInfo);

            this.authenticationProvider = new TestAdalAuthenticationByCodeAuthenticationProvider(this.adalServiceInfo, authenticationCode);

            this.authenticationContextWrapper = new MockAuthenticationContextWrapper();
            this.authenticationProvider.authenticationContextWrapper = this.authenticationContextWrapper.Object;
        }

        [TestMethod]
        public async Task AuthenticateResourceAsync_ClientCertificate()
        {
            var resource = "https://resource.sharepoint.com/";
            var expectedAuthenticationResult = new MockAuthenticationResult();

            var clientCertificate = new X509Certificate2(@"Certs\testwebapplication.pfx", "password");
            this.adalServiceInfo.ClientCertificate = clientCertificate;

            this.authenticationContextWrapper
                .Setup(wrapper => wrapper.AcquireTokenByAuthorizationCodeAsync(
                    It.Is<string>(code => authenticationCode.Equals(code)),
                    It.Is<Uri>(returnUri => this.adalServiceInfo.ReturnUrl.Equals(returnUri.ToString())),
                    It.Is<ClientAssertionCertificate>(certificate =>
                        certificate.Certificate == clientCertificate
                        && this.adalServiceInfo.AppId.Equals(certificate.ClientId)),
                    It.Is<string>(resourceValue => resource.Equals(resourceValue))))
                .Returns(Task.FromResult(expectedAuthenticationResult.Object));

            var authenticationResult = await this.authenticationProvider.AuthenticateResourceAsyncWrapper(resource);

            Assert.AreEqual(expectedAuthenticationResult.Object, authenticationResult, "Unexpected authentication result returned.");
        }

        [TestMethod]
        public async Task AuthenticateResourceAsync_ClientSecret()
        {
            this.authenticationProvider.ServiceInfo = this.serviceInfo;
            this.authenticationProvider.authenticationContextWrapper = this.authenticationContextWrapper.Object;
            
            this.serviceInfo.ClientSecret = "clientSecret";

            var resource = "https://resource.sharepoint.com/";
            var expectedAuthenticationResult = new MockAuthenticationResult();

            this.authenticationContextWrapper
                .Setup(wrapper => wrapper.AcquireTokenByAuthorizationCodeAsync(
                    It.Is<string>(code => authenticationCode.Equals(code)),
                    It.Is<Uri>(returnUri => this.serviceInfo.ReturnUrl.Equals(returnUri.ToString())),
                    It.Is<ClientCredential>(credential => this.serviceInfo.AppId.Equals(credential.ClientId)),
                    It.Is<string>(resourceValue => resource.Equals(resourceValue))))
                .Returns(Task.FromResult(expectedAuthenticationResult.Object));

            var authenticationResult = await this.authenticationProvider.AuthenticateResourceAsyncWrapper(resource);

            Assert.AreEqual(expectedAuthenticationResult.Object, authenticationResult, "Unexpected authentication result returned.");
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public void AuthenticateResourceAsync_NoAuthenticationCode()
        {
            try
            {
                var testAuthenticationProvider = new AdalAuthenticationByCodeAuthenticationProvider(this.adalServiceInfo, null);
            }
            catch (OneDriveException exception)
            {
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error thrown.");
                Assert.AreEqual("Authorization code is required for authentication by code.", exception.Error.Message, "Unexpected error thrown.");
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task AuthenticateResourceAsync_NoClientCertificateOrSecret()
        {
            try
            {
                var authenticationResult = await this.authenticationProvider.AuthenticateResourceAsyncWrapper("resource");
            }
            catch (OneDriveException exception)
            {
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error thrown.");
                Assert.AreEqual("Client certificate or client secret is required for authentication by code.", exception.Error.Message, "Unexpected error thrown.");
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task AuthenticateResourceAsync_HandleAdalException()
        {
            this.adalServiceInfo.ClientSecret = "clientSecret";

            var resource = "https://resource.sharepoint.com/";
            var expectedAuthenticationResult = new MockAuthenticationResult();

            var adalException = new AdalException("code");

            this.authenticationContextWrapper
                .Setup(wrapper => wrapper.AcquireTokenByAuthorizationCodeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Uri>(),
                    It.IsAny<ClientCredential>(),
                    It.IsAny<string>()))
                .Throws(adalException);

            try
            {
                var authenticationResult = await this.authenticationProvider.AuthenticateResourceAsyncWrapper(resource);
            }
            catch (OneDriveException exception)
            {
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error thrown.");
                Assert.AreEqual("An error occurred during active directory authentication.", exception.Error.Message, "Unexpected error thrown.");
                Assert.AreEqual(adalException, exception.InnerException, "Unexpected inner exception.");
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task AuthenticateResourceAsync_HandleException()
        {
            this.adalServiceInfo.ClientSecret = "clientSecret";

            var resource = "https://resource.sharepoint.com/";
            var expectedAuthenticationResult = new MockAuthenticationResult();

            var innerException = new Exception();

            this.authenticationContextWrapper
                .Setup(wrapper => wrapper.AcquireTokenByAuthorizationCodeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Uri>(),
                    It.IsAny<ClientCredential>(),
                    It.IsAny<string>()))
                .Throws(innerException);

            try
            {
                var authenticationResult = await this.authenticationProvider.AuthenticateResourceAsyncWrapper(resource);
            }
            catch (OneDriveException exception)
            {
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error thrown.");
                Assert.AreEqual("An error occurred during active directory authentication.", exception.Error.Message, "Unexpected error thrown.");
                Assert.AreEqual(innerException, exception.InnerException, "Unexpected inner exception.");
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task AuthenticateResourceAsync_NullAuthenticationResult()
        {
            this.adalServiceInfo.ClientSecret = "clientSecret";

            var resource = "https://resource.sharepoint.com/";
            var expectedAuthenticationResult = new MockAuthenticationResult();

            var innerException = new Exception();

            this.authenticationContextWrapper
                .Setup(wrapper => wrapper.AcquireTokenByAuthorizationCodeAsync(
                    It.IsAny<string>(),
                    It.IsAny<Uri>(),
                    It.IsAny<ClientCredential>(),
                    It.IsAny<string>()))
                .Returns(Task.FromResult<IAuthenticationResult>(null));

            try
            {
                var authenticationResult = await this.authenticationProvider.AuthenticateResourceAsyncWrapper(resource);
            }
            catch (OneDriveException exception)
            {
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error thrown.");
                Assert.AreEqual("An error occurred during active directory authentication.", exception.Error.Message, "Unexpected error thrown.");
                Assert.IsNull(exception.InnerException, "Unexpected inner exception.");
                throw;
            }
        }
    }
}
