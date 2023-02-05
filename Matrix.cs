﻿using MathNet.Numerics;
using MathNet.Numerics.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DecimalMath;

namespace NeuronalNetworkReverseEngineering
{
    public class Matrix
    {
        public Matrix(int numRow, int numCol)
        {
            content = new decimal[numRow, numCol];
            this.numRow = numRow;
            this.numCol = numCol;
        }

        private decimal[,] content { get; }
        public int numRow { get; }
        public int numCol { get; }

        public void SetValue(int rowIndex, int colIndex, decimal value)
        {
            content[rowIndex, colIndex] = value;
        }
        public void PopulateAllRandomly(Random rand)
        {
            for (int i = 0; i < numRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {
                    content[i, j] = (rand.NextDecimal() - 0.5M) * 2;
                }
            }
        }
        public void PopulateAllRandomlyFarFromZero(Random rand)
        {
            for (int i = 0; i < numRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {
                    var temp = rand.Next(4_000, 10_000);
                    content[i, j] = temp % 2 == 0 ? temp / 10_000.0M : -temp / 10_000.0M;
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
                    decimal tempVal = 0;
                    for (int n = 0; n < leftMatrix.numCol; n++)
                    {
                        tempVal += leftContent[i, n] * rightContent[n, j];
                    }
                    retMatrix.SetValue(i, j, tempVal);
                }
            }

            return retMatrix;
        }
        public static Matrix Multiplication(Matrix matrix, decimal factor)
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

        public static Matrix ConcatHorizontally (Matrix leftMatrix, Matrix rightMatrix)
        {
            if (leftMatrix.numRow != rightMatrix.numRow)
            {
                return null;
            }

            var retMatrix = new Matrix(leftMatrix.numRow, leftMatrix.numCol + rightMatrix.numCol);
            var leftContent = leftMatrix.content;
            var rightContent = rightMatrix.content;

            for (int i = 0; i < retMatrix.numRow; i++)
            {
                for (int j = 0; j < retMatrix.numCol; j++)
                {
                    if (j < leftMatrix.numCol)
                    {
                        retMatrix.SetValue(i, j, leftContent[i, j]);
                    }
                    else
                    {
                        retMatrix.SetValue(i, j, rightContent[i, j - leftMatrix.numCol]);
                    }
                }
            }

            return retMatrix;
        }

