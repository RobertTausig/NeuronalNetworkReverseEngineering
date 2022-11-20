using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    class LayerCalculation
    {
        public LayerCalculation(Model model, SamplingLine sampler)
        {
            this.model = model;
            this.sampler = sampler;
        }

        private Model model { get; }
        private SamplingLine sampler { get; }
        private int stdMaxMagnitude = 8;
        private int stdNumTestPoints = 500;
        private int stdNumTestLines = 30;

        public List<Hyperplane> GetFirstLayer(List<Hyperplane> hyperPlanes)
        {
            var retVal = new List<Hyperplane>();

            List<int> papap = new List<int>();
            foreach (var plane in hyperPlanes)
            {
                int cnt = 0;
                for (int i = 0; i < stdNumTestPoints; i++)
                {
                    var genPoint = plane.GenerateRandomPointOnPlane(1_000);
                    var norm = Matrix.GetEuclideanNormForVector(genPoint);
                    int sphereCnt = 0;
                    for (int j = 0; j < stdNumTestLines; j++)
                    {
                        var directionVector = new Matrix(genPoint.numRow, genPoint.numCol);
                        directionVector.PopulateAllRandomlyFarFromZero(model.RandomGenerator);
                        directionVector = Matrix.NormalizeVector(directionVector, (double)norm / 31_000);
                        var boundaryPoints = sampler.BidirectionalLinearRegionChanges(genPoint, directionVector, stdMaxMagnitude);
                        sphereCnt += boundaryPoints.Count;
                        if (sphereCnt > 0)
                        {
                            break;
                        }
                    }
                    if (sphereCnt > 0)
                    {
                        cnt++;
                    }
                }
                papap.Add(cnt);
            }

            for (int i = 0; i < hyperPlanes.Count; i++)
            {
                if (papap[i] > 0.9 * stdNumTestPoints)
                {
                    retVal.Add(hyperPlanes[i]);
                }
            }
            return retVal;
        }





    }
}
