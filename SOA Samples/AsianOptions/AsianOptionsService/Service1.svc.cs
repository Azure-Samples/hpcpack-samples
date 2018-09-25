using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Diagnostics;
using Microsoft.Hpc.Scheduler.Session;

namespace AsianOptionsService
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class Service1 : IService1
    {
        public double PriceAsianOptions(double initial, double exercise, double up, double down, double interest, int periods, int runs)
        {

            //trace
            ServiceContext.Logger.TraceEvent(TraceEventType.Start, 0, "Start in PriceAsianOptions Service.");
                        
            ServiceContext .Logger .TraceData (TraceEventType.Information ,100,initial ,exercise ,up,down,interest ,periods ,runs);

            double[] pricePath = new double[periods + 1];

            // Risk-neutral probabilities
            double piup = (interest - down) / (up - down);
            double pidown = 1 - piup;

            double temp = 0.0;

            Random rand = new Random();
            double priceAverage = 0.0;
            double callPayOff = 0.0;

            for (int index = 0; index < runs; index++)
            {
                // Generate Path
                double sumPricePath = initial;

                for (int i = 1; i <= periods; i++)
                {
                    pricePath[0] = initial;
                    double rn = rand.NextDouble();

                    if (rn > pidown)
                    {
                        pricePath[i] = pricePath[i - 1] * up;
                    }
                    else
                    {
                        pricePath[i] = pricePath[i - 1] * down;
                    }
                    sumPricePath += pricePath[i];
                }

                priceAverage = sumPricePath / (periods + 1);
                callPayOff = Math.Max(priceAverage - exercise, 0);

                temp += callPayOff;
            }

            double returnValue=(temp / Math.Pow(interest, periods)) / runs;

            ServiceContext.Logger.TraceData(TraceEventType.Information, 200, returnValue );

            ServiceContext.Logger.TraceEvent(TraceEventType.Stop , 1, "Stop in PriceAsianOptions Service.");

            return returnValue;
        }
    }
}
