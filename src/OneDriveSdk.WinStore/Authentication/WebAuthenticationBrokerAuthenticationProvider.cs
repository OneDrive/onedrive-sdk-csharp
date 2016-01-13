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
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Security.Authentication.Web;

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
            var returnUrlForRequest = string.IsNullOrEmpty(this.ServiceInfo.ReturnUrl)
                ? WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString()
                : this.ServiceInfo.ReturnUrl;

            var requestUriStringBuilder = new StringBuilder();
            requestUriStringBuilder.Append(this.ServiceInfo.SignOutUrl);
            requestUriStringBuilder.AppendFormat("?{0}={1}", Constants.Authentication.RedirectUriKeyName, returnUrlForRequest);
            requestUriStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ClientIdKeyName, this.ServiceInfo.AppId);
            
            await this.ServiceInfo.WebAuthenticationUi.AuthenticateAsync(
                new Uri(requestUriStringBuilder.ToString()),
                string.IsNullOrEmpty(this.ServiceInfo.ReturnUrl)
                    ? null
                    : new Uri(this.ServiceInfo.ReturnUrl));

            this.DeleteUserCredentialsFromCache(this.CurrentAccountSession);
            this.CurrentAccountSession = null;
        }

        protected override Task<AccountSession> GetAuthenticationResultAsync()
        {
            return this.GetAccountSessionAsync();
        }

        internal async Task<AccountSession> GetAccountSessionAsync()
        {
            // Log the user in if we haven't already pulled their credentials from the cache.
            var code = await this.GetAuthorizationCodeAsync();

            if (!string.IsNullOrEmpty(code))
            {
                var authResult = await this.SendTokenRequestAsync(this.GetCodeRedemptionRequestBody(code));
                authResult.CanSignOut = true;

                return authResult;
            }

            return null;
        }

        internal string GetCodeRedemptionRequestBody(string code)
        {
            var returnUrlForRequest = string.IsNullOrEmpty(this.ServiceInfo.ReturnUrl)
                ? WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString()
                : this.ServiceInfo.ReturnUrl;

            var requestBodyString = string.Format(
                "{0}={1}&{2}={3}&{4}={5}&{6}={7}&{8}=authorization_code",
                Constants.Authentication.RedirectUriKeyName,
                returnUrlForRequest,
                Constants.Authentication.ClientIdKeyName,
                this.ServiceInfo.AppId,
                Constants.Authentication.ScopeKeyName,
                WebUtility.UrlEncode(string.Join(" ", this.ServiceInfo.Scopes)),
                Constants.Authentication.CodeKeyName,
                code,
                Constants.Authentication.GrantTypeKeyName);

            if (!string.IsNullOrEmpty(this.ServiceInfo.ClientSecret))
            {
                requestBodyString += "&client_secret=" + this.ServiceInfo.ClientSecret;
            }

            return requestBodyString;
        }

        private async Task<string> GetAuthorizationCodeAsync()
        {
            var returnUrlForRequest = string.IsNullOrEmpty(this.ServiceInfo.ReturnUrl)
                ? WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString()
                : this.ServiceInfo.ReturnUrl;

            var requestUriStringBuilder = new StringBuilder();
            requestUriStringBuilder.Append(this.ServiceInfo.AuthenticationServiceUrl);
            requestUriStringBuilder.AppendFormat("?{0}={1}", Constants.Authentication.RedirectUriKeyName, returnUrlForRequest);
            requestUriStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ClientIdKeyName, this.ServiceInfo.AppId);
            requestUriStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ScopeKeyName, string.Join("%20", this.ServiceInfo.Scopes));

            if (!string.IsNullOrEmpty(this.ServiceInfo.UserId))
            {
                requestUriStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.UserIdKeyName, this.ServiceInfo.UserId);
            }

            requestUriStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ResponseTypeKeyName, Constants.Authentication.CodeKeyName);

            var requestUri = new Uri(requestUriStringBuilder.ToString());

            var authenticationResponseValues = await this.ServiceInfo.WebAuthenticationUi.AuthenticateAsync(
                requestUri,
                string.IsNullOrEmpty(this.ServiceInfo.ReturnUrl)
                    ? null
                    : new Uri(this.ServiceInfo.ReturnUrl));

            OAuthErrorHandler.ThrowIfError(authenticationResponseValues);

            string code;
            if (authenticationResponseValues != null && authenticationResponseValues.TryGetValue("code", out code))
            {
                return code;
            }

            return null;
        }
    }
}
