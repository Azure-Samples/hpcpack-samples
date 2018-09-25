namespace Microsoft.Hpc.SOASample.BatchMode
{
    using System;

    using global::RequestSender.ServiceReference;

    using Microsoft.Hpc.Scheduler.Session;

    class RequestSender
    {

        static void Main(string[] args)
        {
            //change headnode name and service name
            SessionStartInfo info = new SessionStartInfo("head.contoso.com", "PrimeFactorizationService");

            try
            {
                //Create a durable session
                DurableSession session = DurableSession.CreateSession(info);
                Console.WriteLine("Session {0} has been created", session.Id);


                //Send batch request
                Random random = new Random();
                const int numRequests = 100;

                using (BrokerClient<IPrimeFactorization> client = new BrokerClient<IPrimeFactorization>(session))
                {
                    Console.WriteLine("Sending {0} requests...", numRequests);
                    for (int i = 0; i < numRequests; i++)
                    {
                        int number = random.Next(1, Int32.MaxValue);

                        FactorizeRequest request = new FactorizeRequest(number);

                        //The second param is used to identify each request.
                        //It can be retrieved from the response. 
                        client.SendRequest<FactorizeRequest>(request, number);
                    }

                    client.EndRequests();
                    Console.WriteLine("All the {0} requests have been sent", numRequests);

                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);            	
            }

        }
    }
}
