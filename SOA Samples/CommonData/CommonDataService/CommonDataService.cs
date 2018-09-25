using System;
using System.Collections.Generic;
using System.Linq;
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Microsoft.Hpc.Scheduler.Session.Data;
using Microsoft.Hpc.Scheduler.Session;

namespace CommonDataService
{
    // NOTE: If you change the class name "CommonDataService" here, you must also update the reference to "CommonDataService" in App.config.
    public class CommonDataService : ICommonDataService
    {
        private static Dictionary<string, string> objects;
        static CommonDataService()
        {
            string dictionary_data_id = Environment.GetEnvironmentVariable("DICTIONARY_DATA_ID");
            if (String.IsNullOrEmpty(dictionary_data_id) == false)
            {
                // Use ServiceContext to obtain DataClient
                using (DataClient client = ServiceContext.GetDataClient(dictionary_data_id))
                {
                    // Using static constructor and objects to share the data across all GetData calls saves memory and reading/deserialization cost.
                    objects = client.ReadAll<Dictionary<string, string>>();
                }
            }
            else
            {
                objects = new Dictionary<string,string>();
            }
        }

        /// <summary>
        /// To demo read common data in SOA service code
        /// </summary>
        /// <param name="raw_data_id">DataClient Id for the raw data</param>
        /// <returns>The result string shows the data information</returns>
        public string GetData(string raw_data_id)
        {
            string result;

            // One can also read and initialize the data in each service request.
            // Notice the IDisposable pattern here. Also dispose dataclient after using it.
            using (DataClient client = ServiceContext.GetDataClient(raw_data_id))
            {
                byte[] bytes = client.ReadRawBytesAll();
                result = bytes.Length.ToString() + "\n";
            }

            // Process data obtained in service static constructor
            foreach (KeyValuePair<string, string> obj in objects)
            {
                result += string.Format("{0}:{1}\n", obj.Key, obj.Value);
            }
            return result;
        }

    }
}
