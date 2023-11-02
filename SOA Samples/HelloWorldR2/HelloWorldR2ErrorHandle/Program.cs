// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Threading ;
// This namespace is defined in the HPC Server 2016 SDK
// which includes the HPC SOA Session API.   
using Microsoft.Hpc.Scheduler.Session;
// This namespace is defined in the "EchoService" service reference
using HelloWorldR2ErrorHandle.EchoService;
using System.ServiceModel;

namespace HelloWorldR2ErrorHandle
{
    class Program
    {
        static void Main(string[] args)
        {
            //change the headnode name here
            const string headnode = "[headnode]";
            const string serviceName = "EchoService";
            const int numRequests = 12;
            SessionStartInfo startInfo = new SessionStartInfo(headnode, serviceName);
            // If the cluster is non-domain joined, add the following statement
            // startInfo.Secure = false;

            startInfo.BrokerSettings.SessionIdleTimeout = 15 * 60 * 1000;
            startInfo.BrokerSettings.ClientIdleTimeout = 15 * 60 * 1000;

            Console.WriteLine("Creating a session for EchoService...");
            DurableSession session = null;
            bool successFlag = false;
            int retryCount = 0;
            const int retryCountMax = 20;
            const int retryIntervalMs = 5000;

            while (!successFlag && retryCount++ < retryCountMax)
            {
                try
                {
                    session = DurableSession.CreateSession(startInfo);
                    successFlag = true;
                }
                catch (EndpointNotFoundException e)
                {
                    Console.WriteLine("EndpointNotFoundException {0}", e.ToString());
                }
                catch (CommunicationException e)
                {
                    Console.WriteLine("CommunicationException {0}", e.ToString());
                }
                catch (TimeoutException e)
                {
                    Console.WriteLine("TimeoutException {0}", e.ToString());
                }
                catch (SessionException e)
                {
                    Console.WriteLine("SessionException {0}, error code 0x{1:x}", e.ToString(), e.ErrorCode);
                    // if session fatal errors happen, no retry
                    if (SOAFaultCode.Category(e.ErrorCode).Equals(SOAFaultCodeCategory.SessionFatalError))
                    {
                        Console.WriteLine("SessionExceptionCategory : SessionFatalError {0}", e.ToString());
                        Console.WriteLine("No retry.");
                        retryCount = retryCountMax;
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("General exception {0}", e.ToString());
                }

                if (!successFlag)
                {
                    Console.WriteLine("=== Sleep {0} ms to retry. Retry count left {1}. ===", retryIntervalMs, retryCountMax - retryCount);
                    Thread.Sleep(retryIntervalMs);
                }

            }

            if (!successFlag)
            {
                Console.WriteLine("Create durable session failed.");
                return;
            }

            // Create a durable session 
            int sessionId = session.Id;
            Console.WriteLine("Done session id = {0}", sessionId);


            //send requests
            successFlag = false;
            retryCount = 0;
            const int sendTimeoutMs = 5000;
            
            const int clientPurgeTimeoutMs = 60000;
            while (!successFlag && retryCount++ < retryCountMax)
            {
                using (BrokerClient<IService1> client = new BrokerClient<IService1>(session))
                {
                    Console.WriteLine("Sending {0} requests...", numRequests);

                    try
                    {
                        for (int i = 0; i < numRequests; i++)
                        {
                            //client.SendRequest<EchoFaultRequest>(new EchoFaultRequest("dividebyzeroexception"), i, sendTimeoutMs);
                            client.SendRequest(new EchoDelayRequest(5000), i, sendTimeoutMs);
                        }

                        client.EndRequests();
                        successFlag = true;
                        Console.WriteLine("done");
                    }
                    
                    catch (TimeoutException e)
                    {
                        // Timeout exceptions
                        Console.WriteLine("TimeoutException {0}", e.ToString());
                    }
                    catch (CommunicationException e)
                    {
                        //CommunicationException
                        Console.WriteLine("CommunicationException {0}", e.ToString());
                    }
                    catch (SessionException e)
                    {

                        Console.WriteLine("SessionException {0}, error code 0x{1:x}", e.ToString(), e.ErrorCode);
                
                        if (SOAFaultCode.Broker_BrokerUnavailable == e.ErrorCode)
                        {
                            Console.WriteLine("SessionException : BrokerUnavailable {0}", e.ToString());
                        }
                        // Session Exceptions are unrecoverable unless they are application errors
                        if (SOAFaultCode.Category(e.ErrorCode).Equals(SOAFaultCodeCategory.ApplicationError))
                        {
                            Console.WriteLine("SessionExceptionCategory : ApplicationError {0}", e.ToString());
                        }

                        // if session fatal errors happen, no retry
                        if (SOAFaultCode.Category(e.ErrorCode).Equals(SOAFaultCodeCategory.SessionFatalError))
                        {
                            Console.WriteLine("SessionExceptionCategory : SessionFatalError {0}", e.ToString());
                            Console.WriteLine("No retry.");
                            retryCount = retryCountMax;
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        //general exceptions
                        Console.WriteLine("Exception {0}", e.ToString());
                    }

                    //purge client if not succeeded, needed?
                    if (!successFlag)
                    {
                        try
                        {
                            client.Close(true,clientPurgeTimeoutMs);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Failed to purge the client after send request failure {0}", e.ToString() ); 
                        }
                    }
                }

                if (!successFlag)
                {
                    Console.WriteLine("=== Sleep {0} ms to retry. Retry count left {1}. ===", retryIntervalMs, retryCountMax - retryCount);
                    Thread.Sleep(retryIntervalMs);
                }
            }

            if (!successFlag)
            {
                Console.WriteLine("Send requests failed.");
                return;
            }

            //dispose the session here
            session.Dispose();

            //attach the session
            SessionAttachInfo attachInfo = new SessionAttachInfo(headnode, sessionId);

            successFlag = false;
            retryCount = 0;
            while (!successFlag && retryCount++ < retryCountMax)
            {
                try
                {
                    session = DurableSession.AttachSession(attachInfo);
                    successFlag = true;
                }
                catch (EndpointNotFoundException e)
                {
                    Console.WriteLine("{0}", e.ToString());
                }
                catch (CommunicationException e)
                {
                    Console.WriteLine("{0}", e.ToString());
                }
                catch (SessionException e)
                {
                    Console.WriteLine("SessionException {0}, error code 0x{1:x}", e.ToString(), e.ErrorCode);
                    // if session fatal errors happen, no retry
                    if (SOAFaultCode.Category(e.ErrorCode).Equals(SOAFaultCodeCategory.SessionFatalError))
                    {
                        Console.WriteLine("SessionExceptionCategory : SessionFatalError {0}", e.ToString());
                        Console.WriteLine("No retry.");
                        retryCount = retryCountMax;
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("General exception {0}", e.ToString());
                }

                if (!successFlag)
                {
                    Console.WriteLine("=== Sleep {0} ms to retry. Retry count left {1}. ===", retryIntervalMs, retryCountMax - retryCount);
                    Thread.Sleep(retryIntervalMs);
                }
            }

            if (!successFlag)
            {
                Console.WriteLine("Attach durable session failed.");
                return;
            }

            successFlag = false;
            retryCount = 0;
            const int getTimeoutMs = 30000;
            const int clientCloseTimeoutMs= 15000;

            Console.WriteLine("Retrieving responses...");

            while (!successFlag && retryCount++ < retryCountMax)
            {
                using (BrokerClient<IService1> client = new BrokerClient<IService1>(session))
                {
                    // GetResponses from the runtime system
                    // EchoResponse class is created as you add Service Reference "EchoService" to the project
                    try
                    {
                        //foreach (var response in client.GetResponses<EchoFaultResponse>(getTimeoutMs))
                        foreach (var response in client.GetResponses<EchoDelayResponse>(getTimeoutMs))
                        {
                            try
                            {
                                //string reply = response.Result.EchoFaultResult ;
                                int reply = response.Result.EchoDelayResult;
                                Console.WriteLine("\tReceived response for delay request {0}: {1}", response.GetUserData<int>(), reply);
                            }
                            catch (FaultException<DivideByZeroException> e)
                            {
                                // Application exceptions
                                Console.WriteLine("FaultException<DivideByZeroException> {0}", e.ToString());
                            }
                            catch (FaultException e)
                            {
                                // Application exceptions
                                Console.WriteLine("FaultException {0}", e.ToString());
                            }
                            catch (RetryOperationException e)
                            {
                                // RetryOperationExceptions may or may not be recoverable
                                Console.WriteLine("RetryOperationException {0}", e.ToString());
                            }
                        }

                        successFlag = true;
                        Console.WriteLine("Done retrieving {0} responses", numRequests);
                    }
                    catch (TimeoutException e)
                    {
                        // Timeout exceptions
                        Console.WriteLine("TimeoutException {0}", e.ToString());
                    }
                    catch (CommunicationException e)
                    {
                        //CommunicationException
                        Console.WriteLine("CommunicationException {0}", e.ToString());
                    }
                    catch (SessionException e)
                    {
                        Console.WriteLine("SessionException {0}, error code 0x{1:x}", e.ToString(), e.ErrorCode);
                
                        if (SOAFaultCode.Broker_BrokerUnavailable == e.ErrorCode)
                        {
                            Console.WriteLine("SessionException : BrokerUnavailable {0}", e.ToString());
                        }
                        // Session Exceptions are unrecoverable unless they are application errors
                        if (SOAFaultCode.Category(e.ErrorCode).Equals(SOAFaultCodeCategory.ApplicationError))
                        {
                            Console.WriteLine("SessionExceptionCategory : ApplicationError {0}", e.ToString());
                        }
                        // if session fatal errors happen, no retry
                        if (SOAFaultCode.Category(e.ErrorCode).Equals(SOAFaultCodeCategory.SessionFatalError))
                        {
                            Console.WriteLine("SessionExceptionCategory : SessionFatalError {0}", e.ToString());
                            Console.WriteLine("No retry.");
                            retryCount = retryCountMax;
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        //general exceptions
                        Console.WriteLine("Exception {0}", e.ToString());
                    }

                    try
                    {
                        client.Close(false, clientCloseTimeoutMs);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception", e.ToString());
                    }
                }

                if (!successFlag)
                {
                    Console.WriteLine("=== Sleep {0} ms to retry. Retry count left {1}. ===", retryIntervalMs, retryCountMax - retryCount);
                    Thread.Sleep(retryIntervalMs);
                }
            }

            //explict close the session to free the resource
            successFlag = false;
            retryCount = 0;

            Console.WriteLine("Close the session...");

            while (!successFlag && retryCount++ < retryCountMax)
            {
                try
                {
                    session.Close(true);
                    successFlag = true;
                }
                catch (SessionException e)
                {
                    Console.WriteLine("SessionException {0}, error code 0x{1:x}", e.ToString(), e.ErrorCode);
                    // if session fatal errors happen, no retry
                    if (SOAFaultCode.Category(e.ErrorCode).Equals(SOAFaultCodeCategory.SessionFatalError))
                    {
                        Console.WriteLine("SessionExceptionCategory : SessionFatalError {0}", e.ToString());
                        Console.WriteLine("No retry.");
                        retryCount = retryCountMax;
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0}", e.ToString());
                }

                if (!successFlag)
                {
                    Console.WriteLine("=== Sleep {0} ms to retry. Retry count left {1}. ===", retryIntervalMs, retryCountMax - retryCount);
                    Thread.Sleep(retryIntervalMs);
                }
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
