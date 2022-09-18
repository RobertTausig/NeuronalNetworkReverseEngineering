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

            RandomGenerator = new Random(0);
            for (int i = 0; i < topology.Length - 1; i++)
            {
                var tempMat = new Matrix(topology[i], topology[i + 1]);
                tempMat.PopulateAllRandomly(RandomGenerator);
                weigthMatrices.Add(tempMat);

                var tempVec = new Matrix(1, topology[i + 1]);
                tempVec.PopulateAllRandomly(RandomGenerator);
                biasVectors.Add(tempVec);
            }
            this.topology = topology;
        }
        private Model(Model model, int? randomSeed = null)
        {
            this.topology = model.topology;
            this.weigthMatrices = model.weigthMatrices;
            this.biasVectors = model.biasVectors;
            this.RandomGenerator = randomSeed == null ? model.RandomGenerator : new Random((int)randomSeed);
        }

        private List<Matrix> weigthMatrices = new List<Matrix>();
        private List<Matrix> biasVectors = new List<Matrix>();
        public int[] topology { get; }
        public List<Matrix> neuronValues { get; } = new List<Matrix>();
        public Random RandomGenerator { get; }



        public Matrix Use(Matrix input)
        {
            if(input.numRow != 1 || input.numCol != this.topology.First())
            {
                return null;
            }
            neuronValues.Clear();

            neuronValues.Add(input);
            for (int i = 0; i < topology.Length - 1; i++)
            {
                var temp = Matrix.Addition(Matrix.Multiplication(neuronValues[i], weigthMatrices[i]), biasVectors[i]);
                if (i != topology.Length - 2)
                {
                    temp.ReLuOnSelf();
                }
                neuronValues.Add(temp);
            }

            return neuronValues.Last();
        }

        public Model Copy(int? randomSeed = null)
        {
            return new Model(this, randomSeed);
        }


    }
}
