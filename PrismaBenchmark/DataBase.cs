using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace PrismaBenchmark
{
    class DataBase
    {
        protected static CustomConfiguration conf;
        private MySqlConnection connection = null;

        public DataBase(bool first = false)
        {
            conf = CustomConfiguration.LoadConfiguration();
            if (first)
                InitDataBase();
            else
                Connect();
        }

        public void InitDataBase()
        {
            var bldr = new MySqlConnectionStringBuilder
            {
                ["user id"] = "init",
                ["password"] = "init",
                ["server"] = conf.host,
                ["port"] = conf.port,//conf.useProxy ? "4000" : "3306", // proxy
                ["database"] = "testdb",
            };
            try
            {
                connection = new MySqlConnection(bldr.ConnectionString);
                connection.Open();
                string Register = $"PRISMADB REGISTER USER '{conf.userid}' PASS '{conf.password}';";
                ExecuteQuery(Register);
                connection.Close();
                //Console.WriteLine("Connected!");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Cannot init connection:\n" + ex.Message);
            }
            connection = null;
        }

        private void Connect()
        {
            var bldr = new MySqlConnectionStringBuilder
            {
                ["user id"] = conf.userid,
                ["password"] = conf.password,
                ["server"] = conf.host,
                ["port"] = conf.port,//conf.useProxy ? "4000" : "3306", // proxy
                ["database"] = "testdb",
            };
            try
            {
                connection = new MySqlConnection(bldr.ConnectionString);
                connection.Open();
                //Console.WriteLine("Connected!");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Cannot create connection:\n" + ex.Message);
                connection = null;
            }
        }

        private bool Retry(int times = 3)
        {
            int retry = 0;
            do
            {
                if (retry > times) return false;
                retry++;
                Console.WriteLine("Trying to reconnect the {0} time ... ", retry);
                Connect();
            } while (connection == null);
            return true;
        }

        public long ExecuteQuery(string query)
        {
            if (connection == null)
            {
                Console.WriteLine("There is no connection!");
                if (!Retry())
                {
                    Console.WriteLine("Connect failed. Press any key to exit ...");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }
            // execute query
            MySqlCommand cmd = new MySqlCommand
            {
                CommandText = query,
                Connection = connection,
                CommandType = CommandType.Text
            };
            // start clock
            //var watch = Stopwatch.StartNew();
            // execute query
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Close();
            }
            // measure latency
            //watch.Stop();
            //var elapsed = watch.ElapsedMilliseconds;
            return 0;
        }

        public string ExecuteReader(string query)
        {
            if (connection == null)
                return null;
            MySqlCommand cmd = new MySqlCommand
            {
                CommandText = query,
                Connection = connection,
                CommandType = CommandType.Text
            };
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while(reader.Read())
                    return reader[1].ToString();
            }
            return null;
        }

        public void Close()
        {
            connection.Close();
            connection = null;
        }
    }
}