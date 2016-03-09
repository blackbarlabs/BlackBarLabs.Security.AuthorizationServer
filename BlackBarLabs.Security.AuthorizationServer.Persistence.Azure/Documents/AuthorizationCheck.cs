using System;

namespace BlackBarLabs.Security.AuthorizationServer.Persistence.Azure.Documents
{
    internal class AuthorizationCheck : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        #region Constructors
        public AuthorizationCheck() { }

        #endregion

        #region Properties
        
        public Guid AuthId { get; set; }

        #endregion
    }
}
