using System;
using System.Threading.Tasks;

namespace BlackBarLabs.Security.AuthorizationServer.Persistence
{
    public interface ITokens
    {
        Task<bool> CreateAsync(string userId, string token);
        Task<string> RetrieveToken(string userId);
    }
}

