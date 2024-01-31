using System;
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
            this.salt = model.RandomGenerator.Next();
        }

        private Model model { get; }
        private SamplingSphere sphere { get; }
        private int salt;
        private int saltIncreasePerUsage = 1_000;

        private int stdMaxMagnitude = 8;
        private int stdNumTestPoints = 100;
        private int stdNumTestLines = 30;

        public SpaceLineBundle DriveLinesThroughSpace(int numLines, bool enableSafeDistance = true)
        {
            int sumHiddenLayerDims = model.topology[1..^1].Sum(x => x);
            double constRadius = Math.Pow(sumHiddenLayerDims, 2) * sumHiddenLayerDims;
            double constMinIsApartDistance = constRadius / Math.Pow(sumHiddenLayerDims, 2);
            double constMinSafeDistance = constMinIsApartDistance / 4;
            double constMinStartingDistance = constMinSafeDistance / 4;
            double constMinPointDistance = constMinStartingDistance / 10;
            int constMaxMagnitude = 16;

            var conc = new ConcurrentDictionary<int, SpaceLine>();
            // Why doing this:
            // To make sure to not accidentally get a "bad" line.
            var result = Parallel.For(0, numLines, index =>
            {
                var tempModel = model.Copy(index + salt);
                var tempSampler = new SamplingLine(tempModel);
                var tempSphere = new SamplingSphere(tempModel);

                var tempLine = new SpaceLine();
                bool loopCondition = true;
                while (loopCondition)
                {
                    var (midPoint, directionVector) = tempSampler.RandomSecantLine(radius: constRadius, minPointDistance: constMinPointDistance);
                    var boundaryPointsSuggestion = tempSampler.BidirectionalLinearRegionChanges(midPoint, directionVector, constMaxMagnitude);
                    if (IsSpacedApart(boundaryPointsSuggestion, constMinIsApartDistance))
                    {
                        if (!enableSafeDistance)
                        {
                            tempLine.SpaceLinePoints = boundaryPointsSuggestion.Select(x => new SpaceLinePoint
                            {
                                BoundaryPoint = x,
                                SafeDistance = 0
                            }).ToList();
                            loopCondition = false;
                        }
                        else
                        {
                            foreach (var point in boundaryPointsSuggestion)
                            {
                                var tempSafeDistance = tempSphere.MinimumDistanceToDifferentBoundary(point, constMinStartingDistance);
                                if (tempSafeDistance != null && tempSafeDistance > constMinSafeDistance)
                                {
                                    tempLine.SpaceLinePoints.Add(new SpaceLinePoint
                                    {
                                        BoundaryPoint = point,
                                        SafeDistance = tempSafeDistance
                                    });
                                    loopCondition = false;
                                }
                                else
                                {
                                    tempLine.SpaceLinePoints.Clear();
                                    loopCondition = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                conc.TryAdd(index, tempLine);
            });

            salt += saltIncreasePerUsage;
            if (result.IsCompleted)
            {
                return new SpaceLineBundle(conc.Values.ToList());
            }
            else
            {
                throw new Exception("LQ49");
            }
        }

        public List<List<Hyperplane>> SpaceLinesToHyperplanes(SpaceLineBundle bundle)
        {
            var retVal = new List<List<Hyperplane>>();

            Parallel.For(0, bundle.SpaceLines.Count, index =>
            {
                var tempModel = model.Copy(salt + index);
                var tempRansac = new RansacAlgorithm(tempModel);

                var hyperPlanes = new List<Hyperplane>();
                foreach (var l in bundle.SpaceLines[index].SpaceLinePoints)
                {
                    var tempPlane = new Hyperplane(tempModel, tempRansac, l.BoundaryPoint, (double)l.SafeDistance / 15, hasIntercept: false);
                    if (tempPlane.pointsOnPlane.Count > 0)
                    {
                        hyperPlanes.Add(tempPlane);
                    }
                }

                retVal.Add(hyperPlanes);
            });

            salt+= saltIncreasePerUsage;
            return retVal;
        }
        public List<Hyperplane> DistinctHyperplanes(List<List<Hyperplane>> hyperPlanesColl)
        {
            var retVal = new List<Hyperplane>();

            for (int i = 0; i < hyperPlanesColl.Count; i++)
            {
                for (int j = 0; j < hyperPlanesColl[i].Count; j++)
                {
                    var temp = hyperPlanesColl[i][j];
                    if (!retVal.Any(x => true == Matrix.ApproxEqual(temp.planeIdentity.Parameters, x.planeIdentity.Parameters, 0.2)))
                    {
                        retVal.Add(temp);
                    }
                }
            }
            return retVal;
        }
        public List<Hyperplane> GetFirstLayer(List<Hyperplane> distinctHyperplanes, double testRadius)
        {
            var conc = new ConcurrentDictionary<int, Hyperplane>();
            var result = Parallel.For(0, distinctHyperplanes.Count, index =>
            {
                var tempModel = model.Copy(index + salt);
                var tempSphere = new SamplingSphere(tempModel);
                var plane = distinctHyperplanes[index];

                var temp = tempSphere.FirstLayerTest(plane, stdNumTestPoints, testRadius);
                if (temp.Count > 0.8 * stdNumTestPoints)
                {
                    conc.TryAdd(index, plane);
                }
            });

            salt += saltIncreasePerUsage;
            if (result.IsCompleted)
            {
                return conc.Values.ToList();
            }
            else
            {
                throw new Exception("IY26");
            }
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
