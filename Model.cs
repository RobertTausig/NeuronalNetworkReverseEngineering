using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    class Model
    {
        public Model(int[] topology)
        {
            if (topology.Length < 3)
            {
                return;
            }

            for (int i = 0; i < topology.Length - 1; i++)
            {
                var tempMat = new Matrix(topology[i], topology[i + 1]);
                tempMat.PopulateAllRandomly(i);
                weigthMatrices.Add(tempMat);

                var tempVec = new Matrix(1, topology[i + 1]);
                tempVec.PopulateAllRandomly(i + 100);
                biasVectors.Add(tempVec);
            }
            this.topology = topology;
        }

        private List<Matrix> weigthMatrices = new List<Matrix>();
        private List<Matrix> biasVectors = new List<Matrix>();
        public int[] topology { get; }
        public List<Matrix> neuronValues {get;}



        public Matrix Use(Matrix input)
        {
            if(input.numRow != 1 || input.numCol != this.topology.First())
            {
                return null;
            }

            neuronValues.Add(input);
            for (int i = 0; i < topology.Length; i++)
            {
                var temp = Matrix.Multiplication(neuronValues[i], weigthMatrices[i]);
            }


        } 


    }
}
