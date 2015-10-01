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

namespace Microsoft.OneDrive.Sdk.WinStore
{
    using System;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    public class WebAuthenticationBrokerAuthenticationProvider : AuthenticationProvider
    {
        public WebAuthenticationBrokerAuthenticationProvider(ServiceInfo serviceInfo)
            : base(serviceInfo)
        {
        }

        /// <summary>
        /// Signs the current user out.
        /// </summary>
        public override async Task SignOutAsync()
        {
            var requestUriStringBuilder = new StringBuilder();
            requestUriStringBuilder.Append(this.ServiceInfo.SignOutUrl);
            requestUriStringBuilder.AppendFormat("{0}={1}", Constants.Authentication.RedirectUriKeyName, this.ServiceInfo.ReturnUrl);
            requestUriStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ClientIdKeyName, this.ServiceInfo.AppId);

            // Don't pass the callbackUri to AuthenticateAsync so we invoke the SSO authentication flow.
            await this.ServiceInfo.WebAuthenticationUi.AuthenticateAsync(
                new Uri(requestUriStringBuilder.ToString()),
                /* callbackUri */ null);

            this.DeleteUserCredentialsFromCache(this.CurrentAccountSession);
            this.CurrentAccountSession = null;
        }

        protected override Task<AccountSession> GetAuthenticationResultAsync()
        {
            return this.GetAccountSessionAsync();
        }

        internal async Task<AccountSession> GetAccountSessionAsync()
        {
            var requestUriStringBuilder = new StringBuilder();
            requestUriStringBuilder.Append(this.ServiceInfo.AuthenticationServiceUrl);
            requestUriStringBuilder.AppendFormat("{0}={1}", Constants.Authentication.RedirectUriKeyName, this.ServiceInfo.ReturnUrl);
            requestUriStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ClientIdKeyName, this.ServiceInfo.AppId);
            requestUriStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ScopeKeyName, string.Join("%20", this.ServiceInfo.Scopes));
            requestUriStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ResponseTypeKeyName, Constants.Authentication.TokenResponseTypeValueName);

            var requestUri = new Uri(requestUriStringBuilder.ToString());

            // Don't pass the callbackUri to AuthenticateAsync so we invoke the SSO authentication flow.
            var authenticationResponseValues = await this.ServiceInfo.WebAuthenticationUi.AuthenticateAsync(requestUri, /* callbackUri */ null);
            OAuthErrorHandler.ThrowIfError(authenticationResponseValues);

            return new AccountSession(authenticationResponseValues, this.ServiceInfo.AppId, AccountType.MicrosoftAccount) { CanSignOut = true };
        }
    }
}
