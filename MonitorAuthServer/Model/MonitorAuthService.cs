using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Extensions.Options;
using MonitorAuthServer.Model.Schema;
using MonitorAuthServer.Extensions;
using MonitorAuthServer.Controllers;

namespace MonitorAuthServer.Model
{
    public class MonitorAuthService : IDisposable
    {
        private AesManaged _aes = new AesManaged() { KeySize = 256, BlockSize = 16 * 8, Mode = CipherMode.CBC, Padding = PaddingMode.PKCS7 };

        private MonitorAuthConfig _config = null;
        private KeyEncryptCertificateService _certKeyEncrypt = null;
        private SignCertificateService _certSign = null;
        private AuthDocSchemaService _schema = null;
        private DatabaseService _db = null;


        public MonitorAuthService(  IOptions<MonitorAuthConfig> config, 
                                    KeyEncryptCertificateService certKeyEncrypt, 
                                    SignCertificateService certSign, 
                                    AuthDocSchemaService schema,
                                    DatabaseService db)
        {
            _config = config.Value;
            _certKeyEncrypt = certKeyEncrypt;
            _certSign = certSign;
            _schema = schema;
            _db = db;
        }

        public MonitorAuthResponse Init(string message, out SessionData sessionData)
        {
            var doc = createDocument(message);

            if (!verifySignature(doc))
            {
                throw new CryptographicException("Nieprawidłowy podpis.");
            }

            removeSignature(doc);
            validate(doc);

            var authdata = deserialize(doc);

            if (!validate(authdata))
            {
                throw new ArgumentException("Nieprawidłowe dane uwierzytelniające.");
            }

            if (!validateKey(authdata, out var aes))
            {
                throw new CryptographicException("Nieprawidłowy klucz.");
            }

            sessionData = new SessionData() { Key = pack(aes), Version = authdata.Version };

            var requiredAtLeast = _db.GetCurrentAppVersion();
            if (requiredAtLeast == null)
            {
                throw new ArgumentException("Brak konfiguracji wersji.");
            }

            var versionValid = authdata.Version.ToVersion() >= requiredAtLeast;
            var userId = _db.UserExists(authdata.Nick);

            var result = new AuthData()
            {
                VersionValid = versionValid,
                Version = authdata.Version,
                UserId = versionValid ? userId : null,
                Key = versionValid && userId.HasValue ? _config.WebApiKey : string.Empty
            };

            var encrypted = encrypt(result, aes);

            return new MonitorAuthResponse() { Message = encrypted };
        }

        public MonitorAuthResponse License(string message, SessionData sessionData)
        {
            if (!sessionData?.Key?.Any() ?? true)
            {
                throw new ArgumentException("Brak danych sesji.");
            }

            var aes = unpack(sessionData.Key);

            var authdata = decrypt(message, aes);

            if (!validate(authdata))
            {
                throw new ArgumentException("Nieprawidłowe dane uwierzytelniające.");
            }

            _db.SaveUserActivity(authdata.UserId.Value, sessionData.Version);

            var license = _db.UserLicense(authdata.UserId.Value);

            var result = new AuthData()
            {
                UserId = license?.UserId,
                License = license?.ValidTo,
                Scope = license?.Scope ?? LicenseScope.Basic
            };

            var encrypted = encrypt(result, aes);

            return new MonitorAuthResponse() { Message = encrypted };
        }


        private XmlDocument createDocument(string data)
        {
            var xml = Encoding.UTF8.GetString(Convert.FromBase64String(data));

            var doc = new XmlDocument() { PreserveWhitespace = true };
            doc.LoadXml(xml);

            return doc;
        }

        private bool verifySignature(XmlDocument doc)
        {
            _certSign.EnsureCertificateAvailable();

            var valid = false;

            var signed = new SignedXml(doc);

            var signatures = doc.GetElementsByTagName("Signature");
            if (signatures?.Count == 1)
            {
                signed.LoadXml(signatures[0] as XmlElement);

                valid = signed.CheckSignature(_certSign.Certificate.PublicKey.Key);
            }

            return valid;
        }

