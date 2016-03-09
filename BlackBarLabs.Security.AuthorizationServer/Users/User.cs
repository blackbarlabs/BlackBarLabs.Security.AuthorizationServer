using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NC2.Constants;
using NC2.CPM.AuthorizationServer.Persistence.Contracts.Users;
using NC2.CPM.AuthorizationServer.Persistence.Contracts.Users.SocialMediaAccounts;
using NC2.Security.AuthorizationServer.Business.Entity;
using NC2.Security.AuthorizationServer.Business.MainContext;

namespace NC2.Security.AuthorizationServer.Business.Users
{
    public delegate void ExtrudeUserInformationDelegate(string userId, string passwordHash, string passwordSalt);

    public class User : Entity<IUser>
    {
        internal User(Context context, IUser data) : base(context, data) { }

        public async Task ExtrudeUserInformationAsync(ExtrudeUserInformationDelegate @delegate)
        {
            @delegate(await Data.UserIdAsync, await Data.PasswordHashAsync, await Data.PasswordSaltAsync);
        }

        private Task<IEnumerable<ISocialMediaAccount>> socialMediaAccounts;

        public Task<IEnumerable<ISocialMediaAccount>> SocialMediaAccountsAsync
        {
            get { return socialMediaAccounts ?? (socialMediaAccounts = GetSocialMediaAccountsAsync()); }
        }

        private async Task<IEnumerable<ISocialMediaAccount>> GetSocialMediaAccountsAsync()
        {
            return (await SocialMediaAccountsInternalAsync).Select(item => (ISocialMediaAccount)item);
        }

        private Task<IEnumerable<ISocialMediaAccount>> socialMediaAccountsInternal;

        private Task<IEnumerable<ISocialMediaAccount>> SocialMediaAccountsInternalAsync
        {
            get { return socialMediaAccountsInternal ?? (socialMediaAccountsInternal = GetSocialMediaAccountsInternalAsync()); }
        }

        private async Task<IEnumerable<ISocialMediaAccount>> GetSocialMediaAccountsInternalAsync()
        {
            throw new NotImplementedException();
            //return (await Data.SocialMediaAccountsAsync).Select(item => new SocialMediaAccount(Context, item));
        }


        public async Task<bool> DeleteAsync()
        {
            await Data.UpdateEntityStateAsync(EntityState.Retired);
            return true;
        }

        #region Actionables
        public Task<bool> UpdatePasswordAsync(string hash, string salt)
        {
            return Data.UpdatePasswordAsync(hash, salt);
        }
        #endregion
    }
}
