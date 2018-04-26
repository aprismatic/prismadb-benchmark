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
        protected static CustomConfiguration conf;
        protected DataStore ds;
        protected static Random rand = new Random();
        protected static DataGenerator dataGen = DataGenerator.Instance(rand);
        public Benchmark()
        {
            conf = CustomConfiguration.LoadConfiguration(); // might be used for other configurations
            ds = new DataStore(); // DataStore take care of db config
        }
        protected void Setup()
        {
            ds.CreateTable("t1");
            ds.CreateTable("t2");
            List<ArrayList> data1 = new List<ArrayList>();
            List<ArrayList> data2 = new List<ArrayList>();
            data1.AddRange(dataGen.GetDataRowsForMultiRange(conf.multiple));
            data2.AddRange(dataGen.GetDataRowsForMultiRange(conf.multiple));
            List<List<ArrayList>> single = dataGen.GetDataRows(80, range:1, copy:2);
            data1.AddRange(single[0]);
            data2.AddRange(single[1]);
            ds.Insert("t1", data1);
            ds.Insert("t2", data2);
        }

        protected string GenerateInsertQuery(int numberOfRecords, int table=1)
        {
            List<ArrayList> data = dataGen.GetDataRows(numberOfRecords, range: 0, copy: 1)[0];
            string query = QueryConstructor.ConstructInsertQuery(string.Format("t{0}", table), data);
            return query;
        }

        protected string GenerateSelectQuery(bool single = true)
        {
            int a = single ? rand.Next(20, 100) : rand.Next(10,20);
            // TODO: add select a+b, a*b after this operation is more stable against issue 21
            return QueryConstructor.ConstructSelectQuery(a);
        }

        protected string GenerateSelectJoinQuery(bool single = true)
        {
            int a = single ? rand.Next(20, 100): rand.Next(10, 20);
            return QueryConstructor.ConstructSelectJoinQuery(a);
        }
        protected string GenerateUpdateQuery(bool single = true)
        {
            int a = single ? rand.Next(20, 100) : rand.Next(10, 20);
            ArrayList data = dataGen.GetDataRows(1, range: 0)[0][0]; // get from random range as we update a to the same value
            return QueryConstructor.ConstructUpdateQuery(a, data);
        }
        protected string GenerateDeleteQuery(int targetA)
        {
            return QueryConstructor.ConstructDeleteQuery(targetA);
        }
        abstract public void RunBenchMark();

    }
}
