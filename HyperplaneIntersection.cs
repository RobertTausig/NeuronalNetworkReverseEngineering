using LinAlg = MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace NeuronalNetworkReverseEngineering
{
    class HyperplaneIntersection
    {
        public static LinAlg.Vector<double> FindIntersection(Hyperplane h1, Hyperplane h2)
        {
            var p1 = MathConvert.MatrixToVector(h1.planeIdentity.Parameters);
            var p2 = MathConvert.MatrixToVector(h2.planeIdentity.Parameters);
            var i1 = h1.planeIdentity.Intercept ?? 0;
            var i2 = h2.planeIdentity.Intercept ?? 0;

            var paramMatrix = LinAlg.Matrix<double>.Build.DenseOfRowVectors(p1, p2);
            var interceptVector = LinAlg.Vector<double>.Build.Dense(new double[] { i1, i2 });
            var x = UnderdeterminedSystemOfLinearEquations(paramMatrix, interceptVector);
            //Debug:
            var aa = paramMatrix * x;

            return x;
        }

        /// <summary>
        /// Solving the system Ax=b when A is underdetermined (More columns than rows).
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        private static LinAlg.Vector<double> UnderdeterminedSystemOfLinearEquations(LinAlg.Matrix<double> A, LinAlg.Vector<double> b)
        {
            var svd = A.Svd();
            var U = svd.U;
            var S = svd.S;
            var VT = svd.VT;

            var SInverse = DenseMatrix.OfDiagonalVector(
                Vector.Build.DenseOfArray(
                    S.Select(s => s != 0 ? 1 / s : 0).ToArray()));
            var AInverse = VT.Transpose().SubMatrix(0, A.ColumnCount, 0, A.RowCount) * SInverse * U.Transpose().SubMatrix(0, A.RowCount, 0, A.RowCount);

            var x = AInverse * b;
            return x;
        }

        //Just an attempt:
        private static LinAlg.Vector<double> MinimiseDistanceToIntersection(LinAlg.Matrix<double> A, LinAlg.Vector<double> b, LinAlg.Vector<double> p_given)
        {
            var svd = A.Svd();
            var U = svd.U;
            var S = svd.S;
            var VT = svd.VT;

            var SInverse = DenseMatrix.OfDiagonalVector(
                Vector.Build.DenseOfArray(
                    S.Select(s => s != 0 ? 1 / s : 0).ToArray()));
            var AInverse = VT.Transpose().SubMatrix(0, A.ColumnCount, 0, A.RowCount) * SInverse * U.Transpose().SubMatrix(0, A.RowCount, 0, A.RowCount);

            var xMinDistance = AInverse * (b + A * p_given);
            return xMinDistance;
        }


    }
}
