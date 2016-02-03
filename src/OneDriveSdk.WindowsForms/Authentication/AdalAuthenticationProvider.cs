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

namespace Microsoft.OneDrive.Sdk
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using IdentityModel.Clients.ActiveDirectory;
    using WindowsForms;

    public class AdalAuthenticationProvider : AdalAuthenticationProviderBase
    {
        private IOAuthRequestStringBuilder oAuthRequestStringBuilder;

        /// <summary>
        /// Constructs an <see cref="AdalAuthenticationProvider"/>.
        /// </summary>
        /// <param name="serviceInfo">The information for authenticating against the service.</param>
        /// <param name="currentAccountSession">The current account session, used for initializing an already logged in user.</param>
        public AdalAuthenticationProvider(ServiceInfo serviceInfo, AccountSession currentAccountSession = null)
            : base(serviceInfo, currentAccountSession)
        {
        }

        internal IOAuthRequestStringBuilder OAuthRequestStringBuilder
        {
            get
            {
                if (this.oAuthRequestStringBuilder == null)
                {
                    this.oAuthRequestStringBuilder = new OAuthRequestStringBuilder(this.ServiceInfo);
                }

                return this.oAuthRequestStringBuilder;
            }

            set
            {
                this.oAuthRequestStringBuilder = value;
            }
        }

        protected override async Task<IAuthenticationResult> AuthenticateResourceAsync(string resource)
        {
            IAuthenticationResult authenticationResult = null;
            var clientCredential = this.GetClientCredentialForAuthentication();

            var returnUri = new Uri(this.ServiceInfo.ReturnUrl);

            var userIdentifier = this.GetUserIdentifierForAuthentication();

            try
            {
                authenticationResult = clientCredential == null
                    ? await this.authenticationContextWrapper.AcquireTokenSilentAsync(resource, this.serviceInfo.AppId)
                    : await this.authenticationContextWrapper.AcquireTokenSilentAsync(resource, clientCredential, userIdentifier);
            }
            catch (Exception)
            {
                // If an exception happens during silent authentication try interactive authentication.
            }

            if (authenticationResult != null)
            {
                return authenticationResult;
            }

            try
            {
                if (clientCredential != null)
                {
                    var webAuthenticationUi = this.serviceInfo.WebAuthenticationUi ?? new FormsWebAuthenticationUi();
                    var redirectUri = new Uri(this.ServiceInfo.ReturnUrl);

                    var requestUri = new Uri(this.OAuthRequestStringBuilder.GetAuthorizationCodeRequestUrl());

                    var authenticationResponseValues = await webAuthenticationUi.AuthenticateAsync(
                        requestUri,
                        redirectUri);

                    OAuthErrorHandler.ThrowIfError(authenticationResponseValues);

                    string code;
                    if (authenticationResponseValues != null && authenticationResponseValues.TryGetValue("code", out code))
                    {
                        authenticationResult = await this.authenticationContextWrapper.AcquireTokenByAuthorizationCodeAsync(
                            code,
                            redirectUri,
                            clientCredential,
                            resource);
                    }
                }
                else
                {
                    authenticationResult = this.authenticationContextWrapper.AcquireToken(
                        resource,
                        this.ServiceInfo.AppId,
                        returnUri,
                        PromptBehavior.Auto,
                        userIdentifier);
                }
            }
            catch (AdalException adalException)
            {
                throw this.GetAuthenticationException(string.Equals(adalException.ErrorCode, Constants.Authentication.AuthenticationCancelled), adalException);
            }
            catch (Exception exception)
            {
                throw this.GetAuthenticationException(false, exception);
            }

            if (authenticationResult == null)
            {
                throw this.GetAuthenticationException();
            }

            return authenticationResult;
        }
    }
}
