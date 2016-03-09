using System;
using System.Threading.Tasks;
using NC2.Security.AuthorizationServer.API.Controllers;
using NC2.Security.AuthorizationServer.Business.Audience;

namespace NC2.Security.AuthorizationServer.API
{
    public class AudienceStore: BaseController
    {
        internal async Task<Audience> FindAudienceAsync(string audienceId)
        {
            var audienceGuid = Guid.Parse(audienceId);
            return audienceGuid == default(Guid) ? null : await DataContext.AudienceCollection.FindByClientIdAsync(audienceGuid);
        }

        internal Audience FindAudience(string audienceId)
        {
            var audienceGuid = Guid.Parse(audienceId);
            return audienceGuid == default(Guid) ? null : DataContext.AudienceCollection.FindByClientId(audienceGuid);
        }

        internal Audience FindByIssuer(string issuer)
        {
            return DataContext.AudienceCollection.FindByName(issuer);
        }
    }
}