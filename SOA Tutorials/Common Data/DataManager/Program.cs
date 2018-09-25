namespace Microsoft.Hpc.SOASample.CommonDataService
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Hpc.Scheduler.Session.Data;

    class Program
    {
        static void Main(string[] args)
        {
            //create prime number table of prime numbers between 1 and 200000
            Console.WriteLine("Generating prime number table...");
            List<int> PrimeNumberTable = GeneratePrimeNumberTable(200000);

            const string headnode = "head.contoso.com";
            string dataId = "PRIME_NUMBER_TABLE";

            Console.WriteLine("Creating data client {0}", dataId);

            try
            {
                //create DataClient to send data
                using (DataClient dataClient = DataClient.Create(headnode, dataId))
                {
                    //WriteAll can be called only once per data client
                    dataClient.WriteAll<List<int>>(PrimeNumberTable);
                }
            }
            catch (DataException ex)
            {
                //If data client already exists, delete it and try again
                if (ex.ErrorCode == 103809028)
                {
                    Console.WriteLine("{0} already exists, delete it and send again.", dataId);
                    DataClient.Delete(headnode, dataId);

                    using (DataClient dataClient = DataClient.Create(headnode, dataId))
                    {
                        dataClient.WriteAll<List<int>>(PrimeNumberTable);
                    }
                }
            }

            Console.WriteLine("{0} has been sent to cluster", dataId);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static List<int> GeneratePrimeNumberTable(int max)
        {
            List<int> PrimeNumberTable = new List<int>();

            for (int i = 2; i < max; i++)
            {
                PrimeNumberTable.Add(i);
            }

            for (int i = 2; i <= Math.Sqrt(max); i++)
            {
                if (PrimeNumberTable.Contains(i))
                {
                    int tmpMax = PrimeNumberTable[PrimeNumberTable.Count - 1];
                    for (int j = i * i; j <= tmpMax; j += i)
                    {
                        PrimeNumberTable.Remove(j);
                    }
                }
            }

            return PrimeNumberTable;
        }

    }

}
