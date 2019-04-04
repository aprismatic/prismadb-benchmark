using System;

namespace PrismaBenchmark
{
    
    class Program
    {
        private static CustomConfiguration conf;

        static void Main(string[] args)
        {
            conf = CustomConfiguration.LoadConfiguration();
            var db = new DataBase(true); // Test Connection
            if (conf.load != null)
            {
                Benchmark load = new LoadBenchmark();
                load.RunBenchMark();
            }
            //Console.WriteLine(conf.load.Operations);
            Console.WriteLine("Press any key to exit ...");
            Console.ReadLine();
        }
 
    }
}
