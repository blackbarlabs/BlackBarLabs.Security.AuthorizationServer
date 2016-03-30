using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BlackBarLabs.Api.Tests;
using BlackBarLabs.Security.AuthorizationServer.API.Controllers;
using System.Net;
using BlackBarLabs.Security.Authorization;
using BlackBarLabs.Security.CredentialProvider.Facebook.Tests;
using System.IdentityModel.Tokens;
using System.Linq;
using BlackBarLabs.Web;

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

        [TestMethod]
        public async Task CredentialsHasClaims()
        {
            await TestSession.StartAsync(async (testSession) =>
            {
                var auth = await testSession.CreateAuthorizationAsync();
                var cred = await testSession.CreateCredentialImplicitAsync(auth.Id, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 
                    new Uri[] { new Uri("http://example.com/1234") });

                var type = "http://example.com/authorization/test/1234";
                var value = "foobar";
                await testSession.ClaimPostAsync(auth.Id, type, value);
                
                var sessionWithClaims = await testSession.CreateSessionWithCredentialsAsync(cred);
                var jwtClaims = sessionWithClaims.SessionHeader.Value.GetClaimsJwtString();
                
                var exampleClaim = jwtClaims
                    .First(claim => String.Compare(claim.Type, type) == 0);
                Assert.AreEqual(value, exampleClaim.Value);
            });
        }
    }
}
