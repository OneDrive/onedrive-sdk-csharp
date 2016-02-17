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
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.OneDrive.Sdk;

    public delegate void AppendAuthHeaderAsyncCallback(HttpRequestMessage request);
    public delegate void AuthenticateAsyncCallback();
    public delegate void SignOutAsyncCallback();

    public class MockAuthenticationProvider : IAuthenticationProvider
    {
        private AccountSession authenticationResult;

        public MockAuthenticationProvider(AccountSession authenticationResult = null)
        {
            this.authenticationResult = authenticationResult;
        }

        public AccountSession CurrentAccountSession { get; set; }

        public AppendAuthHeaderAsyncCallback OnAppendAuthHeaderAsync { get; set; }

        public AuthenticateAsyncCallback OnAuthenticateAsync { get; set; }

        public SignOutAsyncCallback OnSignOutAsync { get; set; }

        public Task AppendAuthHeaderAsync(HttpRequestMessage request)
        {
            if (this.OnAppendAuthHeaderAsync != null)
            {
                this.OnAppendAuthHeaderAsync(request);
            }

            return Task.FromResult(0);
        }

        public Task<AccountSession> AuthenticateAsync()
        {
            if (this.OnAuthenticateAsync != null)
            {
                this.OnAuthenticateAsync();
            }

            return Task.FromResult(this.authenticationResult);
        }

        public Task SignOutAsync()
        {
            if (this.OnSignOutAsync != null)
            {
                this.OnSignOutAsync();
            }

            return Task.FromResult(0);
        }
    }
}
