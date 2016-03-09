using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using NC2.CPM.AuthorizationServer.Persistence.Contracts.Users;

namespace NC2.CPM.AuthorizationServer.Persistence.Metadata
{
    internal class MetadataItems : CPM.Persistence.Common.Azure.AzureStorageTables.Entities, IUsers
    {
        #region Constructors

        internal Users(CPM.Persistence.Common.Azure.DataContext dataContext) :
            base(dataContext)
        {
        }

        #endregion

        #region Actionables

        public async Task<IUser> CreateAsync(string userId, string passwordHash, string passwordSalt)
        {
            var entity = new UserEntity(Guid.NewGuid())
            {
                UserId = userId,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            var document = await DataContext.DataNexus.AtsRepository.CreateAsync(entity);
            return new User(DataContext, document);
        }

        public Task<IUser> FindByUserIdAsync(string userId)
        {
            var query = new TableQuery<UserEntity>().Where(TableQuery.GenerateFilterCondition("UserId", QueryComparisons.Equal, userId));
            return DataContext.DataNexus.AtsRepository.GetFirstAsync(query, document => (IUser)new User(DataContext, document));
        }

        //public Task<IUser> FindByIdAsync(Guid id)
        //{
        //    return DataContext.DataNexus.AtsRepository.GetAsync<UserEntity, IUser>(id, document => new User(DataContext, document));
        //}


        public async Task<IUser> CreateFromSocialMediaProviderAsync(string email, string name, string key, string baseKey,
           string publicKey, string privateKey)
        {
            throw new NotImplementedException();
            //Guid id = Guid.NewGuid();
            //var document = new UserDocument(id)
            //{
            //    PrimaryEmailAddress = email.ToLowerInvariant(),
            //    BaseKey = baseKey,
            //    PrivateKey = privateKey
            //};

            //await DataContext.DataNexus.AtsRepository.CreateAsync(document);
            //var user = new User(DataContext, document);

            ////Create the Email Address
            //await user.CreateEmailAddressAsync(email, UserRole.User, Priority.Primary);

            //await user.AddSocialMediaAccountAsync(SocialMediaProvider.cpm, publicKey);
            //await user.AddSocialMediaAccountAsync(SocialMediaProvider.cpm, key);

            //return user;
        }

        #endregion
    }
}
