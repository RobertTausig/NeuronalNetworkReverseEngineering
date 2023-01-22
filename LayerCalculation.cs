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
        private int stdNumTestPoints = 500;
        private int stdNumTestLines = 30;

        public SpaceLineBundle DriveLinesThroughSpace(int numLines, double minSpacedApartDistance)
        {
            int sumHiddenLayerDims = model.topology[1..^1].Sum(x => x);
            double constRadius = Math.Pow(sumHiddenLayerDims, 2) * Math.Sqrt(minSpacedApartDistance);
            double constMinSpacedApartDistance = minSpacedApartDistance;
            double constMinSafeDistance = constMinSpacedApartDistance / 2;
            double constMinStartingDistance = constMinSafeDistance / 10;
            double constMinPointDistance = constMinStartingDistance / 10;
            int constMaxMagnitude = 24;

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
                if (IsSpacedApart(boundaryPointsSuggestion, constMinSafeDistance))
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
                conc.TryAdd(index, tempLine);
            });

            salt += saltIncreasePerUsage;
            if (result.IsCompleted)
            {
                return new SpaceLineBundle(conc.Select(x => x.Value).ToList());
            }
            else
            {
                throw new Exception("LQ49");
            }
        }

        public List<List<Hyperplane>> SpaceLinesToHyperplanes(SpaceLineBundle bundle)
        {
            var retVal = new List<List<Hyperplane>>();
            for (int i = 0; i < bundle.SpaceLines.Count(); i++)
            {
                var tempModel = model.Copy(salt + i);

                var hyperPlanes = new List<Hyperplane>();
                foreach (var l in bundle.SpaceLines[i].SpaceLinePoints)
                {
                    hyperPlanes.Add(new Hyperplane(tempModel, l.BoundaryPoint, (double)l.SafeDistance / 15, hasIntercept: false));
                }

                retVal.Add(hyperPlanes);
            }

            salt+= saltIncreasePerUsage;
            return retVal;
        }
        public List<Hyperplane> New_GetFirstLayer(List<List<Hyperplane>> hyperPlanesColl, double testRadius)
        {
            var conc = new ConcurrentDictionary<int, List<Hyperplane>>();
            var result = Parallel.For(0, hyperPlanesColl.Count, index =>
            {
                var tempModel = model.Copy(index + salt);
                var tempSphere = new SamplingSphere(tempModel);

                var resultingHyperplanes = new List<Hyperplane>();
                foreach (var plane in hyperPlanesColl[index])
                {
                    var temp = tempSphere.Old_FirstLayerTest(plane, stdNumTestPoints, testRadius);
                    if (temp.Count(x => x.Count == 1) > 0.8 * stdNumTestPoints)
                    {
                        resultingHyperplanes.Add(plane);
                    }
                }
                conc.TryAdd(index, resultingHyperplanes);
            });

            salt += saltIncreasePerUsage;
            if (result.IsCompleted)
            {
                var gg = new List<Hyperplane>();
                var tt = conc.Select(x => x.Value).ToList();
                gg.AddRange(tt[0]);
                for (int i = 0; i < tt.Count; i++)
                {
                    for (int j = 0; j < tt[i].Count; j++)
                    {
                        var temp = tt[i][j];
                        var booli = gg.Any(x => true == Matrix.ApproxEqual(temp.planeIdentity.parameters, x.planeIdentity.parameters, 0.3));
                        if (!booli)
                        {
                            gg.Add(temp);
                        }
                    }
                }
                return gg;
            }
            else
            {
                throw new Exception("IY26");
            }
        }
        public List<Hyperplane> Old_GetFirstLayer(List<Hyperplane> hyperPlanes, double testRadius)
        {
            var retVal = new List<Hyperplane>();

            foreach (var plane in hyperPlanes)
            {
                var temp = sphere.Old_FirstLayerTest(plane, stdNumTestPoints, testRadius);
                //Console.WriteLine(temp.Count + ", " + temp.Count(x => x.Count == 1));
                if (temp.Count(x => x.Count == 1) > 0.8 * stdNumTestPoints) {
                    retVal.Add(plane);
                }
            }

            //Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            salt += saltIncreasePerUsage;
            return retVal;
        }

        public List<Hyperplane> GetFirstLayer(List<List<(Matrix boundaryPoint, double safeDistance)>> linesThroughSpace, List<Hyperplane> hyperPlanes)
        {
            var retVal = new List<Hyperplane>();
            var firstLayerPlanes = new List<Hyperplane>();
            var potentiallyFirstLayer = new List<List<Matrix>>();
            var spaceDim = model.topology[0];

            foreach (var plane in hyperPlanes)
            {
                var temp = FirstLayerTest(linesThroughSpace, plane, 0.08);

                if (temp.Count > linesThroughSpace.Count * 0.75)
                {
                    firstLayerPlanes.Add(plane);
                }
                else if (temp.Count > spaceDim)
                {
                    potentiallyFirstLayer.Add(temp);
                }
            }

            foreach (var pfl in potentiallyFirstLayer)
            {
                var cc = new Hyperplane(model, pfl, hasIntercept: true);
                var temp = FirstLayerTest(linesThroughSpace, cc, 0.08);
                if (temp.Count > linesThroughSpace.Count * 0.75)
                {
                    retVal.Add(cc);
                }
            }

            //foreach (var flp in firstLayerPlanes)
            //{
            //    var cc = new Hyperplane(model, flp, hasIntercept: true);
            //    var temp = FirstLayerTest(linesThroughSpace, cc, 0.15);
            //    if (temp.Count > linesThroughSpace.Count * 0.8)
            //    {
            //        retVal.Add(cc);
            //    }
            //}
            retVal.AddRange(firstLayerPlanes);

            Console.WriteLine($@"firstLayer Count: {retVal.Count}");
            return retVal;
        }
        private List<Matrix> FirstLayerTest(List<List<(Matrix boundaryPoint, double safeDistance)>> linesThroughSpace, Hyperplane plane, double accuracy)
        {
            var collection = new List<List<Matrix>>();
            foreach (var line in linesThroughSpace)
            {
                var temp = new List<Matrix>();
                foreach (var point in line)
                {
                    if ((bool)plane.IsPointOnPlane(point.boundaryPoint, accuracy))
                    {
                        temp.Add(point.boundaryPoint);
                    }
                }
                collection.Add(temp);
            }

            collection.RemoveAll(x => x.Count != 1);
            return collection.SelectMany(x => x).ToList();
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
