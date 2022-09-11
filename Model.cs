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
                this.weigthMatrices.Add(tempMat);

                var tempVec = new Matrix(1, topology[i + 1]);
                tempVec.PopulateAllRandomly(i + 100);
                this.biasVectors.Add(tempVec);
            }
        }

        private List<Matrix> weigthMatrices = new List<Matrix>();
        private List<Matrix> biasVectors = new List<Matrix>();



    }
}
