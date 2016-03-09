using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BlackBarLabs.Security.AuthorizationServer.API.Models;
using BlackBarLabs.Security.Crypto;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BlackBarLabs.Api.Tests;
using BlackBarLabs.Security.AuthorizationServer.API.Controllers;

namespace BlackBarLabs.Security.AuthorizationServer.API.Tests
{
    [TestClass]
    public class AuthorizationTests
    {
        internal void GenerateKeys()
        {
            var rsa = new RSACryptoServiceProvider(2048);

            rsa.PersistKeyInCsp = false; //This is important because we don't want to store these keys in the windows files system

            string publicPrivateKeyXML = rsa.ToXmlString(true);
            string publicOnlyKeyXML = rsa.ToXmlString(false);

            string publicPrivateKeystring = CryptoTools.UrlBase64Encode(publicPrivateKeyXML);
            string publicOnlyKeystring = CryptoTools.UrlBase64Encode(publicOnlyKeyXML);
            
            // do stuff with keys...
        }

        [TestMethod]
        public void GenerateKeysTest()
        {
            this.GenerateKeys();
        }

        [TestMethod]
        public async Task CreateAuthorizationWithCredentials()
        {
            await TestSession.StartAsync(async (testSession) =>
            {
                // Create Auth resource
                await testSession.CreateAuthorizationAsync();
            });
        }
        
        public async Task CreateAuthorizationWithoutCredentials()
        {
            await TestSession.StartAsync(async (testSession) =>
            {
                var id = Guid.NewGuid();
                var authorization = new Resources.AuthorizationPost()
                {
                    Id = id,
                };
                await testSession.PostAsync<AuthorizationController>(authorization)
                    .AssertAsync(System.Net.HttpStatusCode.Conflict);
            });
        }
        
        [TestMethod]
        public async Task CreateAuthorizationWithBadCredentials()
        {
            await TestSession.StartAsync(async (testSession) =>
            {
                var badCredential = new CredentialsType
                {
                    Method = CredentialValidationMethodTypes.Facebook,
                    Provider = new Uri("http://www.facebook.com"),
                    UserId = default(string),
                    Token = default(string),
                };
                var id = Guid.NewGuid();
                var authorization = new Resources.AuthorizationPost()
                {
                    Id = id,
                    CredentialProviders = new CredentialsType [] { badCredential },
                };
                var response = await testSession.PostAsync<AuthorizationController>(authorization);
                var responseAuth = response.GetContent<Resources.Authorization>();
                Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
                Assert.AreEqual(id, responseAuth.Id);
                Assert.AreEqual(1, responseAuth.CredentialProviders.Length);
                Assert.AreEqual(badCredential.Method, responseAuth.CredentialProviders[0].Method);
                Assert.AreEqual(badCredential.Provider, responseAuth.CredentialProviders[0].Provider);
                Assert.AreEqual(badCredential.UserId, responseAuth.CredentialProviders[0].UserId);
            });
        }

        [TestMethod]
        public async Task AuthorizationDuplicateCredentials()
        {
            await TestSession.StartAsync(async (testSession) =>
            {
                var auth1 = await testSession.CreateAuthorizationAsync();
                var credential = auth1.CredentialProviders[0];
                var auth2 = new Resources.AuthorizationPost()
                {
                    Id = Guid.NewGuid(),
                    CredentialProviders = new CredentialsType[] { credential },
                };
                await testSession.PostAsync<AuthorizationController>(auth2)
                    .AssertAsync(System.Net.HttpStatusCode.Conflict);
            });
        }
    }
}
