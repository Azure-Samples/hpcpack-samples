namespace Microsoft.Hpc.SOASample.FirstSOAService
{
    using System;

    using Client.CalculatorService;

    using Microsoft.Hpc.Scheduler.Session;

    class Program
    {
        static void Main(string[] args)
        {
            //change the head node name here
            SessionStartInfo info = new SessionStartInfo("head.contoso.com", "CalculatorService");

            //create an interactive session 
            using (Session session = Session.CreateSession(info))
            {
                Console.WriteLine("Session {0} has been created", session.Id);

                //create a broker client
                using (BrokerClient<ICalculator> client = new BrokerClient<ICalculator>(session))
                {
                    //send request
                    AddRequest request = new AddRequest(1, 2);
                    client.SendRequest<AddRequest>(request);
                    client.EndRequests();

                    //get response
                    foreach (BrokerResponse<AddResponse> response in client.GetResponses<AddResponse>())
                    {
                        double result = response.Result.AddResult;
                        Console.WriteLine("Add 1 and 2, and we get {0}", result);
                    }

                    //This can be omitted if a BrokerClient object
                    //is created in a "using" clause.
                    client.Close();
                }

                //This should be explicitly invoked
                session.Close();
            }

            Console.WriteLine("Done invoking SOA service");

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
