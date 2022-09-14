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

        public List<double[]> RandomSecantLine(double radius, double minPointDistance)
        {
            var firstVector = new Matrix(1, model.topology.First());
            firstVector.PopulateAllRandomly(model.RandomGenerator);
            var secondVector = new Matrix(1, model.topology.First());
            secondVector.PopulateAllRandomly(model.RandomGenerator);

            var aa = Matrix.NormalizeVector(firstVector);

            return new List<double[]>();
        }


    }
}
