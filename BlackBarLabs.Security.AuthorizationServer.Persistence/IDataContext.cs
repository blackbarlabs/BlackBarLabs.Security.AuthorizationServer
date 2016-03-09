namespace BlackBarLabs.Security.AuthorizationServer.Persistence
{
    public interface IDataContext
    {
        IAuthorizations Authorizations { get; }

        ISessions Sessions { get; }

        IRoles Roles { get; }

        ITokens Tokens { get; }

    }
}
