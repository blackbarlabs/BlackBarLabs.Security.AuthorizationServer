using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using BlackBarLabs.Api.Tests;
using BlackBarLabs.Security.Authorization;
using BlackBarLabs.Security.AuthorizationServer.API.Controllers;

namespace BlackBarLabs.Security.AuthorizationServer.API.Tests
{
    public static class SessionHelpers
    {
        public static async Task<Resources.Session> CreateSessionAsync(this TestSession testSession)
        {
            var id = Guid.NewGuid();
            var session = new Resources.SessionPost()
            {
                Id = id,
            };
            var response = await testSession.PostAsync<SessionController>(session);
            response.Assert(HttpStatusCode.Created);
            return session;
        }

        public static async Task<Resources.Session> CreateSessionWithCredentialsAsync(this TestSession testSession)
        {
            var auth = await testSession.CreateAuthorizationAsync();

            var sessionId = Guid.NewGuid();
            var session = new Resources.SessionPost()
            {
                Id = sessionId,
                AuthorizationId = auth.Id,
                Credentials = new CredentialsType()
                {
                    Method = CredentialValidationMethodTypes.Facebook,
                    Provider = new Uri("http://api.facebook.com"),
                    Token = auth.CredentialProviders[0].Token,
                    UserId = auth.CredentialProviders[0].UserId,
                },
            };
            var createSessionResponse = await testSession.PostAsync<SessionController>(session);
            createSessionResponse.Assert(HttpStatusCode.Created);
            return session;
        }

        public static async Task<HttpResponseMessage> AuthenticateSession(this TestSession testSession,
            Guid sessionId, CredentialsType credential)
        {
            var session = new Resources.SessionPut()
            {
                Id = sessionId,
                Credentials = credential,
            };
            var authenticateSessionResponse = await testSession.PutAsync<SessionController>(session);
            return authenticateSessionResponse;
        }
    }
}