        public static bool? ApproxEqual(Matrix leftMatrix, Matrix rightMatrix, decimal accuracy = 0.05M)
        {
            if (leftMatrix.numRow != rightMatrix.numRow || leftMatrix.numCol != rightMatrix.numCol)
            {
                return null;
            }

            decimal upperLimit = 1 + accuracy;
            decimal lowerLimit = 1 / upperLimit;
            var leftContent = leftMatrix.content;
            var rightContent = rightMatrix.content;

            for (int i = 0; i < leftMatrix.numRow; i++)
            {
                for (int j = 0; j < leftMatrix.numCol; j++)
                {
                    if (leftContent[i, j] == rightContent[i, j])
                    {
                        continue;
                    }
                    else if (rightContent[i, j] == decimal.Zero)
                    {
                        return false;
                    }
                    var temp = leftContent[i, j] / rightContent[i, j];
                    if (!(lowerLimit < temp && temp < upperLimit))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static Matrix NormalizeVector(Matrix matrix, decimal norm = 1M)
        {
            var currentNorm = GetEuclideanNormForVector(matrix);
            if (currentNorm == null)
            {
                return null;
            }

            decimal stretchFactor = norm / (decimal)currentNorm;
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

        public static decimal? GetEuclideanNormForVector(Matrix matrix)
        {
            if (!(matrix.numRow == 1 || matrix.numCol == 1))
            {
                return null;
            }

            decimal euclidianNorm = 0;
            var content = matrix.content;
            for (int i = 0; i < matrix.numRow; i++)
            {
                for (int j = 0; j < matrix.numCol; j++)
                {
                    euclidianNorm += DecimalEx.Pow(content[i, j], 2);
                }
            }

            return DecimalEx.Sqrt(euclidianNorm);
        }
        public static Matrix GetRandomlyJitteredVector(Matrix matrix, Random rand)
        {
            if (!(matrix.numRow == 1 || matrix.numCol == 1))
            {
                return null;
            }

            var retMatrix = new Matrix(matrix.numRow, matrix.numCol);
            var content = matrix.content;
            for (int i = 0; i < matrix.numRow; i++)
            {
                for (int j = 0; j < matrix.numCol; j++)
                {
                    retMatrix.SetValue(i, j, content[i, j] * (decimal)(0.95 + rand.NextDouble() / 10));
                }
            }

            return retMatrix;
        }
        public static Matrix GetRandomlyJitteredVectorFarFromZero(Matrix matrix, Random rand)
        {
            if (!(matrix.numRow == 1 || matrix.numCol == 1))
            {
                return null;
            }

            var retMatrix = new Matrix(matrix.numRow, matrix.numCol);
            var content = matrix.content;
            for (int i = 0; i < matrix.numRow; i++)
            {
                for (int j = 0; j < matrix.numCol; j++)
                {
                    var temp = rand.Next(4_000, 10_000);
                    var jitterFactor = temp % 2 == 0 ? 1 + temp / 200_000.0M : 1 - temp / 200_000.0M;
                    retMatrix.SetValue(i, j, content[i, j] * jitterFactor);
                }
            }

            return retMatrix;
        }
        public static decimal[] FlattenVector (Matrix matrix)
        {
            if (!(matrix.numRow == 1 || matrix.numCol == 1))
            {
                return null;
            }

            var content = matrix.content;
            decimal[] retVal = new decimal[matrix.numRow + matrix.numCol - 1];
            for (int i = 0; i < matrix.numRow; i++)
            {
                for (int j = 0; j < matrix.numCol; j++)
                {
                    retVal[i + j] = content[i, j];
                }
            }

            return retVal;
        }
        public static StringBuilder PrintContent(Matrix matrix)
        {
            var retVal = new StringBuilder();
            var content = matrix.content;
            for (int i = 0; i < matrix.numRow; i++)
            {
                for (int j = 0; j < matrix.numCol; j++)
                {
                     retVal.Append(content[i, j] + ", ");
                }
                retVal.AppendLine();
            }
            return retVal;
        }
        public static (Matrix parameters, decimal? intercept) CalculateLinearRegression(List<Matrix> points, bool hasIntercept)
        {
            //--- Start: Working Example ---
            //double[] parameters = new[] { 2.37, -3.8, -0.22, 7.19 };
            //double[] x1 = new[] { 0.312928943462965, -1.16839616233405, 0.689276486707173, -1.53368475681262 };
            //double[] x2 = new[] { -2.31558794808863, -1.496227468087, 1.75210536235624, -1.9902897333437 };
            //double[] x3 = new[] { -1.70107862279079, -1.04064266557997, 2.51017395821844, -1.62947294479157 };
            //double[] x4 = new[] { 2.06266069488705, -1.15373838976752, -1.60689690501272, 0.922334820817665 };
            //double[] x5 = new[] { -0.243180393240999, -1.22898418193215, 2.4079249671106, 1.5112760469485 };
            //double[] y = new[] { -5.9972872156817, -14.4979254206991, -12.3452629506697, 16.2578164087807, 14.4301336441564 };

            //double[] p = Fit.MultiDim(
            //    new[] { x1, x2, x3, x4, x5 },
            //    y,
            //    intercept: true);
            //--- End: Working Example ---

            if (points.Count < 2)
            {
                return (null, null);
            }

            double[][] xArr = new double[points.Count][];
            double[] yArr = new double[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                var flat = FlattenVector(points[i]).Select(x => (double)x).ToArray();
                xArr[i] = flat[..^1];
                yArr[i] = flat[^1];
            }

            var temp = Fit.MultiDim(xArr, yArr, intercept: hasIntercept).Select(x => (decimal)x).ToArray(); ;
            var parameters = new Matrix(temp.Length - (hasIntercept ? 1 : 0), 1);
            for (int i = 0; i < temp.Length - (hasIntercept ? 1 : 0); i++)
            {
                parameters.SetValue(i, 0, temp[i + (hasIntercept ? 1 : 0)]);
            }
            return (parameters, (hasIntercept ? temp[0] : 0));
        }


    }

    

}
