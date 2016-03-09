using BlackBarLabs.Api.Tests;
using BlackBarLabs.Security.AuthorizationServer.API.Controllers;
using BlackBarLabs.Security.CredentialProvider.Facebook.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BlackBarLabs.Security.AuthorizationServer.API.Tests
{
    public static class AuthorizationHelpers
    {
        public static async Task<Resources.Authorization> CreateAuthorizationAsync(this TestSession testSession,
            Models.CredentialsType credential = default(Models.CredentialsType))
        {
            if (default(Models.CredentialsType) == credential)
            {
                string userId, token;
                CredentialProviderFacebookTests.CreateFbCredentials(out userId, out token);
                credential = new Models.CredentialsType()
                {
                    Method = CredentialValidationMethodTypes.Facebook,
                    Provider = new Uri("http://api.facebook.com"),
                    UserId = userId,
                    Token = token,
                };
            }

            var auth = new Resources.AuthorizationPost()
            {
                Id = Guid.NewGuid(),
                CredentialProviders =
                    new Models.CredentialsType[]
                    {
                        credential
                    },
            };
            var createAuthResponse = await testSession.PostAsync<AuthorizationController>(auth);
            createAuthResponse.Assert(HttpStatusCode.Created);
            return auth;
        }
    }
}
