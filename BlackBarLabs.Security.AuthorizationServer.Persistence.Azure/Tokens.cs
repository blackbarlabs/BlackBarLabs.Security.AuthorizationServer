using System;
using System.Threading.Tasks;
using BlackBarLabs.Persistence.Azure.StorageTables;
using BlackBarLabs.Security.AuthorizationServer.Persistence.Azure.Documents;

namespace BlackBarLabs.Security.AuthorizationServer.Persistence.Azure
{
    internal class Tokens : Persistence.ITokens
    {
        private AzureStorageRepository repository;
        public Tokens(AzureStorageRepository repository)
        {
            this.repository = repository;
        }
        
        public async Task<bool> CreateAsync(string userId, string token)
        {
            var saved = await repository.CreateOrUpdateAtomicAsync<TokenDocument>(userId, (document) =>
            {
                document.Token = token;
                return Task.FromResult(document);
            });
            return saved;
        }

        public async Task<string> RetrieveToken(string userId)
        {
            var tokenDocument = await repository.FindById<Documents.TokenDocument>(userId);
            if (tokenDocument != null)
            {
                return tokenDocument.Token;
            }
            return string.Empty;
        }
        
    }
}
