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
            var iterations = boundaryPoint.numRow + boundaryPoint.numCol + 3;
            var aa = Parallel.For(0, iterations, index =>
            {
                var tempModel = model.Copy(index);

                var bb = new Matrix(boundaryPoint.numRow, boundaryPoint.numCol);
                bb.PopulateAllRandomlyFarFromZero(tempModel.RandomGenerator);
                bb = Matrix.NormalizeVector(bb, 1);

                var cc = new Matrix(boundaryPoint.numRow, boundaryPoint.numCol);
                cc.PopulateAllRandomly(tempModel.RandomGenerator);
                cc = Matrix.NormalizeVector(cc, 0.02);

                var zz = Matrix.Addition(boundaryPoint, bb);
                var uuu = new SamplingLine(tempModel);
                var dd = uuu.BidirectionalLinearRegionChanges(zz, cc, 8);
                bag.Add(dd);
            });
            //Debug:
            if (aa.IsCompleted)
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
