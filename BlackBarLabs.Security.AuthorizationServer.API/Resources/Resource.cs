using BlackBarLabs.Security.AuthorizationServer.Persistence.Azure;
using BlackBarLabs.Security.CredentialProvider.ImplicitCreation;

namespace BlackBarLabs.Security.AuthorizationServer.API.Resources
{
    public class Resource : BlackBarLabs.Api.Resource
    {
        private static readonly object DataContextLock = new object();

        private Context context;

        protected Context Context
        {
            get
            {
                if (default(Context) == context)
                {
                    lock (DataContextLock)
                    {
                        context = GetContext();
                    }
                }

                return context;
            }
        }

        private static Context GetContext()
        {
            var context = new Context(() => new DataContext("Azure.Authorization.Storage"),
                (credentialValidationMethodType) =>
                {
                    switch (credentialValidationMethodType)
                    {
                        case CredentialValidationMethodTypes.Facebook:
                            return new Security.CredentialProvider.Facebook.FacebookCredentialProvider();
                        case CredentialValidationMethodTypes.Implicit:
                            return new ImplicitlyCreatedCredentialProvider();
                        default:
                            break;
                    }
                    return new Security.CredentialProvider.OpenIdConnect.OpenIdConnectCredentialProvider();
                });
            return context;
        }
    }
}