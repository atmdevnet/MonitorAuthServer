using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MonitorAuthServer.Controllers
{
    [Route("api/allegro")]
    [Authorize("api:app")]
    public class AllegroController : Controller
    {
        private Model.IAllegroService _allegro = null;

        public AllegroController(Model.IAllegroService allegro)
        {
            _allegro = allegro;
        }

        [HttpGet("systime")]
        public async Task<IActionResult> GetSystemTime()
        {
            var systime = await _allegro.GetSystemTime();

            if (systime.HasValue)
            {
                return Ok(new { systime = systime.Value });
            }
            else
            {
                return BadRequest(new Model.AppError(_allegro.Errors));
            }
        }

        [HttpPost("userinfo")]
        public async Task<IActionResult> GetUser([FromBody] UserRequest data)
        {
            var userinfo = await _allegro.GetUser(data.Id, data.Login);

            if (userinfo.HasValue)
            {
                var result = userinfo.Value;

                return Ok(new {
                    id = result.id,
                    login = result.login,
                    country = result.country,// == 1 ? "Polska" : $"{result.country}",
                    rating = result.rating,
                    created = result.created,//.ToString("d MMMM yyyy, HH:mm"),
                    lastLogin = result.lastLogin,//.ToString("d MMMM yyyy, HH:mm"),
                    blocked = result.blocked,// ? "tak" : "nie",
                    closed = result.closed,// ? "tak" : "nie",
                    terminated = result.terminated,// ? "tak" : "nie",
                    shop = result.shop,// ? "tak" : "nie",
                    standard = result.standard,// ? "tak" : "nie",
                    newbie = result.newbie,// ? "tak" : "nie",
                    junior = result.junior,// ? "tak" : "nie",
                    notActivated = result.notActivated// ? "tak" : "nie"
                });
            }
            else
            {
                return BadRequest(new Model.AppError(_allegro.Errors));
            }
        }

        [HttpGet("userfieldheaders")]
        public IActionResult GetUserFieldHeaders()
        {
            return Ok(new {
                id = "Numer",
                login = "Login",
                country = "Kraj",
                rating = "Punkty",
                created = "Na allegro od",
                lastLogin = "Ostatnio logowa³ siê",
                blocked = "Zablokowany?",
                closed = "Konto zamkniête?",
                terminated = "Umowa rozwi¹zana?",
                shop = "Sklep?",
                standard = "Standard Allegro?",
                newbie = "Nowo zarejestrowany?",
                junior = "Konto Junior?",
                notActivated = "Niepe³na aktywacja?"
            });
        }


        public class UserRequest
        {
            public long? Id { get; set; }
            public string Login { get; set; }
        }
    }
}