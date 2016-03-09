using BlackBarLabs.Security.AuthorizationServer.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace BlackBarLabs.Security.AuthorizationServer.API.Resources
{
    public class Session : Resource
    {

        public class AuthHeaderProps
        {
            #region Properties
            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public string Value { get; set; }
            #endregion
        }


        #region Properties

        [DataMember]
        public Guid Id { get; set; }

        [IgnoreDataMember]
        public Guid AuthorizationId { get; set; }

        [DataMember]
        public AuthHeaderProps SessionHeader { get; set; }

        [DataMember]
        public CredentialsType Credentials { get; set; }

        [DataMember]
        public string RefreshToken { get; set; }

        #endregion

        protected bool IsCredentialsPopulated()
        {
            if (default(CredentialsType) == Credentials)
                return false;
            return this.Credentials.IsPopulated();
        }
    }
}