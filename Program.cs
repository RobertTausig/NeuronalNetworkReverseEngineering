using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeuronalNetworkReverseEngineering
{
    class Program
    {
        const double constRadius = 100;
        const double constMinDistance = 10;

        static void Main(string[] args)
        {
            var clock = new Stopwatch();
            clock.Start();


            var model = new Model(new int[4] { 7, 5, 6, 4 });
            var sampler = new SamplingLine(model);

            var boundaryPoints_1 = new List<Matrix>();
            // Why doing this:
            // To make sure to not accidentally get a "bad" line.
            while (true)
            {
                var (midPoint, directionVector) = sampler.RandomSecantLine(radius: constRadius);
                var firstBoundaryPointsSuggestion = sampler.BidirectionalLinearRegionChanges(midPoint, directionVector);
                var secondBoundaryPointsSuggestion = sampler.BidirectionalLinearRegionChanges(Matrix.Multiplication(midPoint, 1.01), directionVector);
                if (firstBoundaryPointsSuggestion.Count == secondBoundaryPointsSuggestion.Count && IsSpacedApart(firstBoundaryPointsSuggestion, constMinDistance))
                {
                    boundaryPoints_1 = firstBoundaryPointsSuggestion;
                    break;
                }
            }

            var planes_1 = new List<Hyperplane>();
            foreach (var item in boundaryPoints_1)
            {
                planes_1.Add(new Hyperplane(model, item));
            }

            //----------------------------------------------
            var boundaryPoints_2 = new List<Matrix>();
            // Why doing this:
            // To make sure to not accidentally get a "bad" line.
            while (true)
            {
                var (midPoint, directionVector) = sampler.RandomSecantLine(radius: constRadius);
                var firstBoundaryPointsSuggestion = sampler.BidirectionalLinearRegionChanges(midPoint, directionVector);
                var secondBoundaryPointsSuggestion = sampler.BidirectionalLinearRegionChanges(Matrix.Multiplication(midPoint, 1.01), directionVector);
                if (firstBoundaryPointsSuggestion.Count == secondBoundaryPointsSuggestion.Count && IsSpacedApart(firstBoundaryPointsSuggestion, constMinDistance))
                {
                    boundaryPoints_2 = firstBoundaryPointsSuggestion;
                    break;
                }
            }

            var planes_2 = new List<Hyperplane>();
            foreach (var item in boundaryPoints_2)
            {
                planes_2.Add(new Hyperplane(model, item));
            }

            //----------------------------------------------
            foreach (var pl1 in planes_1)
            {
                foreach (var pl2 in planes_2)
                {
                    if(Matrix.ApproxEqual(pl1.planeIdentity.parameters, pl2.planeIdentity.parameters) == true)
                    {
                        Console.WriteLine("x");
                        break;
                    }
                }
            }



            clock.Stop();
            Console.WriteLine("Time passed: " + clock.Elapsed.TotalMilliseconds);
            Console.ReadLine();
        }



        public static bool IsSpacedApart(List<Matrix> list, double minDistance)
        {
            for (int i = 1; i < list.Count; i++)
            {
                var norm = Matrix.GetEuclideanNormForVector(Matrix.Substraction(list[i], list[i - 1]));
                if (norm == null || norm < minDistance)
                {
                    return false;
                }
            }

            return true;
        }

    }
}
