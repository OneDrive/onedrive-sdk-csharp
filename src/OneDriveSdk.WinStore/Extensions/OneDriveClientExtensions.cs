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
    public static class OneDriveClientExtensions
    {
        public static IOneDriveClient GetClientUsingOnlineIdAuthenticator(
            string[] scopes,
            string returnUrl = null,
            IHttpProvider httpProvider = null)
        {
            return new OneDriveClient(
                new AppConfig { MicrosoftAccountScopes = scopes },
                httpProvider: httpProvider ?? new HttpProvider(),
                serviceInfoProvider: new OnlineIdServiceInfoProvider());
        }

        public static IOneDriveClient GetClientUsingWebAuthenticationBroker(
            string appId,
            string[] scopes,
            IHttpProvider httpProvider = null)
        {
            return OneDriveClientExtensions.GetClientUsingWebAuthenticationBroker(appId, null, scopes, httpProvider);
        }

        public static IOneDriveClient GetClientUsingWebAuthenticationBroker(
            string appId,
            string returnUrl,
            string[] scopes,
            IHttpProvider httpProvider = null)
        {
            return new OneDriveClient(
                new AppConfig
                {
                    MicrosoftAccountAppId = appId,
                    MicrosoftAccountReturnUrl = returnUrl,
                    MicrosoftAccountScopes = scopes
                },
                httpProvider: httpProvider ?? new HttpProvider(),
                serviceInfoProvider: new WebAuthenticationBrokerServiceInfoProvider());
        }

        public static IOneDriveClient GetUniversalClient(
            string[] scopes,
            string returnUrl = null,
            IHttpProvider httpProvider = null)
        {
            return OneDriveClientExtensions.GetClientUsingOnlineIdAuthenticator(scopes, returnUrl, httpProvider);
        }
    }
}
