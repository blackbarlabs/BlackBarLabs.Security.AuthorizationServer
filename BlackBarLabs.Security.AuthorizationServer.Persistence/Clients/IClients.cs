using System;
using System.Threading.Tasks;

namespace NC2.CPM.AuthorizationServer.Persistence.Contracts.Clients
{
    public interface IClients
    {
        Task<IClient> CreateAsync(string clientId, string base64Secret, string name);

        Task<IClient> FindByIdAsync(Guid id);

        Task<IClient> FindByClientId(Guid clientId);
    }
}
