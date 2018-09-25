namespace Microsoft.Hpc.SOASample.CommonDataService
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Data;

    public class PrimeFactorizationService : IPrimeFactorization
    {
        private static List<int> PrimeNumberTable;

        static PrimeFactorizationService()
        {
            GetCommonData();
        }

        //Get prime number table by data id
        private static void GetCommonData()
        {
            using (DataClient dataClient = ServiceContext.GetDataClient("PRIME_NUMBER_TABLE"))
            {
                PrimeNumberTable = dataClient.ReadAll<List<int>>();
            }
        }

        public List<int> Factorize(int n)
        {
            List<int> factors = new List<int>();

            //When factors are in PrimeNumberTable
            for (int i = 0; i < PrimeNumberTable.Count;)
            {
                if (n % PrimeNumberTable[i] == 0)
                {
                    factors.Add(PrimeNumberTable[i]);
                    n /= PrimeNumberTable[i];
                }
                else
                {
                    i++;
                }
            }

            //When factors are not in PrimeNumberTable
            for (int i = PrimeNumberTable.Max() + 1; i <= n;)
            {
                if (n % i == 0)
                {
                    factors.Add(i);
                    n /= i;
                }
                else
                {
                    i++;
                }
            }

            return factors;
        }
    }
}