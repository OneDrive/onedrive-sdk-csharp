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

namespace Test.OneDriveSdk.WinRT.Mocks
{
    using System;

    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.OneDrive.Sdk;

    public class MockAuthenticationResult : IAuthenticationResult
    {
        public string AccessToken { get; set; }

        public string AccessTokenType { get; set; }

        public string Error { get; set; }

        public string ErrorDescription { get; set; }

        public DateTimeOffset ExpiresOn { get; set; }

        public string IdToken { get; set; }

        public bool IsMultipleResourceRefreshToken { get; set; }

        public string RefreshToken { get; set; }

        public AuthenticationStatus Status { get; set; }

        public int StatusCode { get; set; }

        public string TenantId { get; set; }

        public UserInfo UserInfo { get; set; }

        public string CreateAuthorizationHeader()
        {
            throw new NotImplementedException();
        }

        public string Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
