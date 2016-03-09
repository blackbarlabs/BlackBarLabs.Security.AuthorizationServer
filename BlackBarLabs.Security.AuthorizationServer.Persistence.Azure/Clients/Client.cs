using System;
using System.Threading.Tasks;
using JoshCodes.Persistence.Azure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NC2.CPM.AuthorizationServer.Persistence.Contracts.Clients;

namespace NC2.CPM.AuthorizationServer.Persistence.Clients
{
    internal class Client : AzureObjectWrapper<ClientEntity>, IClient
    {
        private readonly Task<ClientEntity> advertiserEntityAsync;
        private ClientEntity entity;

        public Client(Task<ClientEntity> advertiserEntityAsync, CloudTableClient tableClient)
            : base(advertiserEntityAsync.Result, tableClient)
        {
            this.advertiserEntityAsync = advertiserEntityAsync;
        }

        public Client(ClientEntity entity, CloudTableClient tableClient)
            : base(entity, tableClient)
        {
            this.entity = entity;
        }

        private ClientEntity Entity
        {
            get { return entity ?? (entity = advertiserEntityAsync.Result); }
        }

        #region Properties

        public Guid Id
        {
            get { return Guid.Parse(Entity.RowKey); }
        }

        public string ClientId
        {
            get { return Entity.ClientId; }
        }

        public string Base64Secret
        {
            get { return Entity.Base64Secret; }
        }

        public string Name
        {
            get { return Entity.Name; }
        }

        #endregion

        #region Actionables
        public Task<bool> UpdateNameAsync(string clientName)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
