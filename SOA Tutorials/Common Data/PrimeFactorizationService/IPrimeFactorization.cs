namespace Microsoft.Hpc.SOASample.CommonDataService
{
    using System.Collections.Generic;
    using System.ServiceModel;

    [ServiceContract]
    public interface IPrimeFactorization
    {
        [OperationContract]
        List<int> Factorize(int n);
    }
}