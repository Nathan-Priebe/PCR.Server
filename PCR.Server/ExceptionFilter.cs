using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace PCR.Server
{
    internal class ExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            Common.LogError(context.Exception.Message);
            context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("An error has occured " + Environment.NewLine + context.Exception.Message)
            };
        }
    }
}
