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
            

            var model = new Model(new int[4] { 7, 5, 6, 4 });
            var sampler = new SamplingLine(model);

            var boundaryPoints = new List<Matrix>();
            // Why doing this:
            // To make sure to not accidentally get a "bad" line.
            while (true)
            {
                var (midPoint, directionVector) = sampler.RandomSecantLine(radius: constRadius);
                var firstBoundaryPointsSuggestion = sampler.BidirectionalLinearRegionChanges(midPoint, directionVector);
                var secondBoundaryPointsSuggestion = sampler.BidirectionalLinearRegionChanges(Matrix.Multiplication(midPoint, 1.01), directionVector);
                if (firstBoundaryPointsSuggestion.Count == secondBoundaryPointsSuggestion.Count && IsSpacedApart(firstBoundaryPointsSuggestion, constMinDistance))
                {
                    boundaryPoints = firstBoundaryPointsSuggestion;
                    break;
                }
            }

            var clock = new Stopwatch();
            clock.Start();

            var planes = new List<Hyperplane>();
            foreach (var item in boundaryPoints)
            {
                planes.Add(new Hyperplane(model, item));
            }

            double[] parameters = new[] { 2.37, -3.8, -0.22, 7.19 };
            double[] x1 = new[] { 0.312928943462965, -1.16839616233405, 0.689276486707173, -1.53368475681262 };
            double[] x2 = new[] { -2.31558794808863, -1.496227468087, 1.75210536235624, -1.9902897333437 };
            double[] x3 = new[] { -1.70107862279079, -1.04064266557997, 2.51017395821844, -1.62947294479157 };
            double[] x4 = new[] { 2.06266069488705, -1.15373838976752, -1.60689690501272, 0.922334820817665 };
            double[] x5 = new[] { -0.243180393240999, -1.22898418193215, 2.4079249671106, 1.5112760469485 };
            double[] y = new[] { -5.9972872156817, -14.4979254206991, -12.3452629506697, 16.2578164087807, 14.4301336441564};

            double[] p = Fit.MultiDim(
                new[] { x1, x2, x3, x4, x5 },
                y,
                intercept: true);


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
