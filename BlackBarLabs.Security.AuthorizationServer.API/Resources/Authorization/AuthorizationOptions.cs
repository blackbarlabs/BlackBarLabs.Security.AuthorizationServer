using BlackBarLabs.Security.AuthorizationServer.API.Models;
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
    public class AuthorizationOptions : Resource, IHttpActionResult
    {
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var credentialProviders = new CredentialsType[]
            {
                new CredentialsType
                {
                    Method = CredentialValidationMethodTypes.Facebook,
                    Provider = new Uri("http://api.facebook.com/Authorization"),
                    UserId = "0123456789",
                    Token = "ABC.123.MXC",
                },
                new CredentialsType
                {
                    Method = CredentialValidationMethodTypes.OpenIdConnect,
                    Provider = new Uri("urn:auth.gibbits.nc2media.com/AuthOpenIdConnect/"),
                    UserId = Guid.NewGuid().ToString("N"),
                    Token = "EDF.123.A3EF",
                },
                new CredentialsType
                {
                    Method = CredentialValidationMethodTypes.Implicit,
                    Provider = new Uri("http://www.example.com/ImplicitAuth"),
                    UserId = Guid.NewGuid().ToString("N"),
                    Token = Guid.NewGuid().ToString("N"),
                }
            };
            var viewModel = new Authorization
            {
                Id = Guid.NewGuid(),
                CredentialProviders = (credentialProviders),
            };
            var response = new BlackBarLabs.Api.Resources.Options()
            {
                Post = new[] { viewModel },
            };

            var responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, response);
            return Task.FromResult(responseMessage);
        }
    }
}