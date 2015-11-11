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
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Security.Authentication.OnlineId;

    public class OnlineIdAuthenticationProvider : AuthenticationProvider
    {
        private OnlineIdAuthenticator authenticator;

        public OnlineIdAuthenticationProvider(ServiceInfo serviceInfo)
            : base(serviceInfo)
        {
            this.authenticator = new OnlineIdAuthenticator();
        }

        /// <summary>
        /// Signs the current user out.
        /// </summary>
        public override async Task SignOutAsync()
        {
            if (this.CurrentAccountSession != null && this.CurrentAccountSession.CanSignOut)
            {
                if (this.authenticator.CanSignOut)
                {
                    await this.authenticator.SignOutUserAsync();
                }

                this.DeleteUserCredentialsFromCache(this.CurrentAccountSession);
                this.CurrentAccountSession = null;
            }
        }

        protected override Task<AccountSession> GetAuthenticationResultAsync()
        {
            return this.GetAccountSessionAsync();
        }

        internal async Task<AccountSession> GetAccountSessionAsync()
        {
            try
            {
                var serviceTicketRequest = new OnlineIdServiceTicketRequest(string.Join(" ", this.ServiceInfo.Scopes), "DELEGATION");
                var authenticationResponse = await this.authenticator.AuthenticateUserAsync(serviceTicketRequest);

                var ticket = authenticationResponse.Tickets.FirstOrDefault();

                var accountSession = new AccountSession
                {
                    AccessToken = ticket == null ? null : ticket.Value,
                    AccountType = this.ServiceInfo.AccountType,
                    CanSignOut = this.authenticator.CanSignOut,
                    ClientId = this.authenticator.ApplicationId.ToString(),
                    UserId = authenticationResponse.SafeCustomerId,
                };

                return accountSession;
            }
            catch (Exception exception)
            {
                throw new OneDriveException(new Error { Code = OneDriveErrorCode.AuthenticationFailure.ToString(), Message = exception.Message }, exception);
            }
        }
    }
}
