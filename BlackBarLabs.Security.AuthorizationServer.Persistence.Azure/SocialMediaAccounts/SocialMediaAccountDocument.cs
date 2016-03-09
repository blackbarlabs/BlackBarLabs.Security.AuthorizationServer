using System;
using NC2.Constants;
using NC2.CPM.Persistence.Common.Azure.AzureStorageTables;

namespace NC2.CPM.AuthorizationServer.Persistence.SocialMediaAccounts
{
    [Serializable]
    internal class SocialMediaAccountDocument : AtomicDocument<Guid>
    {
        public SocialMediaAccountDocument()
        {
        }

        public SocialMediaAccountDocument(Guid id)
            : base(id)
        {
        }

        public SocialMediaProvider Provider { get; set; }
        public String Key { get; set; }

    }
}