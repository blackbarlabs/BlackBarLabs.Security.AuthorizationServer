using System;
using System.Linq;
using System.Runtime.Serialization;
using BlackBarLabs.Security.Authorization;

namespace BlackBarLabs.Security.AuthorizationServer.API.Resources
{
    public class Authorization : Resource, IAuthorization
    {
        #region Properties

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public CredentialsType[] CredentialProviders { get; set; }

        #endregion
        
        protected bool HasCredentials()
        {
            return
                this.CredentialProviders != null &&
                this.CredentialProviders.Any();
        }
    }
}
