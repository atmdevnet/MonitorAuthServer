using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonitorAuthServer.Controllers
{
    public class StatusController : Controller
    {
        public IActionResult Code(int id)
        {
            return View(id);
        }
    }
}
