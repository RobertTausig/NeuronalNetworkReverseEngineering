using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    public class Matrix
    {
        public Matrix(int numRow, int numCol)
        {
            content = new double[numRow, numCol];
            this.numRow = numRow;
            this.numCol = numCol;
        }

        private double[,] content;
        public int numRow { get; }
        public int numCol { get; }

        public void SetValue(int x, int y, double value)
        {
            content[x, y] = value;
        }



        public static Matrix Multiplication(Matrix leftMatrix, Matrix rightMatrix)
        {
            if(leftMatrix.numCol != rightMatrix.numRow || leftMatrix.numRow != rightMatrix.numCol)
            {
                return null;
            }

            for (int i = 0; i < leftMatrix.numCol; i++)
            {

            }


            return new Matrix(3, 2);
        }
    }

    

}
