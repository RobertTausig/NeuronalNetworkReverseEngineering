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
        public Hyperplane(Model model, Matrix boundaryPoint)
        {
            this.model = model;
            this.originalBoundaryPoint = boundaryPoint;
            this.spaceDim = boundaryPoint.numRow + boundaryPoint.numCol - 1;
            this.pointsOnPlane = SupportPointsOnBoundary(boundaryPoint, 0);
            this.planeIdentity = Matrix.CalculateLinearRegression(pointsOnPlane);
        }

        private Model model { get; }
        public List<Matrix> pointsOnPlane { get; } = new List<Matrix>();
        public (Matrix parameters, double? intercept) planeIdentity { get; }
        public int spaceDim { get; }
        public Matrix originalBoundaryPoint { get; }

        private const int saltIncreasePerRecursion = 1_000;
        private const int maxSalt = 4*saltIncreasePerRecursion;

        private List<Matrix> SupportPointsOnBoundary(Matrix boundaryPoint, int salt)
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
            var iterations = spaceDim + 11;
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
                var samplePoints = tempSampler.BidirectionalLinearRegionChanges(startPoint, directionVector, 8);
                bag.Add(samplePoints);
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
                if (retVal.Count < 2)
                {
                    return new List<Matrix>();
                }
                //Mathematical minimum: retVal.Count < spaceDim
                else if (retVal.Count < spaceDim + 3)
                {
                    salt += saltIncreasePerRecursion;
                    return SupportPointsOnBoundary(boundaryPoint, salt);
                }
                return retVal;

            }
            else
            {
                throw new Exception("FA80");
            }

        }

        public Matrix GenerateRandomPointOnPlane()
        {
            //var retVal = new Matrix(1, spaceDim);
            //var xCoords = new Matrix(1, spaceDim - 1);
            //xCoords.PopulateAllRandomlyFarFromZero(model.RandomGenerator);
            //xCoords = Matrix.NormalizeVector(xCoords, 70);

            //var yCoord = Matrix.Multiplication(xCoords, planeIdentity.parameters);
            //var potentialPointOnPlane = Matrix.ConcatHorizontally(xCoords, yCoord);

            //Debug:
            for (int i = 0; i < 50; i++)
            {
                var notOnPlane = new Matrix(1, spaceDim);
                notOnPlane.PopulateAllRandomlyFarFromZero(new Random(i));
                notOnPlane = Matrix.NormalizeVector(notOnPlane, 500);
                Console.WriteLine(SupportPointsOnBoundary(notOnPlane, i + 444).Count);
            }
            



            return new Matrix(1, 1);
        }


    }
}
