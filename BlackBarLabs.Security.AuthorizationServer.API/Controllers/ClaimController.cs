using System;
using BlackBarLabs.Security.Tokens;
using System.Web.Http;
using BlackBarLabs.Security.Authorization;

namespace BlackBarLabs.Security.AuthorizationServer.API.Controllers
{
    public class ClaimController : BaseController
    {
        public IHttpActionResult Get([FromUri]Resources.ClaimGet model)
        {
            model.Request = Request;
            return model;
        }

        public IHttpActionResult Post([FromBody]Resources.ClaimPost model)
        {
            model.Request = Request;
            return model;
        }

        public IHttpActionResult Put([FromBody]Resources.ClaimPut model)
        {
            model.Request = Request;
            return model;
        }
    }
}

