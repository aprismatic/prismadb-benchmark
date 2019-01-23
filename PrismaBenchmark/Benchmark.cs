using System;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace PrismaBenchmark
{
    abstract class Benchmark
    {
        public static CustomConfiguration conf;
        protected DataBase ds;
        protected static Random rand = new Random();
        protected static DataGenerator dataGen = DataGenerator.Instance(rand);
        private readonly int single_size;
        private readonly int multiple_size;

        public Benchmark()
        {
            conf = CustomConfiguration.LoadConfiguration(); // might be used for other configurations
            ds = new DataBase(); // DataStore take care of db config
            single_size = 4 * conf.rows / 10;
            multiple_size = 6 * conf.rows / 10;

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
                    d VARCHAR(30) ENCRYPTED FOR(SEARCH),
                    e VARCHAR(30) ENCRYPTED FOR(WILDCARD)
                );", tableName);
            }
            else
            {
                query = String.Format(@"CREATE TABLE {0}
                (
                    a INT ENCRYPTED FOR(SEARCH),
                    b INT,
                    c INT,
                    d VARCHAR(30),
                    e VARCHAR(30)
                );", tableName);
            }

            // execute query
            try
            {
                ds.ExecuteQuery(query);
            }
            catch (MySqlException e)
            {
                if (e.Message == String.Format("Table '{0}' already exists", tableName))
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
                return ds.ExecuteQuery(query);
            }
            catch (MySqlException e)
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
            catch (MySqlException e)
            {
                Console.WriteLine("Query caused error: {0}", e.Message);
                return null;
            }
        }

        protected void Close()
        {
            ds.Close();
        }

        protected void Refresh()
        {
            ds.Close();
            ds = new DataBase();
        }

        protected void SetupForSelect()
            // populate tables with 500M rows
        {
            // populate single range
            int single_batch_size = 1000;
            while (single_size % single_batch_size != 0)
                single_batch_size /= 10;
            for (int i = 0; i < single_size / single_batch_size; i++)
            {
                string query = QueryConstructor.ConstructInsertQuery("t1", 
                    dataGen.GetDataRowsForSelect(i * single_batch_size, batch_size: single_batch_size));
                // execute query
                ExecuteQuery(query);
                query = QueryConstructor.ConstructInsertQuery("t2",
                    dataGen.GetDataRowsForSelect(i * single_batch_size, batch_size: single_batch_size));
                // execute query
                ExecuteQuery(query);
            }
            // populate multiple range
            int multiple_batch_size = multiple_size / conf.multiple;
            for (int m = 0; m < conf.multiple; m++)
            {
                string query = QueryConstructor.ConstructInsertQuery("t1",
                    dataGen.GetDataRowsForSelect(single_size, batch_size: multiple_batch_size));
                // execute query
                ExecuteQuery(query);
                query = QueryConstructor.ConstructInsertQuery("t2",
                    dataGen.GetDataRowsForSelect(single_size, batch_size: multiple_batch_size));
                // execute query
                ExecuteQuery(query);
            }
        }

        protected string GenerateInsertQuery(int numberOfRecords)
        {
            List<ArrayList> data = dataGen.GetDataRows(numberOfRecords, range: 0, copy: 1)[0];
            string query = QueryConstructor.ConstructInsertQuery("t1", data);
            return query;
        }

        protected string GenerateSelectQuery(bool single = true, int operationCase = 1)
        {
            int a = single ? rand.Next(0, single_size) : rand.Next(single_size, single_size + multiple_size/conf.multiple);
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
            return QueryConstructor.ConstructSelectWithoutQuery(a);
        }

        protected string GenerateDeleteQuery(bool single = true)
        {
            int a = single ? rand.Next(0, single_size) : rand.Next(single_size, single_size + multiple_size / conf.multiple);
            return QueryConstructor.ConstructDeleteQuery(a);
        }

        protected string GenerateSelectJoinQuery(bool single = true)
        {
            int a = single ? rand.Next(0, single_size) : rand.Next(single_size, single_size + multiple_size / conf.multiple);
            return QueryConstructor.ConstructSelectJoinQuery(a);
        }

        protected string GenerateUpdateQuery(bool single = true)
        {
            int a = single ? rand.Next(20, 100) : rand.Next(10, 20);
            ArrayList data = dataGen.GetDataRows(1, range: 0)[0][0]; // get from random range as we update a to the same value
            return QueryConstructor.ConstructUpdateQuery(a, data);
        }

        protected string GenerateDecryptQuery(bool check = false)
        {
            return QueryConstructor.ConstructDecryptQuery(check);
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
                    type = "WILDCARD";
                    break;
                case 5:
                    type = "ADDITION";
                    break;
                case 6:
                    type = "MULTIPLICATION";
                    break;
                case 7:
                    type = "SEARCH, STORE, RANGE, ADDITION, MULTIPLICATION";
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
