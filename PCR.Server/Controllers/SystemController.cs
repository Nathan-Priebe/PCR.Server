using System.Web.Http;
using PCR.Server.Data;

namespace PCR.Server.Controllers
{
    [RoutePrefix("System")]
    public class SystemController : ApiController
    {
        [ExceptionFilter]
        [Route("UpdateLog")]
        [HttpPost]
        public void UpdateLog([FromBody]string error)
        {
            Common.LogError(error);
        }

        [ExceptionFilter]
        [Route("Version")]
        [HttpGet]
        public string Version()
        {
            return PcrSystem.version();
        }

    }
}
