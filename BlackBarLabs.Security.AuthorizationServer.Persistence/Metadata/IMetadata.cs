using System;
using System.Threading.Tasks;
using NC2.CPM.Persistence.Common.Azure;

namespace NC2.CPM.AuthorizationServer.Persistence.Contracts.Metadata
{
    public delegate void ExtrudeMetadataInformationDelegate(Guid id, string method, string username, string provider, string token, string metadataJson);

    public interface IMetadata : IPersistenceEntity
    {

        #region Properties
        Task ExtrudeInformationAsync(ExtrudeMetadataInformationDelegate @delegate);
        #endregion

        #region Actionables
        Task<bool> UpdateToken(string token);
        Task<bool> UpdateJson(string json);
        #endregion

    }
}
