using System;
using System.Linq;
using BlackBarLabs.Persistence.Azure;

namespace BlackBarLabs.Security.AuthorizationServer.Persistence.Azure.Documents
{
    internal class AuthorizationCheck : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        #region Constructors
        public AuthorizationCheck() { }

        #endregion

        #region Properties
        
        public Guid AuthId { get; set; }

        public byte[] ExternalClaimsLocations { get; set; }

        #endregion
        
    }
}
