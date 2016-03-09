using BlackBarLabs.Api.Tests;
using BlackBarLabs.Security.AuthorizationServer.API.Controllers;
using BlackBarLabs.Security.CredentialProvider.Facebook.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
                Credentials = new Models.CredentialsType()
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
            Guid sessionId, Guid authId, Models.CredentialsType credential)
        {
            var session = new Resources.SessionPut()
            {
                Id = sessionId,
                AuthorizationId = authId,
                Credentials = credential,
            };
            var authenticateSessionResponse = await testSession.PutAsync<SessionController>(session);
            return authenticateSessionResponse;
        }
    }
}
