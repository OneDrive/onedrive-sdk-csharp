﻿// ------------------------------------------------------------------------------
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
    using System.Threading.Tasks;

    public class AndroidServiceInfoProvider : IServiceInfoProvider
    {
        private IWebAuthenticationUi authenticationUi;

        public AndroidServiceInfoProvider(IWebAuthenticationUi authenticationUi)
        {
            this.authenticationUi = authenticationUi;
        }

        public IAuthenticationProvider AuthenticationProvider { get; set; }

        public Task<ServiceInfo> GetServiceInfo(AppConfig appConfig, Microsoft.OneDrive.Sdk.CredentialCache credentialCache, IHttpProvider httpProvider, ClientType clientType)
        {
            if (clientType == ClientType.Business)
            {
                throw new OneDriveException(
                    new Error
                    {
                        Code = OneDriveErrorCode.AuthenticationFailure.ToString(),
                        Message = "AndroidServiceInfoProvider only supports Microsoft account authentication."
                    });
            }

            var microsoftAccountServiceInfo = new MicrosoftAccountServiceInfo
            {
                AppId = appConfig.MicrosoftAccountAppId,
                ClientSecret = appConfig.MicrosoftAccountClientSecret,
                CredentialCache = credentialCache,
                HttpProvider = httpProvider,
                ReturnUrl = appConfig.MicrosoftAccountReturnUrl,
                Scopes = appConfig.MicrosoftAccountScopes,
                WebAuthenticationUi = this.authenticationUi,
            };

            microsoftAccountServiceInfo.AuthenticationProvider = this.AuthenticationProvider ?? new MicrosoftAccountAuthenticationProvider(microsoftAccountServiceInfo);
            return Task.FromResult<ServiceInfo>(microsoftAccountServiceInfo);
        }
    }
}
