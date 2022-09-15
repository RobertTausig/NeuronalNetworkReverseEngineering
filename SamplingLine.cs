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
        private const double stdlineLength = 500;
        private const double stdMinPointDistance = 0.25;

        public List<Matrix> RandomSecantLine(double radius = stdRadius, double lineLength = stdlineLength, double minPointDistance = stdMinPointDistance)
        {
            var retVal = new List<Matrix>();

            var firstVector = new Matrix(1, model.topology.First());
            firstVector.PopulateAllRandomly(model.RandomGenerator);
            firstVector = Matrix.NormalizeVector(firstVector, radius);

            var secondVector = new Matrix(1, model.topology.First());
            secondVector.PopulateAllRandomly(model.RandomGenerator);
            secondVector = Matrix.NormalizeVector(secondVector, radius);

            var directionVector = Matrix.Substraction(secondVector, firstVector);
            directionVector = Matrix.NormalizeVector(directionVector, minPointDistance);
            for (int i = -(int)(lineLength / minPointDistance) / 2; i < (int)(lineLength / minPointDistance) / 2; i++)
            {
                retVal.Add(Matrix.Addition(firstVector, Matrix.Multiplication(directionVector, i)));
            }

            return retVal;
        }


    }
}
