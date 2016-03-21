using System.Web.Http;

namespace BlackBarLabs.Security.AuthorizationServer.API.Controllers
{
    public class CredentialController : BaseController
    {
        // POST: api/Order
        public IHttpActionResult Post([FromBody]Resources.CredentialPost model)
        {
            model.Request = Request;
            return model;
        }

        public IHttpActionResult Delete([FromBody]Resources.CredentialDelete model)
        {
            model.Request = Request;
            return model;
        }

        //public IHttpActionResult Options()
        //{
        //    var model = new Resources.CredentialOptions();
        //    return model;
        //}
    }
}

