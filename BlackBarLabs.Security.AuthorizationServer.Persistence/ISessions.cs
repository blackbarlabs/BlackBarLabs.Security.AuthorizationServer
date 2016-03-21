using System;
using System.Threading.Tasks;

namespace BlackBarLabs.Security.AuthorizationServer.Persistence
{
    public delegate T UpdateAuthenticationDelegate<T>(Guid storedAuthenticationId, Action<Guid> saveNewAuthenticationId);
    public interface ISessions
    {
        Task CreateAsync(Guid sessionId, string refreshToken, Guid authorizationId = default(Guid));

        /// <summary>
        /// Calls back the invocation method with currently stored authorization Id and 
        /// updates the authorization id with the return value of the method.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="authIdFunc"></param>
        /// <returns></returns>
        Task<TResult> UpdateAuthentication<TResult>(Guid sessionId, UpdateAuthenticationDelegate<TResult> authIdFunc, Func<TResult> notFound);

        /// <summary>
        /// Check if sessionId is stored in the database
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<bool> DoesExistsAsync(Guid sessionId);
    }
}

