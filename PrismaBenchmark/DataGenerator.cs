using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PrismaBenchmark
{
    class DataGenerator
    {
        private static DataGenerator _self = null;
        private Tuple<int, int> RANDOM_RANGE = Tuple.Create(0,10);
        private Tuple<int, int> MULTI_RANGE = Tuple.Create(10, 20);
        private int SINGLE_RANGE_L = 20;
        private int nextSingle;
        private Random rand;

        private DataGenerator(Random rand)
        {
            nextSingle = SINGLE_RANGE_L;
            this.rand = rand;
        }

        public static DataGenerator Instance(Random rand)
        {
            if (_self == null)
                _self = new DataGenerator(rand);
            return _self;
        }

        public void ResetNextSingle()
        {
            nextSingle = SINGLE_RANGE_L;
        }

        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[rand.Next(s.Length)]).ToArray());
        }

        public List<List<ArrayList>> GetDataRows(int numberOfRows, int range=0, int copy=1)
        // range 0: random range [0..9]
        // range 1: single range [20..]
        // copy: number of tables of the same data structure
        {
            List<List<ArrayList>> data = new List<List<ArrayList>>();
            // init data with List<ArrayList>
            for (var i = 0; i < copy; i++)
            {
                data.Add(new List<ArrayList>());
            }
            int l, r;
            for (var i = 0; i < numberOfRows; i++) 
            {
                if (range == 0) (l, r) = RANDOM_RANGE;
                else
                {
                    l = GetNextSingle();
                    r = l + 1;
                }
                for (var j = 0; j < copy; j++)
                {
                    data[j].Add(new ArrayList { rand.Next(l, r), rand.Next(1000), rand.Next(1000), RandomString(10), RandomString(10) });
                }
                
            }
            return data;
        }

        private int GetNextSingle()
        {
            return this.nextSingle++;
        }

        public List<ArrayList> GetDataRowsForSelect(int start, int batch_size=1000)
        // sequence number of batch, each batch contains batch_size rows, with index starting with batch*batch_size, end with index (batch+1)*batch_size - 1 
        {
            List<ArrayList> data = new List<ArrayList>(batch_size);
            // init data with List<ArrayList>
            for (var i = start; i < start + batch_size; i++)
            {
                data.Add(new ArrayList { i, rand.Next(1000), i, RandomString(10), RandomString(10) });
            }
            return data;
        }
    }
}
