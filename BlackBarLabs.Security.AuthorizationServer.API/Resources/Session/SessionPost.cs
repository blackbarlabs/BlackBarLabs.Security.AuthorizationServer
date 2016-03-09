using BlackBarLabs.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace BlackBarLabs.Security.AuthorizationServer.API.Resources
{
    public class SessionPost : Session, IHttpActionResult
    {
        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            //Get the session and Extrude it's information
            Sessions.CreateSessionSuccessDelegate<HttpResponseMessage> createSessionCallback = (token, refreshToken) =>
            {
                this.SessionHeader = new AuthHeaderProps { Name = "Authorization", Value = token };
                this.RefreshToken = refreshToken;
                return this.Request.CreateResponse(HttpStatusCode.Created, this);
            };

            Sessions.CreateSessionAlreadyExistsDelegate<HttpResponseMessage> alreadyExistsCallback = () =>
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.Conflict, this.PreconditionViewModelEntityAlreadyExists());
            };

            if (!this.IsCredentialsPopulated())
            {
                return await this.Context.Sessions.CreateSessionAsync(Id, createSessionCallback, alreadyExistsCallback);
            }

            return await this.Context.Sessions.CreateSessionAsync(Id, this.AuthorizationId,
                this.Credentials.Method, this.Credentials.Provider, this.Credentials.UserId, this.Credentials.Token,
                createSessionCallback, alreadyExistsCallback,
                () =>
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Conflict, new Exception("Invalid credentials"));
                });
        }
    }
}