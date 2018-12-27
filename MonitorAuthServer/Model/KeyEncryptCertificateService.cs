using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace MonitorAuthServer.Model
{
    public class KeyEncryptCertificateService : IDisposable
    {
        private KeyEncryptCertificateConfig _config = null;
        private string _rootPath = null;
        private X509Certificate2 _certificate = null;

        public bool IsAvailable { get { return _certificate?.HasPrivateKey ?? false; } }
        public X509Certificate2 Certificate { get { return _certificate; } }
        public Exception LastError { get; private set; }

        public KeyEncryptCertificateService(IOptions<KeyEncryptCertificateConfig> config, IHostingEnvironment env)
        {
            _config = config.Value;
            _rootPath = env.ContentRootPath;

            init();
        }

        public void EnsureCertificateAvailable()
        {
            if (!IsAvailable)
            {
                throw (LastError ?? new CryptographicException("Brak certyfikatu szyfrowania klucza."));
            }
        }

        public byte[] Decrypt(string value)
        {
            using (var rsa = _certificate.GetRSAPrivateKey())
            {
                var encrypted = Convert.FromBase64String(value);
                return rsa.Decrypt(encrypted, RSAEncryptionPadding.Pkcs1);
            }
        }


        private void init()
        {
            try
            {
                var data = File.ReadAllBytes(Path.Combine(_rootPath, _config.Path ?? "", _config.FileName));
                if (data?.Any() ?? false)
                {
                    _certificate = new X509Certificate2(data, _config.Password);
                }
            }
            catch (Exception ex)
            {
                LastError = ex;
            }
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _certificate?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~EncryptCertificateService() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
