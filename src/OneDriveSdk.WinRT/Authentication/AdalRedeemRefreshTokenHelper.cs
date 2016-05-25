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
    /// Helper class to redeem refresh tokens during ADAL authentication.
    /// </summary>
    public class AdalRedeemRefreshTokenHelper : IAdalRedeemRefreshTokenHelper
    {
        private IAuthenticationContextWrapper authenticationContextWrapper;

        private ServiceInfo serviceInfo;

        /// <summary>
        /// Instantiates a new instance of <see cref="AdalRedeemRefreshTokenHelper"/>.
        /// </summary>
        /// <param name="serviceInfo">The information for authenticating against the service.</param>
        /// <param name="authenticationContextWrapper"></param>
        public AdalRedeemRefreshTokenHelper(ServiceInfo serviceInfo, IAuthenticationContextWrapper authenticationContextWrapper)
        {
            this.authenticationContextWrapper = authenticationContextWrapper;
            this.serviceInfo = serviceInfo;
        }

        /// <summary>
        /// Redeems the refresh token for the provided service info to retrieve an authentication result.
        /// </summary>
        /// <param name="refreshToken">The code for retrieving the authentication token.</param>
        /// <returns>The <see cref="IAuthenticationResult"/> returned for the resource.</returns>
        public async Task<IAuthenticationResult> RedeemRefreshToken(string refreshToken)
        {
            IAuthenticationResult authenticationResult = null;

            try
            {
                authenticationResult = await this.authenticationContextWrapper.AcquireTokenByRefreshTokenAsync(
                    refreshToken,
                    this.serviceInfo.AppId,
                    this.serviceInfo.ServiceResource);
            }
            catch (Exception exception)
            {
                AuthenticationExceptionHelper.HandleAuthenticationException(exception);
            }

            if (authenticationResult.Status != AuthenticationStatus.Success)
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = authenticationResult == null
                            ? "An error occurred during Azure Active Directory authentication."
                            : string.Format("An error occurred during Azure Active Directory authentication. Error: {0}. Description: {1}",
                                authenticationResult.Error,
                                authenticationResult.ErrorDescription),
                    });
            }

            return authenticationResult;
        }
    }
}
