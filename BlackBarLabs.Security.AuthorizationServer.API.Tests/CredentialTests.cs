using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BlackBarLabs.Api.Tests;
using BlackBarLabs.Security.AuthorizationServer.API.Controllers;
using System.Net;
using BlackBarLabs.Security.Authorization;
using BlackBarLabs.Security.CredentialProvider.Facebook.Tests;

namespace BlackBarLabs.Security.AuthorizationServer.API.Tests
{
    [TestClass]
    public class CredentialTests
    {
        [TestMethod]
        public async Task InvalidCredentials()
        {
            await TestSession.StartAsync(async (testSession) =>
            {
                var auth = await testSession.CreateAuthorizationAsync();
                string userId, token;
                CredentialProviderFacebookTests.CreateFbCredentials(out userId, out token);
                var goodCredential = new Resources.CredentialPost
                {
                    AuthorizationId = auth.Id,
                    Method = CredentialValidationMethodTypes.Facebook,
                    Provider = new Uri("http://www.facebook.com"),
                    UserId = userId,
                    Token = token,
                };
                await testSession.PostAsync<CredentialController>(goodCredential)
                    .AssertAsync(HttpStatusCode.Created);
                
                CredentialProviderFacebookTests.CreateFbCredentials(out userId, out token);
                var badCredential = new Resources.CredentialPost
                {
                    AuthorizationId = auth.Id,
                    Method = CredentialValidationMethodTypes.Facebook,
                    Provider = new Uri("http://www.facebook.com"),
                    UserId = userId,
                    Token = Guid.NewGuid().ToString("N"),
                };
                await testSession.PostAsync<CredentialController>(badCredential)
                    .AssertAsync(HttpStatusCode.Conflict);
            });
        }
    }
}
