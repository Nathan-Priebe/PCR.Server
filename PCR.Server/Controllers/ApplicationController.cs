using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using PCR.Server.Data;
using PCR.Server.Models;
using PCRServer;

namespace PCR.Server.Controllers
{
    [RoutePrefix("Application")]
    class ApplicationController : ApiController
    {
        [ExceptionFilter]
        [Route("All")]
        [HttpGet]
        public List<Applications> AllApplications()
        {
            return Application.AllApps();
        }

        [ExceptionFilter]
        [Route("Execute")]
        [HttpPost]
        public void ExecuteApp(int appId)
        {
            Application.Execute(appId);
        }
    }
}
