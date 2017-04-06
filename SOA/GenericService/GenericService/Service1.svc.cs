using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

namespace GenericService
{
    public class Service1: IService1
    {
        public string GetData(int value)
        {
            return value.ToString();
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            return composite;
        }
    }
}
