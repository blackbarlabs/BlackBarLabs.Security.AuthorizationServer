using System;
using System.Threading.Tasks;

namespace BlackBarLabs.Security.AuthorizationServer.Persistence
{
    public interface IRoles
    {
        Task<bool> CreateAsync(int roleType, string userId);
        Task<bool> CheckRoleType(string userId, int roleType);
    }
}

