﻿using System.Collections.Generic;
using System.Web.Http;
using PCR.Server.Data;
using PCR.Server.Models;

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
