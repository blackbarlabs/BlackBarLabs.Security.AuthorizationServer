using System;
using System.Runtime.Serialization;

namespace BlackBarLabs.Security.AuthorizationServer.API.Models
{
    [DataContract]
    public class CredentialsType
    {
        #region Properties

        [DataMember]
        public CredentialValidationMethodTypes Method { get; set; }

        [DataMember]
        public Uri Provider { get; set; }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string Token { get; set; }

        #endregion

        public bool IsPopulated()
        {
            return
                (Provider != default(Uri)) &&
                (!String.IsNullOrWhiteSpace(UserId)) &&
                (!String.IsNullOrWhiteSpace(Token));
        }
    }
}