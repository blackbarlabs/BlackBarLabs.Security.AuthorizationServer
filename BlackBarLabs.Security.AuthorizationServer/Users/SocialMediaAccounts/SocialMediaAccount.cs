using System.Threading.Tasks;
using NC2.Constants;
using NC2.CPM.AuthorizationServer.Persistence.Contracts.Users.SocialMediaAccounts;
using NC2.Security.AuthorizationServer.Business.Entity;
using NC2.Security.AuthorizationServer.Business.MainContext;

namespace NC2.Security.AuthorizationServer.Business.Users.SocialMediaAccounts
{
    class SocialMediaAccount : Entity<ISocialMediaAccount>, Contracts.ISocialMediaAccount
    {
        public SocialMediaAccount(Context context, ISocialMediaAccount data) : base(context, data)
        {
        }

        public Task<SocialMediaProvider> ProviderAsync
        {
            get { return GetProviderAsync(); }
        }
        private async Task<SocialMediaProvider> GetProviderAsync()
        {
            return (await Data.ProviderAsync);
        }

        public Task<string> KeyAsync
        {
            get { return GetKeyAsync(); }
        }
        private async Task<string> GetKeyAsync()
        {
            return (await Data.KeyAsync);
        }
    }
}
