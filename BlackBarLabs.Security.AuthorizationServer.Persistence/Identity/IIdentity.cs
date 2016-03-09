using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NC2.Constants;
using NC2.CPM.AuthorizationServer.Persistence.Contracts.Metadata;
using NC2.CPM.Persistence.Common.Azure;

namespace NC2.CPM.AuthorizationServer.Persistence.Contracts.Identity
{
    public delegate void ExtrudeStageInformationDelegate(Guid gameFlowId, int order, DateTime? unlockDate, TimeSpan? unlockDuration,
        string unlockConditionsJson);

    public interface IIdentity : IPersistenceEntity
    {
        #region Properties
        Task ExtrudeInformationAsync(ExtrudeStageInformationDelegate @delegate);

        Task<IEnumerable<Guid>> MetadataIdsAsync { get; }
        Task<IEnumerable<IMetadata>> MetadatasAsync { get; }
        #endregion

        #region GameFlowSteps

        Task<IMetadata> FindMetadataByIdAsync(Guid stageId);

        Task<IMetadata> CreateMetadataAsync(Guid id, Guid gameId, GameStepPlayType gameStepPlayType);

        Task<bool> RemoveStepAsync(Guid stageId);

        #endregion

        #region Mutators

        Task<bool> DeleteAsync();
        Task<bool> UpdateInformationAsync(int order, DateTime? unlockDate, TimeSpan? unlockDateDuration, string unlockConditionJson);

        #endregion
    }
}