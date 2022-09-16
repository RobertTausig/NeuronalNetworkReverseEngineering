using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeuronalNetworkReverseEngineering
{
    class Program
    {
        const double startRadius = 100;
        const double iterateRadiusFactor = 1.3;
        static void Main(string[] args)
        {
            var clock = new Stopwatch();
            clock.Start();

            var model = new Model(new int[4] { 7, 5, 6, 4 });
            var sampler = new SamplingLine(model);

            var firstLayerPoints = new List<Matrix>();
            for (int i = 1; i < 60; i++)
            {
                var firstLine = sampler.RandomSecantLine(radius: startRadius * iterateRadiusFactor * i, lineLength: 8 * startRadius * iterateRadiusFactor * i);
                var secondLine = sampler.RandomSecantLine(radius: startRadius * iterateRadiusFactor * (i + 1), lineLength: 8 * startRadius * iterateRadiusFactor * (i + 1));
                var firstChangePoints = sampler.LinearRegionChanges(firstLine);
                var secondChangePoints = sampler.LinearRegionChanges(secondLine);
                //if (firstChangePoints.Count == secondChangePoints.Count)
                //{
                //    firstLayerPoints = firstChangePoints;
                //    break;
                //}
            }

            Console.WriteLine("Time passed: " + clock.Elapsed.TotalMilliseconds);
            Console.ReadLine();
        }
    }
}
