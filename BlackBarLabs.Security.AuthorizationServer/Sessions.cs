using System;
using System.Threading.Tasks;
using BlackBarLabs.Security.AuthorizationServer.Exceptions;
using BlackBarLabs.Security.Authorization;
using System.Configuration;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using BlackBarLabs.Core.Web;

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

        public delegate T CreateSessionSuccessDelegate<T>(Guid authorizationId, string token, string refreshToken);
        public delegate T CreateSessionAlreadyExistsDelegate<T>();
        public async Task<T> CreateSessionAsync<T>(Guid sessionId,
            CreateSessionSuccessDelegate<T> onSuccess,
            CreateSessionAlreadyExistsDelegate<T> alreadyExists)
        {
            var refreshToken = JoshCodes.Core.SecureGuid.Generate().ToString("N");

            try
            {
                await this.dataContext.Sessions.CreateAsync(sessionId, refreshToken);
                var jwtToken = await this.GenerateToken(sessionId);
                return onSuccess.Invoke(default(Guid), jwtToken, refreshToken);
            } catch(BlackBarLabs.Persistence.ResourceAlreadyExistsException)
            {
                return alreadyExists();
            }
        }

        public async Task<T> CreateSessionAsync<T>(Guid sessionId,
            CredentialValidationMethodTypes method, Uri providerId, string username, string token,
            CreateSessionSuccessDelegate<T> onSuccess, CreateSessionAlreadyExistsDelegate<T> alreadyExists,
            Func<string, T> invalidCredentials)
        {
            var result = await await AuthenticateAsync(method, providerId, username, token,
                async (authorizationId, externalClaimsLocations) =>
                {
                    var refreshToken = JoshCodes.Core.SecureGuid.Generate().ToString("N");
                    try
                    {
                        await this.dataContext.Sessions.CreateAsync(sessionId, refreshToken, authorizationId); // AuthorizationId may not need to be stored

                        var jwtToken = await GenerateToken(sessionId, authorizationId);
                        return onSuccess(authorizationId, jwtToken, refreshToken);
                    }
                    catch (BlackBarLabs.Persistence.ResourceAlreadyExistsException)
                    {
                        return alreadyExists();
                    }
                },
                () => Task.FromResult(invalidCredentials("Credential not found")),
                () => Task.FromResult(invalidCredentials("Credential failed")));
            return result;
        }
        
        public delegate T AuthenticateSuccessDelegate<T>(Guid authorizationId, string token, string refreshToken);
        public delegate T AuthenticateInvalidCredentialsDelegate<T>();
        public delegate T AuthenticateAlreadyAuthenticatedDelegate<T>();
        public delegate T AuthenticateNotFoundDelegate<T>();
        public async Task<T> AuthenticateAsync<T>(Guid sessionId,
            CredentialValidationMethodTypes credentialValidationMethod, Uri credentialsProviderId, string username, string token,
            AuthenticateSuccessDelegate<T> onSuccess,
            AuthenticateInvalidCredentialsDelegate<T> onInvalidCredentials,
            AuthenticateAlreadyAuthenticatedDelegate<T> onAlreadyAuthenticated,
            AuthenticateNotFoundDelegate<T> onNotFound)
        {
            var result = await await AuthenticateAsync(credentialValidationMethod, credentialsProviderId, username, token,
                async (authorizationId, externalClaimsLocations) =>
                {
                    var updateAuthResult = await await this.dataContext.Sessions.UpdateAuthentication(sessionId, async (authId, saveAuthId) =>
                    {
                        if (default(Guid) != authId)
                            return onAlreadyAuthenticated();

                        saveAuthId(authorizationId);

                        var jwtToken = await GenerateToken(sessionId, authorizationId);
                        
                        return onSuccess.Invoke(authorizationId, jwtToken, string.Empty);
                    });
                    return updateAuthResult;
                },
                () => Task.FromResult(onNotFound()),
                () => Task.FromResult(onInvalidCredentials()));
            return result;
        }

        private async Task<T> AuthenticateAsync<T>(
            CredentialValidationMethodTypes method, Uri providerId, string username, string token,
            Func<Guid, Uri [], T> onSuccess, Func<T> onAuthIdNotFound, Func<T> onInvalidCredential)
        {
            var provider = this.context.GetCredentialProvider(method);
            var accessTokenTask = provider.RedeemTokenAsync(providerId, username, token);

            var result = await await this.dataContext.Authorizations.FindAuthId(providerId, username,
                async (authorizationId, externalClaimsLocations) =>
                {
                    var doesMatch = await this.dataContext.Authorizations.DoesMatchAsync(
                        authorizationId, providerId, username);
                    if (!doesMatch)
                        return onAuthIdNotFound();

                    var accessSuccessful = default(string) != await accessTokenTask;
                    if (!accessSuccessful)
                    {
                        return onInvalidCredential();
                    }
                    return onSuccess(authorizationId, externalClaimsLocations);
                },
                () => Task.FromResult(onAuthIdNotFound()));
            return result;
        }

        private async Task<string> GenerateToken(Guid sessionId, Guid authorizationId = default(Guid), Uri [] credentialProviders = default(Uri[]))
        {
            var tokenExpirationInMinutesConfig = ConfigurationManager.AppSettings["BlackBarLabs.Security.AuthorizationServer.tokenExpirationInMinutes"];
            if (string.IsNullOrEmpty(tokenExpirationInMinutesConfig))
                throw new SystemException("TokenExpirationInMinutes was not found in the configuration file");
            var tokenExpirationInMinutes = Double.Parse(tokenExpirationInMinutesConfig);

            if (default(Uri[]) == credentialProviders)
            {
                return Security.Tokens.JwtTools.CreateToken(
                    sessionId, authorizationId, tokenExpirationInMinutes,
                    "AuthServer.issuer", "AuthServer.key");
            }

            var claims = (IEnumerable<Claim>)new[] {
                new Claim(ClaimIds.Session, sessionId.ToString()),
                new Claim(ClaimIds.Authorization, authorizationId.ToString()) };

            var providedClaimsTasks = credentialProviders.Select(async (credentialProvider) =>
                {
                    var httpWebRequest = WebRequest.Create(credentialProvider);
                    return await httpWebRequest.GetAsync(
                        (IClaim[] returnedClaims) =>
                        {
                            return returnedClaims.Select((fetchedClaim) =>
                             {
                                 return new Claim(fetchedClaim.Type.AbsoluteUri, fetchedClaim.Value);
                             });
                        },
                        (code, message) => default(IEnumerable<Claim>));
                });
            var providedClaims = (await Task.WhenAll(providedClaimsTasks)).SelectMany(a => a);
            
            var jwtToken = Security.Tokens.JwtTools.CreateToken(
                sessionId.ToString(), DateTimeOffset.UtcNow, 
                DateTimeOffset.UtcNow + TimeSpan.FromMinutes(tokenExpirationInMinutes),
                claims.Concat(providedClaims),
                "AuthServer.issuer", "AuthServer.key");

            return jwtToken;
        }
    }
}
