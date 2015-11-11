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
    using System.Net;

    public class AccountSession
    {
        public AccountSession()
        {
        }

        public AccountSession(IDictionary<string, string> authenticationResponseValues, string clientId = null, AccountType accountType = AccountType.None)
        {
            this.AccountType = accountType;
            this.ClientId = clientId;

            this.ParseAuthenticationResponseValues(authenticationResponseValues);
        }

        public string AccessToken { get; set; }

        public string AccessTokenType { get; set; }

        public AccountType AccountType { get; set; }

        public bool CanSignOut { get; set; }

        public string ClientId { get; set; }

        public DateTimeOffset ExpiresOnUtc { get; set; }

        public string RefreshToken { get; set; }

        public string[] Scopes { get; set; }

        public string UserId { get; set; }

        public bool IsExpiring()
        {
            return this.ExpiresOnUtc <= DateTimeOffset.Now.UtcDateTime.AddMinutes(5);
        }

        private void ParseAuthenticationResponseValues(IDictionary<string, string> authenticationResponseValues)
        {
            if (authenticationResponseValues != null)
            {
                foreach (var value in authenticationResponseValues)
                {
                    switch (value.Key)
                    {
                        case Constants.Authentication.AccessTokenKeyName:
                            this.AccessToken = value.Value;
                            break;
                        case Constants.Authentication.ExpiresInKeyName:
                            this.ExpiresOnUtc = DateTimeOffset.UtcNow.Add(new TimeSpan(0, 0, int.Parse(value.Value)));
                            break;
                        case Constants.Authentication.ScopeKeyName:
                            var decodedScopes = WebUtility.UrlDecode(value.Value);
                            this.Scopes = string.IsNullOrEmpty(decodedScopes) ? null : decodedScopes.Split(' ');
                            break;
                        case Constants.Authentication.UserIdKeyName:
                            this.UserId = value.Value;
                            break;
                        case Constants.Authentication.RefreshTokenKeyName:
                            this.RefreshToken = value.Value;
                            break;
                    }
                }
            }
        }
    }
}
