using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BlackBarLabs.Api.Tests;
using BlackBarLabs.Security.AuthorizationServer.API.Controllers;
using System.Net;

namespace BlackBarLabs.Security.AuthorizationServer.API.Tests
{
    [TestClass]
    public class SessionTests
    {
        [TestMethod]
        public async Task CreateSessionTest()
        {
            await TestSession.StartAsync(async (testSession) =>
            {
                await testSession.CreateSessionAsync();
            });
        }

        [TestMethod]
        public async Task CreateSessionWithCredentialsTest()
        {
            await TestSession.StartAsync(async (testSession) =>
            {
                await testSession.CreateSessionWithCredentialsAsync();
            });
        }
        
        public async Task InvalidCredentials()
        {
            await TestSession.StartAsync(async (testSession) =>
            {
                var session = await testSession.CreateSessionWithCredentialsAsync();
                var auth = await testSession.CreateAuthorizationAsync();
                session.AuthorizationId = auth.Id;
                session.Credentials = auth.CredentialProviders[0];

                // Make credential invalid
                session.Credentials.Token = Guid.NewGuid().ToString("N");
                
                var authenticateSessionResponse = await testSession.PutAsync<SessionController>(session);
                authenticateSessionResponse.Assert(HttpStatusCode.Conflict);
            });
        }

    }
}
