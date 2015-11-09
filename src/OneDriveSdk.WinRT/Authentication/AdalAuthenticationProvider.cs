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
    using Windows.Security.Authentication.Web;

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

        /// <summary>
        /// Signs the current user out.
        /// </summary>
        public override async Task SignOutAsync()
        {
            if (this.CurrentAccountSession != null && this.CurrentAccountSession.CanSignOut)
            {
                if (this.ServiceInfo.WebAuthenticationUi != null)
                {
                    await this.ServiceInfo.WebAuthenticationUi.AuthenticateAsync(new Uri(this.ServiceInfo.SignOutUrl), null);
                }

                this.DeleteUserCredentialsFromCache(this.CurrentAccountSession);
                this.CurrentAccountSession = null;
            }
        }

        protected override async Task<IAuthenticationResult> AuthenticateResourceAsync(string resource)
        {
            IAuthenticationResult authenticationResult = null;

            try
            {
                authenticationResult = await this.authenticationContextWrapper.AcquireTokenSilentAsync(resource, this.serviceInfo.AppId);
            }
            catch (Exception)
            {
                // If an exception happens during silent authentication try interactive authentication.
            }

            if (authenticationResult != null && authenticationResult.Status == AuthenticationStatus.Success)
            {
                return authenticationResult;
            }

            authenticationResult = await this.authenticationContextWrapper.AcquireTokenAsync(
                resource,
                this.ServiceInfo.AppId,
                new Uri(this.ServiceInfo.ReturnUrl));

            if (authenticationResult == null || authenticationResult.Status != AuthenticationStatus.Success)
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = authenticationResult == null
                            ? "An error occured during active directory authentication."
                            : string.Format("An error occured during active directory authentication. Error: {0}. Description: {1}",
                                authenticationResult.Error,
                                authenticationResult.ErrorDescription),
                    });
            }

            return authenticationResult;
        }
    }
}
