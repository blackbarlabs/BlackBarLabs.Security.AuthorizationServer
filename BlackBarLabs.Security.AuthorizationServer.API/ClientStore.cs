using System;
using NC2.Security.AuthorizationServer.API.Controllers;
using NC2.Security.AuthorizationServer.Business.Clients;

namespace NC2.Security.AuthorizationServer.API
{
    public class ClientStore: BaseController
    {
        internal Client FindClient(string clienId)
        {
            var clientGuid = new Guid();
            clientGuid = Guid.Parse(clienId);
            return clientGuid != default(Guid) ? DataContext.ClientCollection.FindByIdAsync(clientGuid).Result : null;
        }
    }
}