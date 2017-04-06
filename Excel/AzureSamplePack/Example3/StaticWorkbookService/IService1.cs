//------------------------------------------------------------------------------
// <copyright file="IService1.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Sample of a SOA service that interacts with an Excel Workbook on an
//      HPC cluster.
// </summary>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace StaticWorkbookService
{
    [ServiceContract]
    public interface IStaticWorkbookService
    {
        [OperationContract]
        object[] CalculateParameters( string spreadsheetPath, string[] inputRanges, object[] inputValues, string[] outputRanges);
    }

}
