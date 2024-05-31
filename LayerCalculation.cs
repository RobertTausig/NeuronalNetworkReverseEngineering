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
            this.stdMinRadius = 250 * model.topology[1..^1].Sum(x => x);
            this.stdMaxRadius = stdMinRadius * 8;
        }

        private Model model { get; }
        private SamplingSphere sphere { get; }
        private int salt;
        private int saltIncreasePerUsage = 1_000;

        private int stdMaxMagnitude = 8;
        private int stdNumTestPoints = 50;
        private int stdNumTestLines = 30;

        private double stdMinRadius;
        private double stdMaxRadius;

        public SpaceLineBundle DriveLinesThroughSpace(int numLines, bool enableSafeDistance = true)
        {
            double constMinIsApartDistance = stdMinRadius / 1_000;
            double constMinSafeDistance = constMinIsApartDistance / 4;
            double constMinStartingDistance = constMinSafeDistance / 4;
            double constMinPointDistance = constMinStartingDistance / 10;
            int constMaxMagnitude = 18;

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
                    var (midPoint, directionVector) = tempSampler.RandomSecantLine(radius: stdMinRadius, minPointDistance: constMinPointDistance);
                    var boundaryPointsSuggestion = tempSampler.BidirectionalLinearRegionChanges(midPoint, directionVector, constMaxMagnitude, hasPointLimit: true);
                    if (boundaryPointsSuggestion.Count < 3)
                    {
                        continue;
                    }
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
                            boundaryPointsSuggestion.RemoveAt(0);
                            boundaryPointsSuggestion.RemoveAt(boundaryPointsSuggestion.Count - 1);
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
                    if (tempPlane.pointsOnPlane != null && tempPlane.pointsOnPlane.Count > 0)
                    {
                        hyperPlanes.Add(tempPlane);
                    }
                }

                retVal.Add(hyperPlanes);
            });

            salt+= saltIncreasePerUsage;
            return retVal;
        }
        public List<Hyperplane> DistinctHyperplanes(List<List<Hyperplane>> hyperPlanesColl, double cosineSimilarityThreshold = 0.999)
        {
            var retVal = new List<Hyperplane>();

            for (int i = 0; i < hyperPlanesColl.Count; i++)
            {
                for (int j = 0; j < hyperPlanesColl[i].Count; j++)
                {
                    var temp = hyperPlanesColl[i][j];
                    if (!retVal.Any(x => cosineSimilarityThreshold < Math.Abs(Hyperplane.NormalVectorCosineSimilarity(temp, x))))
                    {
                        retVal.Add(temp);
                    }
                }
            }
            return retVal;
        }
        public (List<Hyperplane> firstLayerPlanes, List<Hyperplane> otherLayerPlanes) DistinguishBySampleability(List<Hyperplane> distinctHyperplanes)
        {
            var s_One = new ConcurrentDictionary<int, Hyperplane>();
            var s_ZeroToHalf = new ConcurrentDictionary<int, Hyperplane>();
            var result = Parallel.For(0, distinctHyperplanes.Count, index =>
            {
                var tempModel = model.Copy(index + salt);
                var tempSphere = new SamplingSphere(tempModel);
                var plane = distinctHyperplanes[index];

                var temp = tempSphere.FirstLayerTest(plane, stdNumTestPoints, (stdMinRadius, stdMaxRadius));
                  if (temp.Count + 1 > 0.80 * stdNumTestPoints)
                {
                    s_One.TryAdd(index, plane);
                }
                else if (temp.Count - 1 < 0.60 * stdNumTestPoints)
                {
                    s_ZeroToHalf.TryAdd(index, plane);
                }
                else
                {
                    //TODO: Program runs frequently into this. This should never happen.
                    //throw new Exception("Could not assess sampleability of hyperplane unambiguously.");
                }
            });

            salt += saltIncreasePerUsage;
            if (result.IsCompleted)
            {
                return (s_One.Values.ToList(), s_ZeroToHalf.Values.ToList());
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
