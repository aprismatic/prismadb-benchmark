using System;

namespace PrismaBenchmark
{
    
    class Program
    {
        static void Main(string[] args)
        {
            Benchmark latency = new LatencyBenchmark();
            latency.RunBenchMark();
            ////////////
            Benchmark load = new LoadBenchmark();
            load.RunBenchMark();
            /////////////////
            Console.WriteLine("Press any key to exit ...");
            Console.ReadLine();
        }
 
    }
}
