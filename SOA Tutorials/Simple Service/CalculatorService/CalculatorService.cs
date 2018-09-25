
namespace Microsoft.Hpc.SOASample.FirstSOAService
{
    public class CalculatorService : ICalculator
    {
        public double Add(double a, double b)
        {
            return a + b;
        }
    }
}
