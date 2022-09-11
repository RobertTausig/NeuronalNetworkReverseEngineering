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

        private double[,] content { get; }
        public int numRow { get; }
        public int numCol { get; }

        public void SetValue(int rowIndex, int colIndex, double value)
        {
            content[rowIndex, colIndex] = value;
        }
        public void PopulateAllRandomly(int? seed)
        {
            var rand = seed == null ? new Random() : new Random((int)seed);
            for (int i = 0; i < numRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {
                    content[i, j] = (rand.NextDouble() - 0.5) * 2;
                }
            }
        }


        public static Matrix Multiplication(Matrix leftMatrix, Matrix rightMatrix)
        {
            if(leftMatrix.numCol != rightMatrix.numRow)
            {
                return null;
            }

            var retMatrix = new Matrix(leftMatrix.numRow, rightMatrix.numCol);
            var leftContent = leftMatrix.content;
            var rightContent = rightMatrix.content;

            for (int i = 0; i < leftMatrix.numRow; i++)
            {
                for (int j = 0; j < rightMatrix.numCol; j++)
                {
                    double tempVal = 0;
                    for (int n = 0; n < leftMatrix.numCol; n++)
                    {
                        tempVal += leftContent[i, n] * rightContent[n, j];
                    }
                    retMatrix.SetValue(i, j, tempVal);
                }
            }

            return retMatrix;
        }
    }

    

}
