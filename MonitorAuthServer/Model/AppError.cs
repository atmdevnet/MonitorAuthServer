using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace MonitorAuthServer.Model
{
    public class AppError
    {
        [JsonProperty("errors")]
        public string[] Errors { get; private set; } = new string[0];

        public AppError(ModelStateDictionary model)
        {
            if (model != null)
            {
                Errors = model.Keys.SelectMany(k => { var m = model[k]; return m.Errors.Select(e => $"Key: {k}, Value: {m.AttemptedValue}, Error: {e.ErrorMessage}"); }).ToArray();
            }
        }

        public AppError(Exception ex)
        {
            if (ex != null)
            {
                var errors = new List<string>();

                errors.Add(ex.Message);

                if (ex.InnerException != null)
                {
                    errors.Add(ex.InnerException.Message);

                    if (ex.InnerException.InnerException != null)
                    {
                        errors.Add(ex.InnerException.InnerException.Message);

                        if (ex.InnerException.InnerException.InnerException != null)
                        {
                            errors.Add(ex.InnerException.InnerException.InnerException.Message);
                        }
                    }
                }

                Errors = errors.ToArray();
            }
        }

        public AppError(params string[] errors)
        {
            if (errors != null)
            {
                Errors = errors;
            }
        }
    }

}
