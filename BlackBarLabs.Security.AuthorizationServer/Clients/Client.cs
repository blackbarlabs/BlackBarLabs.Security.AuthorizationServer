using System;
using NC2.CPM.AuthorizationServer.Persistence.Contracts.Clients;
using NC2.CPM.AuthorizationServer.Persistence.Contracts.Context;
using NC2.Security.AuthorizationServer.Business.Entity;
using NC2.Security.AuthorizationServer.Business.MainContext;

namespace NC2.Security.AuthorizationServer.Business.Clients
{
    public class Client : Entity<IClient>
    {
        internal Client(Context context, IDataContext dataContext, Func<IClient> dataFetchFunc) :
            base(context, dataContext, dataFetchFunc)
        {
        }

        #region Properties
        public string ClientId
        {
            get { return Data.ClientId; }
        }

        public string Base64Secret
        {
            get { return Data.Base64Secret; }
        }

        public string Name
        {
            get { return Data.Name; }
        }
        #endregion

        #region Actionables
        public System.Threading.Tasks.Task<bool> UpdateNameAsync(string clientName)
        {
            return Data.UpdateNameAsync(clientName);
        }
        #endregion

    }
}
