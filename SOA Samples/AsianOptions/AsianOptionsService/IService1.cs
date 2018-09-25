using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace AsianOptionsService
{
    // NOTE: If you change the interface name "IService1" here, you must also update the reference to "IService1" in Web.config.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        double PriceAsianOptions(double initial, double exercise, double up, double down, double interest, int periods, int runs);
    }
}
