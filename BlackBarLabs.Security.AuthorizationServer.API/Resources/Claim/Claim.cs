using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BlackBarLabs.Security.AuthorizationServer.API.Resources
{
    [DataContract]
    public class Claim : Resource, Security.Authorization.IClaim
    {
        public Guid Id { get; set; }

        public Guid AuthorizationId { get; set; }

        public Uri Issuer { get; set; }

        public Uri Type { get; set; }

        public string Value { get; set; }

        public string Signature { get; set; }
    }
}
