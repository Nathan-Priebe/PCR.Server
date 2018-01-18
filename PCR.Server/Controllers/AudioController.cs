using System.Collections.Generic;
using System.Web.Http;
using PCR.Server.Data;
using PCR.Server.Models;

namespace PCR.Server.Controllers
{
    [RoutePrefix("Audio")]
    public class AudioController : ApiController
    {
        [ExceptionFilter]
        [Route("Update")]
        [HttpPost]
        public void VolumeUpdate([FromBody]Message message)
        {
            Audio.UpdateVolume(message);
        }

        [ExceptionFilter]
        [Route("Mute")]
        [HttpPost]
        public void Mute([FromBody] int processId)
        {
            Audio.MuteApp(processId);
        }

        [ExceptionFilter]
        [Route("All")]
        [HttpGet]
        public IEnumerable<AppDetails> AllApps ()
        {
            return Audio.AllApps();
        }
    }
}
