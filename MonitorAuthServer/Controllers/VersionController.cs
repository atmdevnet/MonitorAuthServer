using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.JavaScript;
using Syncfusion.JavaScript.DataSources;

namespace MonitorAuthServer.Controllers
{
    [Route("api/version")]
    [Authorize("api:app")]
    public class VersionController : Controller
    {
        private Model.DatabaseService _db = null;

        public VersionController(Model.DatabaseService db)
        {
            _db = db;
        }

        [HttpPost("list")]
        public IActionResult List([FromBody] DataManager query)
        {
            var data = _db.SelectVersions();
            var operation = new DataOperations();

            if (query.Select?.Any() ?? false)
            {
                data = operation.PerformSelect(data, query.Select);
                data = operation.PerformWhereFilter(data, query.Where, null);

                return Ok(data);
            }
            else
            {
                if (query.Sorted?.Any() ?? false)
                {
                    data = operation.PerformSorting(data, query.Sorted);
                }

                var count = data.Cast<object>().Count();

                if (query.Skip > 0)
                {
                    data = operation.PerformSkip(data, query.Skip);
                }

                if (query.Take > 0)
                {
                    data = operation.PerformTake(data, query.Take);
                }

                return Ok(new { result = data, count = count });
            }
        }

        [HttpPost("edit")]
        [Model.RequireValidation]
        public async Task<IActionResult> Edit([FromBody] CRUDModel<Model.AppVersion> payload)
        {
            if (payload.Action.Equals("insert", StringComparison.OrdinalIgnoreCase))
            {
                await _db.AppendVersion(payload.Value);

                return Ok(payload.Value);
            }
            else if (payload.Action.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                await _db.RemoveVersion(payload.Value);

                return Ok(payload.Value);
            }
            else
            {
                return BadRequest(new Model.AppError($"Unknown action: {payload.Action}."));
            }
        }
    }
}