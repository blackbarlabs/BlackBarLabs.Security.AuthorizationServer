using System;
using NC2.CPM.Persistence.Common.Azure.AzureStorageTables;

namespace NC2.CPM.AuthorizationServer.Persistence.Users
{
    internal class UserEntity : AtomicDocument<Guid>
    {
        #region Constructors
        public UserEntity() { }

        public UserEntity(Guid id) : base(id) { }
        #endregion

        #region Properties

        public string UserId { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        #endregion
    }
}
