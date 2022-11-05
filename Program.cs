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

            //Just for debugging:
            for (int i = 0; i < 4; i++)
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
                hyperPlanes.Add(new Hyperplane(model, l.boundaryPoint, l.safeDistance / 18));
            }

            List<int> papap = new List<int>();
            foreach (var plane in hyperPlanes)
            {
                int cnt = 0;
                for (int i = 0; i < 500; i++)
                {
                    var genPoint = plane.GenerateRandomPointOnPlane(1_000);
                    var norm = Matrix.GetEuclideanNormForVector(genPoint);
                    int sphereCnt = 0;
                    for (int j = 0; j < 30; j++)
                    {
                        var directionVector = new Matrix(genPoint.numRow, genPoint.numCol);
                        directionVector.PopulateAllRandomlyFarFromZero(model.RandomGenerator);
                        directionVector = Matrix.NormalizeVector(directionVector, (double)norm / 31_000);
                        var boundaryPoints = sampler.BidirectionalLinearRegionChanges(genPoint, directionVector, 8);
                        sphereCnt += boundaryPoints.Count;
                        if (sphereCnt > 0)
                        {
                            break;
                        }
                    }
                    if (sphereCnt > 0)
                    {
                        cnt++;
                    }
                }
                Console.WriteLine(cnt.ToString());
                papap.Add(cnt);
            }
            Console.WriteLine("----------------------------------------");
            for (int i = 0; i < hyperPlanes.Count; i++)
            {
                if (papap[i] > 480)
                {
                    //var aa = hyperPlanes[i].planeIdentity.parameters;
                    //var bb = Matrix.FlattenVector(aa);
                    foreach (var param in Matrix.FlattenVector(hyperPlanes[i].planeIdentity.parameters))
                    {
                        Console.WriteLine(param);
                    }
                    Console.WriteLine(hyperPlanes[i].planeIdentity.intercept);
                    Console.WriteLine("~ ~ ~ ~");
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
