using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace MonitorAuthServer.Model.Schema
{
    [XmlType(Namespace = "https://monitor.atmdev.net")]
    [XmlRoot(Namespace = "https://monitor.atmdev.net")]
    public class Authorize
    {
        public string Id { get; set; }

        public string Version { get; set; }

        public string Nick { get; set; }

        public long? UserId { get; set; }

        [XmlElement(IsNullable = true)]
        public EncryptionKey EncryptionKey { get; set; }

        [XmlElement(IsNullable = true)]
        public Encryption Encryption { get; set; }
    }


    [XmlType(Namespace = "https://monitor.atmdev.net")]
    public class EncryptionKey
    {
        [XmlAttribute("algorithm")]
        public string Algorithm { get; set; }

        [XmlAttribute("mode")]
        public string Mode { get; set; }

        [XmlAttribute("padding")]
        public string Padding { get; set; }

        [XmlAttribute("encoding")]
        public string Encoding { get; set; }

        [XmlText]
        public string Value { get; set; }
    }


    [XmlType(Namespace = "https://monitor.atmdev.net")]
    public class Encryption
    {
        public AES AES { get; set; }
    }


    [XmlType(Namespace = "https://monitor.atmdev.net")]
    public class AES
    {
        [XmlAttribute("size")]
        public uint Size { get; set; }

        [XmlAttribute("block")]
        public uint Block { get; set; }

        [XmlAttribute("mode")]
        public string Mode { get; set; }

        [XmlAttribute("padding")]
        public string Padding { get; set; }

        public IV IV { get; set; }
    }


    [XmlType(Namespace = "https://monitor.atmdev.net")]
    public class IV
    {
        [XmlAttribute("bytes")]
        public uint Bytes { get; set; }

        [XmlAttribute("encoding")]
        public string Encoding { get; set; }

        [XmlText]
        public string Value { get; set; }
    }




    public class AuthData
    {
        public string Version { get; set; }
        public bool VersionValid { get; set; }
        public long? UserId { get; set; }
        public DateTime? License { get; set; }
        public LicenseScope Scope { get; set; }
        public string Key { get; set; }
    }


}
