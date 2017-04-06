//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Sample custom web service host
// </summary>
//------------------------------------------------------------------------------

namespace CustomWebServiceHost
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Threading;
    using System.Diagnostics;

    /// <summary>
    /// Sample custom web service host
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point
        /// </summary>
        /// <param name="args">not necessary</param>
        public static void Main(string[] args)
        {
            //TextWriterTraceListener filelistener = new TextWriterTraceListener(System.IO.File.CreateText(@"C:\Services\ServiceHost.log"));
            //Trace.Listeners.Add(filelistener);
            //Trace.AutoFlush = true;

            string dll = @"C:\Services\WebHttpDemo.dll";
            Assembly ass = Assembly.LoadFrom(dll);
            Type intf = ass.GetType("WebHttpDemo.IService1");
            Type impl = ass.GetType("WebHttpDemo.Service1");

            WebServiceHost host = new WebServiceHost(impl, new Uri("http://localhost:8088/"));
            ServiceEndpoint ep = host.AddServiceEndpoint(intf, new WebHttpBinding(), string.Empty);
            ep.Behaviors.Add(new WebHttpBehavior());

            try
            {
                host.Open();
                Trace.WriteLine("Service Host Opened");
                Console.WriteLine("Ready...");
                Thread.Sleep(2 * 60 * 60 * 1000);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Trace.WriteLine(ex.Message);            	
            }
        }
    }
}
