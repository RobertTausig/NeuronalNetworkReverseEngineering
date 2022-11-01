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
        const double constMinDistance = 100;
        const int constMaxMagnitude = 24;

        static void Main(string[] args)
        {
            var clock = new Stopwatch();
            clock.Start();


            var model = new Model(new int[4] { 7, 5, 6, 4 });
            var sampler = new SamplingLine(model);

            var linesThroughSpace = new List<List<Matrix>>();
            // Why doing this:
            // To make sure to not accidentally get a "bad" line.
            while (linesThroughSpace.Count < 4)
            {
                var (midPoint, directionVector) = sampler.RandomSecantLine(radius: constRadius);
                var firstBoundaryPointsSuggestion = sampler.BidirectionalLinearRegionChanges(midPoint, directionVector, constMaxMagnitude);
                if(IsSpacedApart(firstBoundaryPointsSuggestion, constMinDistance))
                {
                    var secondBoundaryPointsSuggestion = sampler.BidirectionalLinearRegionChanges(Matrix.Multiplication(midPoint, 1.03), directionVector, constMaxMagnitude);
                    var thirdBoundaryPointsSuggestion = sampler.BidirectionalLinearRegionChanges(Matrix.Multiplication(midPoint, 0.97), directionVector, constMaxMagnitude);
                    if (firstBoundaryPointsSuggestion.Count == secondBoundaryPointsSuggestion.Count && firstBoundaryPointsSuggestion.Count == thirdBoundaryPointsSuggestion.Count)
                    {
                        linesThroughSpace.Add(firstBoundaryPointsSuggestion);
                    }
                }
            }

            var hyperPlanes = new List<Hyperplane>();
            foreach (var point in linesThroughSpace[0])
            {
                hyperPlanes.Add(new Hyperplane(model, point, 10));
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


            //var firstLayerPlanes = new List<Hyperplane>();
            //foreach (var plane in hyperPlanes)
            //{
            //    int succeededLineFinds = 0;
            //    for (int i = 1; i < linesThroughSpace.Count; i++)
            //    {
            //        bool innerTest = false;
            //        var comparisonLine = linesThroughSpace[i];
            //        for (int j = 0; j < comparisonLine.Count; j++)
            //        {
            //            if (plane.IsPointOnPlane(comparisonLine[j], 0.10) == true)
            //            {
            //                innerTest = true;
            //            }
            //        }
            //        if (innerTest)
            //        {
            //            succeededLineFinds++;
            //        }
            //    }
            //    Console.WriteLine(succeededLineFinds);
            //    if (succeededLineFinds > 80)
            //    {
            //        firstLayerPlanes.Add(plane);
            //    }
            //}



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
