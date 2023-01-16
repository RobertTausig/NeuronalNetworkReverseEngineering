﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    class LayerCalculation
    {
        public LayerCalculation(Model model, SamplingSphere sphere)
        {
            this.model = model;
            this.sphere = sphere;
        }

        private Model model { get; }
        private SamplingSphere sphere { get; }
        private int stdMaxMagnitude = 8;
        private int stdNumTestPoints = 500;
        private int stdNumTestLines = 30;

        public List<List<(Matrix boundaryPoint, double safeDistance)>> DriveLinesThroughSpace(int numLines, double minSpacedApartDistance)
        {
            int sumHiddenLayerDims = model.topology[1..^1].Sum(x => x);
            double constRadius = Math.Pow(sumHiddenLayerDims, 2) * Math.Sqrt(minSpacedApartDistance);
            double constMinSpacedApartDistance = minSpacedApartDistance;
            double constMinSafeDistance = constMinSpacedApartDistance / 2;
            double constMinStartingDistance = constMinSafeDistance / 10;
            double constMinPointDistance = constMinStartingDistance / 10;
            int constMaxMagnitude = 24;

            var linesThroughSpace = new ConcurrentDictionary<int, List<(Matrix boundaryPoint, double safeDistance)>>();
            // Why doing this:
            // To make sure to not accidentally get a "bad" line.
            var result = Parallel.For(0, numLines, index =>
            {
                var tempModel = model.Copy(index * 500);
                var tempSampler = new SamplingLine(tempModel);
                var tempSphere = new SamplingSphere(tempModel);

                var tempLine = new List<(Matrix boundaryPoint, double safeDistance)>();
                bool loopCondition = true;
                while (loopCondition)
                {
                    var (midPoint, directionVector) = tempSampler.RandomSecantLine(radius: constRadius, minPointDistance: constMinPointDistance);
                    var boundaryPointsSuggestion = tempSampler.BidirectionalLinearRegionChanges(midPoint, directionVector, constMaxMagnitude);
                    if (IsSpacedApart(boundaryPointsSuggestion, constMinSafeDistance))
                    {
                        foreach (var point in boundaryPointsSuggestion)
                        {
                            var tempSafeDistance = tempSphere.MinimumDistanceToDifferentBoundary(point, constMinStartingDistance);
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
                linesThroughSpace.TryAdd(index, tempLine);
                loopCondition = true;
            });

            if (result.IsCompleted)
            {
                return linesThroughSpace.Select(x => x.Value).ToList();
            }
            else
            {
                throw new Exception("LQ49");
            }
        }

        public List<Hyperplane> GetFirstLayer(List<Hyperplane> hyperPlanes, double testRadius)
        {
            var retVal = new List<Hyperplane>();

            foreach (var plane in hyperPlanes)
            {
                var temp = sphere.FirstLayerTest(plane, stdNumTestPoints, testRadius);
                if (temp.Count(x => x.Count == 1) > 0.8 * stdNumTestPoints) {
                    retVal.Add(plane);
                }
            }
            return retVal;
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
