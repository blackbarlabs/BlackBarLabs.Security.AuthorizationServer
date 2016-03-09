using System;
using System.Threading.Tasks;
using Facebook;

namespace BlackBarLabs.Security.CredentialProvider.Facebook
{
    public class FacebookCredentialProvider : IProvideCredentials
    {
        public async Task<string> RedeemTokenAsync(Uri providerId, string username, string accessToken)
        {
            if (String.IsNullOrWhiteSpace(accessToken))
                return default(string);
            var client = new FacebookClient(accessToken);
            dynamic result = await client.GetTaskAsync("me", new { fields = "name,id" });
            if (null == result)
                return null;
            if (username != result.id)
                return null;
            return client.AccessToken;
        }
    }
}
