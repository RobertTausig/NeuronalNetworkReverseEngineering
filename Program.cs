using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

            int cnt = 0;
            for (int i = 0; i < 1_000; i++)
            {
                var randPoint = planes_1.Last().GenerateRandomPointOnPlane();
                if (planes_1.Last().IsPointOnPlane(randPoint) == true)
                {
                    cnt++;
                }
            }
            Console.WriteLine(cnt);


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
