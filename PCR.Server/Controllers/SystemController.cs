using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using PCR.Server.Data;
using PCRServer;

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
            PCRServer.Common.LogError(error);
        }

        [ExceptionFilter]
        [Route("Version")]
        [HttpGet]
        public string Version()
        {
            return PCRSystem.version();
        }

    }
}
