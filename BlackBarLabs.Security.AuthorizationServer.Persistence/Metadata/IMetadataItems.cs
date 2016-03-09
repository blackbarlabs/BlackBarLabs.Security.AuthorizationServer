using System;
using System.Threading.Tasks;

namespace NC2.CPM.AuthorizationServer.Persistence.Contracts.Metadata
{
    public interface IMetadataItems
    {
        Task<IMetadata> CreateAsync(string type, string method, string username, string provider, string token, string metaData);

        //Task<IUser> FindByProvider(string provider);

        Task<IMetadata> FindByIdAsync(Guid id);
    }
}
