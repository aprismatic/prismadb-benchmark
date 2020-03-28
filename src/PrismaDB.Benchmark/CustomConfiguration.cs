using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace PrismaDB.Benchmark
{
    class CustomConfiguration
    {
        private static CustomConfiguration _self = null;

        private CustomConfiguration()
        {

            IConfiguration conf = new ConfigurationBuilder()
                .AddJsonFile("settings.json", true, false)
                .AddEnvironmentVariables()
                .Build();
            host = conf["host"];
            port = conf["port"];
            userid = conf["userid"];
            password = conf["password"];
            SqlPassword = conf["SqlPassword"];
            BuildVersion = conf["BuildVersion"];
            ServerType = conf["ServerType"];
            switch (ServerType)
            {
                case "mssql":
                    userid = conf["msuserid"];
                    break;
                case "mysql":
                    userid = conf["myuserid"];
                    break;
                case "postgres":
                    userid = conf["pguserid"];
                    break;
            }
            database = conf["database"];
            if (!Boolean.TryParse(conf["useProxy"], out useProxy))
                useProxy = true; // default
            if (!Boolean.TryParse(conf["encrypt"], out encrypt))
                encrypt = true; // default
            if (!useProxy && encrypt)
            {
                throw new Exception("Error: Inappropriate configuration - cannot encrypt while not use proxy!");
            }
            if (!Boolean.TryParse(conf["useIndex"], out useIndex))
                useIndex = true; // default
            List<string> temp = new List<string>();
            foreach (var setting in conf.GetSection("operations").GetChildren())
            {
                temp.Add(setting.Value);
            };
            load = new Load(temp);
            foreach (var setting in conf.GetSection("sizes").GetChildren())
            {
                sizes.Add(Int32.TryParse(setting.Value, out int size) ? size : 10000);
            };
            //// loadTest variables
            if (!Int32.TryParse(conf["multiple"], out multiple))
                multiple = 10; // default
            if (!Int32.TryParse(conf["threads"], out threads))
                threads = 1; // default
#if !DEBUG
            if (!Int32.TryParse(conf["verbal"], out verbal))
                verbal = 1; // default
#endif
            if (!Int32.TryParse(conf["connectionTime"], out connectionTime))
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
        public readonly Load load = null;
        public readonly List<int> sizes = new List<int>();
        public readonly Boolean useIndex;
        public readonly string host = "127.0.0.1";
        public readonly string port;
        public readonly string userid;
        public readonly string password;
        public readonly string SqlPassword;
        public readonly string BuildVersion;
        public readonly string database;
        public readonly string ServerType;
        public readonly int multiple;
        public readonly int threads;
        public readonly int verbal = 1;
        public readonly int connectionTime;

    }

    public class Load
    {
        public List<string> Operations { get; }

        public Load(List<string> operations)
        {
            Operations = operations;
        }
    }
}
