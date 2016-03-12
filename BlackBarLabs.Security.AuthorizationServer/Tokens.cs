using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BlackBarLabs.Security.AuthorizationServer
{
    public class Tokens
    {
        private Context context;
        public delegate void ReturnedUserTokenDelegate(Guid userToken);
        private Persistence.IDataContext dataContext;

        internal Tokens(Context context, Persistence.IDataContext dataContext)
        {
            this.dataContext = dataContext;
            this.context = context;
        }

        public async Task<bool> CreateAsync(string userId, string token)
        {
            return await dataContext.Tokens.CreateAsync(userId, token);
        }


        public async Task<string> RetrieveToken(string userId)
        {
            return await dataContext.Tokens.RetrieveToken(userId);
        }
    }
}
