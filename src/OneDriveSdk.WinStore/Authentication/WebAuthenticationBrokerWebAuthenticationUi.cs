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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Windows.Security.Authentication.Web;

    public class WebAuthenticationBrokerWebAuthenticationUi : IWebAuthenticationUi
    {
        /// <summary>
        /// Displays authentication UI to the user for the specified request URI, returning
        /// the key value pairs from the query string upon reaching the callback URL.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="callbackUri">The callback URI.</param>
        /// <returns>The <see cref="IDictionary{string, string}"/> of key value pairs from the callback URI query string.</returns>
        public async Task<IDictionary<string, string>> AuthenticateAsync(Uri requestUri, Uri callbackUri = null)
        {
            WebAuthenticationResult result = null;

            // Attempt to authentication without prompting the user first.
            try
            {
                result = await this.AuthenticateAsync(requestUri, callbackUri, WebAuthenticationOptions.SilentMode);
            }
            catch (Exception exception)
            {
                // WebAuthenticationBroker can throw an exception in silent authentication mode when not using SSO and
                // silent authentication isn't available. Swallow it and try authenticating with user prompt. Even if
                // the exception is another type of exception we'll swallow and try again with the user prompt.
            }

            // AuthenticateAsync will return a UserCancel status in SSO mode if authentication requires user input. Try
            // authentication again using the user prompt flow.
            if (result == null || result.ResponseStatus == WebAuthenticationStatus.UserCancel)
            {
                try
                {
                    result = await this.AuthenticateAsync(requestUri, callbackUri, WebAuthenticationOptions.None);
                }
                catch (Exception exception)
                {
                    throw new OneDriveException(new Error { Code = OneDriveErrorCode.AuthenticationFailure.ToString() }, exception);
                }
            }

            if (result != null && !string.IsNullOrEmpty(result.ResponseData))
            {
                return UrlHelper.GetQueryOptions(new Uri(result.ResponseData));
            }
            else if (result != null && result.ResponseStatus == WebAuthenticationStatus.UserCancel)
            {
                throw new OneDriveException(new Error { Code = OneDriveErrorCode.AuthenticationCancelled.ToString() });
            }

            throw new OneDriveException(new Error { Code = OneDriveErrorCode.AuthenticationFailure.ToString() });
        }

        private async Task<WebAuthenticationResult> AuthenticateAsync(Uri requestUri, Uri callbackUri, WebAuthenticationOptions authenticationOptions)
        {
            return callbackUri == null
                ? await WebAuthenticationBroker.AuthenticateAsync(authenticationOptions, requestUri)
                : await WebAuthenticationBroker.AuthenticateAsync(authenticationOptions, requestUri, callbackUri);
        }
    }
}
