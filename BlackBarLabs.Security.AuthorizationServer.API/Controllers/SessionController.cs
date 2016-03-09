using System;
using BlackBarLabs.Security.Tokens;
using System.Web.Http;
using BlackBarLabs.Security.Authorization;

namespace BlackBarLabs.Security.AuthorizationServer.API.Controllers
{
    public class SessionController : BaseController
    {
        public IHttpActionResult Get([FromUri]Resources.SessionGet query)
        {
            if (default(Resources.SessionGet) == query)
                query = new Resources.SessionGet();

            query.Request = this.Request;
            return query;
        }
        
        public IHttpActionResult Post([FromBody]Resources.SessionPost model)
        {
            model.Request = Request;
            return model;
        }
        
        public IHttpActionResult Put([FromBody]Resources.SessionPut model)
        {
            model.Request = Request;
            return model;
        }
        
        public IHttpActionResult Options(Nullable<Guid> Id = default(Nullable<Guid>))
        {
            var post1 = new Resources.Session
            {
                Id = Guid.NewGuid(),
            };
            var response = new BlackBarLabs.Api.Resources.Options
            {
                Post = new[] { post1 },
            };
            if(default(Nullable<Guid>) != Id)
            {
                var authId = Guid.NewGuid();
                var jwtToken = JwtTools.CreateToken(
                    Id.Value, authId, 200000,
                    "AuthServer.issuer", "AuthServer.key");

                var put1 = new Resources.Session
                {
                    Id = Id.Value,
                    AuthorizationId = authId,
                    Credentials = new CredentialsType()
                    {
                        Method = CredentialValidationMethodTypes.Facebook,
                        Provider = new Uri("urn:facebook.com/Auth"),
                        UserId = "0123455690",
                        Token = "ABC.123.U_AND_ME"
                    },
                    SessionHeader = new Resources.Session.AuthHeaderProps
                    {
                        Name = "Authorization",
                        Value = jwtToken,
                    }
                };
                response.Put = new[] { put1 };
            }
            return Json(response);
        }
    }
}

