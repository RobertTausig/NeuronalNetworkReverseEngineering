using DecimalMath;
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

        public decimal? MinimumDistanceToDifferentBoundary(Matrix boundaryPoint, decimal startingDistance)
        {
            var retVal = new List<Matrix>();

            decimal directionNorm = startingDistance / DecimalEx.Pow(2, stdMaxMagnitude);
            var spaceDim = boundaryPoint.numRow + boundaryPoint.numCol - 1;
            var iterations = (spaceDim + 1) * 30;
            var iterationGrowth = DecimalEx.Pow(10_000, 1.0M / iterations);

            var conc = new ConcurrentDictionary<int, List<Matrix>>();
            var result = Parallel.For(0, iterations, (index, state) =>
            {
                var tempModel = model.Copy(index + salt);
                var tempSampler = new SamplingLine(tempModel);

                var directionVector = new Matrix(boundaryPoint.numRow, boundaryPoint.numCol);
                directionVector.PopulateAllRandomlyFarFromZero(tempModel.RandomGenerator);
                directionVector = Matrix.NormalizeVector(directionVector, directionNorm * DecimalEx.Pow(iterationGrowth, index));

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
                    directionVector = Matrix.NormalizeVector(directionVector, directionNorm * DecimalEx.Pow(iterationGrowth, (long)boundaryIndex - shrinkIndex));
                    var samplePoints = sampler.BidirectionalLinearRegionChanges(boundaryPoint, directionVector, stdMaxMagnitude);
                    if(samplePoints.Count < 2)
                    {
                        break;
                    }
                    shrinkIndex++;
                }
                return startingDistance * DecimalEx.Pow(iterationGrowth, (long)boundaryIndex - shrinkIndex);
            }
        }

        public SpaceLine MinimumDistanceToDifferentBoundary(List<Matrix> boundaryPoints, decimal startingDistance)
        {
            var retVal = new SpaceLine();
            var conc = new ConcurrentDictionary<int, decimal?>();

            var result = Parallel.For(0, boundaryPoints.Count, index =>
            {
                var tempModel = model.Copy(index + salt);
                var tempSphere = new SamplingSphere(tempModel);

                conc.TryAdd(index, this.MinimumDistanceToDifferentBoundary(boundaryPoints[index], startingDistance));
            });

            salt += saltIncreasePerUsage;
            if (result.IsCompleted)
            {
                for (int i = 0; i < boundaryPoints.Count; i++)
                {
                    retVal.SpaceLinePoints.Add(new SpaceLinePoint
                    {
                        BoundaryPoint = boundaryPoints[i],
                        SafeDistance = conc[i]
                    });
                }
                return retVal;
            }
            else
            {
                throw new Exception("TJ90");
            }
        }

        public List<bool> FirstLayerTest(Hyperplane plane, int numTestPoints, decimal radius)
        {
            var retVal = new List<Matrix>();
            var maxTestLines = (plane.spaceDim + 1) * 5;
            var conc = new ConcurrentDictionary<int, bool>();

            var result = Parallel.For(0, numTestPoints, index =>
            {
                var tempModel = model.Copy(index + salt);
                var tempSampler = new SamplingLine(tempModel);

                var genPoint = plane.GenerateRandomPointOnPlane(radius);
                var norm = Matrix.GetEuclideanNormForVector(genPoint);
                for (int j = 0; j < maxTestLines; j++)
                {
                    var directionVector = new Matrix(genPoint.numRow, genPoint.numCol);
                    directionVector.PopulateAllRandomlyFarFromZero(tempModel.RandomGenerator);
                    directionVector = Matrix.NormalizeVector(directionVector, (decimal)norm / radius * 8);
                    if (tempSampler.IsPointInRangeOfBoundary(genPoint, directionVector))
                    {
                        conc.TryAdd(index, true);
                        break;
                    }
                }
            });

            salt += saltIncreasePerUsage;
            if (result.IsCompleted)
            {
                return conc.Values.ToList();
            }
            else
            {
                throw new Exception("KQ35");
            }
        }
        public List<Hyperplane> CorrectIntercepts(List<Hyperplane> hyperPlanes, decimal radius)
        {
            var retVal = new List<Hyperplane>();
            var conc = new ConcurrentDictionary<int, List<Matrix>>();

            var result = Parallel.For(0, hyperPlanes.Count, index =>
            {
                var tempModel = model.Copy(index + salt);
                var tempSampler = new SamplingLine(tempModel);
                var plane = hyperPlanes[index];
                var bb = new List<Matrix>();

                while (bb.Count < plane.spaceDim)
                {
                    var genPoint = plane.GenerateRandomPointOnPlane(radius);
                    var norm = Matrix.GetEuclideanNormForVector(genPoint);

                    var directionVector = new Matrix(genPoint.numRow, genPoint.numCol);
                    directionVector.PopulateAllRandomlyFarFromZero(tempModel.RandomGenerator);
                    directionVector = Matrix.NormalizeVector(directionVector, (decimal)norm / radius / 32);
                    var aa = tempSampler.LinearRegionChanges(genPoint, directionVector, 8);
                    if (aa.Count == 1)
                    {
                        bb.Add(aa.First());
                    }
                }
                conc.TryAdd(index, bb);
            });

            salt += saltIncreasePerUsage;
            if (result.IsCompleted)
            {
                foreach (var points in conc.Values)
                {
                    retVal.Add(new Hyperplane(model, points));
                }
                return retVal;
            }
            else
            {
                throw new Exception("KQ35");
            }
        }


    }
}
