using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.OneDrive.Sdk
{
    public class GraphAuthenticationProvider : IAuthenticationProvider
    {
        public AccountSession CurrentAccountSession { get; private set; }

        public async Task AppendAuthHeaderAsync(HttpRequestMessage request)
        {
            if (this.CurrentAccountSession == null || string.IsNullOrEmpty(CurrentAccountSession.AccessToken))
            {
                await this.AuthenticateAsync();
            }

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", this.CurrentAccountSession.AccessToken);
        }

        public Task<AccountSession> AuthenticateAsync()
        {
            if (this.CurrentAccountSession == null)
            {
                var redirectUri = new Uri("http://localhost:44323");
                AuthenticationContext authenticationContext = new AuthenticationContext(Constants.AuthString, false);
                AuthenticationResult userAuthnResult = authenticationContext.AcquireToken(Constants.ResourceUrl,
                    Constants.ClientIdForUserAuthn, redirectUri, PromptBehavior.RefreshSession);

                Console.WriteLine("\n Welcome " + userAuthnResult.UserInfo.GivenName + " " +
                                  userAuthnResult.UserInfo.FamilyName);

                this.CurrentAccountSession = new AccountSession { AccessToken = userAuthnResult.AccessToken, ExpiresOnUtc = userAuthnResult.ExpiresOn };
            }

            return Task.FromResult(this.CurrentAccountSession);
        }

        public Task SignOutAsync()
        {
            throw new NotImplementedException();
        }
    }
}
