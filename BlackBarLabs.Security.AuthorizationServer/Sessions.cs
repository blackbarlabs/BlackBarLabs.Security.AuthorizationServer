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
using BlackBarLabs.Collections.Generic;

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
        public async Task<T> CreateAsync<T>(Guid sessionId,
            AuthorizationClient.IContext authClient,
            CreateSessionSuccessDelegate<T> onSuccess,
            CreateSessionAlreadyExistsDelegate<T> alreadyExists)
        {
            var refreshToken = JoshCodes.Core.SecureGuid.Generate().ToString("N");

            try
            {
                await this.dataContext.Sessions.CreateAsync(sessionId, refreshToken);
                var jwtToken = await this.GenerateToken(sessionId, default(Guid), default(Uri[]), authClient);
                return onSuccess.Invoke(default(Guid), jwtToken, refreshToken);
            } catch(BlackBarLabs.Persistence.ResourceAlreadyExistsException)
            {
                return alreadyExists();
            }
        }

        public async Task<T> CreateAsync<T>(Guid sessionId,
            CredentialValidationMethodTypes method, Uri providerId, string username, string token,
            AuthorizationClient.IContext authClient,
            CreateSessionSuccessDelegate<T> onSuccess,
            CreateSessionAlreadyExistsDelegate<T> alreadyExists,
            Func<string, T> invalidCredentials)
        {
            var result = await await AuthenticateCredentialsAsync(method, providerId, username, token,
                async (authorizationId, externalClaimsLocations) =>
                {
                    var refreshToken = JoshCodes.Core.SecureGuid.Generate().ToString("N");
                    try
                    {
                        await this.dataContext.Sessions.CreateAsync(sessionId, refreshToken, authorizationId); // AuthorizationId may not need to be stored

                        var jwtToken = await GenerateToken(sessionId, authorizationId, externalClaimsLocations, authClient);
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
            AuthorizationClient.IContext authClient,
            AuthenticateSuccessDelegate<T> onSuccess,
            AuthenticateInvalidCredentialsDelegate<T> onInvalidCredentials,
            AuthenticateAlreadyAuthenticatedDelegate<T> onAlreadyAuthenticated,
            AuthenticateNotFoundDelegate<T> onNotFound)
        {
            var result = await await AuthenticateCredentialsAsync(credentialValidationMethod, credentialsProviderId, username, token,
                async (authorizationId, externalClaimsLocations) =>
                {
                    var updateAuthResult = await await this.dataContext.Sessions.UpdateAuthentication(sessionId,
                        async (authId, saveAuthId) =>
                        {
                            if (default(Guid) != authId)
                                return onAlreadyAuthenticated();

                            saveAuthId(authorizationId);
                            var jwtToken = await GenerateToken(sessionId, authorizationId, externalClaimsLocations, authClient);
                            return onSuccess.Invoke(authorizationId, jwtToken, string.Empty);
                        },
                        () => Task.FromResult(onNotFound()));
                    return updateAuthResult;
                },
                () => Task.FromResult(onNotFound()),
                () => Task.FromResult(onInvalidCredentials()));
            return result;
        }

        private async Task<T> AuthenticateCredentialsAsync<T>(
            CredentialValidationMethodTypes method, Uri providerId, string username, string token,
            Func<Guid, Uri [], T> onSuccess, Func<T> onAuthIdNotFound, Func<T> onInvalidCredential)
        {
            var provider = this.context.GetCredentialProvider(method);
            return await await provider.RedeemTokenAsync(providerId, username, token,
                async (accessToken) =>
                {
                    var result = await await this.dataContext.Authorizations.FindAuthId(providerId, username,
                        async (authorizationId, externalClaimsLocations) =>
                        {
                            var doesMatch = await this.dataContext.Authorizations.DoesMatchAsync(
                                authorizationId, providerId, username);
                            if (!doesMatch)
                                return onInvalidCredential();
                            return onSuccess(authorizationId, externalClaimsLocations);
                        },
                        () => Task.FromResult(onAuthIdNotFound()));
                    return result;
                },
                () => Task.FromResult(onAuthIdNotFound()),
                () => { throw new Exception("Could not connect to auth system"); });
        }

        private async Task<string> GenerateToken(Guid sessionId, Guid authorizationId, Uri [] credentialProviders,
            AuthorizationClient.IContext authClient)
        {
            var tokenExpirationInMinutesConfig = ConfigurationManager.AppSettings["BlackBarLabs.Security.AuthorizationServer.tokenExpirationInMinutes"];
            if (string.IsNullOrEmpty(tokenExpirationInMinutesConfig))
                throw new SystemException("TokenExpirationInMinutes was not found in the configuration file");
            var tokenExpirationInMinutes = Double.Parse(tokenExpirationInMinutesConfig);

            var claims = await GetClaimsAsync(sessionId, authorizationId, credentialProviders, authClient);

            var jwtToken = Security.Tokens.JwtTools.CreateToken(
                sessionId.ToString(), DateTimeOffset.UtcNow, 
                DateTimeOffset.UtcNow + TimeSpan.FromMinutes(tokenExpirationInMinutes),
                claims,
                "AuthServer.issuer", "AuthServer.key");

            return jwtToken;
        }

        private async Task<IEnumerable<Claim>> GetClaimsAsync(Guid sessionId, Guid authorizationId, Uri[] credentialProviders,
            AuthorizationClient.IContext authClient)
        {
            var claims = (IEnumerable<Claim>)new[] {
                new Claim(ClaimIds.Session, sessionId.ToString()),
                new Claim(ClaimIds.Authorization, authorizationId.ToString()) };

            if (default(Uri[]) == credentialProviders || credentialProviders.Length == 0)
            {
                return claims;
            }

            var externalProvidedClaims = await GetClaimsExternalAsync(credentialProviders, authClient);
            return claims.Concat(externalProvidedClaims);
        }

        private async Task<IEnumerable<Claim>> GetClaimsExternalAsync(Uri[] credentialProviders,
            AuthorizationClient.IContext authClient)
        {
            var providedClaimsTasks = credentialProviders.Select(async (credentialProvider) =>
            {
                return await authClient.ClaimsGetAsync(credentialProvider,
                    (IClaim[] returnedClaims) => returnedClaims.Select((fetchedClaim) =>
                    {
                        return new Claim(fetchedClaim.Type.AbsoluteUri, fetchedClaim.Value);
                    }),
                    (code, message) => new Claim[] { new Claim(credentialProvider.AbsoluteUri, code.ToString() + message) });
            });
            var providedClaimss = await Task.WhenAll(providedClaimsTasks);
            var externalProvidedClaims = providedClaimss
                .Where(claims => default(IEnumerable<Claim>) != claims)
                .SelectMany()
                .Distinct(JoshCodes.Core.Equality<Claim>.CreateComparer(claim => claim.Type))
                .Select(claim => new Claim(claim.Type, claim.Value));

            return externalProvidedClaims;
        }
    }
}
