using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Collections;

namespace PrismaBenchmark
{
    class LatencyBenchmark:Benchmark
    {
        public LatencyBenchmark() : base()
        {
            ds = new DataStore();
        }
        private void InsertionTest(int numberOfRecords) // depends on setup() call to create the table
        {
            int AVERAGE_OVER = 10;
            long total_latency = 0;
            if (numberOfRecords == 1)
                Console.WriteLine("Benchmarking single insertion ... ");
            else
                Console.WriteLine("Benchmarking multiple insertions ... ");
            //DBConnection db = DBConnection.Instance();
            for (var i = 0; i < AVERAGE_OVER; i++)
            {
                
                String query = GenerateInsertQuery(numberOfRecords);
                // execute query
                long elapsed = ds.Execute(query); // return latency for executing the query
                // measure latency
                if (elapsed != -1)
                    total_latency += elapsed;
                else
                {
                    // query caused error, caught and log out from DataStore. Stop execution here.
                    return;
                }
            }
            float average_latency = (float)total_latency / (AVERAGE_OVER);
            Console.WriteLine("Insertion {1} record takes on average: {0}ms", average_latency, numberOfRecords);
        }

        private void SelectionTest(Boolean single)
        {
            int AVERAGE_OVER = 10;
            long total_latency = 0;
            int rows = single ? 1 : conf.multiple;
            if (single)
                Console.WriteLine("Benchmarking {0} selection ... ", rows);
            else
                Console.WriteLine("Benchmarking {0} selections ...", rows);
            for (var i = 0; i < AVERAGE_OVER; i++)
            {
                String query = GenerateSelectQuery(single);
                // execute query
                long elapsed = ds.Execute(query); // return latency for executing the query
                                                    // measure latency
                if (elapsed != -1)
                    total_latency += elapsed;
                else
                {
                    // query caused error, caught and log out from DataStore. Stop execution here.
                    return;
                }
            }
            float average_latency = (float)total_latency / (AVERAGE_OVER);
            Console.WriteLine("Select {1} record(s) takes on average: {0}ms", average_latency, rows);
        }

        private void SelectionTestWithJoin(Boolean single)
        {
            int AVERAGE_OVER = 10;
            long total_latency = 0;
            int rows = single ? 1 : conf.multiple*conf.multiple;
            if (single)
                Console.WriteLine("Benchmarking {0} selection with join ... ", rows);
            else
                Console.WriteLine("Benchmarking {0} selections with join ...", rows);
            for (var i = 0; i < AVERAGE_OVER; i++)
            {
                String query = GenerateSelectJoinQuery(single);
                // execute query
                long elapsed = ds.Execute(query); // return latency for executing the query
                                                    // measure latency
                if (elapsed != -1)
                    total_latency += elapsed;
                else
                {
                    // query caused error, caught and log out from DataStore. Stop execution here.
                    return;
                }
            }
            float average_latency = (float)total_latency / (AVERAGE_OVER);
            Console.WriteLine("Select {1} record(s) with joins takes on average: {0}ms", average_latency, rows);
        }

        private void UpdateTest(Boolean single)
        {
            int AVERAGE_OVER = 10;
            long total_latency = 0;
            int rows = single ? 1 : conf.multiple;
            if (single)
                Console.WriteLine("Benchmarking update on {0} row... ", rows);
            else
                Console.WriteLine("Benchmarking update on {0} rows... ", rows);
            for (var i = 0; i < AVERAGE_OVER; i++)
            {
                String query = GenerateUpdateQuery(single); // GenerateUpdateQuery(single);
                // execute query
                long elapsed = ds.Execute(query); // return latency for executing the query
                                                  // measure latency
                if (elapsed != -1)
                    total_latency += elapsed;
                else
                {
                    // query caused error, caught and log out from DataStore. Stop execution here.
                    return;
                }
            }
            float average_latency = (float)total_latency / (AVERAGE_OVER);
            Console.WriteLine("Update {1} record(s) takes on average: {0}ms", average_latency, rows);
        }
        private void DeleteTest(int rows)
        {
            int AVERAGE_OVER = 10;
            long total_latency = 0;
            Console.WriteLine("Benchmarking deletion of {0} rows ... ", rows);
            int targetA = 10;
            // clear targetA rows beforehand
            ds.Execute(GenerateDeleteQuery(targetA));
            for (var i = 0; i < AVERAGE_OVER; i++)
            {
                // prepre data to delete
                List<ArrayList> data = dataGen.GetDataRowsForMultiRange(rows, targetA);
                ds.Insert("t1", data);
                //// deletion
                String query = GenerateDeleteQuery(targetA); // GenerateUpdateQuery(single);
                // execute query
                long elapsed = ds.Execute(query); // return latency for executing the query
                                                  // measure latency
                if (elapsed != -1)
                    total_latency += elapsed;
                else
                {
                    // query caused error, caught and log out from DataStore. Stop execution here.
                    return;
                }
            }
            float average_latency = (float)total_latency / (AVERAGE_OVER);
            Console.WriteLine("Delete {1} record(s) takes on average: {0}ms", average_latency, rows);
        }
        public override void RunBenchMark()
        {
            Console.WriteLine("Start Latency Benchmarking ... ");
            foreach (var test in conf.latency.Operations)
            {
                switch (test)
                {
                    case "insert":
                        Setup();
                        InsertionTest(1);
                        InsertionTest(10);
                        break;
                    case "select":
                        SetupForSelect();
                        SelectionTest(true);
                        SelectionTest(false);
                        SelectionTestWithJoin(true);
                        SelectionTestWithJoin(false);
                        break;
                    case "update":
                        Setup();
                        UpdateTest(true);
                        UpdateTest(false);
                        break;
                    case "delete":
                        Setup();
                        DeleteTest(1);
                        DeleteTest(10);
                        break;
                }
            }
            /////
            dataGen.ResetNextSingle();
            Console.WriteLine("Finish Latency Benchmarking ... ");
        }

    }
}
