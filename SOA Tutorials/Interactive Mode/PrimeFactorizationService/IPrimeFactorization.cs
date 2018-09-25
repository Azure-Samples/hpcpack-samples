namespace Microsoft.Hpc.SOASample.RealTimeMode
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