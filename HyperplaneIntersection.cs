using LinAlg = MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Concurrent;

namespace NeuronalNetworkReverseEngineering
{
    class HyperplaneIntersection
    {
        public static (int intersectIndex, Matrix intersectPoint) FindClosestIntersection(Hyperplane currentPlane, Matrix pointOnCurrentPlane, List<Hyperplane> intersectPlanes)
        {
            if (true != currentPlane.IsPointOnPlane(pointOnCurrentPlane))
            {
                return (-1, null);
            }

            var conc = new ConcurrentDictionary<int, Matrix>();
            var result = Parallel.For(0, intersectPlanes.Count, index =>
            {
                conc.TryAdd(index, FindClosestIntersection(currentPlane, intersectPlanes[index], pointOnCurrentPlane));
            });

            if (result.IsCompleted)
            {
                var closestIntersection = conc.MinBy(x => Matrix.GetEuclideanNormForVector(Matrix.Substraction(x.Value, pointOnCurrentPlane)));
                if (currentPlane.IsPointOnPlane(closestIntersection.Value) != true || intersectPlanes[closestIntersection.Key].IsPointOnPlane(closestIntersection.Value) != true)
                {
                    throw new Exception("The calculated point doesn't lie on the expected intersection.");
                }
                return (closestIntersection.Key, closestIntersection.Value);
            }
            else
            {
                throw new Exception("AI17");
            }
        }
        public static Matrix FindClosestIntersection(Hyperplane h1, Hyperplane h2, Matrix closestPointToIntersection)
        {
            var params1 = MathConvert.MatrixToVector(h1.planeIdentity.Parameters);
            var params2 = MathConvert.MatrixToVector(h2.planeIdentity.Parameters);
            var closestPoint = MathConvert.MatrixToVector(closestPointToIntersection);
            var intercept1 = h1.planeIdentity.Intercept ?? 0;
            var intercept2 = h2.planeIdentity.Intercept ?? 0;
            // Add "-1" to left-hand-side of equation system to also solve for last coordinate (That otherwise is implicitly a result of the constraints of the hyperplane).
            var extendedParams1 = LinAlg.Vector<double>.Build.DenseOfArray(params1.ToArray().Concat(new double[] { -1 }).ToArray());
            var extendedParams2= LinAlg.Vector<double>.Build.DenseOfArray(params2.ToArray().Concat(new double[] { -1 }).ToArray());

            var paramMatrix = LinAlg.Matrix<double>.Build.DenseOfRowVectors(extendedParams1, extendedParams2);
            var interceptVector = LinAlg.Vector<double>.Build.Dense(new double[] { intercept1, intercept2 });
            var x = MinimiseDistanceToIntersection(paramMatrix, interceptVector, closestPoint);

            return MathConvert.VectorToMatrix(closestPoint - x);
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

        private static LinAlg.Vector<double> MinimiseDistanceToIntersection(LinAlg.Matrix<double> A, LinAlg.Vector<double> b, LinAlg.Vector<double> constrainingClosestPoint)
        {
            var svd = A.Svd();
            var U = svd.U;
            var S = svd.S;
            var VT = svd.VT;

            var SInverse = DenseMatrix.OfDiagonalVector(
                Vector.Build.DenseOfArray(
                    S.Select(s => s != 0 ? 1 / s : 0).ToArray()));
            var AInverse = VT.Transpose().SubMatrix(0, A.ColumnCount, 0, A.RowCount) * SInverse * U.Transpose().SubMatrix(0, A.RowCount, 0, A.RowCount);

            var xMinDistance = AInverse * (b + A * constrainingClosestPoint);
            return xMinDistance;
        }


    }
}
