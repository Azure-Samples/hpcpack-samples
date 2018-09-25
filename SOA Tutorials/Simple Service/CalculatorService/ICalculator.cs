namespace Microsoft.Hpc.SOASample.FirstSOAService
{
    using System.ServiceModel;

    [ServiceContract]
    public interface ICalculator
    {
        [OperationContract]
        double Add(double a, double b);
    }
}
