using System;

namespace BlackBarLabs.Security.AuthorizationServer.Persistence.Azure.Documents
{
    [Serializable]
    internal class TokenDocument : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        public string Token { get; set; }
    }
}
