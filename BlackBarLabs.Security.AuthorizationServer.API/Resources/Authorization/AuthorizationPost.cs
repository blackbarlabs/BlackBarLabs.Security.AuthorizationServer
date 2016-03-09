using System;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using BlackBarLabs.Security.AuthorizationServer.API.Models;
using System.Web.Http;
using System.Net.Http;
using System.Threading;
using BlackBarLabs.Api;
using BlackBarLabs.Security.Authorization;

namespace BlackBarLabs.Security.AuthorizationServer.API.Resources
{
    [DataContract]
    public class AuthorizationPost : Authorization, IHttpActionResult
    {
        #region Actionables
        
        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            // Ensure that some credentials are provided
            if (!this.HasCredentials())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                    this.InvalidViewModelProperty((m) => m.CredentialProviders,
                        "Credentials are required for authorization (otherwise this authorization could not be used again"));
            }

            var creationResults = await this.Context.Authorizations.CreateCredentialsAsync(this.Id, this.CredentialProviders.Select(credProvider =>
            {
                Func<Authorizations.CredentialProviderDelegate, Task<Authorizations.CreateCredentialResult>> result = async (callback) =>
                {
                    return await callback(credProvider.Method, credProvider.Provider, credProvider.UserId, credProvider.Token);
                };
                return result;
            }));

            var authenticationFailures = creationResults.Where((cs) => cs.AuthenicationFailed).ToArray();
            if (authenticationFailures.Count() > 0)
            {
                var response = new Authorization()
                {
                    Id = this.Id,
                    CredentialProviders = authenticationFailures.Select(af => af.IfAuthenticationFailed((validationMethod, providerId, userId) =>
                        new CredentialsType()
                        {
                            Method = validationMethod,
                            Provider = providerId,
                            UserId = userId,
                            Token = "****************",
                        })).ToArray(),
                };
                return Request.CreateResponse(HttpStatusCode.Conflict, response);
            }

            var firstAssociationFailure = creationResults.FirstOrDefault((cs) => cs.IsAlreadyAssociated);
            if (default(Authorizations.CreateCredentialResult) != firstAssociationFailure)
            {
                var response = firstAssociationFailure.IfAlreadyAssociated((authId, validationMethod, providerId, userId) =>
                {
                    return new Authorization()
                    {
                        Id = authId,
                        CredentialProviders = new CredentialsType[] {
                                new CredentialsType()
                                {
                                    Method = validationMethod,
                                    Provider = providerId,
                                    UserId = userId,
                                    Token = "****************",
                                } },
                    };
                });
                return Request.CreateResponse(HttpStatusCode.Conflict, response);
            }

            return Request.CreateResponse(HttpStatusCode.Created, this);
        }

        #endregion
    }
}
