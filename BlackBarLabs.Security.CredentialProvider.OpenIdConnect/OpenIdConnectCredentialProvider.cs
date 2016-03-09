using System;
using System.Threading.Tasks;

namespace BlackBarLabs.Security.CredentialProvider.OpenIdConnect
{
    public class OpenIdConnectCredentialProvider : IProvideCredentials
    {
        public Task<string> RedeemTokenAsync(Uri providerId, string username, string accessToken)
        {
            var returnValue = String.Compare(username, accessToken) == 0 ? accessToken : default(string);
            return Task.FromResult(returnValue);
        }
    }
}