        private void removeSignature(XmlDocument doc)
        {
            var signature = doc.GetElementsByTagName("Signature")[0];
            signature.ParentNode.RemoveChild(signature);
        }

        private void validate(XmlDocument doc)
        {
            Exception lastError = null;

            _schema.EnsureSchemaAvailable();

            doc.Schemas = _schema.Schema;
            doc.Validate(validationHandler);

            if (lastError != null)
            {
                throw lastError;
            }

            void validationHandler(object sender, ValidationEventArgs e)
            {
                if (e.Severity == XmlSeverityType.Error)
                {
                    lastError = e.Exception;
                }
            }
        }

        private Authorize deserialize(XmlDocument doc)
        {
            using (var xml = new MemoryStream())
            using (var writer = XmlWriter.Create(xml, new XmlWriterSettings() { Encoding = Encoding.UTF8, OmitXmlDeclaration = true }))
            {
                doc.WriteTo(writer);

                writer.Flush();
                xml.Seek(0, SeekOrigin.Begin);

                var deserializer = new XmlSerializer(typeof(Authorize));
                return deserializer.Deserialize(xml) as Authorize;
            }
        }

        private bool validate(Authorize data)
        {
            return
                data != null &&
                data.Id.IsBase64() &&
                data.Version.IsVersion() &&
                !data.UserId.HasValue &&
                (data.EncryptionKey?.Value.IsBase64() ?? false) &&
                (data.Encryption?.AES?.IV?.Value.IsBase64() ?? false);
        }

        private bool validate(AuthData data)
        {
            return 
                data != null &&
                data.UserId.HasValue;
        }

        private bool validateKey(Authorize data, out (byte[] key, byte[] iv) aes)
        {
            _certKeyEncrypt.EnsureCertificateAvailable();

            var aesKey = _certKeyEncrypt.Decrypt(data.EncryptionKey.Value);
            var aesIv = Convert.FromBase64String(data.Encryption.AES.IV.Value);

            aes = (aesKey, aesIv);

            using (var decryptor = _aes.CreateDecryptor(aesKey, aesIv))
            {
                var encryptedId = Convert.FromBase64String(data.Id);
                var id = Encoding.UTF8.GetString(decryptor.TransformFinalBlock(encryptedId, 0, encryptedId.Length));

                return id.Equals(_config.AuthorizationId);
            }
        }

        private string encrypt(AuthData data, (byte[] key, byte[] iv) aes)
        {
            using (var encryptor = _aes.CreateEncryptor(aes.key, aes.iv))
            {
                var encoded = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(data));
                var encrypted = encryptor.TransformFinalBlock(encoded, 0, encoded.Length);

                return Convert.ToBase64String(encrypted);
            }
        }

        private AuthData decrypt(string data, (byte[] key, byte[] iv) aes)
        {
            using (var decryptor = _aes.CreateDecryptor(aes.key, aes.iv))
            {
                var decoded = Convert.FromBase64String(data);
                var decrypted = decryptor.TransformFinalBlock(decoded, 0, decoded.Length);

                return Newtonsoft.Json.JsonConvert.DeserializeObject<AuthData>(Encoding.UTF8.GetString(decrypted));
            }
        }

        private byte[] pack((byte[] key, byte[] iv) aes)
        {
            return Encoding.UTF8.GetBytes($"{Convert.ToBase64String(aes.key)}|{Convert.ToBase64String(aes.iv)}");
        }

        private (byte[] key, byte[] iv) unpack(byte[] key)
        {
            var aesdata = Encoding.UTF8.GetString(key).Split('|');
            return (Convert.FromBase64String(aesdata[0]), Convert.FromBase64String(aesdata[1]));
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
                    _aes?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MonitorAuthService() {
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
