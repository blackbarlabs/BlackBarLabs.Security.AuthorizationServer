using System;
using System.Collections.Concurrent;

namespace BlackBarLabs.Security.AuthorizationServer
{
    public class Context
    {
        private Persistence.IDataContext dataContext;
        private readonly Func<Persistence.IDataContext> dataContextCreateFunc;

        private ConcurrentDictionary<CredentialValidationMethodTypes, CredentialProvider.IProvideCredentials> credentialProviders = 
            new ConcurrentDictionary<CredentialValidationMethodTypes, CredentialProvider.IProvideCredentials>();
        private readonly Func<CredentialValidationMethodTypes, CredentialProvider.IProvideCredentials> credentialProvidersFunc;

        public Context(Func<Persistence.IDataContext> dataContextCreateFunc,
            Func<CredentialValidationMethodTypes, CredentialProvider.IProvideCredentials> credentialProvidersFunc)
        {
            dataContextCreateFunc.ValidateArgumentIsNotNull("dataContextCreateFunc");
            this.dataContextCreateFunc = dataContextCreateFunc;

            credentialProvidersFunc.ValidateArgumentIsNotNull("credentialProvidersFunc");
            this.credentialProvidersFunc = credentialProvidersFunc;
        }

        internal Persistence.IDataContext DataContext
        {
            get { return dataContext ?? (dataContext = dataContextCreateFunc.Invoke()); }
        }
        
        internal CredentialProvider.IProvideCredentials GetCredentialProvider(CredentialValidationMethodTypes method)
        {
            if (!this.credentialProviders.ContainsKey(method))
            {
                var newProvider = this.credentialProvidersFunc.Invoke(method);
                this.credentialProviders.AddOrUpdate(method, newProvider, (m, p) => newProvider);
            }
            var provider = this.credentialProviders[method];
            return provider;
        }
        
        private Sessions sessions;
        public Sessions Sessions
        {
            get
            {
                if (default(Sessions) == sessions)
                    sessions = new Sessions(this, this.DataContext);
                return sessions;
            }
        }

        private Authorizations authorizations;
        public Authorizations Authorizations
        {
            get
            {
                if (default(Authorizations) == authorizations)
                    authorizations = new Authorizations(this, this.DataContext);
                return authorizations;
            }
        }

        private Roles roles;
        public Roles Roles
        {
            get
            {
                if (default(Roles) == roles)
                    roles = new Roles(this, this.DataContext);
                return roles;
            }
        }

        private Tokens tokens;
        public Tokens Tokens
        {
            get
            {
                if (default(Tokens) == tokens)
                    tokens = new Tokens(this, this.DataContext);
                return tokens;
            }
        }

        #region Authorizations

        public delegate bool GetCredentialDelegate(CredentialValidationMethodTypes validationMethod, Uri provider, string userId, string userToken);
        
        #endregion



    }
}
