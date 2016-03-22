using System;
using System.Net;
using System.Threading.Tasks;

using BlackBarLabs.Api.Tests;
using BlackBarLabs.Security.Authorization;
using BlackBarLabs.Security.AuthorizationServer.API.Controllers;
using BlackBarLabs.Security.CredentialProvider.Facebook.Tests;

namespace BlackBarLabs.Security.AuthorizationServer.API.Tests
{
    public static class CredentialHelpers
    {
        public static async Task<Resources.Credential> CreateCredentialFacebookAsync(this TestSession testSession, 
            Guid authId)
        {
            string userId, token;
            CredentialProviderFacebookTests.CreateFbCredentials(out userId, out token);

            var credential = new Resources.CredentialPost
            {
                AuthorizationId = authId,
                Method = CredentialValidationMethodTypes.Facebook,
                Provider = new Uri("http://api.facebook.com"),
                UserId = userId,
                Token = token,
            };
            await testSession.PostAsync<CredentialController>(credential)
                .AssertAsync(HttpStatusCode.Created);
            return credential;
        }

        public static async Task<Resources.Credential> CreateCredentialVoucherAsync(this TestSession testSession,
            Guid authId, TimeSpan duration = default(TimeSpan))
        {
            if (duration == default(TimeSpan))
                duration = TimeSpan.FromMinutes(10.0);

            var trustedVoucherProverId = CredentialProvider.Voucher.Utilities.GetTrustedProviderId();
            var token = CredentialProvider.Voucher.Utilities.GenerateToken(authId, DateTime.UtcNow + duration);
            var credentialVoucher = new Resources.CredentialPost
            {
                AuthorizationId = authId,
                Method = CredentialValidationMethodTypes.Voucher,
                Provider = trustedVoucherProverId,
                Token = token,
                UserId = authId.ToString("N"),
            };
            await testSession.PostAsync<CredentialController>(credentialVoucher)
                .AssertAsync(HttpStatusCode.Created);
            return credentialVoucher;
        }

        public static async Task<Resources.Credential> CreateCredentialImplicitAsync(this TestSession testSession,
            Guid authId, string username = default(string), string password = default(string),
            Uri [] claimsProviders = default(Uri[]))
        {
            if (default(string) == username)
                username = Guid.NewGuid().ToString("N");
            if (default(string) == password)
                password = Guid.NewGuid().ToString("N");

            var trustedVoucherProverId = CredentialProvider.Voucher.Utilities.GetTrustedProviderId();
            var credentialImplicit = new Resources.CredentialPost
            {
                AuthorizationId = authId,
                Method = CredentialValidationMethodTypes.Implicit,
                Provider = trustedVoucherProverId,
                UserId = username,
                Token = password,
                ClaimsProviders = claimsProviders,
            };
            await testSession.PostAsync<CredentialController>(credentialImplicit)
                .AssertAsync(HttpStatusCode.Created);
            return credentialImplicit;
        }
    }
}
