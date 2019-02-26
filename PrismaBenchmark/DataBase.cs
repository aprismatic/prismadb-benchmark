using System;
using System.Data;
using System.Data.SqlClient;

namespace PrismaBenchmark
{
    class DataBase
    {
        protected static CustomConfiguration conf;
        private SqlConnection connection = null;

        public DataBase(bool first = false)
        {
            conf = CustomConfiguration.LoadConfiguration();
            if (first)
                IsServerConnected();
            else
                Connect();
        }

        public void Connect()
        {
            var bldr = new SqlConnectionStringBuilder
            {
                UserID = conf.userid,
                Password = conf.password,
                DataSource = conf.host + "," + conf.port,
                InitialCatalog = conf.database
            };
            try
            {
                connection = new SqlConnection(bldr.ConnectionString);
                connection.Open();
                //Console.WriteLine("Connected!");
            }
            catch (SqlException e)
            {
                Console.WriteLine("Cannot create connection:\n" + e.Message);
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

        public void IsServerConnected()
        {
            int attempt = 0;
            bool isConnected = false;
            var bldr = new SqlConnectionStringBuilder
            {
                UserID = conf.userid,
                Password = conf.password,
                DataSource = conf.host + "," + conf.port,
                InitialCatalog = conf.database
            };
            while(!isConnected)
            {
                if (attempt < 5)
                {
                    using (var l_oConnection = new SqlConnection(bldr.ConnectionString))
                    {
                        try
                        {
                            l_oConnection.Open();
                            isConnected = true;
                        }
                        catch (SqlException)
                        {
                            isConnected = false;
                        }
                    }
                    attempt++;
                    System.Threading.Thread.Sleep(5000);
                }
                else
                {
                    Console.WriteLine("Connect failed. Press any key to exit ...");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }
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
            SqlCommand cmd = new SqlCommand
            {
                CommandText = query,
                Connection = connection,
                CommandType = CommandType.Text,
                CommandTimeout = 300
            };
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Close();
            }
            return 0;
        }

        public string ExecuteReader(string query)
        {
            if (connection == null)
                return null;
            SqlCommand cmd = new SqlCommand
            {
                CommandText = query,
                Connection = connection,
                CommandType = CommandType.Text,
                CommandTimeout = 300
            };
            using (SqlDataReader reader = cmd.ExecuteReader())
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