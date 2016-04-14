using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BlackBarLabs.Security.Authorization;

namespace BlackBarLabs.Security.AuthorizationServer
{
    public class Authorizations
    {
        private Context context;

        private Persistence.IDataContext dataContext;

        internal Authorizations(Context context, Persistence.IDataContext dataContext)
        {
            this.dataContext = dataContext;
            this.context = context;
        }
        
        #region Credentials
        
        public async Task<TResult> CreateAsync<TResult>(Guid authorizationId, Func<TResult> onSuccess, Func<TResult> onAlreadyExists)
        {
            var result = await this.dataContext.Authorizations.CreateAuthorizationAsync(authorizationId,
                () => onSuccess(),
                () => onAlreadyExists());
            return result;
        }

        

        public async Task<TResult> CreateCredentialsAsync<TResult>(Guid authorizationId, 
            CredentialValidationMethodTypes method, Uri providerId, string username, string token,
            Func<TResult> success, Func<TResult> authenticationFailed,
            Func<TResult> authorizationDoesNotExists,
            Func<Guid, TResult> alreadyAssociated)
        {
            // ... validates the provider credentials before accepting / storing them.
            var provider = this.context.GetCredentialProvider(method);
            var result = await await provider.RedeemTokenAsync(providerId, username, token,
                async (resultToken) =>
                {
                    return await this.dataContext.Authorizations.CreateCredentialProviderAsync(authorizationId,
                        providerId, username,
                        () => success(),
                        () => authorizationDoesNotExists(),
                        (alreadyAssociatedAuthorizationId) => alreadyAssociated(alreadyAssociatedAuthorizationId));
                },
                (errorMessage) => Task.FromResult(authenticationFailed()),
                () => Task.FromResult(default(TResult)));
            return result;
            }


        public async Task<TResult> UpdateCredentialsAsync<TResult>(Guid authorizationId,
            CredentialValidationMethodTypes method, Uri providerId, string username, string token,
            Func<TResult> success, 
            Func<TResult> authorizationDoesNotExists,
            Func<TResult> updateFailed)
        {
            //Updates the Credential Password
            var provider = this.context.GetCredentialProvider(method);
            var result = await provider.UpdateTokenAsync(providerId, username, token,
                            (returnToken) => success(),
                            () => authorizationDoesNotExists(),
                            () => updateFailed());
            return result;
        }


        #endregion

    }
}
