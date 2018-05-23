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
    class DataStore
    {
        protected static CustomConfiguration conf;
        private MySqlConnection connection = null;
        public DataStore()
        {
            conf = CustomConfiguration.LoadConfiguration();
            var bldr = new MySqlConnectionStringBuilder
            {
                ["user id"] = "root",
                ["password"] = "toor",
                ["server"] = conf.host,
                ["port"] = conf.port, //conf.useProxy? "4000": "3306", // proxy
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
        private void Reconnect()
        {
            var bldr = new MySqlConnectionStringBuilder
            {
                ["user id"] = "root",
                ["password"] = "toor",
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
        private bool Retry(int times=3)
        {
            int retry = 0;
            do
            {
                if (retry > times) return false;
                retry++;
                Console.WriteLine("Trying to reconnect the {0} time ... ", retry);
                Reconnect();
            } while (connection == null) ;
            return true;
        }
        public void CreateTable(String tableName, Boolean overwrite = true)
        // hardcoded schema at the moment
        //  a INT ENCRYPTED FOR(INTEGER_MULTIPLICATION, INTEGER_ADDITION, SEARCH),
        //  b INT ENCRYPTED FOR(INTEGER_MULTIPLICATION, INTEGER_ADDITION, SEARCH),
        //  c INT,
        //  d VARCHAR(30) ENCRYPTED FOR(STORE, SEARCH),
        //  e VARCHAR(30)
        {
            // same schema specified in configuration
            if (connection == null)
            {
                Console.WriteLine("There is no connection!");
                return;
            }
            // build query
            String query;
            if (conf.useProxy && conf.encrypt)
            {
                query = String.Format(@"CREATE TABLE {0}
                (
                    a INT ENCRYPTED FOR(INTEGER_MULTIPLICATION, INTEGER_ADDITION, SEARCH),
                    b INT ENCRYPTED FOR(INTEGER_MULTIPLICATION, INTEGER_ADDITION, SEARCH),
                    c INT,
                    d VARCHAR(30) ENCRYPTED FOR(STORE, SEARCH),
                    e VARCHAR(30)
                );", tableName);
            } else if (conf.useIndex)
            {
                query = String.Format(@"CREATE TABLE {0}
                (
                    a INT,
                    b INT,
                    c INT,
                    d VARCHAR(30),
                    e VARCHAR(30));
                    ALTER TABLE {0} ADD INDEX (a);"
                , tableName);
            } else
            {
                query = String.Format(@"CREATE TABLE {0}
                (
                    a INT,
                    b INT,
                    c INT,
                    d VARCHAR(30),
                    e VARCHAR(30)
                );", tableName);
            }

            // execute query
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = query.ToString();
            cmd.Connection = connection;
            cmd.CommandType = CommandType.Text;
            try
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine(String.Format("Table {0} created.", tableName));
            }
            catch (MySqlException e)
            {
                if (e.Message == String.Format("Table '{0}' already exists", tableName))
                    if (overwrite)
                    {
                        cmd.CommandText = String.Format("DROP TABLE {0}", tableName);
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Deleted previously created table with the same name!");
                        CreateTable(tableName);
                        return;
                    }
                Console.WriteLine("Query caused error: " + e.Message);
                return;
            }

        }

        public long Insert(string tableName, List<ArrayList> tuples) // return runtime in ms
        {
            if (connection == null)
            {
                Console.WriteLine("There is no connection!");
                return -1;
            }
            string query = QueryConstructor.ConstructInsertQuery(tableName, tuples);
            // execute query
            return Execute(query);
        }
        public void Update(string tableName)
        {
            
        }
        public long Execute(String query)
        {
            if (connection == null)
            {
                Console.WriteLine("There is no connection!");
                if (!Retry())
                    return -1;
            }
            // execute query
            try
            {
                MySqlCommand cmd = new MySqlCommand
                {
                    CommandText = query,
                    Connection = connection,
                    CommandType = CommandType.Text
                };
                // start clock
                var watch = Stopwatch.StartNew();
                // execute query
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    reader.Close();
                }
                // measure latency
                watch.Stop();
                var elapsed = watch.ElapsedMilliseconds;
                return elapsed;
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Query caused error: {0}", e.Message);
                return -1;
            }
        }
        public void Close() {
            connection.Close();
            connection = null;
        }
    }
}
