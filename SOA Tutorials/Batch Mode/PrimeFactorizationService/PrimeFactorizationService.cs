namespace Microsoft.Hpc.SOASample.BatchMode
{
    using System.Collections.Generic;

    public class PrimeFactorizationService : IPrimeFactorization
    {
        public List<int> Factorize(int n)
        {
            List<int> factors = new List<int>();

            for (int i = 2; n > 1; )
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