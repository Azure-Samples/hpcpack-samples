namespace Microsoft.Hpc.SOASample.CommonDataService
{
    using System;

    using Client.ServiceReference;

    using Microsoft.Hpc.Scheduler.Session;

    class Program
    {
        static void Main(string[] args)
        {
            //Change headnode here
            const string headnode = "head.contoso.com";
            const string serviceName = "CommonData.PrimeFactorization";

            SessionStartInfo info = new SessionStartInfo(headnode, serviceName);

            Random random = new Random();

            try
            {
                //create an interactive session
                using (Session session = Session.CreateSession(info))
                {
                    Console.WriteLine("Session {0} has been created", session.Id);

                    using (BrokerClient<IPrimeFactorization> client = new BrokerClient<IPrimeFactorization>(session))
                    {
                        //send request
                        int num = random.Next(1, Int32.MaxValue);
                        FactorizeRequest request = new FactorizeRequest(num);
                        client.SendRequest<FactorizeRequest>(request, num);
                        client.EndRequests();

                        //get response
                        foreach (BrokerResponse<FactorizeResponse> response in client.GetResponses<FactorizeResponse>())
                        {
                            int number = response.GetUserData<int>();
                            int[] factors = response.Result.FactorizeResult;

                            Console.WriteLine("{0} = {1}", number, string.Join<int>(" * ", factors));
                        }
                    }

                    session.Close();
                    Console.WriteLine("done");

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
