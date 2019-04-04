using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PrismaBenchmark
{
    abstract class Benchmark
    {
        public static CustomConfiguration conf;
        protected DataBase ds;
        protected static Random rand = new Random();
        private int single_size;
        private int multiple_size;
        private const string MSSQL = "mssql";
        private const string MYSQL = "mysql";
        private const string PGSQL = "postgres";

        public Benchmark()
        {
            conf = CustomConfiguration.LoadConfiguration(); // might be used for other configurations
            ds = new DataBase(); // DataStore take care of db config
        }

        protected void CreateTable(string tableName, bool overwrite = true, bool encrypt = true)
        {
            // build query
            string query;
            if (encrypt)
            {
                query = String.Format(@"CREATE TABLE {0}
                (
                    a INT ENCRYPTED FOR(MULTIPLICATION, ADDITION, SEARCH, RANGE),
                    b INT ENCRYPTED FOR(ADDITION),
                    c INT ENCRYPTED FOR(MULTIPLICATION),
                    d INT,
                    e VARCHAR(30) ENCRYPTED FOR(SEARCH)
                );", tableName);
            }
            else
            {
                query = String.Format(@"CREATE TABLE {0}
                (
                    a INT ENCRYPTED FOR(SEARCH),
                    b INT,
                    c INT,
                    d INT,
                    e VARCHAR(30)
                );", tableName);
            }

            string create_index_i1 = "", create_index_i2 = "";
            switch (conf.ServerType)
            {
                case MSSQL:
                    create_index_i1 = String.Format(@"CREATE INDEX {0}_i1 on {0} ([a.Fingerprint])", tableName);
                    create_index_i2 = String.Format(@"CREATE INDEX {0}_i2 on {0} ([d])", tableName);
                    break;
                case MYSQL:
                    create_index_i1 = String.Format(@"CREATE INDEX {0}_i1 on {0} (`a.Fingerprint`)", tableName);
                    create_index_i2 = String.Format(@"CREATE INDEX {0}_i2 on {0} (`d`)", tableName);
                    break;
                case PGSQL:
                    create_index_i1 = String.Format(@"CREATE INDEX {0}_i1 on {0} (""a.Fingerprint"")", tableName);
                    create_index_i2 = String.Format(@"CREATE INDEX {0}_i2 on {0} (""d"")", tableName);
                    break;
            }

            // execute query
            try
            {
                ds.ExecuteNonQuery(query);
                ds.ExecuteNonQuery(create_index_i1);
                ds.ExecuteNonQuery(create_index_i2);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("already"))
                    if (overwrite)
                    {
                        DropTable(tableName);
                        CreateTable(tableName, overwrite, encrypt);
                        return;
                    }
                Console.WriteLine("Query caused error: " + e.Message);
                return;
            }
            Console.WriteLine(String.Format("Table {0} created.", tableName));
        }

        protected void DropTable(string tableName)
        {
            string query = String.Format("DROP TABLE {0}", tableName);
            if(ExecuteQuery(query) != -1)
                Console.WriteLine(String.Format("Table {0} dropped.", tableName));
        }

        protected long ExecuteQuery(string query)
        {
            try
            {
                return ds.ExecuteNonQuery(query);
            }
            catch (SqlException e)
            {
                Console.WriteLine("Query caused error: {0}", e.Message);
                return  -1;
            }
        }

        protected string ExecuteReader(string query)
        {
            try
            {
                return ds.ExecuteReader(query);
            }
            catch (SqlException e)
            {
                Console.WriteLine("Query caused error: {0}", e.Message);
                return null;
            }
        }

        protected void Close()
        {
            ds.Close();
        }

        protected void SetupForSelect(int size, string table = "t1")
        // populate tables with 500M rows
        {
            single_size = 4 * size / 10;
            multiple_size = 6 * size / 10;
            ConcurrentQueue<string> cq = new ConcurrentQueue<string>();

            // populate single range
            int batch_size = 1000;
            while (size % batch_size != 0)
                batch_size /= 10;

            var watch = Stopwatch.StartNew();

            for (int i = 0; i < single_size / batch_size; i++)
            {
                string query = QueryConstructor.ConstructInsertQuery(table,
                    DataGenerator.GetDataRowsForSelect(i * batch_size, batch_size: batch_size));
                // execute query
                cq.Enqueue(query);
            }

            while ((multiple_size / conf.multiple ) % batch_size != 0)
                batch_size /= 10;
            // populate multiple range
            for (int m = 0; m < conf.multiple; m++)
            {
                for (int i = 0; i < multiple_size / (conf.multiple * batch_size); i++)
                {
                    string query = QueryConstructor.ConstructInsertQuery(table,
                        DataGenerator.GetDataRowsForSelect(single_size + i * batch_size, batch_size: batch_size));
                    // execute query
                    cq.Enqueue(query);
                }
            }

            void startWorker()
            {
                DataBase database = new DataBase();
                while (cq.TryDequeue(out string query))
                {
                    database.ExecuteNonQuery(query);
                }
                database.Close();
            }

            Parallel.For(0, 5, i => startWorker());

            watch.Stop();
            Console.WriteLine("====Time of INSERT {0} records: {1}====\n", size, watch.Elapsed.ToString(@"hh\:mm\:ss\.fff"));
        }

        protected string GenerateInsertQuery(int numberOfRecords)
        {
            List<ArrayList> data = DataGenerator.GetDataRows(numberOfRecords);
            string query = QueryConstructor.ConstructInsertQuery("t1", data);
            return query;
        }

        protected string GenerateSelectQuery(bool single = true, int operationCase = 1)
        {
            int a = single ? rand.Next(0, single_size) : rand.Next(single_size, single_size + multiple_size / conf.multiple);
            string operation;
            switch (operationCase)
            {
                case 2:
                    operation = "a + b";
                    break;
                case 3:
                    operation = "a * c";
                    break;
                case 4:
                    operation = "a + a * c + b";
                    break;
                default:
                    operation = "a";
                    break;
            }
            return QueryConstructor.ConstructSelectQuery(operation, a);
        }

        protected string GenerateSelectWithoutQuery(bool single = true)
        {
            int a = single ? rand.Next(0, single_size) : rand.Next(single_size, single_size + multiple_size / conf.multiple);
            switch (conf.ServerType)
            {
                case MSSQL:
                    return QueryConstructor.ConstructMsSelectWithoutQuery(a);
                case MYSQL:
                case PGSQL:
                    return QueryConstructor.ConstructMySelectWithoutQuery(a);
            }
            return null;
        }

        protected string GenerateSelectLimitQuery()
        {
            switch(conf.ServerType)
            {
                case MSSQL:
                    return "SELECT TOP 1 * FROM t1";
                case MYSQL:
                case PGSQL:
                    return "SELECT * FROM t1 LIMIT 1";
            }
            return null;
        }

        protected string GenerateDeleteQuery(bool single = true)
        {
            int a = single ? rand.Next(0, single_size) : rand.Next(single_size, single_size + multiple_size / conf.multiple);
            return QueryConstructor.ConstructDeleteQuery(a);
        }

        protected string GenerateSelectJoinQuery(int size, bool single = true)
        {
            int single_size = 4 * size / 10;
            int multiple_size = 4 * size / 10;
            int a = single ? rand.Next(0, single_size) : rand.Next(single_size, single_size + multiple_size / conf.multiple);
            return QueryConstructor.ConstructSelectJoinQuery(a);
        }

        protected string GenerateUpdateQuery(bool single = true)
        {
            int a = single ? rand.Next(0, single_size) : rand.Next(single_size, single_size + multiple_size / conf.multiple);
            ArrayList data = DataGenerator.GetDataRows(1)[0]; // get from random range as we update a to the same value
            return QueryConstructor.ConstructUpdateQuery(a, data);
        }

        protected string GenerateDecryptQuery(bool check = false, bool str = false)
        {
            return QueryConstructor.ConstructDecryptQuery(check, str);
        }

        protected string GenerateEncryptQuery(bool check = false, int typeCase = 1)
        {
            string type;
            switch (typeCase)
            {
                case 1:
                    type = "STORE";
                    break;
                case 2:
                    type = "SEARCH";
                    break;
                case 3:
                    type = "RANGE";
                    break;
                case 4:
                    type = "ADDITION";
                    break;
                case 5:
                    type = "MULTIPLICATION";
                    break;
                case 6:
                    type = "SEARCH, STORE, RANGE, ADDITION, MULTIPLICATION";
                    break;
                case 7:
                    type = "WILDCARD";
                    break;
                default:
                    type = "STORE";
                    break;
            }
            return QueryConstructor.ConstructEncryptQuery(check, type);
        }

        protected string GenerateUpdateKeyQuery(bool check = false)
        {
            return QueryConstructor.ConstructUpdateKeyQuery(check);
        }

        abstract public void RunBenchMark();

    }
}
