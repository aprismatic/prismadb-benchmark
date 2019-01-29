using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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

        private void InitDataBase()
        {
            while (!IsServerConnected()) System.Threading.Thread.Sleep(5000);
            var bldr = new MySqlConnectionStringBuilder
            {
                ["user id"] = "init",
                ["password"] = "init",
                ["server"] = conf.host,
                ["port"] = conf.port,//conf.useProxy ? "4000" : "3306", // proxy
                ["database"] = conf.database,
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
                if (ex.Message != "Couldn't connect to server")
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
                ["database"] = conf.database,
            };
            try
            {
                connection = new MySqlConnection(bldr.ConnectionString);
                connection.Open();
                //Console.WriteLine("Connected!");
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Cannot create connection:\n" + e.Message);
                connection = null;
            }
        }

        public bool IsServerConnected()
        {
            var bldr = new MySqlConnectionStringBuilder
            {
                ["user id"] = conf.userid,
                ["password"] = conf.password,
                ["server"] = conf.dbhost,
                ["port"] = conf.dbport,//conf.useProxy ? "4000" : "3306", // proxy
            };
            using (var l_oConnection = new MySqlConnection(bldr.ConnectionString))
            {
                try
                {
                    l_oConnection.Open();
                    return true;
                }
                catch (MySqlException)
                {
                    return false;
                }
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
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Close();
            }
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