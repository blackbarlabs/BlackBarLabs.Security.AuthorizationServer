using System;
using System.Linq;
using System.Threading.Tasks;
using JoshCodes.Persistence.Azure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NC2.CPM.AuthorizationServer.Persistence.Contracts.Clients;

namespace NC2.CPM.AuthorizationServer.Persistence.Clients
{
    internal class Clients : AzureObjectStore<IClient, Client, ClientEntity>, IClients
    {
        #region Constructors

        internal CloudTableClient AzureTablesContext;
        internal Func<ClientEntity, Client> CreateFunc;

        internal Clients(CloudTableClient azureTablesContext) :
            base(azureTablesContext)
        {
            AzureTablesContext = azureTablesContext;
        }


        protected override Client CreateObjectStore(ClientEntity entity)
        {
            return CreateFunc.Invoke(entity);
        }

        #endregion

        #region Actionables

        public async Task<IClient> CreateAsync(string clientId, string base64Secret, string name)
        {
            var entity = new ClientEntity(Guid.NewGuid())
            {
                ClientId = clientId,
                Base64Secret = base64Secret,
                Name = name
            };

            await CreateAsync(entity);
            var newClient = new Client(entity, _tableClient);
            return newClient;
        }

        public Task<IClient> FindByIdAsync(Guid rowKey)
        {
            var where1 = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey.ToString().Replace("-",""));

            var query = Query.Where(where1);

            var gameEntity = Table.ExecuteQuery(query).AsEnumerable().FirstOrDefault();

            if (gameEntity != null)
            {
                IClient newClient = new Client(gameEntity, _tableClient);
                return Task.FromResult(newClient);
            }
            return Task.FromResult<IClient>(null);
        }

        public Task<IClient> FindByClientId(Guid clientId)
        {
            var where1 = TableQuery.GenerateFilterConditionForGuid("ClientId", QueryComparisons.Equal, clientId);

            var query = Query.Where(where1);

            var gameEntity = Table.ExecuteQuery(query).AsEnumerable().FirstOrDefault();

            if (gameEntity != null)
            {
                IClient newClient = new Client(gameEntity, _tableClient);
                return Task.FromResult(newClient);
            }
            return Task.FromResult<IClient>(null);
        }


        #endregion
    }
}
