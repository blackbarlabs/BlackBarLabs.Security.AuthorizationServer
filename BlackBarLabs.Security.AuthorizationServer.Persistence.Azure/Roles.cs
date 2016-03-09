using System;
using System.Threading.Tasks;
using BlackBarLabs.Persistence.Azure;
using BlackBarLabs.Persistence.Azure.StorageTables;

namespace BlackBarLabs.Security.AuthorizationServer.Persistence.Azure
{
    internal class Roles : Persistence.IRoles
    {
        private AzureStorageRepository repository;
        public Roles(AzureStorageRepository repository)
        {
            this.repository = repository;
        }

        #region Actionables

        public async Task<bool> CreateAsync(int roleType, string userId)
        {
            var id = roleType.ToString() + userId;
            var document = new Documents.RoleDocument()
            {
                RowKey = id,
                PartitionKey = id.GeneratePartitionKey(),
            };
            await repository.CreateAsync(document);
            return true;
        }

        public async Task<bool> CheckRoleType(string userId, int roleType)
        {
            var id = roleType.ToString() + userId;
            var sessionDocument = await repository.FindById<Documents.RoleDocument>(id);
            return null != sessionDocument;
        }

        #endregion
        
    }
}
