using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    class SamplingSphere
    {
        public SamplingSphere(Model model)
        {
            this.model = model;
        }

        private Model model { get; }
        private int stdMaxMagnitude = 6;
        private int salt = 0;
        private int saltIncreasePerUsage = 1_000;

        public double? MinimumDistanceToDifferentBoundary(Matrix boundaryPoint, double startingDistance)
        {
            var retVal = new List<Matrix>();

            double directionNorm = startingDistance / Math.Pow(2, stdMaxMagnitude);
            var spaceDim = boundaryPoint.numRow + boundaryPoint.numCol - 1;
            var iterations = (spaceDim + 1) * 15;
            var iterationGrowth = Math.Pow(3_000, 1.0 / iterations);

            var conc = new ConcurrentDictionary<int, List<Matrix>>();
            var result = Parallel.For(0, iterations, index =>
            {
                var tempModel = model.Copy(index + salt);
                var tempSampler = new SamplingLine(tempModel);

                var directionVector = new Matrix(boundaryPoint.numRow, boundaryPoint.numCol);
                directionVector.PopulateAllRandomlyFarFromZero(tempModel.RandomGenerator);
                directionVector = Matrix.NormalizeVector(directionVector, directionNorm * Math.Pow(iterationGrowth, index));

                var samplePoints = tempSampler.BidirectionalLinearRegionChanges(boundaryPoint, directionVector, stdMaxMagnitude);
                conc.TryAdd(index, samplePoints);
            });

            if (result.IsCompleted)
            {
                salt += saltIncreasePerUsage;
                int boundaryIndex = conc.FirstOrDefault(x => x.Value.Count > 1).Key;
                if (boundaryIndex == 0)
                {
                    return null;
                }
                else
                {
                    return startingDistance * Math.Pow(iterationGrowth, boundaryIndex);
                }
            }
            else
            {
                throw new Exception("CX31");
            }
        }



    }
}
