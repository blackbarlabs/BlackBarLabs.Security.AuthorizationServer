using System;
using JoshCodes.Persistence.Azure.Storage;

namespace NC2.CPM.AuthorizationServer.Persistence.Clients
{
    internal class ClientEntity : Entity
    {
        #region Constructors

        public ClientEntity()
        {
        }

        public ClientEntity(Guid id)
            : base(id, DateTime.UtcNow)
        {
        }
        #endregion

        #region Properties

        public string ClientId { get; set; }

        public string Base64Secret { get; set; }

        public string Name { get; set; }

        #endregion

    }
}
