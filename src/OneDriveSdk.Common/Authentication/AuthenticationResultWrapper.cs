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

    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    public class AuthenticationResultWrapper : IAuthenticationResult
    {
        private AuthenticationResult authenticationResult;

        public AuthenticationResultWrapper(AuthenticationResult authenticationResult)
        {
            this.authenticationResult = authenticationResult;
        }

        public string AccessToken
        {
            get
            {
                if (this.authenticationResult != null)
                {
                    return this.authenticationResult.AccessToken;
                }

                return null;
            }
        }

        public string AccessTokenType
        {
            get
            {
                if (this.authenticationResult != null)
                {
                    return this.authenticationResult.AccessTokenType;
                }

                return null;
            }
        }

        public DateTimeOffset ExpiresOn
        {
            get
            {
                if (this.authenticationResult != null)
                {
                    return this.authenticationResult.ExpiresOn;
                }

                return default(DateTimeOffset);
            }
        }

        public string IdToken
        {
            get
            {
                if (this.authenticationResult != null)
                {
                    return this.authenticationResult.IdToken;
                }

                return null;
            }
        }

        public bool IsMultipleResourceRefreshToken
        {
            get
            {
                if (this.authenticationResult != null)
                {
                    return this.authenticationResult.IsMultipleResourceRefreshToken;
                }

                return false;
            }
        }

        public string RefreshToken
        {
            get
            {
                if (this.authenticationResult != null)
                {
                    return this.authenticationResult.RefreshToken;
                }

                return null;
            }
        }

        public string TenantId
        {
            get
            {
                if (this.authenticationResult != null)
                {
                    return this.authenticationResult.TenantId;
                }

                return null;
            }
        }

        public UserInfo UserInfo
        {
            get
            {
                if (this.authenticationResult != null)
                {
                    return this.authenticationResult.UserInfo;
                }

                return null;
            }
        }

#if !WINFORMS
        public string Error
        {
            get
            {
                if (this.authenticationResult != null)
                {
                    return this.authenticationResult.Error;
                }

                return null;
            }
        }

        public string ErrorDescription
        {
            get
            {
                if (this.authenticationResult != null)
                {
                    return this.authenticationResult.ErrorDescription;

                }

                return null;
            }
        }

        public AuthenticationStatus Status
        {
            get
            {
                if (this.authenticationResult != null)
                {
                    return this.authenticationResult.Status;
                }

                return default(AuthenticationStatus);
            }
        }

        public int StatusCode
        {
            get
            {
                if (this.authenticationResult != null)
                {
                    return this.authenticationResult.StatusCode;
                }

                return default(int);
            }

            set
            {
                if (this.authenticationResult != null)
                {
                    this.authenticationResult.StatusCode = value;
                }
            }
        }
#endif

        public string CreateAuthorizationHeader()
        {
            if (this.authenticationResult != null)
            {
                return this.authenticationResult.CreateAuthorizationHeader();
            }

            return null;
        }

        public string Serialize()
        {
            if (this.authenticationResult != null)
            {
                return this.authenticationResult.Serialize();
            }

            return null;
        }
    }
}
