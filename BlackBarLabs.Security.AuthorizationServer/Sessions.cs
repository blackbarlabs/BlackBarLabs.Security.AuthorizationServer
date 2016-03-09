using System;
using System.Threading.Tasks;
using BlackBarLabs.Security.AuthorizationServer.Exceptions;

namespace BlackBarLabs.Security.AuthorizationServer
{
    public class Sessions
    {
        private Context context;
        private Persistence.IDataContext dataContext;

        internal Sessions(Context context, Persistence.IDataContext dataContext)
        {
            this.dataContext = dataContext;
            this.context = context;
        }

        public delegate T CreateSessionSuccessDelegate<T>(string token, string refreshToken);
        public delegate T CreateSessionAlreadyExistsDelegate<T>();
        public async Task<T> CreateSessionAsync<T>(Guid sessionId, CreateSessionSuccessDelegate<T> callback,
            CreateSessionAlreadyExistsDelegate<T> alreadyExists)
        {
            var refreshToken = JoshCodes.Core.SecureGuid.Generate().ToString("N");

            try
            {
                await this.dataContext.Sessions.CreateAsync(sessionId, refreshToken);
                var jwtToken = await this.GenerateToken(sessionId);
                return callback.Invoke(jwtToken, refreshToken);
            } catch(BlackBarLabs.Persistence.ResourceAlreadyExistsException)
            {
                return alreadyExists();
            }
        }

        public async Task<T> CreateSessionAsync<T>(Guid sessionId, Guid authorizationId,
            CredentialValidationMethodTypes method, Uri providerId, string username, string token,
            CreateSessionSuccessDelegate<T> onSuccess, CreateSessionAlreadyExistsDelegate<T> alreadyExists,
            Func<T> invalidCredentials)
        {
            if (!await AuthenticateAsync(authorizationId, method, providerId, username, token))
                return invalidCredentials();

            var refreshToken = JoshCodes.Core.SecureGuid.Generate().ToString("N");
            
            try
            {
                await this.dataContext.Sessions.CreateAsync(sessionId, refreshToken, authorizationId); // AuthorizationId may not need to be stored

                var jwtToken = await GenerateToken(sessionId, authorizationId);
                return onSuccess(jwtToken, refreshToken);
            }
            catch (BlackBarLabs.Persistence.ResourceAlreadyExistsException)
            {
                return alreadyExists();
            }
        }
        
        public delegate T AuthenticateSuccessDelegate<T>(string token, string refreshToken);
        public delegate T AuthenticateInvalidCredentialsDelegate<T>();
        public delegate T AuthenticateAlreadyAuthenticatedDelegate<T>();
        public delegate T AuthenticateNotFoundDelegate<T>();
        public async Task<T> AuthenticateAsync<T>(Guid sessionId, Guid authorizationId,
            CredentialValidationMethodTypes credentialValidationMethod, Uri credentialsProviderId, string username, string token,
            AuthenticateSuccessDelegate<T> onSuccess,
            AuthenticateInvalidCredentialsDelegate<T> onInvalidCredentials,
            AuthenticateAlreadyAuthenticatedDelegate<T> onAlreadyAuthenticated,
            AuthenticateNotFoundDelegate<T> onNotFound)
        {
            if (!await AuthenticateAsync(authorizationId, credentialValidationMethod, credentialsProviderId, username, token))
                return onInvalidCredentials();

            // Check if user is already authenticated
            var result = await await this.dataContext.Sessions.UpdateAuthentication(sessionId, async (authId, saveAuthId) =>
            {
                if (default(Guid) != authId)
                    return onAlreadyAuthenticated();

                saveAuthId(authorizationId);

                var jwtToken = await GenerateToken(sessionId, authorizationId);

                await context.Tokens.CreateAsync(authorizationId.ToString(), jwtToken);
                return onSuccess.Invoke(jwtToken, string.Empty);
            });
            return result;
        }

        private async Task<bool> AuthenticateAsync(Guid authorizationId, CredentialValidationMethodTypes method, Uri providerId, string username, string token)
        {
            var provider = this.context.GetCredentialProvider(method);
            var accessTokenTask = provider.RedeemTokenAsync(providerId, username, token);
            var doesMatchOrWasCreatedTask = this.dataContext.Authorizations.DoesMatchAsync(
                authorizationId, providerId, username);

            var accessSuccessful = default(string) != await accessTokenTask;
            var doesMatchOrWasCreated = await doesMatchOrWasCreatedTask;

            return accessSuccessful && doesMatchOrWasCreated;
        }

        private async Task<string> GenerateToken(Guid sessionId, Guid authorizationId = default(Guid))
        {
            var tokenExpirationInMinutesConfig = "20"; // ConfigurationManager.AppSettings["tokenExpirationInMinutes"];
            if (string.IsNullOrEmpty(tokenExpirationInMinutesConfig))
                throw new SystemException("TokenExpirationInMinutes was not found in the configuration file");
            var tokenExpirationInMinutes = Double.Parse(tokenExpirationInMinutesConfig) + 900000;

            if (await context.Roles.IsUserAdmin(authorizationId.ToString()))
            {
                var jwtTokenWithRoles = Security.Tokens.JwtTools.CreateToken(
                sessionId, authorizationId, tokenExpirationInMinutes, 1,  //TODO: This 1 should be an enumeration of SuperAdmin
                "AuthServer.issuer", "AuthServer.key");
                return jwtTokenWithRoles;

            }
            var jwtToken = Security.Tokens.JwtTools.CreateToken(
                sessionId, authorizationId, tokenExpirationInMinutes,
                "AuthServer.issuer", "AuthServer.key");

            return jwtToken;
        }
    }
}
