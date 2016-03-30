using System;
using System.Threading.Tasks;
using BlackBarLabs.Security.AuthorizationServer.Exceptions;
using BlackBarLabs.Security.Authorization;
using System.Configuration;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using BlackBarLabs.Core.Web;
using BlackBarLabs.Collections.Async;

namespace BlackBarLabs.Security.AuthorizationServer
{
    public class Claims
    {
        private Context context;
        private Persistence.IDataContext dataContext;

        internal Claims(Context context, Persistence.IDataContext dataContext)
        {
            this.dataContext = dataContext;
            this.context = context;
        }

        public async Task<TResult> CreateAsync<TResult>(Guid claimId,
            Guid authorizationId, Uri issuer, Uri type, string value, string signature,
            Func<TResult> success,
            Func<TResult> authorizationNotFound,
            Func<TResult> alreadyExist,
            Func<string, TResult> failure)
        {
            return await await this.dataContext.Authorizations.UpdateClaims(authorizationId,
                async (claimsStored, addClaim) =>
                {
                    bool existingClaimFound = false;
                    await claimsStored.ForAllAsync(
                        async (claimIdStorage, issuerStorage, typeStorage, valueStorage) =>
                        {
                            if (claimIdStorage == claimId)
                                existingClaimFound = true;
                            await Task.FromResult(true);
                        });
                    if (existingClaimFound)
                        return alreadyExist();

                    addClaim(claimId, issuer, type, value);

                    return success();
                },
                () => Task.FromResult(authorizationNotFound()),
                (whyFailed) => Task.FromResult(failure(whyFailed)));
        }
    }
}
