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

            var boundaryPoints = new List<Matrix>();
            // Why doing this:
            // To make sure to not accidentally get a "bad" line.
            for (int i = 0; i < 60; i++)
            {
                var (midPoint, directionVector) = sampler.RandomSecantLine(radius:100);
                var firstBoundaryPointsSuggestion = sampler.BidirectionalLinearRegionChanges(midPoint, directionVector);
                var secondBoundaryPointsSuggestion = sampler.BidirectionalLinearRegionChanges(Matrix.Multiplication(midPoint, 1.01), directionVector);
                if (firstBoundaryPointsSuggestion.Count == secondBoundaryPointsSuggestion.Count)
                {
                    Console.WriteLine(firstBoundaryPointsSuggestion.Count);
                    //boundaryPoints = firstBoundaryPointsSuggestion;
                    //break;
                }

                //for (int i = 1; i < firstBoundaryPointsSuggestion.Count; i++)
                //{
                //    Console.WriteLine(Matrix.GetEuclideanNormForVector(Matrix.Substraction(firstBoundaryPointsSuggestion[i], firstBoundaryPointsSuggestion[i - 1])));
                //}
                //Console.WriteLine("----------------------");

            }

            Console.WriteLine("Time passed: " + clock.Elapsed.TotalMilliseconds);
            Console.ReadLine();
        }
    }
}
