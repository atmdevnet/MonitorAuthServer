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
    [Route("api/licenses")]
    [Authorize("api:app")]
    public class LicenseController : Controller
    {
        private Model.DatabaseService _db = null;

        public LicenseController(Model.DatabaseService db)
        {
            _db = db;
        }

        [HttpPost("active")]
        public IActionResult GetActive([FromBody] DataManager query)
        {
            var data = _db.SelectActive();
            var operation = new DataOperations();

            if (query.Select?.Any() ?? false)
            {
                data = operation.PerformSelect(data, query.Select);
                data = operation.PerformWhereFilter(data, query.Where, null);

                return Ok(data);
            }
            else
            {
                if (query.Search?.Any() ?? false)
                {
                    data = operation.PerformSearching(data, query.Search);
                }

                if (query.Where?.Any() ?? false)
                {
                    data = operation.PerformWhereFilter(data, query.Where, query.Where[0].Condition);
                }

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

        [HttpPost("expired")]
        public IActionResult GetExpired([FromBody] DataManager query)
        {
            var data = _db.SelectExpired();
            var operation = new DataOperations();

            if (query.Select?.Any() ?? false)
            {
                data = operation.PerformSelect(data, query.Select);
                data = operation.PerformWhereFilter(data, query.Where, null);

                return Ok(data);
            }
            else
            {
                if (query.Search?.Any() ?? false)
                {
                    data = operation.PerformSearching(data, query.Search);
                }

                if (query.Where?.Any() ?? false)
                {
                    data = operation.PerformWhereFilter(data, query.Where, query.Where[0].Condition);
                }

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
        public IActionResult Edit([FromBody] CRUDModel<Model.License> payload)
        {
            if (payload.Action.Equals("update", StringComparison.OrdinalIgnoreCase))
            {
                if (_db.Update(payload.Value))
                {
                    return Ok(payload.Value);
                }
                else
                {
                    return BadRequest(new Model.AppError("Update entity failed."));
                }
            }
            else if (payload.Action.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                if (_db.Remove(Convert.ToInt64(payload.Key)))
                {
                    return Ok(payload.Key);
                }
                else
                {
                    return BadRequest(new Model.AppError("Remove entity failed."));
                }
            }
            else
            {
                return BadRequest(new Model.AppError($"Unknown edit action: {payload.Action}."));
            }
        }

        [HttpPost("add")]
        [Model.RequireValidation]
        public IActionResult Add([FromBody] Model.License license)
        {
            if (_db.Add(license))
            {
                return Ok(license);
            }
            else
            {
                return BadRequest(new Model.AppError("Add entity failed."));
            }
        }

        [HttpGet("audit/{userId}")]
        public IActionResult LicenseAudit(long userId)
        {
            var audit = _db.UserLicenseAudit(userId);

            return Ok(audit);
        }

        [HttpGet("activity/{userId}")]
        public IActionResult Activity(long userId)
        {
            var activity = _db.UserActivity(userId);

            return Ok(activity);
        }

        [HttpPost("recent")]
        public IActionResult RecentActivity([FromBody] DataManager query)
        {
            var data = _db.RecentActivity();
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

        [HttpGet("scopes")]
        public IActionResult GetScopes()
        {
            var scopes =
                Enum.GetValues(typeof(Model.LicenseScope))
                .Cast<Model.LicenseScope>()
                .Where(v => v != Model.LicenseScope.Basic)
                .Select(v => new { value = (int)v, name = $"{v}" })
                .ToArray();

            return Ok(new { count = scopes.Length, emptyName = $"{Model.LicenseScope.Basic}", scopes = scopes });
        }
    }
}