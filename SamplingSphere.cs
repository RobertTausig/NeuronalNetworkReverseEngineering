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
        int stdIterations = 100;
        double stdIterationGrowth = 1.08;
        int stdMaxMagnitude = 6;

        public double MinimumDistanceToDifferentBoundary(Matrix boundaryPoint, double startingDistance)
        {
            var retVal = new List<Matrix>();
            double directionNorm = startingDistance / Math.Pow(2, stdMaxMagnitude);
            var conc = new ConcurrentDictionary<int, List<Matrix>>();
            var result = Parallel.For(0, stdIterations, index =>
            {
                var tempModel = model.Copy(index);
                var tempSampler = new SamplingLine(tempModel);

                var directionVector = new Matrix(boundaryPoint.numRow, boundaryPoint.numCol);
                directionVector.PopulateAllRandomlyFarFromZero(tempModel.RandomGenerator);
                directionVector = Matrix.NormalizeVector(directionVector, directionNorm * Math.Pow(stdIterationGrowth, index));

                var samplePoints = tempSampler.BidirectionalLinearRegionChanges(boundaryPoint, directionVector, stdMaxMagnitude);
                conc.TryAdd(index, samplePoints);
            });

            if (result.IsCompleted)
            {
                var temp = conc.First(x => x.Value.Count > 1).Key;
                return startingDistance * Math.Pow(stdIterationGrowth, temp);
            }
            else
            {
                throw new Exception("CX31");
            }
        }



    }
}
