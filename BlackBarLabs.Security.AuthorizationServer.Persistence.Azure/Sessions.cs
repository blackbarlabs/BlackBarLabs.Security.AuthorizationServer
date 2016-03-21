using System;
using System.Threading.Tasks;
using BlackBarLabs.Persistence.Azure;
using BlackBarLabs.Persistence.Azure.StorageTables;
using BlackBarLabs.Persistence;

namespace BlackBarLabs.Security.AuthorizationServer.Persistence.Azure
{
    internal class Sessions : Persistence.ISessions
    {
        private AzureStorageRepository repository;
        public Sessions(AzureStorageRepository repository)
        {
            this.repository = repository;
        }

        #region Actionables

        public async Task CreateAsync(Guid sessionId, string refreshToken, Guid authorizationId = default(Guid))
        {
            var document = new Documents.SessionDocument()
            {
                RowKey = sessionId.AsRowKey(),
                PartitionKey = sessionId.AsRowKey().GeneratePartitionKey(),
                SessionId = sessionId,
                RefreshToken = refreshToken,
                AuthorizationId = authorizationId,
            };
            try
            {
                await repository.CreateAsync(document);
            } catch(Microsoft.WindowsAzure.Storage.StorageException)
            {
                throw new BlackBarLabs.Persistence.ResourceAlreadyExistsException();
            }
        }

        public async Task<bool> DoesExistsAsync(Guid sessionId)
        {
            var sessionDocument = await repository.FindById<Documents.SessionDocument>(sessionId);
            return null != sessionDocument;
        }

        #endregion


        public async Task UpdateAuthentication(Guid sessionId, Func<Guid, Guid> authIdFunc)
        {
            await repository.UpdateAtomicAsync<Documents.SessionDocument>(sessionId, (sessionDoc) =>
                {
                    sessionDoc.AuthorizationId = authIdFunc.Invoke(sessionDoc.AuthorizationId);
                    return sessionDoc;
                });
        }

        public async Task<TResult> UpdateAuthentication<TResult>(Guid sessionId,
            UpdateAuthenticationDelegate<TResult> authIdFunc,
            Func<TResult> notFound)
        {
            var result = await repository.UpdateAsync<Documents.SessionDocument, TResult>(sessionId,
                (sessionDoc, onSave) =>
                {
                    return authIdFunc.Invoke(sessionDoc.AuthorizationId, (updatedAuthId) =>
                    {
                        sessionDoc.AuthorizationId = updatedAuthId;
                        onSave(sessionDoc);
                    });
                },
                () => notFound());
            return result;
        }
    }
}
