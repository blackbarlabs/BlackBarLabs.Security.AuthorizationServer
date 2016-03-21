using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BlackBarLabs.Persistence;
using BlackBarLabs.Persistence.Azure;
using BlackBarLabs.Persistence.Azure.StorageTables;

namespace BlackBarLabs.Security.AuthorizationServer.Persistence.Azure
{
    internal class Authorizations : IAuthorizations
    {
        private AzureStorageRepository repository;
        public Authorizations(AzureStorageRepository repository)
        {
            this.repository = repository;
        }

        private Guid GetRowKey(Uri providerId, string username)
        {
            var concatination = providerId.AbsoluteUri + username;
            var md5 = MD5.Create();
            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(concatination));

            var rowId = new Guid(data);
            var md5Hash = GetMd5Hash(md5, concatination);
            return md5Hash;
        }

        public async Task<T> FindAuthId<T>(Uri providerId, string username,
            Func<Guid, Uri[], T> onSuccess, Func<T> onFailure)
        {
            var authCheckId = GetRowKey(providerId, username);
            var result = await repository.FindByIdAsync(authCheckId,
                (Documents.AuthorizationCheck document) => onSuccess(document.AuthId, document.GetExternalClaimsLocations()),
                () => onFailure());
            return result;
        }

        public async Task<bool> DoesMatchAsync(Guid authorizationId, Uri providerId, string username)
        {
            var md5Hash = GetRowKey(providerId, username);
            var result = await repository.FindById<Documents.AuthorizationCheck>(md5Hash);
            if (default(Documents.AuthorizationCheck) != result)
            {
                return result.AuthId == authorizationId;
            }
            return false;
        }
        
        public async Task<T> CreateAuthorizationAsync<T>(Guid authorizationId, Func<T> onSuccess, Func<T> onAlreadyExist)
        {
            var authorizationDocument = new Documents.AuthorizationDocument()
            {
                RowKey = authorizationId.AsRowKey(),
                PartitionKey = authorizationId.AsRowKey().GeneratePartitionKey(),
            };

            return await repository.CreateAsync(authorizationId, authorizationDocument,
                () => onSuccess(),
                () => onAlreadyExist());
        }

        public async Task<bool> CreateAuthorizationAsync<T>(Guid authorizationId,
            Func<CredentialProviderDelegate, IEnumerable<Task<T>>> createCredentialProviderDelegateCallback,
            Func<IEnumerable<T>, bool> validateResultsCallback)
        {
            var authorizationDocument = new Documents.AuthorizationDocument()
            {
                RowKey = authorizationId.AsRowKey(),
                PartitionKey = authorizationId.AsRowKey().GeneratePartitionKey(),
            };

            if (!await repository.CreateAtomicAsync<Documents.AuthorizationDocument>(authorizationId, () =>
            {
                return Task.FromResult(authorizationDocument);
            }))
            {
                throw new ResourceAlreadyExistsException();
            }

            // Collect a list of working hashes so the save fails it can clean up after itself at the end of the process
            var hashes = new System.Collections.Concurrent.ConcurrentBag<Documents.AuthorizationCheck>();
            
            var creationTasks = createCredentialProviderDelegateCallback.Invoke(async (providerId, username, externalClaimsLocations) =>
            {
                var md5Hash = GetRowKey(providerId, username);
                var document = new Documents.AuthorizationCheck
                {
                    RowKey = md5Hash.AsRowKey(),
                    PartitionKey = md5Hash.AsRowKey().GeneratePartitionKey(),
                    AuthId = authorizationId,
                };
                document.SetExternalClaimsLocations(externalClaimsLocations);

                try
                {
                    await repository.CreateAsync(document);
                    hashes.Add(document);
                }
                catch (Microsoft.WindowsAzure.Storage.StorageException ex)
                {
                    if (ex.IsProblemResourceAlreadyExists())
                        return false;
                    throw;
                }
                return true;
            });

            
            // Check if all task succeeded
            var creationSuccesses = await Task.WhenAll(creationTasks);
            if (validateResultsCallback(creationSuccesses))
                return true;

            // If calling method rejected the results, cleanup
            var cleanupTasks = hashes.Select(async (doc) =>
            {
                return await repository.DeleteAsync<Documents.AuthorizationCheck>(doc, (doc1, doc2) => true);
            });
            
            // Wait until the cleanup completes to remove the authorization document.
            // In other words, keep the lock until after the cleanup.
            await Task.WhenAll(cleanupTasks);
            await repository.DeleteAsync(authorizationDocument, (authDoc1, authDoc2) => true);
            return false;
        }
        

        static Guid GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            return new Guid(data);
        }

        public async Task<TResult> CreateCredentialProviderAsync<TResult>(Guid authorizationId, Uri providerId, string username, Uri[] claimsProviders,
            Func<TResult> success, Func<TResult> authorizationDoesNotExists, Func<Guid, TResult> alreadyAssociated)
        {
            return await await repository.FindByIdAsync(authorizationId,
                async (Documents.AuthorizationDocument authorizationStored) =>
                {
                    var authorizationCheckId = GetRowKey(providerId, username);
                    var authorizationDocument = new Documents.AuthorizationCheck
                    {
                        AuthId = authorizationId,
                    };
                    return await await await repository.CreateAsync(authorizationCheckId, authorizationDocument,
                        () => Task.FromResult(Task.FromResult(success())),
                        () =>
                        {
                            return repository.FindByIdAsync(authorizationCheckId,
                                (Documents.AuthorizationCheck authorizationCheckDocument) =>
                                    Task.FromResult(alreadyAssociated(authorizationCheckDocument.AuthId)),
                                () => CreateCredentialProviderAsync(authorizationId, providerId, username, claimsProviders,
                                        success, authorizationDoesNotExists, alreadyAssociated));
                        });
                },
                () => Task.FromResult(authorizationDoesNotExists()));
        }
        
    }
}