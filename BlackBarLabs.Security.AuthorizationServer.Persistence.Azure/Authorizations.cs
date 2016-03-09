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

        private string GetRowKey(Uri providerId, string username)
        {
            var concatination = providerId.AbsoluteUri + username;
            var md5 = MD5.Create();
            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(concatination));

            var rowId = new Guid(data);
            var md5Hash = GetMd5Hash(md5, concatination).AsRowKey();
            return md5Hash;
        }

        public async Task<Guid> FindAuthId(Uri providerId, string username)
        {
            var md5Hash = GetRowKey(providerId, username);
            var result = await repository.FindById<Documents.AuthorizationCheck>(md5Hash);
            if (default(Documents.AuthorizationCheck) != result)
            {
                return result.AuthId;
            }
            return default(Guid);
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
            
            var creationTasks = createCredentialProviderDelegateCallback.Invoke(async (providerId, username) =>
            {
                var md5Hash = GetRowKey(providerId, username);
                var document = new Documents.AuthorizationCheck
                {
                    RowKey = md5Hash,
                    PartitionKey = md5Hash.GeneratePartitionKey(),
                    AuthId = authorizationId,
                };

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
    }
}