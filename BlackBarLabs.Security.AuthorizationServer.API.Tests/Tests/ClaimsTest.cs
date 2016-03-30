using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BlackBarLabs.Api.Tests;
using BlackBarLabs.Security.AuthorizationServer.API.Controllers;
using System.Net;
using System.Linq;
using BlackBarLabs.Web;

namespace BlackBarLabs.Security.AuthorizationServer.API.Tests
{
    [TestClass]
    public class ClaimsTest
    {
        [TestMethod]
        public async Task ClaimsWorks()
        {
            await TestSession.StartAsync(async (testSession) =>
            {
                var type = "urn:example.com/Claim/type-abc";
                var value = Guid.NewGuid().ToString();

                var auth = await testSession.CreateAuthorizationAsync();
                await testSession.ClaimPostAsync(auth.Id, type, value)
                    .AssertAsync(HttpStatusCode.Created);
                
                var credential = await testSession.CreateCredentialVoucherAsync(auth.Id);
                var session = await testSession.CreateSessionWithCredentialsAsync(credential);

                var claims = session.SessionHeader.Value.GetClaimsJwtString();
                var thisClaim = claims.First(clm => String.Compare(clm.Type, type) == 0);
                Assert.AreEqual(value, thisClaim.Value);
            });
        }
    }
}
