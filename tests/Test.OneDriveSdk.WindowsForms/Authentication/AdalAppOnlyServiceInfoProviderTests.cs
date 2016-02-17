﻿// ------------------------------------------------------------------------------
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
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using OneDriveSdk.Mocks;

    [TestClass]
    public class AdalAppOnlyServiceInfoProviderTests
    {
        private BusinessAppConfig appConfig;
        private MockAdalCredentialCache credentialCache;
        private MockHttpProvider httpProvider;
        private AdalAppOnlyServiceInfoProvider serviceInfoProvider;

        [TestInitialize]
        public void Setup()
        {
            this.appConfig = new BusinessAppConfig
            {
                ActiveDirectoryAppId = "12345",
                ActiveDirectoryReturnUrl = "https://localhost/return",
                ActiveDirectoryServiceResource = "https://resource/",
            };
            
            this.credentialCache = new MockAdalCredentialCache();
            this.httpProvider = new MockHttpProvider(null);
            this.serviceInfoProvider = new AdalAppOnlyServiceInfoProvider { UserSignInName = "12345" };
        }

        [TestMethod]
        public async Task GetServiceInfo()
        {
            var clientCertificate = new X509Certificate2(@"Certs\testwebapplication.pfx", "password");
            this.appConfig.ActiveDirectoryClientCertificate = clientCertificate;

            var serviceInfo = await this.serviceInfoProvider.GetServiceInfo(
                this.appConfig,
                this.credentialCache.Object,
                this.httpProvider.Object,
                ClientType.Business) as AdalServiceInfo;

            Assert.IsNotNull(serviceInfo, "Unexpected service info type.");

            var authenticationProvider = serviceInfo.AuthenticationProvider as AdalAppOnlyAuthenticationProvider;
            Assert.IsNotNull(authenticationProvider, "Unexpected authentication provider type.");

            Assert.AreEqual(serviceInfo, authenticationProvider.ServiceInfo, "Unexpected service info set on authentication provider.");

            Assert.AreEqual(this.appConfig.ActiveDirectoryAppId, serviceInfo.AppId, "Unexpected app ID set.");
            Assert.AreEqual(this.credentialCache.Object, serviceInfo.CredentialCache, "Unexpected credential cache set.");
            Assert.AreEqual(this.httpProvider.Object, serviceInfo.HttpProvider, "Unexpected HTTP provider set.");
            Assert.AreEqual(this.appConfig.ActiveDirectoryClientSecret, serviceInfo.ClientSecret, "Unexpected client secret set.");
            Assert.AreEqual(this.appConfig.ActiveDirectoryReturnUrl, serviceInfo.ReturnUrl, "Unexpected return URL set.");
            Assert.AreEqual(clientCertificate, serviceInfo.ClientCertificate, "Unexpected certificate set.");
            Assert.AreEqual(this.serviceInfoProvider.UserSignInName, serviceInfo.UserId, "Unexpected user ID set.");
            Assert.AreEqual(
                string.Format(
                    Constants.Authentication.OneDriveBusinessBaseUrlFormatString,
                    this.appConfig.ActiveDirectoryServiceResource.TrimEnd('/'),
                    serviceInfo.OneDriveServiceEndpointVersion),
                serviceInfo.BaseUrl,
                "Unexpected base URL set.");
            Assert.IsNull(serviceInfo.WebAuthenticationUi, "Unexpected web UI set.");
        }

        [TestMethod]
        public async Task GetServiceInfo_AuthenticationProviderAlreadySet()
        {
            var authenticationProvider = new MockAuthenticationProvider();
            this.serviceInfoProvider = new AdalAppOnlyServiceInfoProvider(authenticationProvider.Object);
            var serviceInfo = await this.serviceInfoProvider.GetServiceInfo(
                this.appConfig,
                this.credentialCache.Object,
                this.httpProvider.Object,
                ClientType.Business);

            Assert.IsNotInstanceOfType(serviceInfo.AuthenticationProvider, typeof(AdalAppOnlyAuthenticationProvider), "Unexpected authentication provider type.");
            Assert.AreEqual(authenticationProvider.Object, serviceInfo.AuthenticationProvider, "Unexpected authentication provider set.");
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task GetServiceInfo_InvalidAppConfigType()
        {
            try
            {
                var serviceInfo = await this.serviceInfoProvider.GetServiceInfo(
                    new AppConfig(),
                    /* credentialCache */ null,
                    /* httpProvider */ null,
                    ClientType.Business);
            }
            catch (OneDriveException exception)
            {
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error thrown.");
                Assert.AreEqual(
                    "AdalAppOnlyServiceInfoProvider requires an AdalAppConfig.",
                    exception.Error.Message,
                    "Unexpected error thrown.");
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task GetServiceInfo_InvalidClientType()
        {
            try
            {
                var serviceInfo = await this.serviceInfoProvider.GetServiceInfo(
                    this.appConfig,
                    /* credentialCache */ null,
                    /* httpProvider */ null,
                    ClientType.Consumer);
            }
            catch (OneDriveException exception)
            {
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error thrown.");
                Assert.AreEqual(
                    "AdalAppOnlyServiceInfoProvider only supports Active Directory authentication.",
                    exception.Error.Message,
                    "Unexpected error thrown.");
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(OneDriveException))]
        public async Task GetServiceInfo_MissingServiceResourceAndBaseUrl()
        {
            try
            {
                this.appConfig.ActiveDirectoryServiceResource = null;

                var serviceInfo = await this.serviceInfoProvider.GetServiceInfo(
                    this.appConfig,
                    /* credentialCache */ null,
                    /* httpProvider */ null,
                    ClientType.Business);
            }
            catch (OneDriveException exception)
            {
                Assert.AreEqual(OneDriveErrorCode.AuthenticationFailure.ToString(), exception.Error.Code, "Unexpected error thrown.");
                Assert.AreEqual(
                    "Service resource ID is required for app-only authentication when service endpoint URL is not initialized.",
                    exception.Error.Message,
                    "Unexpected error thrown.");
                throw;
            }
        }
    }
}
