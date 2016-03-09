using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlackBarLabs.Security.AuthorizationServer
{
    public enum CredentialValidationMethodTypes
    {
        OpenIdConnect,
        Facebook,
        Twitter,
        Google,
        Implicit,
    }

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
        
        public delegate Task<CreateCredentialResult> CredentialProviderDelegate(CredentialValidationMethodTypes method, Uri providerId, string username, string token);

        public delegate IEnumerable<Task<bool>> CreateCredentialProviderCallback(CredentialProviderDelegate callback);
        
        public delegate bool UpdateCredentialProviderCallback(
            CredentialValidationMethodTypes method, Uri providerId, string username);
        
        public class CreateCredentialResult
        {
            public delegate void SuccessDelegate();
            public delegate void FailedToSaveDelegate(string message);
            public delegate T AuthenticationFailedDelegate<T>(CredentialValidationMethodTypes validationMethod, Uri providerId, string userId);
            public delegate T AlreadyAssociatedWithAuthIdDelegate<T>(Guid associatedAuthId,
                CredentialValidationMethodTypes validationMethod, Uri providerId, string userId);

            private bool authFailed = false;
            private bool alreadyAssociated = false;
            private bool saveFailed = false;

            private CredentialValidationMethodTypes validationMethod;
            private string message = string.Empty;
            private Guid associatedAuthId = Guid.Empty;
            private Uri providerId;
            private string userId;
            
            public bool IsSuccess
            {
                get
                {
                    return
                        (!authFailed) &&
                        (!saveFailed) &&
                        (!alreadyAssociated);
                }
            }

            public bool AuthenicationFailed { get { return this.authFailed; } }

            public bool IsAlreadyAssociated { get { return this.alreadyAssociated; } }

            public T IfAuthenticationFailed<T>(AuthenticationFailedDelegate<T> callback)
            {
                if (this.authFailed)
                {
                    return callback(validationMethod, providerId, userId);
                }
                return default(T);
            }
            
            public T IfAlreadyAssociated<T>(AlreadyAssociatedWithAuthIdDelegate<T> callback)
            {
                if (this.alreadyAssociated)
                {
                    return callback(associatedAuthId, validationMethod, providerId, userId);
                }
                return default(T);
            }

            #region Factory methods

            internal static CreateCredentialResult AlreadyAssociated(Guid associatedAuthId,
                CredentialValidationMethodTypes validationMethod,
                Uri providerId, string userId)
            {
                var result = new CreateCredentialResult();
                result.alreadyAssociated = true;
                result.associatedAuthId = associatedAuthId;
                result.validationMethod = validationMethod;
                result.providerId = providerId;
                result.userId = userId;
                return result;
            }

            internal static CreateCredentialResult AuthenticationFailed(
                CredentialValidationMethodTypes validationMethod,
                Uri providerId, string userId)
            {
                var result = new CreateCredentialResult();
                result.authFailed = true;
                result.validationMethod = validationMethod;
                result.providerId = providerId;
                result.userId = userId;
                return result;
            }

            internal static CreateCredentialResult FailedToSave(string message)
            {
                var result = new CreateCredentialResult();
                result.saveFailed = true;
                result.message = message;
                return result;
            }

            internal static CreateCredentialResult Success()
            {
                return new CreateCredentialResult();
            }

            #endregion
        }


        private IEnumerable<Task<CreateCredentialResult>> AuthenticateCredentialProviders(
            IEnumerable<Func<CredentialProviderDelegate, Task<CreateCredentialResult>>> credentialProviders,
            Persistence.CredentialProviderDelegate storeCredentialProvider)
        {
            // This method calls the calling method to get the next credential provider and ...
            var credentialValidations = credentialProviders.Select(credentialProvider => credentialProvider(
                async (method, providerId, username, token) =>
                {
                    // ... validates the provider credentials before accepting / storing them.
                    var provider = this.context.GetCredentialProvider(method);

                    var accessToken = await provider.RedeemTokenAsync(providerId, username, token);
                    if (default(string) == accessToken)
                        return CreateCredentialResult.AuthenticationFailed(method, providerId, username);

                    try
                    {
                        if (!await storeCredentialProvider(providerId, username))
                        {
                            var authId = await this.dataContext.Authorizations.FindAuthId(providerId, username);
                            return CreateCredentialResult.AlreadyAssociated(authId, method, providerId, username);
                        }
                    } catch(Exception ex)
                    {
                        return CreateCredentialResult.FailedToSave(ex.Message);
                    }

                    return CreateCredentialResult.Success();
                }));

            return credentialValidations;
        }

        public async Task<IEnumerable<CreateCredentialResult>> CreateCredentialsAsync(Guid authorizationId,
            IEnumerable<Func<CredentialProviderDelegate, Task<CreateCredentialResult>>> credentialProviders)
        {
            IEnumerable<CreateCredentialResult> results = default(IEnumerable<CreateCredentialResult>);
            // Persistence calls us back to create each credential provider
            await this.dataContext.Authorizations.CreateAuthorizationAsync(
                authorizationId,
                (storeCredentialProvider) => AuthenticateCredentialProviders(credentialProviders, storeCredentialProvider),
                (creationResults) =>
                {
                    results = creationResults;
                    var firstFailedCredential = creationResults.FirstOrDefault((cs) => !cs.IsSuccess);
                    var completeSuccess = firstFailedCredential == default(CreateCredentialResult);
                    return completeSuccess;
                });
            return results;
        }


        //TODO: The name of this function is confusing because the Update could fail, resulting in false
        //TODO: Also, the return value of false could mean that the Authorization didn't exist. 
        //TODO: AKA: False means 2 things
        public Task<bool> CreateOrUpdateIfExistsAsync(Guid authorizationId,
            CreateCredentialProviderCallback createCallback,
            Func<IEnumerator<UpdateCredentialProviderCallback>, Task<bool>> updateCallback)
        {
            // For now, just assume the authorization does not exist
            return Task.FromResult(false);
        }

        #endregion
        
    }
}
