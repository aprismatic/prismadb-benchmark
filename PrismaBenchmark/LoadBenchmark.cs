using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PrismaBenchmark
{
    class LoadBenchmark : Benchmark
    {
        private static Dictionary<string, int> queryTypeMap = new Dictionary<string, int>();
        public LoadBenchmark(): base()
        {
            CreateTable("t1");
            CreateTable("t2", encrypt: false);
            foreach (var test in conf.load.Operations)
            {
                switch (test)
                {
                    case "insert":
                        queryTypeMap.Add("SINGLE_INSERT", 1);
                        queryTypeMap.Add("MULTIPLE_INSERT", 2);
                        break;
                    case "select":
                        queryTypeMap.Add("SINGLE_SELECT_WITHOUT_EN/DECRYPTION", 3);
                        queryTypeMap.Add("MULTIPLE_SELECT_WITHOUT_EN/DECRYPTION", 4);
                        queryTypeMap.Add("SINGLE_SELECT", 5);
                        queryTypeMap.Add("MULTIPLE_SELECT", 6);
                        queryTypeMap.Add("SINGLE_SELECT_ADDITION", 7);
                        queryTypeMap.Add("MULTIPLE_SELECT_ADDITION", 8);
                        queryTypeMap.Add("SINGLE_SELECT_MULTIPLICATION", 9);
                        queryTypeMap.Add("MULTIPLE_SELECT_MULTIPLICATION", 10);
                        queryTypeMap.Add("SINGLE_SELECT_COMPOSITION", 11);
                        queryTypeMap.Add("MULTIPLE_SELECT_COMPOSITION", 12);
                        queryTypeMap.Add("SINGLE_SELECT_WITH_JOIN", 13);
                        queryTypeMap.Add("MULTIPLE_SELECT_WITH_JOIN", 14);
                        break;
                    case "update":
                        queryTypeMap.Add("SINGLE_UPDATE", 15);
                        queryTypeMap.Add("MULTIPLE_UPDATE", 16);
                        break;
                    case "delete":
                        queryTypeMap.Add("SINGLE_DELETE", 17);
                        queryTypeMap.Add("MULTIPLE_DELETE", 18);
                        break;
                    case "decrypt":
                        queryTypeMap.Add("DECRYPT", 19);
                        break;
                    case "encrypt":
                        queryTypeMap.Add("ENCRYPT_STORE", 20);
                        queryTypeMap.Add("ENCRYPT_SEARCH", 21);
                        queryTypeMap.Add("ENCRYPT_RANGE", 22);
                        queryTypeMap.Add("ENCRYPT_WILDCARD", 23);
                        queryTypeMap.Add("ENCRYPT_ADDITION", 24);
                        queryTypeMap.Add("ENCRYPT_MULTIPLICATION", 25);
                        queryTypeMap.Add("ENCRYPT_ALL", 26);
                        break;
                    case "updatekey":
                        queryTypeMap.Add("UPDATE_KEY", 27);
                        break;
                }
            }
        }

        private string ProduceQuery(int type)
        {
            switch (type){
                case 1: return GenerateInsertQuery(1); // single insertion
                case 2: return GenerateInsertQuery(10); // multiple insertion
                case 3: return GenerateSelectWithoutQuery(single: true); // single selection without en/decryption
                case 4: return GenerateSelectWithoutQuery(single: false); // multiple selection without en/decryption
                case 5: return GenerateSelectQuery(single: true); // single selection
                case 6: return GenerateSelectQuery(single: false); // multiple selection
                case 7: return GenerateSelectQuery(single: true, operationCase: 2); // single selection a+b
                case 8: return GenerateSelectQuery(single: false, operationCase: 2); // multiple selection a+b
                case 9: return GenerateSelectQuery(single: true, operationCase: 3); // single selection a*c
                case 10: return GenerateSelectQuery(single: false, operationCase: 3); // multiple selection a*c
                case 11: return GenerateSelectQuery(single: true, operationCase: 4); // single selection a+a*c+b
                case 12: return GenerateSelectQuery(single: false, operationCase: 4); // multiple selection a+a*c+b
                case 13: return GenerateSelectJoinQuery(true); // single selection with join
                case 14: return GenerateSelectJoinQuery(false); // multiple selection with join
                case 15: return GenerateUpdateQuery(true); // update single row
                case 16: return GenerateUpdateQuery(false); // update multiple rows
                case 17: return GenerateDeleteQuery(true); // delete single row
                case 18: return GenerateDeleteQuery(false); // delete multiple rows
                case 19: return GenerateDecryptQuery(); // decrypt columns
                case 29: return GenerateDecryptQuery(str: true); // decrypt columns
                case 20: return GenerateEncryptQuery(typeCase: 1); // encrypt columns for store
                case 21: return GenerateEncryptQuery(typeCase: 2); // encrypt columns for search
                case 22: return GenerateEncryptQuery(typeCase: 3); // encrypt columns for range
                case 23: return GenerateEncryptQuery(typeCase: 4); // encrypt columns for wildcard
                case 24: return GenerateEncryptQuery(typeCase: 5); // encrypt columns for paillier addition
                case 25: return GenerateEncryptQuery(typeCase: 6); // encrypt columns for elgmal multiplication
                case 26: return GenerateEncryptQuery(typeCase: 7); // encrypt columns for all types
                case 27: return GenerateUpdateKeyQuery(); // update key for table
                default: return GenerateInsertQuery(1);
            }
        }

        private string CheckQuery(int type)
        {
            switch (type)
            {
                case 19: return GenerateDecryptQuery(check: true); // check status of decrypt columns
                case 29: return GenerateDecryptQuery(check: true, str: true); // check status of decrypt columns
                case 20: return GenerateEncryptQuery(true, typeCase: 1); // check status of encrypt columns for store
                case 21: return GenerateEncryptQuery(true, typeCase: 2); // check status of encrypt columns for search
                case 22: return GenerateEncryptQuery(true, typeCase: 3); // check status of encrypt columns for range
                case 23: return GenerateEncryptQuery(true, typeCase: 4); // check status of encrypt columns for wildcard
                case 24: return GenerateEncryptQuery(true, typeCase: 5); // check status of encrypt columns for paillier addition
                case 25: return GenerateEncryptQuery(true, typeCase: 6); // check status of encrypt columns for elgmal multiplication
                case 26: return GenerateEncryptQuery(true, typeCase: 7); // check status of encrypt columns for all types
                case 27: return GenerateUpdateKeyQuery(true); // check status of update key for tables
                default: return GenerateInsertQuery(1);
            }

        }

        public override void RunBenchMark()
        {
            Console.WriteLine("\nStart Load Benchmarking ... \n");

            foreach (var entry in queryTypeMap)
            {
                string queryType = entry.Key;
                int queryTypeInt = entry.Value;
                if (queryTypeInt <= 27 && queryTypeInt >= 19)
                {
                    // Drop table t1 before update keys
                    if (queryTypeInt == 27)
                        DropTable("t1");

                    RunTime(queryTypeInt, queryType);

                    // Decrypt the column after Encrypt
                    if (queryTypeInt >= 20 && queryTypeInt <= 26)
                    {
                        if (queryTypeInt == 23)
                            RunTime(29, "DECRYPT_" + queryType.Split("_")[1]);
                        else
                            RunTime(19, "DECRYPT_" + queryType.Split("_")[1]);
                    }
                }
                else
                {
                    if (queryTypeInt == 3)
                    {
                        CreateTable("t1");
                        SetupForSelect();
                    }
                    if (queryTypeInt >= 15 && queryTypeInt <= 18)
                        RunLoad(ProduceQuery, queryType, conf.startSpeed, conf.stride, 1, conf.verbal);
                    else
                        RunLoad(ProduceQuery, queryType, conf.startSpeed, conf.stride, conf.threads, conf.verbal);
                }
            }
            dataGen.ResetNextSingle();
            Close();

            Console.WriteLine("Finish Load Benchmarking ... ");
        }

        private void RunTime(int queryTypeInt, string queryType)
        {
            Console.WriteLine("Benchmarking load {0}...", queryType);
            string query = ProduceQuery(queryTypeInt);
            string queryCheck = CheckQuery(queryTypeInt);
            string result;
            var watch = Stopwatch.StartNew();

            ExecuteQuery(query);
            Thread.Sleep(500); // Sleep to wait for the last query execute
            do
            {
                result = ExecuteReader(queryCheck);
                Thread.Sleep(100);
            } while (result != "Completed");

            watch.Stop();
            Console.WriteLine("====Time of {0}: {1} ms====\n", queryType, watch.ElapsedMilliseconds);
        }

        protected void RunLoad(Func<int, string> ProduceQuery, string queryType, int startSpeed, int stride, int workers, int verbal)
        {
            Console.WriteLine("Benchmarking load {0}...", queryType);

            ThreadInfo threadInfo;
            int v = startSpeed;
            int queryTypeInt = queryTypeMap.TryGetValue(queryType, out queryTypeInt)? queryTypeInt: 1;
            if (verbal >= 1)
                Console.WriteLine("Start Speed: {0}", v);

            ConcurrentQueue<string> cq = new ConcurrentQueue<string>();
            threadInfo = new ThreadInfo(cq, ProduceQuery, queryTypeInt, v:v, stride:stride, numberOfWorkers:workers, verbal: verbal);
            Task master = Task.Run(() => MasterProc(threadInfo));
            Task worker = Task.Run(() => WorkerProc(threadInfo));
            Task[] tasks = new Task[2] { worker, master };
            Task.WaitAll(tasks);

            if (threadInfo.verbal >= 1)
                Console.WriteLine("\nDone processing!");
            Console.WriteLine("====Max RPS of {0}: {1}====\n", queryType, threadInfo.rps);
        }

        void WorkerProc(Object threadInfo)
        {
            ThreadInfo info = threadInfo as ThreadInfo;
            ConcurrentQueue<string> cq = info.cq;

            int v = info.v;
            // An action to consume the ConcurrentQueue.
            void startWorker()
            {
                DataBase database = new DataBase();
                string query;
                while (info.rps == 0)
                {
                    if (cq.TryDequeue(out query))
                    {
                        database.ExecuteQuery(query);
                        Interlocked.Add(ref info.processed, 1);
                    }
                }
                database.Close();
            }

            Parallel.For(0, info.numberOfWorkers, i => startWorker());
        }

        void MasterProc(Object threadInfo)
        {
            ThreadInfo info = threadInfo as ThreadInfo;
            Thread.Sleep(conf.connectionTime); // wait for workers to connect to db
            ConcurrentQueue<string> cq = info.cq;
            int v = info.v;
            int floor = v;
            int ceil = -1;

            void action()
            {
                do
                {
                    if (info.verbal >= 1)
                        Console.Write("\rSpeed: {0}  ", v); // extra space at the end to make sure to overwrite the previously written line
                    long elapsed = 0;
                    var watch = Stopwatch.StartNew();
                    Parallel.For(0, v, i => {
                        string query = info.produceQuery(info.queryType); // type of query
                        cq.Enqueue(query);
                    });
                    Interlocked.Add(ref info.produced, v);
                    //if (info.verbal >= 2)
                    //    Console.Write("\rQueue now have: {0}", cq.Count);
                    do
                    {
                        elapsed = watch.ElapsedMilliseconds;
                    } while (elapsed < 1000); // each pulse last 1s
                    if (cq.Count > 0 || ceil != -1)
                    {
                        if (cq.Count > 0)
                            ceil = v;
                        else
                            floor = v;
                        v = (floor + ceil) / 2;
                        while (cq.Count > 0) { } // wait for the queue to clear off
                    }
                    else
                    {   // change strategy to doubling speed every cycle
                        floor = v;
                        v *= 2;
                    }
                    if (ceil <= floor + 1 && ceil != -1)
                    {
                        info.rps = ceil; // only 1 master thread write to info.rps -> no need lock
                    }
                    watch.Stop();
                } while (info.rps == 0);
            }

            Parallel.Invoke(action);
        }
    }

    class ThreadInfo
    {
        public ThreadInfo(ConcurrentQueue<string> cq, Func<int, string> produceQuery, int queryType, int v = 100, int stride=1, int numberOfWorkers = 1, int verbal=0)
        {
            this.cq = cq;
            this.v = v;
            this.numberOfWorkers = numberOfWorkers;
            this.verbal = verbal;
            this.produceQuery = produceQuery;
            this.queryType = queryType;
            this.stride = stride;
        }
        public int processed = 0;
        public int produced = 0;
        public int rps = 0;
        public ConcurrentQueue<string> cq;
        public readonly int v;
        public readonly int stride;
        public readonly int numberOfWorkers;
        public readonly int verbal;
        public readonly Func<int, string> produceQuery;
        public readonly int queryType;
    }
}
