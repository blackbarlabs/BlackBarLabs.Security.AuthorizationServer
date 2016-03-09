using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NC2.Constants;
using NC2.CPM.AuthorizationServer.Persistence.Contracts.Users;
using NC2.CPM.AuthorizationServer.Persistence.Users;
using NC2.CPM.Persistence.Common.Azure.AzureStorageTables;

namespace NC2.CPM.AuthorizationServer.Persistence.Metadata
{
    internal class Metadata : AtomicEntity<Guid, UserEntity>, IUser
    {
        private readonly Task<UserEntity> userEntityAsync;
        private UserEntity entity;

        public User(CPM.Persistence.Common.Azure.DataContext dataContext, Guid id, Task<UserEntity> fetchTask = null)
            : base(dataContext, id, fetchTask)
        {
        }

        public User(CPM.Persistence.Common.Azure.DataContext dataContext, UserEntity document)
            : base(dataContext, document)
        {
        }

        public User(CPM.Persistence.Common.Azure.DataContext dataContext, UserEntity document, Task<UserEntity> createTask)
            : base(dataContext, document, createTask)
        {
        }

        #region Properties

        private PropTask<string> userIdTask;
        public Task<string> UserIdAsync
        {
            get
            {
                if (userIdTask == null) userIdTask = new PropTask<string>(this, document => document.UserId);
                return userIdTask.GetTask();
            }
        }

        private PropTask<string> passwordHashTask;
        public Task<string> PasswordHashAsync
        {
            get
            {
                if (passwordHashTask == null) passwordHashTask = new PropTask<string>(this, document => document.PasswordHash);
                return passwordHashTask.GetTask();
            }
        }

        private PropTask<string> passwordSaltTask;
        public Task<string> PasswordSaltAsync
        {
            get
            {
                if (passwordSaltTask == null) passwordSaltTask = new PropTask<string>(this, document => document.PasswordSalt);
                return passwordSaltTask.GetTask();
            }
        }

        #endregion

        #region Actionables
        public Task<bool> UpdatePasswordAsync(string passwordHash, string passwordSalt)
        {
            return UpdateAtomicAsync(document =>
            {
                document.PasswordHash = passwordHash;
                passwordHashTask = null;
                document.PasswordSalt = passwordSalt;
                passwordSaltTask = null;
                return true;
            });
        }

 
        #endregion

        //#region SocialMediaAccount

        //private Task<IEnumerable<ISocialMediaAccount>> socialMediaAccountTask;
        //public Task<IEnumerable<ISocialMediaAccount>> SocialMediaAccountsAsync
        //{
        //    get { return socialMediaAccountTask ?? (socialMediaAccountTask = GetSocialMediaAccountsAsync()); }
        //}

        //private async Task<IEnumerable<ISocialMediaAccount>> GetSocialMediaAccountsAsync()
        //{
        //    return await DataContext.DataNexus.AtsRepository.GetAssociatedListAsync<SocialMediaAccountDocument, SocialMediaAccount>(await SocialMediaAccountListIdAsync, y => new SocialMediaAccount(DataContext, y));
        //}

        //public async Task<ISocialMediaAccount> FindSocialMediaAccountByIdAsync(Guid socialMediaAccountId)
        //{
        //    return await DataContext.DataNexus.AtsRepository.GetAssociatedAsync<SocialMediaAccountDocument, SocialMediaAccount>(await SocialMediaAccountListIdAsync, socialMediaAccountId, y => new SocialMediaAccount(DataContext, y));
        //}

        //public async Task<ISocialMediaAccount> AddSocialMediaAccountAsync(SocialMediaProvider provider, string key)
        //{
        //    var newId = Guid.NewGuid();
        //    var newDocument = new SocialMediaAccountDocument(newId)
        //    {
        //        Provider = provider,
        //        Key = key
        //    };

        //    var createdItem = await DataContext.DataNexus.AtsRepository.CreateAndAssociateAsync(newDocument, Id, await SocialMediaAccountListIdAsync,
        //                 (sharedDocument) => UpdateAtomicAsync(document =>
        //                 {
        //                     document.SocialMediaAccountIds = sharedDocument.RowKey.AsGuid();
        //                     return true;
        //                 }));
        //    socialMediaAccountTask = null;
        //    socialMediaAccountListTask = null;

        //    return createdItem != null ? new SocialMediaAccount(DataContext, newDocument) : null;
        //}

        //public async Task<bool> RemoveSocialMediaAccountAsync(Guid socialMediaAccountId)
        //{
        //    socialMediaAccountTask = null;
        //    socialMediaAccountListTask = null;
        //    return await DataContext.DataNexus.AtsRepository.DisassociateAsync(await SocialMediaAccountListIdAsync, socialMediaAccountId);
        //}

        //#endregion



        public Task<IUser> CreateFromSocialMediaProviderAsync(string email, string name, string key, string baseKey, string publicKey, string privateKey)
        {
            throw new NotImplementedException();
        }

        public Task<string> BaseKeyAsync
        {
            get { throw new NotImplementedException(); }
        }

        public Task<string> PrivateKeyAsync
        {
            get { throw new NotImplementedException(); }
        }


        public Task<IEnumerable<ISocialMediaAccount>> SocialMediaAccountsAsync
        {
            get { throw new NotImplementedException(); }
        }

        public Task<ISocialMediaAccount> AddSocialMediaAccountAsync(SocialMediaProvider provider, string key)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveSocialMediaAccountAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}