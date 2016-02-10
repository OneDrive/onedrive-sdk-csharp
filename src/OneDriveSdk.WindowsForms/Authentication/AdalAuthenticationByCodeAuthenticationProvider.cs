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
    using System.Threading.Tasks;

    using IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// Authenticates an application by retrieving an authentication token using a provided authorization code.
    /// </summary>
    public class AdalAuthenticationByCodeAuthenticationProvider : AdalAuthenticationProviderBase
    {
        internal string authenticationCode;

        /// <summary>
        /// Constructs an <see cref="AdalAuthenticationByCodeAuthenticationProvider"/> for use with web apps that perform their own initial login
        /// and already have a code for receiving an authentication token.
        /// </summary>
        /// <param name="serviceInfo">The information for authenticating against the service.</param>
        /// <param name="authenticationCode">The code for retrieving the authentication token.</param>
        public AdalAuthenticationByCodeAuthenticationProvider(
            ServiceInfo serviceInfo,
            string authenticationCode)
            : base(serviceInfo, currentAccountSession: null)
        {
            if (string.IsNullOrEmpty(authenticationCode))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Authorization code is required for authentication by code.",
                    });
            }

            this.authenticationCode = authenticationCode;
        }

        /// <summary>
        /// Retrieves an authentication result for the specified resource.
        /// </summary>
        /// <param name="resource">The resource to authenticate.</param>
        /// <returns>The <see cref="IAuthenticationResult"/> returned for the resource.</returns>
        protected override async Task<IAuthenticationResult> AuthenticateResourceAsync(string resource)
        {
            IAuthenticationResult authenticationResult = null;

            try
            {
                var adalServiceInfo = this.ServiceInfo as AdalServiceInfo;

                // If we have a client certificate authenticate using it. Use client secret authentication if not.
                if (adalServiceInfo != null && adalServiceInfo.ClientCertificate != null)
                {
                    authenticationResult = await this.AuthenticateUsingCertificate(adalServiceInfo, resource);
                }
                else
                {
                    authenticationResult = await this.AuthenticateUsingClientSecret(resource);
                }
            }
            catch (AdalException adalException)
            {
                throw this.GetAuthenticationException(string.Equals(adalException.ErrorCode, Constants.Authentication.AuthenticationCancelled), adalException);
            }
            catch (OneDriveException)
            {
                // If authentication threw a OneDriveException assume we already handled it and let it bubble up.
                throw;
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

        private Task<IAuthenticationResult> AuthenticateUsingCertificate(AdalServiceInfo adalServiceInfo, string resource)
        {
            var returnUri = new Uri(this.ServiceInfo.ReturnUrl);

            var clientAssertionCertificate = new ClientAssertionCertificate(adalServiceInfo.AppId, adalServiceInfo.ClientCertificate);

            return this.authenticationContextWrapper.AcquireTokenByAuthorizationCodeAsync(
                this.authenticationCode,
                returnUri,
                clientAssertionCertificate,
                resource);
        }

        private Task<IAuthenticationResult> AuthenticateUsingClientSecret(string resource)
        {
            var clientCredential = this.GetClientCredentialForAuthentication();

            if (clientCredential == null)
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Client certificate or client secret is required for authentication by code.",
                    });
            }

            var userIdentifier = this.GetUserIdentifierForAuthentication();

            var returnUri = new Uri(this.ServiceInfo.ReturnUrl);

            return this.authenticationContextWrapper.AcquireTokenByAuthorizationCodeAsync(
                    this.authenticationCode,
                    returnUri,
                    clientCredential,
                    resource);
        }
    }
}
