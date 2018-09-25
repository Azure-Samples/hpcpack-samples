// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace EchoService
{
    // NOTE: If you change the interface name "IService1" here, you must also update the reference to "IService1" in Web.config.
    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        string Echo(string input);

        [OperationContract]
        int EchoDelay(int delayMs);

        [OperationContract]
        [FaultContract(typeof(ArgumentNullException))]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract (typeof (DivideByZeroException))]
        [FaultContract (typeof (OutOfMemoryException))]
        [FaultContract (typeof (Exception))]
        [ServiceKnownType(typeof(ArgumentNullException))]
        [ServiceKnownType(typeof(ArgumentException))]
        [ServiceKnownType(typeof(DivideByZeroException))]
        [ServiceKnownType(typeof(OutOfMemoryException))]
        [ServiceKnownType(typeof(Exception))]
        string EchoFault(string exceptionType);

        [OperationContract]
        string EchoOnExit(TimeSpan delay);
    }
}
