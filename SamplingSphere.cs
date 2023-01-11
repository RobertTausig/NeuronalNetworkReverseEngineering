using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    class SamplingSphere
    {
        public SamplingSphere(Model model)
        {
            this.model = model;
            this.sampler = new SamplingLine(model);
            this.salt = model.RandomGenerator.Next();
        }

        private Model model { get; }
        private SamplingLine sampler { get; }
        private int stdMaxMagnitude = 6;
        private int salt;
        private int saltIncreasePerUsage = 1_000;

        public double? MinimumDistanceToDifferentBoundary(Matrix boundaryPoint, double startingDistance)
        {
            var retVal = new List<Matrix>();

            double directionNorm = startingDistance / Math.Pow(2, stdMaxMagnitude);
            var spaceDim = boundaryPoint.numRow + boundaryPoint.numCol - 1;
            var iterations = (spaceDim + 1) * 30;
            var iterationGrowth = Math.Pow(10_000, 1.0 / iterations);

            var conc = new ConcurrentDictionary<int, List<Matrix>>();
            var result = Parallel.For(0, iterations, (index, state) =>
            {
                var tempModel = model.Copy(index + salt);
                var tempSampler = new SamplingLine(tempModel);

                var directionVector = new Matrix(boundaryPoint.numRow, boundaryPoint.numCol);
                directionVector.PopulateAllRandomlyFarFromZero(tempModel.RandomGenerator);
                directionVector = Matrix.NormalizeVector(directionVector, directionNorm * Math.Pow(iterationGrowth, index));

                var samplePoints = tempSampler.BidirectionalLinearRegionChanges(boundaryPoint, directionVector, stdMaxMagnitude);
                conc.TryAdd(index, samplePoints);
                if(samplePoints.Count > 1)
                {
                    state.Break();
                }
            });

            salt += saltIncreasePerUsage;
            long? boundaryIndex = result.LowestBreakIteration;
            if (boundaryIndex == null)
            {
                return null;
            }
            else
            {
                int shrinkIndex = 1;
                var directionVector = Matrix.Substraction(conc[(int)boundaryIndex].First(), boundaryPoint);
                while (true)
                {
                    directionVector = Matrix.NormalizeVector(directionVector, directionNorm * Math.Pow(iterationGrowth, (long)boundaryIndex - shrinkIndex));
                    var samplePoints = sampler.BidirectionalLinearRegionChanges(boundaryPoint, directionVector, stdMaxMagnitude);
                    if(samplePoints.Count < 2)
                    {
                        break;
                    }
                    shrinkIndex++;
                }
                return startingDistance * Math.Pow(iterationGrowth, (long)boundaryIndex - shrinkIndex);
            }
        }

        public List<(Matrix boundaryPoint, double? safeDistance)> MinimumDistanceToDifferentBoundary(List<Matrix> boundaryPoints, double startingDistance)
        {
            var retVal = new List<(Matrix boundaryPoint, double? safeDistance)>();
            var conc = new ConcurrentDictionary<int, double?>();

            var result = Parallel.For(0, boundaryPoints.Count, index =>
            {
                var tempModel = model.Copy(index + salt);
                var tempSphere = new SamplingSphere(tempModel);

                conc.TryAdd(index, this.MinimumDistanceToDifferentBoundary(boundaryPoints[index], startingDistance));
            });

            if (result.IsCompleted)
            {
                for (int i = 0; i < boundaryPoints.Count; i++)
                {
                    retVal.Add((boundaryPoints[i], conc[i]));
                }
                return retVal;
            }
            else
            {
                throw new Exception("TJ90");
            }
        }

        public List<List<Matrix>> FirstLayerTest(Hyperplane plane, int numTestPoints, double radius)
        {
            var retVal = new List<Matrix>();
            var maxTestLines = (plane.spaceDim + 1) * 5;
            var conc = new ConcurrentDictionary<int, List<Matrix>>();

            var result = Parallel.For(0, numTestPoints, index =>
            {
                var tempModel = model.Copy(index + salt);
                var tempSampler = new SamplingLine(tempModel);

                var genPoint = plane.GenerateRandomPointOnPlane(radius);
                var norm = Matrix.GetEuclideanNormForVector(genPoint);
                for (int j = 0; j < maxTestLines; j++)
                {
                    var directionVector = new Matrix(genPoint.numRow, genPoint.numCol);
                    directionVector.PopulateAllRandomlyFarFromZero(model.RandomGenerator);
                    directionVector = Matrix.NormalizeVector(directionVector, (double)norm / (12 * radius));
                    var boundaryPoints = tempSampler.BidirectionalLinearRegionChanges(genPoint, directionVector, stdMaxMagnitude);
                    if (boundaryPoints.Count > 0)
                    {
                        conc.TryAdd(index, boundaryPoints);
                        break;
                    }
                }
            });

            if (result.IsCompleted)
            {
                salt += saltIncreasePerUsage;
                return conc.Values.ToList();
            }
            else
            {
                throw new Exception("KQ35");
            }
        }



    }
}
