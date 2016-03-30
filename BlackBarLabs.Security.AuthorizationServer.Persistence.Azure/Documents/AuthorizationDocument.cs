using BlackBarLabs.Collections.Async;
using System;
using System.Linq;
using BlackBarLabs.Persistence.Azure;
using BlackBarLabs.Persistence.Azure.StorageTables;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BlackBarLabs.Security.AuthorizationServer.Persistence.Azure.Documents
{
    internal class AuthorizationDocument : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        #region Constructors

        public AuthorizationDocument() { }

        internal IEnumerableAsync<ClaimDelegate> GetClaims(AzureStorageRepository repository)
        {
            return EnumerableAsync.YieldAsync<ClaimDelegate>(
                async (yieldAsync) =>
                {
                    var claimDocumentIds = Claims.ToGuidsFromByteArray();
                    foreach (var claimDocumentId in claimDocumentIds)
                    {
                        await repository.FindByIdAsync(claimDocumentId,
                            async (ClaimDocument claimsDoc) => await yieldAsync(claimsDoc.ClaimId, new Uri(claimsDoc.Issuer), new Uri(claimsDoc.Type), claimsDoc.Value),
                            () => Task.FromResult(true));
                    }
                });
        }

        #endregion

        #region Properties

        public byte [] Claims { get; set; }

        // TODO: Make concurrency safe
        internal async Task<IEnumerable<Guid>> AddClaimsAsync(List<ClaimDocument> claimsDocs, AzureStorageRepository repository)
        {
            var saveTasks = claimsDocs.Select(
                async (claimsDoc) =>
                {
                    return await repository.CreateAsync(claimsDoc.ClaimId, claimsDoc,
                        () => claimsDoc.ClaimId,
                        () => Guid.Empty);
                });
            return await Task.WhenAll(saveTasks);
        }

        #endregion

    }
}
