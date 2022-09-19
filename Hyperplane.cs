using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    class Hyperplane
    {
        public Hyperplane(Model model)
        {
            this.model = model;
            this.sampler = new SamplingLine(model);
        }

        private Model model { get; }
        private SamplingLine sampler { get; }

        public List<Matrix> SupportPointsOnBoundary(Matrix boundaryPoint)
        {
            if (!(boundaryPoint.numRow == 1 || boundaryPoint.numRow == 1))
            {
                return null;
            }

            var bag = new ConcurrentBag<List<Matrix>>();
            var iterations = boundaryPoint.numRow + boundaryPoint.numCol + 6;
            var result = Parallel.For(0, iterations, index =>
            {
                var tempModel = model.Copy(index);

                var displacementVector = new Matrix(boundaryPoint.numRow, boundaryPoint.numCol);
                displacementVector.PopulateAllRandomlyFarFromZero(tempModel.RandomGenerator);
                displacementVector = Matrix.NormalizeVector(displacementVector, 1);

                var directionVector = new Matrix(boundaryPoint.numRow, boundaryPoint.numCol);
                directionVector.PopulateAllRandomlyFarFromZero(tempModel.RandomGenerator);
                directionVector = Matrix.NormalizeVector(directionVector, 0.02);

                var startPoint = Matrix.Addition(boundaryPoint, displacementVector);
                var tempSampler = new SamplingLine(tempModel);
                var dd = tempSampler.BidirectionalLinearRegionChanges(startPoint, directionVector, 8);
                bag.Add(dd);
            });
            //Debug:
            if (result.IsCompleted)
            {
                foreach (var item in bag)
                {
                    if (item.Count != 1)
                    {
                        continue;
                    }
                    Console.WriteLine(Matrix.GetEuclideanNormForVector(Matrix.Substraction(boundaryPoint, item.First()))*100);
                }
            }
            
            return new List<Matrix>();
        }


    }
}
