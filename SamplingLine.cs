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

        public List<double[]> RandomSecantLine(double radius = stdRadius, double lineLength = stdlineLength, double minPointDistance = stdMinPointDistance)
        {
            var firstVector = new Matrix(1, model.topology.First());
            firstVector.PopulateAllRandomly(model.RandomGenerator);
            var secondVector = new Matrix(1, model.topology.First());
            secondVector.PopulateAllRandomly(model.RandomGenerator);

            var firstNormVector = Matrix.NormalizeVector(firstVector, radius);
            var secondNormVector = Matrix.NormalizeVector(secondVector, radius);

            var aa = Matrix.Substraction(secondNormVector, firstNormVector);
            var bb = Matrix.GetEuclideanNormForVector(aa);


            return new List<double[]>();
        }


    }
}
