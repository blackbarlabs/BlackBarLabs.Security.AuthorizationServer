using System;
using System.Linq;
using BlackBarLabs.Persistence.Azure;

namespace BlackBarLabs.Security.AuthorizationServer.Persistence.Azure.Documents
{
    internal class ClaimDocument : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        #region Properties
        
        public Guid ClaimId { get; set; }
        public string Issuer { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        #endregion
    }
}
