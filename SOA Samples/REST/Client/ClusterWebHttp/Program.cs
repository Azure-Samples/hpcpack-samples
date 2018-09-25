//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      WebHttp sample client
// </summary>
//------------------------------------------------------------------------------

namespace ClusterWebHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Text;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session;
    using WebHttpDemo;

    /// <summary>
    /// WebHttp sample client
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point
        /// </summary>
        /// <param name="args">Head node to contact</param>
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("\tClusterWebHttp.exe <headNode>");
                Console.WriteLine("Example:");
                Console.WriteLine("\tClusterWebHttp.exe myHeadNode");
                return;
            }

            string headNode = args[0];
            string service = "WebHttpDemo";
            SessionStartInfo ssi = new SessionStartInfo(headNode, service);
            ssi.SessionResourceUnitType = SessionUnitType.Core;
            ssi.MaximumUnits = 1;
            ssi.MinimumUnits = 1;
            ssi.Secure = false;

            Session session = Session.CreateSession(ssi);

            ChannelFactory<IService1> factory = new ChannelFactory<IService1>(new WebHttpBinding(WebHttpSecurityMode.None), session.EndpointReference);
            factory.Endpoint.Behaviors.Add(new WebHttpBehavior());

            IService1 proxy = factory.CreateChannel();
            for (int i = 0; i < 5; ++i)
            {
                Console.WriteLine(proxy.Echo("hello" + i));
            }
            ((IClientChannel)proxy).Close();
            factory.Close();
            session.Close();
        }
    }
}
