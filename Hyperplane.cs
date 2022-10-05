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
        private List<Matrix> temporaryPointsOnPlane = new List<Matrix>();
        public (Matrix parameters, double? intercept) planeIdentity { get; }
        public int spaceDim { get; }
        public Matrix originalBoundaryPoint { get; }

        private const int saltIncreasePerRecursion = 1_000;
        private const int maxSalt = 4*saltIncreasePerRecursion;

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
                else if (fineSample.Count() + temporaryPointsOnPlane.Count > iterations * 0.5)
                {
                    foreach (var item in fineSample)
                    {
                        retVal.AddRange(item);
                    }
                    retVal.AddRange(temporaryPointsOnPlane);
                    temporaryPointsOnPlane.Clear();
                    return retVal;
                }
                else
                {
                    foreach (var item in fineSample)
                    {
                        temporaryPointsOnPlane.AddRange(item);
                    }

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

        //Probability for a random point to return "true" with accuracy 0.1: ~2%
        public bool? IsPointOnPlane (Matrix point, double accuracy = 0.05)
        {
            if (point.numRow != 1)
            {
                return null;
            }

            double upperLimit = 1 + accuracy;
            double lowerLimit = 1 / upperLimit;

            var flattenPoint = Matrix.FlattenVector(point);
            var xCoords = new Matrix(1, spaceDim - 1);
            for (int i = 0; i < flattenPoint.Length - 1; i++)
            {
                xCoords.SetValue(0, i, flattenPoint[i]);
            }
            var yCoord = flattenPoint[^1];

            var yCoordCalculated = Matrix.Multiplication(xCoords, planeIdentity.parameters);
            var quotient = yCoord / (Matrix.FlattenVector(yCoordCalculated).First() + planeIdentity.intercept);

            return (lowerLimit < quotient) && (upperLimit > quotient);
        }

    }
}
