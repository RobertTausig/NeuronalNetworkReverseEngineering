using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeuronalNetworkReverseEngineering
{
    class Program
    {
        static void Main(string[] args)
        {
            var clock = new Stopwatch();
            clock.Start();

            var model = new Model(new int[4] { 7, 5, 6, 4 });
            var sampler = new SamplingLine(model);

            for (int i = 0; i < 60; i++)
            {
                var aa = sampler.RandomSecantLine();
                var dd = sampler.BidirectionalLinearRegionChanges(aa.midPoint, aa.directionVector);
                Console.WriteLine(dd.Count);
            }

            Console.WriteLine("Time passed: " + clock.Elapsed.TotalMilliseconds);
            Console.ReadLine();
        }
    }
}
