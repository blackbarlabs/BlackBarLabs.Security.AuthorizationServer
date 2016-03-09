using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlackBarLabs.Security.AuthorizationServer.API.Tests
{
    [TestClass]
    public class RolesTest
    {
        [TestMethod]
        public async Task CreateAndCheckAdminRole()
        {
            CredentialProvider.Doubles.Stubs.StubCredentialProvider.ModifierDelegate credentialResponse =
                (provider, username, token) => Task.FromResult(Guid.NewGuid().ToString());

            var context = new Context(() =>
            {
                const string connectionStringKeyName = "Azure.Authorization.Storage";
                return new Persistence.Azure.DataContext(connectionStringKeyName);
            },
            (providerMethod) => new CredentialProvider.Doubles.Stubs.StubCredentialProvider((provider, username, token) =>
                credentialResponse(provider, username, token)));

            var userId = Guid.NewGuid().ToString();
            await context.Roles.CreateAsync(1, userId);
            var isAdmin = await context.Roles.IsUserAdmin(userId);
            Assert.IsTrue(isAdmin);
        }
    }
}
