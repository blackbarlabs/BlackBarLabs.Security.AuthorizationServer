using System;
using System.Threading.Tasks;

namespace NC2.CPM.AuthorizationServer.Persistence.Contracts.Users
{
    public interface IUsers
    {
        Task<IUser> CreateAsync(string userId, string passwordHash, string passwordSalt);
        
        Task<IUser> FindByUserIdAsync(string userId);

        //Task<IUser> FindByIdAsync(Guid id);
    }
}
