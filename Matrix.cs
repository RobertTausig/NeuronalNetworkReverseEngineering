using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    public class Matrix
    {
        public Matrix(int x, int y)
        {
            content = new double[x, y];
        }

        private double[,] content;

        public void SetValue(int x, int y, double value)
        {
            content[x, y] = value;
        }


    }
}
