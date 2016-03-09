using System;
using System.Threading.Tasks;
using NC2.Gameserver.Persistence.Contracts;

namespace NC2.CPM.AuthorizationServer.Persistence.Contracts.Clients
{
    public interface IClient : IEntity<Guid>
    {
        #region Properties
        string ClientId { get; }

        string Base64Secret { get; }

        string Name { get; }
        #endregion

        #region Actionables
        Task<bool> UpdateNameAsync(string clientName);
        #endregion
    }
}
