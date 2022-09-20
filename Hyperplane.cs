﻿using System;
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

        private const int saltIncreasePerRecursion = 1_000;
        private const int maxSalt = 4*saltIncreasePerRecursion;

        public List<Matrix> SupportPointsOnBoundary(Matrix boundaryPoint, int salt = 0)
        {
            if (!(boundaryPoint.numRow == 1 || boundaryPoint.numRow == 1))
            {
                return null;
            }
            if (salt > maxSalt)
            {
                throw new Exception("PM06");
            }

            var retVal = new List<Matrix>();
            var bag = new ConcurrentBag<List<Matrix>>();
            var iterations = boundaryPoint.numRow + boundaryPoint.numCol + 10;
            var result = Parallel.For(0, iterations, index =>
            {
                var tempModel = model.Copy(index + salt);
                var tempSampler = new SamplingLine(tempModel);

                var displacementVector = new Matrix(boundaryPoint.numRow, boundaryPoint.numCol);
                displacementVector.PopulateAllRandomlyFarFromZero(tempModel.RandomGenerator);
                displacementVector = Matrix.NormalizeVector(displacementVector, 1);

                var directionVector = new Matrix(boundaryPoint.numRow, boundaryPoint.numCol);
                directionVector.PopulateAllRandomlyFarFromZero(tempModel.RandomGenerator);
                directionVector = Matrix.NormalizeVector(directionVector, 0.02);

                var startPoint = Matrix.Addition(boundaryPoint, displacementVector);
                var supportPoints = tempSampler.BidirectionalLinearRegionChanges(startPoint, directionVector, 8);
                bag.Add(supportPoints);
            });

            if (result.IsCompleted)
            {
                foreach (var item in bag)
                {
                    if (item.Count == 1)
                    {
                        retVal.AddRange(item);
                    }
                }
                //Mathematical minimum: retVal.Count < (boundaryPoint.numRow + boundaryPoint.numCol - 1)
                if (retVal.Count < (boundaryPoint.numRow + boundaryPoint.numCol + 2))
                {
                    salt += saltIncreasePerRecursion;
                    return SupportPointsOnBoundary(boundaryPoint, salt);
                }
                Console.WriteLine(retVal.Count);
                return retVal;

            }
            else
            {
                throw new Exception("FA80");
            }

        }


    }
}
