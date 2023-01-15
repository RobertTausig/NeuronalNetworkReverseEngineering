using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeuronalNetworkReverseEngineering
{
    class Program
    {
        const int inputDim = 7;
        const int firstLayerDim = 5;
        const int secondLayerDim = 6;
        const int outputDim = 4;

        const double constRadius = (firstLayerDim + secondLayerDim)* (firstLayerDim + secondLayerDim) * 10;
        const double constMinSpacedApartDistance = 100;
        const double constMinSafeDistance = constMinSpacedApartDistance / 2;
        const double constMinStartingDistance = constMinSafeDistance / 10;
        const double constMinPointDistance = constMinStartingDistance / 10;
        const int constMaxMagnitude = 24;

        static void Main(string[] args)
        {
            var clock = new Stopwatch();
            clock.Start();


            var model = new Model(new int[4] { inputDim, firstLayerDim, secondLayerDim, outputDim });
            var sampler = new SamplingLine(model);
            var sphere = new SamplingSphere(model);
            var layer = new LayerCalculation(model, sphere);

            //Just for debugging:
            for (int i = 0; i < 1; i++)
            {
                var aa = model.RandomGenerator.Next();
            }

            bool loopCondition = true;
            var linesThroughSpace = new List<List<(Matrix boundaryPoint, double safeDistance)>>();
            // Why doing this:
            // To make sure to not accidentally get a "bad" line.
            while (linesThroughSpace.Count < 5 * inputDim + 1)
            {
                var tempLine = new List<(Matrix boundaryPoint, double safeDistance)>();
                while (loopCondition)
                {
                    var (midPoint, directionVector) = sampler.RandomSecantLine(radius: constRadius, minPointDistance: constMinPointDistance);
                    var boundaryPointsSuggestion = sampler.BidirectionalLinearRegionChanges(midPoint, directionVector, constMaxMagnitude);
                    if (IsSpacedApart(boundaryPointsSuggestion, constMinSafeDistance))
                    {
                        foreach (var point in boundaryPointsSuggestion)
                        {
                            var tempSafeDistance = sphere.MinimumDistanceToDifferentBoundary(point, constMinStartingDistance);
                            if (tempSafeDistance != null && tempSafeDistance > constMinSafeDistance)
                            {
                                tempLine.Add((point, (double)tempSafeDistance));
                                loopCondition = false;
                            }
                            else
                            {
                                tempLine.Clear();
                                loopCondition = true;
                                break;
                            }
                        }
                    }
                }
                linesThroughSpace.Add(tempLine);
                loopCondition = true;
            }

            var hyperPlanes = new List<Hyperplane>();
            foreach (var l in linesThroughSpace.First())
            {
                hyperPlanes.Add(new Hyperplane(model, l.boundaryPoint, l.safeDistance / 15, hasIntercept: false));
            }

            var firstLayerPlanes = new List<Hyperplane>();
            var bb = new List<Matrix>();
            foreach (var plane in hyperPlanes)
            {
                var temp = new List<Matrix>();
                foreach (var line in linesThroughSpace)
                {
                    foreach (var point in line)
                    {
                        if((bool)plane.IsPointOnPlane(point.boundaryPoint, 0.1))
                        {
                            temp.Add(point.boundaryPoint);
                            break;
                        }
                    }
                }

                Console.WriteLine(temp.Count());
                if (temp.Count() > 23)
                {
                    firstLayerPlanes.Add(plane);
                }
                else if (temp.Count() == 10)
                {
                    bb = temp;
                }
            }

            var cc = new Hyperplane(model, bb, hasIntercept: true);
            var tempo = new List<Matrix>();
            foreach (var line in linesThroughSpace)
            {
                foreach (var point in line)
                {
                    if ((bool)cc.IsPointOnPlane(point.boundaryPoint, 0.1))
                    {
                        tempo.Add(point.boundaryPoint);
                        break;
                    }
                }
            }



            //var firstLayerPlanes = layer.GetFirstLayer(hyperPlanes, constRadius);



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
