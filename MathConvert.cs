using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinAlg = MathNet.Numerics.LinearAlgebra;

namespace NeuronalNetworkReverseEngineering
{
    public class MathConvert
    {
        public static LinAlg.Matrix<double> MatrixToMatrix(Matrix input)
        {
            if (input.numRow == 1 || input.numCol == 1)
            {
                return null;
            }
            var retVal = LinAlg.Matrix<double>.Build.Dense(input.numRow, input.numCol);

            for (int i = 0; i < input.numRow; i++)
            {
                for (int j = 0; j < input.numCol; j++)
                {
                    retVal[i, j] = input.content[i, j];
                }
            }
            return retVal;
        }

        public static LinAlg.Vector<double> MatrixToVector(Matrix input)
        {
            if (!(input.numRow == 1 || input.numCol == 1))
            {
                return null;
            }
            var retVal = LinAlg.Vector<double>.Build.Dense(input.numRow + input.numCol - 1);

            for (int i = 0; i < input.numRow; i++)
            {
                for (int j = 0; j < input.numCol; j++)
                {
                    retVal[i + j] = input.content[i, j];
                }
            }
            return retVal;
        }

    }
}
