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

    public class AdalAuthenticationProvider : AdalAuthenticationProviderBase
    {
        /// <summary>
        /// Constructs an <see cref="AdalAuthenticationProvider"/>.
        /// </summary>
        /// <param name="serviceInfo">The information for authenticating against the service.</param>
        /// <param name="currentAccountSession">The current account session, used for initializing an already logged in user.</param>
        public AdalAuthenticationProvider(ServiceInfo serviceInfo, AccountSession currentAccountSession = null)
            : base(serviceInfo, currentAccountSession)
        {
        }

        protected override async Task<AuthenticationResult> AuthenticateResourceAsync(string resource)
        {
            AuthenticationResult authenticationResult = null;

            try
            {
                if (!string.IsNullOrEmpty(this.serviceInfo.ClientSecret))
                {
                    var clientCredential = new ClientCredential(this.serviceInfo.AppId, this.serviceInfo.ClientSecret);
                    authenticationResult = await this.authenticationContext.AcquireTokenSilentAsync(resource, clientCredential, UserIdentifier.AnyUser);
                }
                else
                {
                    authenticationResult = await this.authenticationContext.AcquireTokenSilentAsync(resource, this.serviceInfo.AppId);
                }
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
                authenticationResult = this.authenticationContext.AcquireToken(
                    resource,
                    this.ServiceInfo.AppId,
                    new Uri(this.ServiceInfo.ReturnUrl),
                    PromptBehavior.Always);
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

        private OneDriveException GetAuthenticationException(bool isCancelled = false, Exception innerException = null)
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
                    Message = "An error occured during active directory authentication.",
                },
                innerException);
        }
    }
}
