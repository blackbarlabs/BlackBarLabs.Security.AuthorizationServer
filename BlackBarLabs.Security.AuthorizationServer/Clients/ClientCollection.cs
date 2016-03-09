using System;
using System.Threading.Tasks;
using NC2.Security.AuthorizationServer.Business.MainContext;

namespace NC2.Security.AuthorizationServer.Business.Clients
{
    public class ClientCollection
    {
        protected Context Context { get; private set; }

        internal ClientCollection(Context context)
        {
            Context = context;
        }

        public Task<Client> FindByIdAsync(Guid id)
        {
            return Task.FromResult(new Client(Context, Context.DataContext, () => Context.DataContext.Clients.FindByIdAsync(id).Result));
        }

        public Client CreateAsync(string base64Secret, string name)
        {
            var game = Context.DataContext.Clients.CreateAsync(Guid.NewGuid().ToString(), base64Secret, name).Result;
            return new Client(Context, Context.DataContext, () => game);
        }
    }
}
