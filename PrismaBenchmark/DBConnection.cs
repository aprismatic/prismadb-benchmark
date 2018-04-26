using System;
using MySql.Data.MySqlClient;


namespace PrismaBenchmark
{
    [Obsolete("Not used anymore", true)]
    class DBConnection
    {
        private DBConnection()
        {
        }

        private string databaseName = string.Empty;
        public string DatabaseName
        {
            get { return databaseName; }
            set { databaseName = value; }
        }

        public string Password { get; set; }
        private MySqlConnection connection = null;
        public MySqlConnection Connection
        {
            get { return connection; }
        }

        private static DBConnection _instance = null;
        public static DBConnection Instance()
        {
            if (_instance == null)
                _instance = new DBConnection();
            return _instance;
        }

        public bool IsConnect()
        {
            if (Connection == null)
            {
                var bldr = new MySqlConnectionStringBuilder
                {
                    ["user id"] = "root",
                    ["password"] = "toor",
                    ["server"] = "127.0.0.1",
                    ["port"] = "4000", // proxy
                    ["database"] = "testdb",
                };
                try
                {
                    connection = new MySqlConnection(bldr.ConnectionString);
                    connection.Open();
                    Console.WriteLine("Connected!");
                } catch (MySqlException ex)
                {
                    Console.WriteLine("Cannot create connection:\n" + ex.Message);
                    connection = null;
                    return false;
                }
            }

            return true;
        }

        public void Close()
        {
            connection.Close();
            //connection = null;
        }
    }
}
