using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using NC2.CPM.AuthorizationServer.Persistence.Contracts.Users;
using NC2.CPM.Security;
using NC2.CPM.Security.Claims;
using NC2.CPM.Security.Crypto;
using NC2.Datatypes;
using NC2.Datatypes.Extensions;
using NC2.Security.AuthorizationServer.Business.Entity;
using NC2.Security.AuthorizationServer.Business.MainContext;

namespace NC2.Security.AuthorizationServer.Business.Users
{
    public class UserCollection : EntityCollection<Users.User, IUser>
    {
        internal UserCollection(Context context) : base(context) { }

        public async Task<Users.User> FindByUserIdAsync(string userId)
        {
            var audience = await Context.DataContext.Users.FindByUserIdAsync(userId);
            return audience == null ? null : new Users.User(Context, audience);
        }

        public async Task<User> CreateAsync(string userId, string password)
        {
            Validator.Initialize()
                .Validate(userId.CheckArgumentIsNotNullOrEmptyRule("email"))
                .Validate(password.CheckArgumentIsNotNullOrEmptyRule("password"));

            var user = await Context.DataContext.Users.FindByUserIdAsync(userId);
            if (user != null)
                throw new ArgumentException("Value must be unique", "userId");

            var hash = CryptoTools.GenerateHash(password);
            return new Users.User(Context, await Context.DataContext.Users.CreateAsync(userId, hash.Hash, hash.Salt));
        }

        public async Task<User> CreateAsync(string email, string password, string salt)
        {
            Validator.Initialize()
                .Validate(email.CheckArgumentIsNotNullOrEmptyRule("email"))
                .Validate(password.CheckArgumentIsNotNullOrEmptyRule("password"))
                .Validate(salt.CheckArgumentIsNotNullOrEmptyRule("salt"))
                .Validate();
            var claimsPrincipal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            SecurityCheckpoint.Initialize()
                .Check(new AddUserToSystemClaimCheck(claimsPrincipal))
                .Check();
            var user = await Context.DataContext.Users.FindByUserIdAsync(email);
            if (user != null)
                throw new ArgumentException("Value must be unique", "email");
            var item = GetAuthenticationKeys();

            return new User(Context, await Context.DataContext.Users.CreateAsync(email, password, salt));

        }

        //public Task<IEnumerable<IUser>> FindByRoleStatusSearchAsync(UserRole? role,
        //    IEnumerable<UserStatus> statuses, string search)
        //{
        //    if (statuses == null || !statuses.Any())
        //    {
        //        var users = Context.DataContext.UserQueries.FindByRoleStatusSearchAsync(null, role, search).Result;
        //        return Task.FromResult( (IEnumerable<IUser>)users.Select(user => new User(Context, user)));
        //    }
        //    var bag = new ConcurrentBag<CPM.Persistence.Users.IUser>();
        //    // because a user can only have one status at a time, the results below will never contain duplicates
        //    Parallel.ForEach(statuses, status =>
        //    {
        //        var users = Context.DataContext.UserQueries.FindByRoleStatusSearchAsync(status, role, search).Result.ToList();
        //        users.ForEach(bag.Add);
        //    });
        //    return Task.FromResult( (IEnumerable<IUser>)bag.Select(user => new User(Context, user)));
        //}

        //#region Create

        //#endregion

        private static Tuple<string, string, string> GetAuthenticationKeys()
        {
            var @base = Guid.NewGuid().ToString();
            var hashes = CryptoTools.GenerateHash(@base);
            return Tuple.Create(@base, hashes.Hash, hashes.Salt);
        }

        public async Task<IUser> CreateFromSocialMediaProviderAsync(string email, string name, string key)
        {
            throw new NotImplementedException();
            //email.ValidateArgumentIsNotNullOrEmpty("email");
            //name.ValidateArgumentIsNotNullOrEmpty("name");
            //key.ValidateArgumentIsNotNullOrEmpty("key");
            //var claimsPrincipal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            //SecurityCheckpoint.Initialize()
            //    .Check(new AddUserToSystemClaimCheck(claimsPrincipal))
            //    .Check();
            //var user = await Context.DataContext.Users.FindByUserIdAsync(email); //TODO: Figure this out
            //if (user != null)
            //    throw new ArgumentException("Value must be unique", "email");

            //var item = GetAuthenticationKeys();
            //return (IUser)new User(Context, await Context.DataContext.UserQueries.CreateFromSocialMediaProviderAsync(email, name, key, item.Item1, item.Item2, item.Item3));
        }

        //public Task<IEnumerable<IUser>> SearchForUsersWithEmailOrLastNameContainingValue(string value)
        //{
        //    return Task.Run(async () =>
        //    {
        //        var items = await Context.DataContext.UserQueries.SearchForUsersWithEmailOrLastNameContainingValueAsync(value);
        //        return items.Select(item => (IUser) new User(Context, item));
        //    });
        //}

















    }
}
