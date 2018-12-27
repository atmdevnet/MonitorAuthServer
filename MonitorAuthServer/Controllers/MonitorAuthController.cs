using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MonitorAuthServer.Extensions;

namespace MonitorAuthServer.Controllers
{
    [Route("api/auth")]
    [Authorize("api:auth")]
    public class MonitorAuthController : Controller
    {
        private Model.MonitorAuthService _service = null;

        public MonitorAuthController(Model.MonitorAuthService service)
        {
            _service = service;
        }

        [HttpPost("init")]
        [Model.RequireValidation]
        public IActionResult Init([FromBody] MonitorAuthRequest data)
        {
            var response = _service.Init(data.Message, out var sessionData);

            HttpContext.Session.Store<SessionData>(sessionData);

            return Ok(response);
        }

        [HttpPost("license")]
        [Model.RequireValidation]
        public IActionResult License([FromBody] MonitorAuthRequest data)
        {
            var sessionData = HttpContext.Session.Restore<SessionData>();

            var response = _service.License(data.Message, sessionData);

            return Ok(response);
        }
    }


    [DataContract]
    public class MonitorAuthRequest
    {
        [Required(AllowEmptyStrings = false)]
        [DataMember]
        public string Message { get; set; }
    }

    [DataContract]
    public class MonitorAuthResponse
    {
        [Required(AllowEmptyStrings = false)]
        [DataMember]
        public string Message { get; set; }
    }

    public class SessionData
    {
        public string Version { get; set; }
        public byte[] Key { get; set; }
    }

}