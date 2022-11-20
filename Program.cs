using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NeuronalNetworkReverseEngineering
{
    class Program
    {
        const double constRadius = 1_000;
        const double constMinSafeDistance = constRadius / 10;
        const double constMinStartingDistance = constMinSafeDistance / 10;
        const int constMaxMagnitude = 24;

        static void Main(string[] args)
        {
            var clock = new Stopwatch();
            clock.Start();


            var model = new Model(new int[4] { 7, 5, 6, 4 });
            var sampler = new SamplingLine(model);
            var sphere = new SamplingSphere(model);
            var layer = new LayerCalculation(model, sphere);

            //Just for debugging:
            for (int i = 0; i < 0; i++)
            {
                var aa = model.RandomGenerator.Next();
            }

            bool loopCondition = true;
            var lineThroughSpace = new List<(Matrix boundaryPoint, double safeDistance)>();
            // Why doing this:
            // To make sure to not accidentally get a "bad" line.
            while (loopCondition)
            {
                var (midPoint, directionVector) = sampler.RandomSecantLine(radius: constRadius);
                var boundaryPointsSuggestion = sampler.BidirectionalLinearRegionChanges(midPoint, directionVector, constMaxMagnitude);
                if(IsSpacedApart(boundaryPointsSuggestion, constMinSafeDistance))
                {
                    foreach (var point in boundaryPointsSuggestion)
                    {
                        var tempSafeDistance = sphere.MinimumDistanceToDifferentBoundary(point, constMinStartingDistance);
                        if (tempSafeDistance != null && tempSafeDistance > constMinSafeDistance)
                        {
                            lineThroughSpace.Add((point, (double)tempSafeDistance));
                            loopCondition = false;
                        }
                        else
                        {
                            lineThroughSpace.Clear();
                            loopCondition = true;
                            break;
                        }
                    }
                }
            }

            var hyperPlanes = new List<Hyperplane>();
            foreach (var l in lineThroughSpace)
            {
                hyperPlanes.Add(new Hyperplane(model, l.boundaryPoint, l.safeDistance / 30, hasIntercept: false));
            }

            var bb = layer.GetFirstLayer(hyperPlanes);



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

        public static List<List<Hyperplane>> SampleLinePointsToHyperplanes(List<List<Matrix>> lineCollection, Model model)
        {
            var retVal = new List<List<Hyperplane>>();
            foreach (var lines in lineCollection)
            {
                var temp = new List<Hyperplane>();
                foreach (var point in lines)
                {
                    temp.Add(new Hyperplane(model, point));
                }
                retVal.Add(temp);
            }
            return retVal;
        }

    }
}
