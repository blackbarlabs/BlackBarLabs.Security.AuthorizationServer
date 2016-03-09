using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using BlackBarLabs.Api.Tests;
using BlackBarLabs.Security.Authorization;
using BlackBarLabs.Security.AuthorizationServer.API.Controllers;
using BlackBarLabs.Security.CredentialProvider.Facebook.Tests;

namespace BlackBarLabs.Security.AuthorizationServer.API.Tests
{
    public static class AuthorizationHelpers
    {
        public static Task<Resources.Authorization> CreateAuthorizationAsync(this TestSession testSession,
            CredentialsType credential = default(CredentialsType))
        {
            if (default(CredentialsType) == credential)
            {
                string userId, token;
                CredentialProviderFacebookTests.CreateFbCredentials(out userId, out token);
                credential = new CredentialsType()
                {
                    Method = CredentialValidationMethodTypes.Facebook,
                    Provider = new Uri("http://api.facebook.com"),
                    UserId = userId,
                    Token = token,
                };
            }
            return testSession.CreateAuthorizationAsync(new CredentialsType[] { credential });
        }

        public static async Task<Resources.Authorization> CreateAuthorizationAsync(this TestSession testSession,
            IEnumerable<CredentialsType> credentials)
        {
            var auth = new Resources.AuthorizationPost()
            {
                Id = Guid.NewGuid(),
                CredentialProviders = credentials.ToArray(),
            };
            var createAuthResponse = await testSession.PostAsync<AuthorizationController>(auth);
            createAuthResponse.Assert(HttpStatusCode.Created);
            return auth;
        }
    }
}
