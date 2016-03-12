using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BlackBarLabs.Api.Tests;
using BlackBarLabs.Security.AuthorizationServer.API.Controllers;
using BlackBarLabs.Security.Authorization;

namespace BlackBarLabs.Security.AuthorizationServer.API.Tests
{
    [TestClass]
    public class WorkflowTests
    {
        [TestMethod]
        public async Task WorkflowSASPut()
        {
            await TestSession.StartAsync(async (testSession) =>
            {
                // Create session resource
                var session = await testSession.CreateSessionAsync();

                // Create Auth resource
                var auth = await testSession.CreateAuthorizationAsync();

                // Authenticate session
                var authenticateSessionResponseMessage = await testSession.AuthenticateSession(
                    session.Id, auth.CredentialProviders[0]);
                authenticateSessionResponseMessage.AssertSuccessPut();
                var authenticateSession = authenticateSessionResponseMessage.GetContent<Resources.SessionPut>();
                Assert.AreEqual(auth.Id, authenticateSession.AuthorizationId);
            });
        }

        [TestMethod]
        public async Task WorkflowSASPost()
        {
            await TestSession.StartAsync(async (testSession) =>
            {
                // Create session resource
                var session = await testSession.CreateSessionAsync();

                // Create Auth resource
                var auth = await testSession.CreateAuthorizationAsync();

                // Authenticate session
                var authenticateSessionResponse = await testSession.AuthenticateSession(
                    session.Id, auth.CredentialProviders[0]);
                authenticateSessionResponse.Assert(System.Net.HttpStatusCode.Accepted);

                // Authenticate session
                var newSession = new Resources.SessionPost()
                {
                    Id = Guid.NewGuid(),
                    AuthorizationId = auth.Id,
                    Credentials = auth.CredentialProviders[0],
                };
                await testSession.PostAsync<SessionController>(newSession)
                    .AssertAsync(System.Net.HttpStatusCode.Created);
            });
        }

        [TestMethod]
        public async Task WorkflowAS()
        {
            await TestSession.StartAsync(async (testSession) =>
            {
                // Create Auth resource
                var auth = await testSession.CreateAuthorizationAsync();
                
                // Authenticate session
                var newSession = new Resources.SessionPost()
                {
                    Id = Guid.NewGuid(),
                    Credentials = auth.CredentialProviders[0],
                };
                var responseMessage = await testSession.PostAsync<SessionController>(newSession);
                responseMessage.Assert(System.Net.HttpStatusCode.Created);
                var responseSession = responseMessage.GetContent<Resources.Session>();
                Assert.AreEqual(auth.Id, responseSession.AuthorizationId);
            });
        }

        [TestMethod]
        public async Task WorkflowImplicitAuth()
        {
            await TestSession.StartAsync(async (testSession) =>
            {
                // Create Auth resource
                var credential = new CredentialsType
                {
                    Method = CredentialValidationMethodTypes.Implicit,
                    Provider = new Uri("http://www.example.com/ImplicitAuth"),
                    UserId = Guid.NewGuid().ToString(),
                    Token = Guid.NewGuid().ToString(),
                };

                var auth = await testSession.CreateAuthorizationAsync(credential);

                // Authenticate session
                var newSession = new Resources.SessionPost()
                {
                    Id = Guid.NewGuid(),
                    AuthorizationId = auth.Id,
                    Credentials = auth.CredentialProviders[0],
                };
                await testSession.PostAsync<SessionController>(newSession)
                    .AssertAsync(System.Net.HttpStatusCode.Created);
            });
        }

        [Ignore]
        [TestMethod]
        public async Task ForBrian()
        {
            await TestSession.StartAsync(async (testSession) =>
            {
                // Create Auth resource
                var credential = new CredentialsType
                {
                    Method = CredentialValidationMethodTypes.Implicit,
                    Provider = new Uri("http://orderowl.com/api/Auth"),
                    UserId = "DarrellWagner",
                    Token = "Password#1",
                };
                
                // Authenticate session
                var newSession = new Resources.SessionPost()
                {
                    Id = Guid.Parse("cb920a68-20b1-d822-d39a-d206b3fd0414"),
                    Credentials = credential,
                };
                await testSession.PostAsync<SessionController>(newSession)
                    .AssertAsync(System.Net.HttpStatusCode.Created);
            });
        }
    }
}
