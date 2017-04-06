using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CommonDataService
{
    // NOTE: If you change the interface name "ICommonDataService" here, you must also update the reference to "ICommonDataService" in App.config.
    [ServiceContract]
    public interface ICommonDataService
    {
        [OperationContract]
        string GetData(string raw_data_id);
    }
}
