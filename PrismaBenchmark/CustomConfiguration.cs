using System;
using System.Xml;

namespace PrismaBenchmark
{
    class CustomConfiguration
    {
        private static CustomConfiguration _self = null;

        private CustomConfiguration() {
            XmlDocument conf = new XmlDocument();
            conf.Load("conf.xml");
            host = conf.GetElementsByTagName("host")[0].InnerText;
            port = conf.GetElementsByTagName("port")[0].InnerText;
            dbhost = conf.GetElementsByTagName("dbhost")[0].InnerText;
            dbport = conf.GetElementsByTagName("dbport")[0].InnerText;
            userid = conf.GetElementsByTagName("userid")[0].InnerText;
            password = conf.GetElementsByTagName("password")[0].InnerText;
            database = conf.GetElementsByTagName("database")[0].InnerText;
            if (!Boolean.TryParse(conf.GetElementsByTagName("useProxy")[0].InnerText, out useProxy))
                useProxy = true; // default
            if (!Boolean.TryParse(conf.GetElementsByTagName("useProxy")[0].InnerText, out useProxy))
                useProxy = true; // default
            if (!Boolean.TryParse(conf.GetElementsByTagName("encrypt")[0].InnerText, out encrypt))
                encrypt = true; // default
            if (!useProxy && encrypt)
            {
                throw new Exception("Error: Inappropriate configuration - cannot encrypt while not use proxy!");
            }
            if (!Boolean.TryParse(conf.GetElementsByTagName("useIndex")[0].InnerText, out useIndex))
                useIndex = true; // default
            if (!Int32.TryParse(conf.GetElementsByTagName("rows")[0].InnerText, out rows))
                rows = 500000; // default
            if (conf.GetElementsByTagName("loadTest").Count != 0)
            {
                XmlNode operations = conf.SelectSingleNode("configuration/loadTest/operations");
                String[] operations_str = new String[operations.ChildNodes.Count];
                for (var i = 0; i < operations.ChildNodes.Count; i++)
                {
                    operations_str[i] = operations.ChildNodes[i].InnerText;
                }
                load = new Load(operations_str);
            };
            //// loadTest variables
            if (!Int32.TryParse(conf.GetElementsByTagName("multiple")[0].InnerText, out multiple))
                multiple = 10; // default
            if (!Int32.TryParse(conf.GetElementsByTagName("startSpeed")[0].InnerText, out startSpeed))
                startSpeed = 10; // default
            if (!Int32.TryParse(conf.GetElementsByTagName("stride")[0].InnerText, out stride))
                stride = 1; // default
            if (!Int32.TryParse(conf.GetElementsByTagName("threads")[0].InnerText, out threads))
                threads = 1; // default
#if !DEBUG
            if (!Int32.TryParse(conf.GetElementsByTagName("verbal")[0].InnerText, out verbal))
                verbal = 1; // default
#endif
            if (!Int32.TryParse(conf.GetElementsByTagName("connectionTime")[0].InnerText, out connectionTime))
                connectionTime = 1000; // default
        }

        public static CustomConfiguration LoadConfiguration()
        {
            if (_self == null)
            {
                _self = new CustomConfiguration();
            }
            return _self;
        }

        public readonly Boolean useProxy;
        public readonly Boolean encrypt;
        public readonly Latency latency = null;
        public readonly Load load = null;
        public readonly Boolean useIndex;
        public readonly string host;
        public readonly string port;
        public readonly string dbhost;
        public readonly string dbport;
        public readonly string userid;
        public readonly string password;
        public readonly string database;
        public readonly int rows;
        public readonly int multiple;
        public readonly int startSpeed;
        public readonly int stride;
        public readonly int threads;
        public readonly int verbal = 1;
        public readonly int connectionTime;

    }

    public class Test
    {
        public String[] Operations { get; }
        public Test(String[] operations)
        {
            this.Operations = operations;
        }
    }
    public class Load : Test
    {
        public Load(String[] operations) : base(operations) { }
    }
    public class Latency : Test
    {
        public Latency(String[] operations) : base(operations) { }
    }
}
