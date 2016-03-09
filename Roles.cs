using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BlackBarLabs.Security.AuthorizationServer
{
    public class Roles
    {
        private enum RoleTypes
        {
            SuperAdmin
        }
        private Context context;
        public delegate void ReturnedUserTokenDelegate(Guid userToken);
        private Persistence.IDataContext dataContext;

        internal Roles(Context context, Persistence.IDataContext dataContext)
        {
            this.dataContext = dataContext;
            this.context = context;
        }

        public async Task<bool> CreateAsync(int roleType, string userId)
        {
            return await dataContext.Roles.CreateAsync((int) RoleTypes.SuperAdmin, userId);
        }


        public async Task<bool> IsUserAdmin(string userId)
        {
            return await dataContext.Roles.CheckRoleType(userId, (int) RoleTypes.SuperAdmin);
        }
    }
}
