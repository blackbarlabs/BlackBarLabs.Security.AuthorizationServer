using System;
using System.Threading.Tasks;
using NC2.Constants;
using NC2.CPM.AuthorizationServer.Persistence.Contracts.Users.SocialMediaAccounts;
using NC2.CPM.Persistence.Common.Azure.AzureStorageTables;

namespace NC2.CPM.AuthorizationServer.Persistence.SocialMediaAccounts
{
    internal class SocialMediaAccount : AtomicEntity<Guid, SocialMediaAccountDocument>, ISocialMediaAccount
    {
        public SocialMediaAccount(CPM.Persistence.Common.Azure.DataContext dataContext, Guid id, Task<SocialMediaAccountDocument> fetchTask = null)
            : base(dataContext, id, fetchTask)
        {
        }

        public SocialMediaAccount(CPM.Persistence.Common.Azure.DataContext dataContext, SocialMediaAccountDocument document)
            : base(dataContext, document)
        {
        }

        public SocialMediaAccount(CPM.Persistence.Common.Azure.DataContext dataContext, SocialMediaAccountDocument document, Task<SocialMediaAccountDocument> createTask)
            : base(dataContext, document, createTask)
        {
        }
        private PropTask<SocialMediaProvider> providerTask;
        public Task<SocialMediaProvider> ProviderAsync
        {
            get
            {
                if (providerTask == null)

                    providerTask = new PropTask<SocialMediaProvider>(this, document => document.Provider);


                return providerTask.GetTask();
            }
        }
        private PropTask<String> keyTask;
        public Task<String> KeyAsync
        {
            get
            {
                if (keyTask == null)

                    keyTask = new PropTask<String>(this, document => document.Key);


                return keyTask.GetTask();
            }
        }
    }
}
