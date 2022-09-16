using System;
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
            var bb = sampler.RandomSecantLine();
            var cc = sampler.LinearRegionChanges(bb);

            Console.WriteLine("Time passed: " + clock.Elapsed.TotalMilliseconds);
            Console.ReadLine();
        }
    }
}
