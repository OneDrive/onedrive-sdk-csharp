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
    public static class Constants
    {
        public const int PollingIntervalInMs = 5000;

        public static class Authentication
        {
            public const string AccessTokenKeyName = "access_token";

            public const string AuthenticationCancelled = "authentication_cancelled";

            public const string AuthorizationServiceKey = "authorization_service";

            public const string ClientIdKeyName = "client_id";

            public const string ClientSecretKeyName = "client_secret";

            public const string CodeKeyName = "code";

            public const string DiscoveryResourceKey = "discovery_resource";

            public const string DiscoveryServiceKey = "discovery_service";

            public const string ErrorDescriptionKeyName = "error_description";

            public const string ErrorKeyName = "error";
            
            public const string ExpiresInKeyName = "expires_in";

            public const string GrantTypeKeyName = "grant_type";

            public const string RedirectUriKeyName = "redirect_uri";

            public const string RefreshTokenKeyName = "refresh_token";

            public const string ResponseTypeKeyName = "response_type";

            public const string ScopeKeyName = "scope";

            public const string TokenResponseTypeValueName = "token";

            public const string TokenServiceKey = "token_service";

            public const string TokenTypeKeyName = "token_type";
            
            public const string UserIdKeyName = "user_id";

            internal const string ActiveDirectoryAuthenticationServiceUrl = "https://login.windows.net/common/oauth2/authorize";

            internal const string ActiveDirectoryDiscoveryResource = "https://api.office.com/discovery/";

            internal const string ActiveDirectoryDiscoveryServiceUrl = "https://api.office.com/discovery/v2.0/me/services";

            internal const string ActiveDirectorySignOutUrl = "https://login.windows.net/common/oauth2/logout";

            internal const string ActiveDirectoryTokenServiceUrl = "https://login.windows.net/common/oauth2/token";

            internal const string MicrosoftAccountAuthenticationServiceUrl = "https://login.live.com/oauth20_authorize.srf";

            internal const string MicrosoftAccountSignOutUrl = "https://login.live.com/oauth20_logout.srf";

            internal const string MicrosoftAccountTokenServiceUrl = "https://login.live.com/oauth20_token.srf";

            internal const string MyFilesCapability = "MyFiles";

            internal const string OneDriveConsumerBaseUrlFormatString = "https://api.onedrive.com/{0}";
        }

        public static class Headers
        {
            public const string Bearer = "Bearer";
            
            public const string BusinessSdkVersionHeaderName = "X-ClientService-ClientTag";

            public const string ConsumerSdkVersionHeaderName = "X-RequestStats";

            public const string FormUrlEncodedContentType = "application/x-www-form-urlencoded";

            public const string SdkVersionHeaderValue = "SDK-Version=CSharp-v{0}";
        }

        public static class Url
        {
            public const string Drive = "drive";

            public const string Root = "root";

            public const string AppRoot = "approot";
        }
    }
}
