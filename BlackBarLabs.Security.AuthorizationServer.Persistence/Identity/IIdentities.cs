using System;
using System.Threading.Tasks;

namespace NC2.CPM.AuthorizationServer.Persistence.Contracts.Identity
{
    public interface IIDentities
    {
        Task<IIdentity> CreateAsync(string type, string method, string username, string provider, string token, string metaData);

        //Task<IUser> FindByProvider(string provider);

        Task<IIdentity> FindByIdAsync(Guid id);
    }
}
