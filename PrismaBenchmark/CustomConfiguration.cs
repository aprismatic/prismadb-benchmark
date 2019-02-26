using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace PrismaBenchmark
{
    class CustomConfiguration
    {
        private static CustomConfiguration _self = null;

        private CustomConfiguration() {

            IConfiguration conf = new ConfigurationBuilder()
                .AddJsonFile("settings.json", true, false)
                .AddEnvironmentVariables()
                .Build();
#if !DEBUG
            host = conf["host"];
            dbhost = conf["dbhost"];
#endif
            port = conf["port"];
            dbport = conf["dbport"];
            userid = conf["userid"];
            password = conf["password"];
            SqlPassword = conf["SqlPassword"];
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
            if (!Int32.TryParse(conf["rows"], out rows))
                rows = 500000; // default
            List<string> temp = new List<string>();
            foreach (var setting in conf.GetSection("Operations").GetChildren())
            {
                temp.Add(setting.Value);
            };
            load = new Load(temp);
            //// loadTest variables
            if (!Int32.TryParse(conf["multiple"], out multiple))
                multiple = 10; // default
            if (!Int32.TryParse(conf["startSpeed"], out startSpeed))
                startSpeed = 10; // default
            if (!Int32.TryParse(conf["stride"], out stride))
                stride = 1; // default
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
        public readonly Boolean useIndex;
        public readonly string host = "localhost";
        public readonly string port;
        public readonly string dbhost = "localhost";
        public readonly string dbport;
        public readonly string userid;
        public readonly string password;
        public readonly string SqlPassword;
        public readonly string database;
        public readonly int rows;
        public readonly int multiple;
        public readonly int startSpeed;
        public readonly int stride;
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
