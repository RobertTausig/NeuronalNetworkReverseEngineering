using MathNet.Numerics;
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
        public Hyperplane(Model model, Matrix boundaryPoint)
        {
            this.model = model;
            this.originalBoundaryPoint = boundaryPoint;
            this.spaceDim = boundaryPoint.numRow + boundaryPoint.numCol - 1;
            this.pointsOnPlane = SupportPointsOnBoundary(boundaryPoint, 0, 1, 0.02, 8);
            this.planeIdentity = Matrix.CalculateLinearRegression(pointsOnPlane);
        }

        private Model model { get; }
        public List<Matrix> pointsOnPlane { get; } = new List<Matrix>();
        public (Matrix parameters, double? intercept) planeIdentity { get; }
        public int spaceDim { get; }
        public Matrix originalBoundaryPoint { get; }

        private const int saltIncreasePerRecursion = 1_000;
        private const int maxSalt = 6*saltIncreasePerRecursion;

        private List<Matrix> SupportPointsOnBoundary(Matrix boundaryPoint, int salt, double displacementNorm, double directionNorm, int maxMagnitude)
        {
            if (!(boundaryPoint.numRow == 1 || boundaryPoint.numRow == 1))
            {
                return null;
            }
            if (salt >= maxSalt)
            {
                return new List<Matrix>();
            }

            var retVal = new List<Matrix>();
            var bag = new ConcurrentBag<List<Matrix>>();
            var iterations = 2 * spaceDim + 6;
            var result = Parallel.For(0, iterations, index =>
            {
                var tempModel = model.Copy(index + salt);
                var tempSampler = new SamplingLine(tempModel);

                var displacementVector = new Matrix(boundaryPoint.numRow, boundaryPoint.numCol);
                displacementVector.PopulateAllRandomlyFarFromZero(tempModel.RandomGenerator);
                displacementVector = Matrix.NormalizeVector(displacementVector, displacementNorm);

                var directionVector = new Matrix(boundaryPoint.numRow, boundaryPoint.numCol);
                directionVector.PopulateAllRandomlyFarFromZero(tempModel.RandomGenerator);
                directionVector = Matrix.NormalizeVector(directionVector, directionNorm);

                var startPoint = Matrix.Addition(boundaryPoint, displacementVector);
                var samplePoints = tempSampler.BidirectionalLinearRegionChanges(startPoint, directionVector, maxMagnitude);
                bag.Add(samplePoints);
            });

            if (result.IsCompleted)
            {
                var overshootSample = bag.Where(x => x.Count > 1);
                var fineSample = bag.Where(x => x.Count == 1);
                var undershootSample = bag.Where(x => x.Count < 1);

                if (undershootSample.Count() > iterations * 0.9)
                {
                    return new List<Matrix>();
                }
                else if (fineSample.Count() > iterations * 0.5)
                {
                    foreach (var item in fineSample)
                    {
                        retVal.AddRange(item);
                    }
                    return retVal;
                }
                else
                {
                    if (overshootSample.Count() > undershootSample.Count())
                    {
                        salt += saltIncreasePerRecursion;
                        return SupportPointsOnBoundary(boundaryPoint, salt, displacementNorm, directionNorm / 1.25, maxMagnitude);
                    }
                    else
                    {
                        salt += saltIncreasePerRecursion;
                        return SupportPointsOnBoundary(boundaryPoint, salt, displacementNorm, directionNorm * 1.25, maxMagnitude);
                    }
                }

                //foreach (var item in bag)
                //{
                //    if (item.Count == 1)
                //    {
                //        retVal.AddRange(item);
                //    }
                //}
                //if (retVal.Count < 2)
                //{
                //    return new List<Matrix>();
                //}
                ////Mathematical minimum: retVal.Count < spaceDim
                //else if (retVal.Count < spaceDim + 3)
                //{
                //    salt += saltIncreasePerRecursion;
                //    return SupportPointsOnBoundary(boundaryPoint, salt, displacementNorm, directionNorm, maxMagnitude);
                //}
                //return retVal;

            }
            else
            {
                throw new Exception("FA80");
            }

        }

        public Matrix GenerateRandomPointOnPlane()
        {
            var xCoords = new Matrix(1, spaceDim - 1);
            xCoords.PopulateAllRandomlyFarFromZero(model.RandomGenerator);
            xCoords = Matrix.NormalizeVector(xCoords, 80);
            var yCoord = Matrix.Multiplication(xCoords, planeIdentity.parameters);
            return Matrix.ConcatHorizontally(xCoords, yCoord);
        }


    }
}
