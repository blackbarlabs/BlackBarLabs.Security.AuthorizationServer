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

        internal Uri[] GetExternalClaimsLocations()
        {
            return this.ExternalClaimsLocations
                .FromByteArray((bytes) => new Uri(System.Text.Encoding.UTF8.GetString(bytes)))
                .ToArray();
        }

        internal void SetExternalClaimsLocations(Uri[] externalClaimsLocations)
        {
            ExternalClaimsLocations = externalClaimsLocations.ToByteArray(
                (externalClaimsLocation) => System.Text.Encoding.UTF8.GetBytes(externalClaimsLocation.AbsoluteUri));
        }
    }
}
