using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    class SamplingLine
    {
        public SamplingLine(Model model)
        {
            this.model = model;
        }

        private Model model { get; }

        private const double stdRadius = 60;
        private const double stdMinPointDistance = 0.1;
        private const int stdMaxMagnitude = 20;

        public (Matrix midPoint, Matrix directionVector) RandomSecantLine(double radius = stdRadius, double minPointDistance = stdMinPointDistance)
        {
            var retVal = new List<Matrix>();

            var firstVector = new Matrix(1, model.topology.First());
            firstVector.PopulateAllRandomlyFarFromZero(model.RandomGenerator);
            firstVector = Matrix.NormalizeVector(firstVector, radius);

            var secondVector = Matrix.GetRandomlyJitteredVectorFarFromZero(firstVector, model.RandomGenerator);
            secondVector = Matrix.NormalizeVector(secondVector, radius);

            var directionVector = Matrix.Substraction(secondVector, firstVector);
            var midPoint = Matrix.Addition(firstVector, Matrix.Multiplication(directionVector, 0.5));
            directionVector = Matrix.NormalizeVector(directionVector, minPointDistance);

            return (midPoint, directionVector);
        }
        public List<Matrix> BidirectionalLinearRegionChanges(Matrix startPoint, Matrix directionVector, int maxMagnitude = stdMaxMagnitude)
        {
            var positivePath = LinearRegionChanges(startPoint, directionVector, maxMagnitude);
            var negativePath = LinearRegionChanges(startPoint, Matrix.Multiplication(directionVector, -1), maxMagnitude);
            positivePath.Reverse();
            return positivePath.Concat(negativePath).ToList();
        }
        public List<Matrix> LinearRegionChanges(Matrix startPoint, Matrix directionVector, int maxMagnitude = stdMaxMagnitude)
        {
            var retVal = new List<Matrix>();

            var oldSamplePoint = Matrix.Addition(startPoint, directionVector);
            var oldOutputDiff = Matrix.Substraction(model.Use(oldSamplePoint), model.Use(startPoint));
            for (int stretchMagnitude = 0; stretchMagnitude < maxMagnitude; stretchMagnitude++)
            {
                var newSamplePoint = Matrix.Addition(oldSamplePoint, Matrix.Multiplication(directionVector, Math.Pow(2, stretchMagnitude)));
                var newOutPutDiff = Matrix.Substraction(model.Use(newSamplePoint), model.Use(Matrix.Substraction(newSamplePoint, directionVector)));
                switch(Matrix.ApproxEqual(newOutPutDiff, oldOutputDiff))
                {
                    case null:
                        throw new Exception();
                    case true:
                        oldSamplePoint = newSamplePoint;
                        continue;
                    case false:
                        if (stretchMagnitude == 0)
                        {
                            retVal.Add(newSamplePoint);
                            var temp = Matrix.Addition(newSamplePoint, directionVector);
                            oldOutputDiff = Matrix.Substraction(model.Use(temp), model.Use(newSamplePoint));
                            oldSamplePoint = temp;

                        }
                        else if (stretchMagnitude < 0)
                        {
                            throw new Exception();
                        }
                        else
                        {
                            stretchMagnitude -= 2;
                        }
                        break;
                }
            }

            return retVal;
        }

    }
}
