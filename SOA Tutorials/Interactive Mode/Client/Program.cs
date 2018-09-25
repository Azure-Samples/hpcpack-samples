namespace Microsoft.Hpc.SOASample.RealTimeMode
{
    using System;
    using System.Threading;

    using global::Client.ServiceReference;

    using Microsoft.Hpc.Scheduler.Session;

    class Client
    {
        static void Main(string[] args)
        {
            //Change headnode here
            const string headnode = "head.contoso.com";
            const string serviceName = "PrimeFactorizationService";

            SessionStartInfo info = new SessionStartInfo(headnode, serviceName);

            //Enable session pool
            info.ShareSession = true;
            info.UseSessionPool = true;

            try
            {                            
                //create an interactive session
                using (Session session = Session.CreateSession(info))
                {
                    Console.WriteLine("Session {0} has been created", session.Id);

                    //in one session, each broker client should have a unique id
                    string ClientId = Guid.NewGuid().ToString();

                    //use this event sync main thread and callback
                    AutoResetEvent done = new AutoResetEvent(false);

                    using (BrokerClient<IPrimeFactorization> client = new BrokerClient<IPrimeFactorization>(ClientId, session))
                    {
                        Console.WriteLine("BrokerClient {0} has been created", ClientId);

                        //set callback function. this handler will be invoke before service replies.
                        client.SetResponseHandler<FactorizeResponse>((response) =>
                        {
                            int number = response.GetUserData<int>();
                            int[] factors = response.Result.FactorizeResult;

                            Console.WriteLine("{0} = {1}", number,
                                string.Join<int>(" * ", factors));

                            //release the lock
                            done.Set();
                        });

                        Random random = new Random();
                        int num = random.Next(1, Int32.MaxValue);

                        //send request
                        FactorizeRequest request = new FactorizeRequest(num);
                        client.SendRequest<FactorizeRequest>(request, num);
                        
                        client.EndRequests();

                        //wait until callback returns
                        done.WaitOne();
                    }

                    Console.WriteLine("Factorization done.");

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
