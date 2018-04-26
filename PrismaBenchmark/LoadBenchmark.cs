﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            ds = new DataStore();
            queryTypeMap.Add("SINGLE_INSERT", 1);
            queryTypeMap.Add("MULTIPLE_INSERT", 2);
            queryTypeMap.Add("SINGLE_SELECT", 3);
            queryTypeMap.Add("MULTIPLE_SELECT", 4);
            queryTypeMap.Add("SINGLE_SELECT_WITH_JOIN", 5);
            queryTypeMap.Add("MULTIPLE_SELECT_WITH_JOIN", 6);
            queryTypeMap.Add("SINGLE_UPDATE", 7);
            queryTypeMap.Add("MULTIPLE_UPDATE", 8);
        }
        private string ProduceQuery(int type)
        {
            switch (type){
                case 1: return GenerateInsertQuery(1); // single insertion
                case 2: return GenerateInsertQuery(10); // multiple insertion
                case 3: return GenerateSelectQuery(true); // single selection
                case 4: return GenerateSelectQuery(false); // multiple selection
                case 5: return GenerateSelectJoinQuery(true); // single selection with join
                case 6: return GenerateSelectJoinQuery(false); // multiple selection with join
                case 7: return GenerateUpdateQuery(true); // update single row
                case 8: return GenerateUpdateQuery(false); // update multiple rows
                default: return GenerateInsertQuery(1);
            }

        }
        public override void RunBenchMark()
        {
            Console.WriteLine("Start Load Benchmarking ... ");
            Setup();
            foreach (KeyValuePair<string, int> entry in queryTypeMap)
            {
                string queryType = entry.Key;
                int startSpeed = conf.startSpeed;
                int stride = conf.stride;
                if (!conf.useProxy && queryType.Contains("SELECT"))
                {
                    startSpeed = 900;
                    stride = 50;
                }
                int rpm = RunLoad(ProduceQuery, queryType, verbal: conf.verbal, startSpeed: startSpeed, stride: stride, workers: conf.workers);
                Console.WriteLine("Max rpm {0}: {1}", queryType, rpm);
            }
            dataGen.ResetNextSingle();
            Console.WriteLine("Finish Load Benchmarking ... ");
        }
        protected int RunLoad(Func<int, string> ProduceQuery, string queryType, int startSpeed = 10, int stride = 1, int workers=2,int verbal=0)
        {
            Console.WriteLine("Benchmarking load {0}...", queryType);
            ThreadInfo threadInfo;
            int v = startSpeed;
            int queryTypeInt = queryTypeMap.TryGetValue(queryType, out queryTypeInt)? queryTypeInt: 1;
            ////
            if (verbal >= 1)
                Console.WriteLine("Start Speed: {0}", v);
            ConcurrentQueue<string> cq = new ConcurrentQueue<string>();
            threadInfo = new ThreadInfo(cq, ProduceQuery, queryTypeInt, v:v, stride:stride, numberOfWorkers:workers, verbal: verbal);
            Task master = Task.Run(() => MasterProc(threadInfo));
            Task worker = Task.Run(() => WorkerProc(threadInfo));
            Task[] tasks = new Task[2] { worker, master };
            Task.WaitAll(tasks);
            return threadInfo.rps;
        }
        void WorkerProc(Object threadInfo)
        {
            ThreadInfo info = threadInfo as ThreadInfo;
            ConcurrentQueue<string> cq = info.cq;

            int v = info.v;
            // An action to consume the ConcurrentQueue.
            void startWorker()
            {
                DataStore privateDS = new DataStore();
                string query;
                while (info.rps == 0)
                {
                    if (cq.TryDequeue(out query))
                    {
                        if (privateDS.Execute(query) == -1)
                        {
                            Console.WriteLine("Lost connection.");
                            return;
                        }
                        Interlocked.Add(ref info.processed, 1);
                    }
                }
                if (info.verbal >= 1)
                    Console.WriteLine("Done processing!");
            }

            Parallel.For(0, conf.workers, i => startWorker());
        }
        void MasterProc(Object threadInfo)
        {
            ThreadInfo info = threadInfo as ThreadInfo;
            Thread.Sleep(conf.connectionTime *info.numberOfWorkers); // wait for workers to connect to db
            ConcurrentQueue<string> cq = info.cq;
            int v = info.v;
            void action()
            {
                do
                {
                    if (info.verbal >= 1)
                        Console.WriteLine(String.Format("Speed: {0}", v));
                    long elapsed = 0;
                    var watch = Stopwatch.StartNew();
                    Parallel.For(0, v, i => {
                        string query = info.produceQuery(info.queryType); // type of query
                        cq.Enqueue(query);
                    });
                    Interlocked.Add(ref info.produced, v);
                    if (info.verbal >= 2)
                        Console.WriteLine("Queue now have: " + cq.Count);
                    do
                    {
                        elapsed = watch.ElapsedMilliseconds;
                    } while (elapsed < 1000); // each pulse last 1s
                    if (cq.Count > 0)
                    {
                        info.rps = v; // only 1 master thread write to info.rps -> no need lock
                        return;
                    }
                    watch.Stop();
                    if (info.verbal >= 2)
                    {
                        Console.WriteLine("Left in queue: " + cq.Count);
                        Console.WriteLine("Total processed: " + info.processed);
                    }
                    v += info.stride; // increase speed after every pulse
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
