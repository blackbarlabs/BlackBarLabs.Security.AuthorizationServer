using System;
using System.Threading.Tasks;

namespace BlackBarLabs.Security.CredentialProvider.Doubles.Stubs
{
    public class StubCredentialProvider : IProvideCredentials
    {
        public delegate Task<string> ModifierDelegate(Uri providerId, string username, string token);

        private ModifierDelegate modifierDelegate = null;

        public StubCredentialProvider(ModifierDelegate modifierDelegate)
        {
            this.modifierDelegate = modifierDelegate;
        }

        public Task<string> RedeemTokenAsync(Uri providerId, string username, string token)
        {
            return modifierDelegate.Invoke(providerId, username, token);
        }
    }
}
