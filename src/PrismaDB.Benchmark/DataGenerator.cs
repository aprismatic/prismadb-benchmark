using Medallion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PrismaDB.Benchmark
{
    static class DataGenerator
    {
        public static string RandomString(int length)
        {

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Rand.Next(0, s.Length)]).ToArray());
        }

        public static List<ArrayList> GetDataRows(int numberOfRows)
        {
            List<ArrayList> data = new List<ArrayList>();
            for (var i = 0; i < numberOfRows; i++)
            {
                data.Add(new ArrayList { Rand.Next(0, 1000), Rand.Next(0, 1000), Rand.Next(0, 1000), Rand.Next(0, 1000), RandomString(10) });
            }
            return data;
        }

        public static List<ArrayList> GetDataRowsForSelect(int start, int batch_size = 1000)
        // sequence number of batch, each batch contains batch_size rows, with index starting with batch*batch_size, end with index (batch+1)*batch_size - 1 
        {
            List<ArrayList> data = new List<ArrayList>(batch_size);
            // init data with List<ArrayList>
            for (var i = start; i < start + batch_size; i++)
            {
                data.Add(new ArrayList { i, Rand.Next(0, 1000), Rand.Next(0, 1000), i, RandomString(10) });
            }
            return data;
        }
    }
}
