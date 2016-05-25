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
    /// Authenticates an application using app-only authentication tokens.
    /// </summary>
    public class AdalAppOnlyAuthenticationProvider : AdalAuthenticationProviderBase
    {
        /// <summary>
        /// Constructs an <see cref="AdalAppOnlyAuthenticationProvider"/>.
        /// </summary>
        /// <param name="serviceInfo">The information for authenticating against the service.</param>
        public AdalAppOnlyAuthenticationProvider(AdalServiceInfo serviceInfo)
            : base(serviceInfo, currentAccountSession: null)
        {
            this.allowDiscoveryService = false;
        }

        /// <summary>
        /// Retrieves an authentication result for the specified resource.
        /// </summary>
        /// <param name="resource">The resource to authenticate.</param>
        /// <returns>The <see cref="IAuthenticationResult"/> returned for the resource.</returns>
        protected override async Task<IAuthenticationResult> AuthenticateResourceAsync(string resource)
        {
            IAuthenticationResult authenticationResult = null;

            var adalServiceInfo = this.ServiceInfo as AdalServiceInfo;

            if (adalServiceInfo == null)
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "AdalAppOnlyServiceInfoProvider requires an AdalServiceInfo."
                    });
            }

            if (adalServiceInfo.ClientCertificate == null)
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "Client certificate is required for app-only authentication."
                    });
            }

            var clientAssertionCertificate = new ClientAssertionCertificate(adalServiceInfo.AppId, adalServiceInfo.ClientCertificate);

            try
            {
                authenticationResult = await this.authenticationContextWrapper.AcquireTokenAsync(resource, clientAssertionCertificate);
            }
            catch (Exception exception)
            {
                AuthenticationExceptionHelper.HandleAuthenticationException(exception);
            }

            if (authenticationResult == null)
            {
                AuthenticationExceptionHelper.HandleAuthenticationException(null);
            }

            return authenticationResult;
        }
    }
}
