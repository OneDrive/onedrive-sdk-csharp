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

    public class AdalAuthenticationByCodeAuthenticationProvider : AdalAuthenticationProviderBase
    {
        private string authenticationCode;

        /// <summary>
        /// Constructs an <see cref="AdalAuthenticationByCodeAuthenticationProvider"/> for use with web apps that perform their own initial login
        /// and already have a code for receiving an authentication token.
        /// </summary>
        /// <param name="serviceInfo">The information for authenticating against the service.</param>
        /// <param name="authenticationCode">The code for retrieving the authentication token.</param>
        /// <param name="currentAccountSession">The current account session, used for initializing an already logged in application.</param>
        public AdalAuthenticationByCodeAuthenticationProvider(
            ServiceInfo serviceInfo,
            string authenticationCode,
            AccountSession currentAccountSession = null)
            : base(serviceInfo, currentAccountSession)
        {
            if (string.IsNullOrEmpty(authenticationCode))
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Authentication code is required for authentication by code.",
                    });
            }

            this.authenticationCode = authenticationCode;
        }

        /// <summary>
        /// Signs the current user out.
        /// </summary>
        public override async Task SignOutAsync()
        {
            if (this.CurrentAccountSession != null && this.CurrentAccountSession.CanSignOut)
            {
                if (this.ServiceInfo.HttpProvider != null)
                {
                    using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, this.ServiceInfo.SignOutUrl))
                    {
                        await this.ServiceInfo.HttpProvider.SendAsync(httpRequestMessage);
                    }
                }

                this.DeleteUserCredentialsFromCache(this.CurrentAccountSession);
                this.CurrentAccountSession = null;
            }
        }

        protected override async Task<IAuthenticationResult> AuthenticateResourceAsync(string resource)
        {
            IAuthenticationResult authenticationResult = null;

            var clientCredential = this.GetClientCredentialForAuthentication();

            if (clientCredential == null)
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Client secret is required for authentication by code.",
                    });
            }

            var userIdentifier = this.GetUserIdentifierForAuthentication();

            var returnUri = new Uri(this.ServiceInfo.ReturnUrl);

            try
            {
                authenticationResult = await this.authenticationContextWrapper.AcquireTokenByAuthorizationCodeAsync(
                    this.authenticationCode,
                    returnUri,
                    clientCredential,
                    resource);
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

        internal OneDriveException GetAuthenticationException(bool isCancelled = false, Exception innerException = null)
        {
            if (isCancelled)
            {
                return new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationCancelled.ToString(),
                        Message = "User cancelled authentication.",
                    },
                    innerException);
            }

            return new OneDriveException(
                new Error
                {
                    Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                    Message = "An error occurred during active directory authentication.",
                },
                innerException);
        }
    }
}
