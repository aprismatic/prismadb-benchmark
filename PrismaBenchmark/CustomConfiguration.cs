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
            ///
            if (conf.GetElementsByTagName("latencyTest").Count != 0)
            {
                XmlNode operations = conf.SelectSingleNode("configuration/latencyTest/operations");
                String[] operations_str = new String[operations.ChildNodes.Count];
                for (var i = 0; i < operations.ChildNodes.Count; i++)
                {
                    operations_str[i] = operations.ChildNodes[i].InnerText;
                }
                latency = new Latency(operations_str);
            }
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
            //// latencyTest variables
            if (!Int32.TryParse(conf.GetElementsByTagName("multiple")[0].InnerText, out multiple))
                multiple = 10; // default
            //// loadTest variables
            if (!Int32.TryParse(conf.GetElementsByTagName("startSpeed")[0].InnerText, out startSpeed))
                startSpeed = 10; // default
            if (!Int32.TryParse(conf.GetElementsByTagName("stride")[0].InnerText, out stride))
                stride = 1; // default
            if (!Int32.TryParse(conf.GetElementsByTagName("workers")[0].InnerText, out workers))
                workers = 1; // default
            if (!Int32.TryParse(conf.GetElementsByTagName("verbal")[0].InnerText, out verbal))
                verbal = 1; // default
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
        public readonly string host;
        public readonly string port;
        public readonly int multiple;
        public readonly int startSpeed;
        public readonly int stride;
        public readonly int workers;
        public readonly int verbal;
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
