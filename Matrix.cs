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
        public void PopulateAllRandomly(Random rand)
        {
            for (int i = 0; i < numRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {
                    content[i, j] = (rand.NextDouble() - 0.5) * 2;
                }
            }
        }
        public void ReLuOnSelf()
        {
            for (int i = 0; i < numRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {
                    if (content[i, j] < 0)
                    {
                        content[i, j] = 0;
                    }
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
        public static Matrix Multiplication(Matrix matrix, double factor)
        {
            var retMatrix = new Matrix(matrix.numRow, matrix.numCol);
            var content = matrix.content;
            for (int i = 0; i < matrix.numRow; i++)
            {
                for (int j = 0; j < matrix.numCol; j++)
                {
                    retMatrix.SetValue(i, j, content[i, j] * factor);
                }
            }

            return retMatrix;
        }
        public static Matrix Addition(Matrix leftMatrix, Matrix rightMatrix)
        {
            if (leftMatrix.numRow != rightMatrix.numRow || leftMatrix.numCol != rightMatrix.numCol)
            {
                return null;
            }

            var retMatrix = new Matrix(leftMatrix.numRow, leftMatrix.numCol);
            var leftContent = leftMatrix.content;
            var rightContent = rightMatrix.content;

            for (int i = 0; i < leftMatrix.numRow; i++)
            {
                for (int j = 0; j < leftMatrix.numCol; j++)
                {
                    retMatrix.SetValue(i, j, leftContent[i, j] + rightContent[i, j]);
                }
            }

            return retMatrix;
        }
        public static Matrix Substraction(Matrix leftMatrix, Matrix rightMatrix)
        {
            if (leftMatrix.numRow != rightMatrix.numRow || leftMatrix.numCol != rightMatrix.numCol)
            {
                return null;
            }

            var retMatrix = new Matrix(leftMatrix.numRow, leftMatrix.numCol);
            var leftContent = leftMatrix.content;
            var rightContent = rightMatrix.content;

            for (int i = 0; i < leftMatrix.numRow; i++)
            {
                for (int j = 0; j < leftMatrix.numCol; j++)
                {
                    retMatrix.SetValue(i, j, leftContent[i, j] - rightContent[i, j]);
                }
            }

            return retMatrix;
        }

        public static bool? ApproxEqual(Matrix leftMatrix, Matrix rightMatrix)
        {
            if (leftMatrix.numRow != rightMatrix.numRow || leftMatrix.numCol != rightMatrix.numCol)
            {
                return null;
            }

            var leftContent = leftMatrix.content;
            var rightContent = rightMatrix.content;

            for (int i = 0; i < leftMatrix.numRow; i++)
            {
                for (int j = 0; j < leftMatrix.numCol; j++)
                {
                    var temp = leftContent[i, j] / rightContent[i, j];
                    if (!((0.9524 < temp && temp < 1.05) || leftContent[i, j] == rightContent[i, j]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static Matrix NormalizeVector(Matrix matrix, double norm = 1)
        {
            var currentNorm = GetEuclideanNormForVector(matrix);
            if (currentNorm == null)
            {
                return null;
            }

            double stretchFactor = norm / (double)currentNorm;
            var retMatrix = new Matrix(matrix.numRow, matrix.numCol);
            var content = matrix.content;
            for (int i = 0; i < matrix.numRow; i++)
            {
                for (int j = 0; j < matrix.numCol; j++)
                {
                    retMatrix.SetValue(i, j, content[i, j] * stretchFactor);
                }
            }

            return retMatrix;
        }

        public static double? GetEuclideanNormForVector(Matrix matrix)
        {
            if (!(matrix.numRow == 1 || matrix.numRow == 1))
            {
                return null;
            }

            double euclidianNorm = 0;
            var content = matrix.content;
            for (int i = 0; i < matrix.numRow; i++)
            {
                for (int j = 0; j < matrix.numCol; j++)
                {
                    euclidianNorm += Math.Pow(content[i, j], 2);
                }
            }

            return Math.Sqrt(euclidianNorm);
        }


    }

    

}
