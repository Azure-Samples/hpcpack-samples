using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Hpc.Scheduler.Session.GenericService;
using System.IO;
using System.ServiceModel;

namespace GenericService
{
    public class GenericServiceWrapper : IGenericService
    {
        private Service1 service;

        public GenericServiceWrapper()
        {
            this.service = new Service1();
        }

        public string GenericOperation(string args)
        {
            // Convert the string into a byte array
            byte[] data = Convert.FromBase64String(args);

            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                // First 32 bit is an index indicating the real operation
                // 0: GetData()
                // 1: GetDataUsingDataContract()
                int operationIndex = reader.ReadInt32();
                switch (operationIndex)
                {
                    case 0:
                        // GetData(), read an int as argument
                        int value = reader.ReadInt32();
                        return this.service.GetData(value);
                    case 1:
                        // GetDataUsingDataContract(), read a bool and a string
                        using (MemoryStream resultStream = new MemoryStream())
                        using (BinaryWriter writer = new BinaryWriter(resultStream))
                        {
                            CompositeType composite = new CompositeType();
                            composite.BoolValue = reader.ReadBoolean();
                            composite.StringValue = reader.ReadString();
                            CompositeType result = this.service.GetDataUsingDataContract(composite);
                            writer.Write(result.BoolValue);
                            writer.Write(result.StringValue);
                            return Convert.ToBase64String(resultStream.ToArray());
                        }
                    default:
                        throw new FaultException("Invalid operation index.");
                }
            }
        }
    }
}