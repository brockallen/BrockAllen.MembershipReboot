using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace ServerApp
{
    [Route("test")]
    [Authorize]
    public class TestController : ApiController
    {
        public IHttpActionResult Get()
        {
            var cp = User as ClaimsPrincipal;
            return Ok(cp.Claims.Select(x => new { x.Type, x.Value }));
        }
    }
}