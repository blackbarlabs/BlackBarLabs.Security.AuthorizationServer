using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlackBarLabs.Security.AuthorizationServer.Persistence
{
    public delegate Task<bool> CredentialProviderDelegate(Uri providerId, string username);
    public delegate Task<bool> ShouldCreateCallback();

    public interface IAuthorizations
    {
        /// <summary>
        /// For a given set of parameters, see if there is a set of credential information that matches.
        /// </summary>
        /// <param name="authorizationId"></param>
        /// <param name="providerId"></param>
        /// <param name="userId"></param>
        /// <returns>True, if a match is found; false, if match was found but did not match; false, if match was not found;</returns>
        Task<bool> DoesMatchAsync(Guid authorizationId, Uri providerId, string userId);

        /// <summary>
        /// For the provided providerId and userId, find the associated authentication id
        /// </summary>
        /// <param name="providerId"></param>
        /// <param name="userId"></param>
        /// <returns>The ID of the associated authentication or NULL if no authentication is associated.</returns>
        Task<Guid> FindAuthId(Uri providerId, string userId);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="authorizationId"></param>
        /// <param name="createCredentialProviderDelegateCallback">Will be invoked until it return null,
        /// is expected to invoke the delegate each time it is called to add a credential set to the authorization.</param>
        /// <returns></returns>
        Task<bool> CreateAuthorizationAsync<T>(Guid authorizationId, Func<CredentialProviderDelegate,
            IEnumerable<Task<T>>> createCredentialProviderDelegateCallback,
            Func<IEnumerable<T>, bool> validateResultsCallback);
        
    }
}
