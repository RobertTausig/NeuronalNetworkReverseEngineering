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
                var bb = sampler.LinearRegionChanges(aa.startPoint, aa.directionVector);
                var cc = sampler.LinearRegionChanges(aa.startPoint, Matrix.Multiplication(aa.directionVector, -1));
                Console.WriteLine(bb.Count + cc.Count);
            }

            Console.WriteLine("Time passed: " + clock.Elapsed.TotalMilliseconds);
            Console.ReadLine();
        }
    }
}
