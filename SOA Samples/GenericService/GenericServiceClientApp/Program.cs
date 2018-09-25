using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Hpc.Scheduler.Session.GenericService;
using Microsoft.Hpc.Scheduler.Session;
using System.ServiceModel;
using System.IO;

namespace GenericServiceClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            SessionStartInfo startInfo = new SessionStartInfo("[headnode]", "GenericService");
            startInfo.Secure = false;
            startInfo.MaximumUnits = 1;

            V2ClientSample(startInfo);
            V3ClientSample(startInfo);
        }

        private static void V3ClientSample(SessionStartInfo startInfo)
        {
            using (DurableSession session = DurableSession.CreateSession(startInfo))
            {
                using (BrokerClient<IGenericServiceV3> client = new BrokerClient<IGenericServiceV3>(session))
                {
                    GenericServiceRequest request1 = new GenericServiceRequest();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (BinaryWriter writer = new BinaryWriter(ms))
                        {
                            writer.Write((int)0);
                            writer.Write((int)123);
                        }

                        request1.Data = Convert.ToBase64String(ms.ToArray());
                    }

                    // Use user data to differentiate operations
                    // 0 stands for GetData()
                    // 1 stands for GetDataUsingDataContract()
                    client.SendRequest<GenericServiceRequest>(request1, 0);

                    GenericServiceRequest request2 = new GenericServiceRequest();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (BinaryWriter writer = new BinaryWriter(ms))
                        {
                            writer.Write((int)1);
                            writer.Write(true);
                            writer.Write("DataData");
                        }

                        request2.Data = Convert.ToBase64String(ms.ToArray());
                    }

                    client.SendRequest<GenericServiceRequest>(request2, 1);
                    client.EndRequests();

                    foreach (BrokerResponse<GenericServiceResponse> response in client.GetResponses<GenericServiceResponse>())
                    {
                        int operationIndex = response.GetUserData<int>();
                        switch (operationIndex)
                        {
                            case 0:
                                // GetData
                                Console.WriteLine("GetDataResult: {0}", response.Result.Data);
                                break;
                            case 1:
                                // GetDataUsingDataContract
                                CompositeType result;
                                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(response.Result.Data)))
                                using (BinaryReader reader = new BinaryReader(ms))
                                {
                                    result = new CompositeType();
                                    result.BoolValue = reader.ReadBoolean();
                                    result.StringValue = reader.ReadString();
                                }

                                Console.WriteLine("GetDataUsingDataContractResult: BoolValue={0}\tStringValue={1}", result.BoolValue, result.StringValue);
                                break;
                        }
                    }
                }

                session.Close();
            }
        }

        private static void V2ClientSample(SessionStartInfo startInfo)
        {
            using (Session session = Session.CreateSession(startInfo))
            {
                using (GenericServiceClient client = new GenericServiceClient(new NetTcpBinding(SecurityMode.None), session.EndpointReference))
                {
                    string getDataInput;
                    using (MemoryStream ms = new MemoryStream())
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write((int)0);
                        writer.Write((int)123);
                        getDataInput = Convert.ToBase64String(ms.ToArray());
                    }

                    Console.WriteLine("GetDataResult: {0}", client.GenericOperation(getDataInput));

                    string getDataUsingDataContractInput;
                    using (MemoryStream ms = new MemoryStream())
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        writer.Write((int)1);
                        writer.Write(true);
                        writer.Write("DataData");
                        getDataUsingDataContractInput = Convert.ToBase64String(ms.ToArray());
                    }

                    string output = client.GenericOperation(getDataUsingDataContractInput);

                    CompositeType result;
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(output)))
                    using (BinaryReader reader = new BinaryReader(ms))
                    {
                        result = new CompositeType();
                        result.BoolValue = reader.ReadBoolean();
                        result.StringValue = reader.ReadString();
                    }

                    Console.WriteLine("GetDataUsingDataContractResult: BoolValue={0}\tStringValue={1}", result.BoolValue, result.StringValue);
                }
            }
        }
    }

    // Does not need to add DataContract here
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}
