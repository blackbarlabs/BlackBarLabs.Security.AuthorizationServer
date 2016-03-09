using System.Threading.Tasks;
using NC2.Constants;
using NC2.CPM.Persistence.Common.Azure;

namespace NC2.CPM.AuthorizationServer.Persistence.Contracts.Users.SocialMediaAccounts
{
    public interface ISocialMediaAccount : IPersistenceEntity
    {
        Task<SocialMediaProvider> ProviderAsync { get; }
        Task<string> KeyAsync { get; }
    }
}