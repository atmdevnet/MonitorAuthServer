using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml.Schema;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace MonitorAuthServer.Model
{
    public class AuthDocSchemaService
    {
        private AuthDocSchemaConfig _config = null;
        private string _rootPath = null;
        private XmlSchemaSet _schema = new XmlSchemaSet();

        public bool IsAvailable { get { return _schema.Count > 0 && LastError == null; } }
        public Exception LastError { get; private set; }
        public XmlSchemaSet Schema { get { return _schema; } }

        public AuthDocSchemaService(IOptions<AuthDocSchemaConfig> config, IHostingEnvironment env)
        {
            _config = config.Value;
            _rootPath = env.ContentRootPath;

            init();
        }

        public void EnsureSchemaAvailable()
        {
            if (!IsAvailable)
            {
                throw (LastError ?? new CryptographicException("Brak pooprawnego schematu danych uwierzytelniających."));
            }
        }


        private void init()
        {
            try
            {
                if (_config.Files?.Any() ?? false)
                {
                    foreach (var file in _config.Files)
                    {
                        using (var data = File.OpenRead(Path.Combine(_rootPath, _config.Path ?? "", file)))
                        {
                            _schema.Add(XmlSchema.Read(data, validationHandler));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex;
            }
        }

        private void validationHandler(object sender, ValidationEventArgs e)
        {
            if (e.Severity == XmlSeverityType.Error)
            {
                LastError = e.Exception;
            }
        }


    }
}
