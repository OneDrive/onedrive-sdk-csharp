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
    using System.Net;
    using System.Text;

    public class OAuthRequestStringBuilder : IOAuthRequestStringBuilder
    {
        private ServiceInfo serviceInfo;

        public OAuthRequestStringBuilder(ServiceInfo serviceInfo)
        {
            this.serviceInfo = serviceInfo;
        }

        /// <summary>
        /// Gets the request URL for OAuth authentication using the code flow.
        /// </summary>
        /// <param name="returnUrl">The return URL for the request. Defaults to the service info value.</param>
        /// <returns>The OAuth request URL.</returns>
        public string GetAuthorizationCodeRequestUrl(string returnUrl = null)
        {
            returnUrl = returnUrl ?? this.serviceInfo.ReturnUrl;

            var requestUriStringBuilder = new StringBuilder();
            requestUriStringBuilder.Append(this.serviceInfo.AuthenticationServiceUrl);
            requestUriStringBuilder.AppendFormat("?{0}={1}", Constants.Authentication.RedirectUriKeyName, returnUrl);
            requestUriStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ClientIdKeyName, this.serviceInfo.AppId);

            if (this.serviceInfo.Scopes != null)
            {
                requestUriStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ScopeKeyName, WebUtility.UrlEncode(string.Join(" ", this.serviceInfo.Scopes)));
            }

            if (!string.IsNullOrEmpty(this.serviceInfo.UserId))
            {
                requestUriStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.UserIdKeyName, this.serviceInfo.UserId);
            }

            requestUriStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ResponseTypeKeyName, Constants.Authentication.CodeKeyName);

            return requestUriStringBuilder.ToString();
        }

        /// <summary>
        /// Gets the request body for redeeming an authorization code for an access token.
        /// </summary>
        /// <param name="code">The authorization code to redeem.</param>
        /// <param name="returnUrl">The return URL for the request. Defaults to the service info value.</param>
        /// <returns>The request body for the code redemption call.</returns>
        public string GetCodeRedemptionRequestBody(string code, string returnUrl = null)
        {
            returnUrl = returnUrl ?? this.serviceInfo.ReturnUrl;

            var requestBodyStringBuilder = new StringBuilder();
            requestBodyStringBuilder.AppendFormat("{0}={1}", Constants.Authentication.RedirectUriKeyName, returnUrl);
            requestBodyStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ClientIdKeyName, this.serviceInfo.AppId);

            if (this.serviceInfo.Scopes != null)
            {
                requestBodyStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ScopeKeyName, WebUtility.UrlEncode(string.Join(" ", this.serviceInfo.Scopes)));
            }

            requestBodyStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.CodeKeyName, code);
            requestBodyStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.GrantTypeKeyName, Constants.Authentication.AuthorizationCodeGrantType);

            if (!string.IsNullOrEmpty(this.serviceInfo.ClientSecret))
            {
                requestBodyStringBuilder.AppendFormat("&client_secret={0}", this.serviceInfo.ClientSecret);
            }

            return requestBodyStringBuilder.ToString();
        }

        /// <summary>
        /// Gets the request body for redeeming a refresh token for an access token.
        /// </summary>
        /// <param name="refreshToken">The refresh token to redeem.</param>
        /// <returns>The request body for the redemption call.</returns>
        public string GetRefreshTokenRequestBody(string refreshToken)
        {
            var requestBodyStringBuilder = new StringBuilder();
            requestBodyStringBuilder.AppendFormat("{0}={1}", Constants.Authentication.RedirectUriKeyName, this.serviceInfo.ReturnUrl);
            requestBodyStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ClientIdKeyName, this.serviceInfo.AppId);

            if (this.serviceInfo.Scopes != null)
            {
                requestBodyStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ScopeKeyName, WebUtility.UrlEncode(string.Join(" ", this.serviceInfo.Scopes)));
            }

            requestBodyStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.RefreshTokenKeyName, refreshToken);
            requestBodyStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.GrantTypeKeyName, Constants.Authentication.RefreshTokenKeyName);

            if (!string.IsNullOrEmpty(this.serviceInfo.ClientSecret))
            {
                requestBodyStringBuilder.AppendFormat("&{0}={1}", Constants.Authentication.ClientSecretKeyName, this.serviceInfo.ClientSecret);
            }

            return requestBodyStringBuilder.ToString();
        }
    }
}
