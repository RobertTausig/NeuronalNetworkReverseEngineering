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
                var temp = new Matrix(topology[i], topology[i + 1]);
                this.matrices.Add(temp);
            }
        }

        private List<Matrix> matrices = new List<Matrix>(); 



    }
}
