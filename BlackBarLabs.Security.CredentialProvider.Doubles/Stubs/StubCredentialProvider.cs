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

        public async Task<TResult> RedeemTokenAsync<TResult>(Uri providerId, string username, string token,
            Func<string, TResult> success, Func<TResult> invalidCredentials, Func<TResult> couldNotConnect)
        {
            var result = await modifierDelegate.Invoke(providerId, username, token);
            if (default(string) == result)
                return invalidCredentials();
            return success(result);
        }
    }
}
