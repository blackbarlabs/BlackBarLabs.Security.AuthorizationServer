using System;
using System.Threading.Tasks;

namespace NC2.CPM.AuthorizationServer.Persistence.Contracts.Users.SocialMediaAccounts
{
    public interface ISocialMediaAccounts
    {
        ISocialMediaAccount Create(string name, string key);

        Task<ISocialMediaAccount> FindByIdAsync(Guid id);
    }
}