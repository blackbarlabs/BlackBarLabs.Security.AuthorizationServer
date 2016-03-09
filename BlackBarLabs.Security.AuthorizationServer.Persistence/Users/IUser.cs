using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NC2.Constants;
//using NC2.CPM.AuthorizationServer.Persistence.Contracts.Users.SocialMediaAccounts;
using NC2.CPM.Persistence.Common.Azure;

namespace NC2.CPM.AuthorizationServer.Persistence.Contracts.Users
{
    public interface IUser : IPersistenceEntity
    {
        #region Properties
        Task<string> UserIdAsync { get; }

        Task<string> PasswordHashAsync { get; }

        Task<string> PasswordSaltAsync { get; }

        #endregion  

        #region Actionables
        Task<bool> UpdatePasswordAsync(string passwordHash, string passwordSalt);

        Task<IUser> CreateFromSocialMediaProviderAsync(string email, string name, string key, string baseKey,
                                                            string publicKey, string privateKey);

        #endregion

        //#region SocialMediaAccounts
        //Task<IEnumerable<ISocialMediaAccount>> SocialMediaAccountsAsync { get; }

        //Task<string> BaseKeyAsync { get; }

        //Task<string> PrivateKeyAsync { get; }

        //Task<ISocialMediaAccount> AddSocialMediaAccountAsync(SocialMediaProvider provider, string key);

        //Task<bool> RemoveSocialMediaAccountAsync(Guid id);
        //#endregion
    }
}
