using MathNet.Numerics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    public class Hyperplane
    {
        public Hyperplane(Model model, RansacAlgorithm ransacAlgorithm, Matrix boundaryPoint, double displacementNorm = 1, bool hasIntercept = true)
        {
            int maxMagnitude = 14;
            double directionNorm = 6.0 * displacementNorm / Math.Pow(2, maxMagnitude);

            this.model = model;
            this.ransacAlgorithm = ransacAlgorithm;
            this.originalBoundaryPoint = boundaryPoint;
            this.spaceDim = boundaryPoint.numRow + boundaryPoint.numCol - 1;
            this.ransacSampleSize = spaceDim;
            //Math.Pow(10, -9) is the probability that the algorithm does not result in a successful hyperplane estimation for the assumed outlier percentage:
            this.ransacMaxIterations = (int)(Math.Log(Math.Pow(10, -9)) / Math.Log(1 - Math.Pow((1 - assumedRansacOutlierPercentage), ransacSampleSize)));
            this.pointsOnPlane = SupportPointsOnBoundary(boundaryPoint, 0, displacementNorm, directionNorm, maxMagnitude);
            this.planeIdentity = Matrix.CalculateLinearRegression_PseudoInverse(pointsOnPlane, hasIntercept);
        }
        public Hyperplane(Model model, RansacAlgorithm ransacAlgorithm, List<Matrix> potentialPointsOnPlane, bool hasIntercept = true)
        {
            this.model = model;
            this.ransacAlgorithm = ransacAlgorithm;
            this.spaceDim = potentialPointsOnPlane.First().numRow + potentialPointsOnPlane.First().numCol - 1;
            this.ransacSampleSize = spaceDim;
            //Math.Pow(10, -9) is the probability that the algorithm does not result in a successful hyperplane estimation for the assumed outlier percentage:
            this.ransacMaxIterations = (int)(Math.Log(Math.Pow(10, -9)) / Math.Log(1 - Math.Pow((1 - assumedRansacOutlierPercentage), ransacSampleSize)));
            (this.ransacMaxDeviation, this.pointsOnPlane) = this.ransacAlgorithm.Ransac(potentialPointsOnPlane, ransacSampleSize, ransacMaxIterations, ransacMaxDeviation, ransacInliersForBreakPercentage, true);
            this.planeIdentity = Matrix.CalculateLinearRegression_PseudoInverse(this.pointsOnPlane, hasIntercept);
        }
        public Hyperplane(Model model, List<Matrix> pointsOnPlane, bool hasIntercept = true)
        {
            this.model = model;
            this.spaceDim = pointsOnPlane.First().numRow + pointsOnPlane.First().numCol - 1;
            this.pointsOnPlane = pointsOnPlane;
            this.planeIdentity = Matrix.CalculateLinearRegression_PseudoInverse(pointsOnPlane, hasIntercept);
        }
        public Hyperplane(Model model, HyperplaneIdentity planeIdentity, bool hasIntercept = true)
        {
            this.model = model;
            this.spaceDim = planeIdentity.Parameters.numRow + planeIdentity.Parameters.numCol;
            this.planeIdentity = planeIdentity;
        }


        private Model model { get; }
        public List<Matrix> pointsOnPlane { get; } = new List<Matrix>();
        private List<Matrix> temporaryPointsOnPlane = new List<Matrix>();
        public HyperplaneIdentity planeIdentity { get; }
        public int spaceDim { get; }
        public Matrix originalBoundaryPoint { get; }

        private const int saltIncreasePerRecursion = 1_000;
        private const int maxSalt = 5 * saltIncreasePerRecursion;

        private RansacAlgorithm ransacAlgorithm { get; }
        private int ransacSampleSize { get; }
        private int ransacMaxIterations { get; }
        private double ransacMaxDeviation = 1.0 / 6_000;
        private const double ransacInliersForBreakPercentage = 0.55;
        private const double assumedRansacOutlierPercentage = 0.16;

        private List<Matrix> SupportPointsOnBoundary(Matrix boundaryPoint, int salt, double displacementNorm, double directionNorm, int maxMagnitude)
        {
            if (!(boundaryPoint.numRow == 1 || boundaryPoint.numCol == 1))
            {
                return null;
            }
            if (salt >= maxSalt)
            {
                throw new Exception("KR09");
            }

            var supportPoints = new List<Matrix>();
            var bag = new ConcurrentBag<List<Matrix>>();
            int iterations = (int)(2.1 * spaceDim + 6);
            var result = Parallel.For(0, iterations, index =>
            {
                var tempModel = model.Copy(index + salt);
                var tempSampler = new SamplingLine(tempModel);

                var displacementVector = new Matrix(boundaryPoint.numRow, boundaryPoint.numCol);
                displacementVector.PopulateAllRandomlyFarFromZero(tempModel.RandomGenerator);
                displacementVector = Matrix.NormalizeVector(displacementVector, displacementNorm);

                var directionVector = new Matrix(boundaryPoint.numRow, boundaryPoint.numCol);
                directionVector.PopulateAllRandomlyFarFromZero(tempModel.RandomGenerator);
                directionVector = Matrix.NormalizeVector(directionVector, directionNorm);

                var startPoint = Matrix.Addition(boundaryPoint, displacementVector);
                var samplePoints = tempSampler.BidirectionalLinearRegionChanges(startPoint, directionVector, maxMagnitude);
                bag.Add(samplePoints);
            });

            if (result.IsCompleted)
            {
                var overshootSample = bag.Where(x => x.Count > 1);
                var fineSample = bag.Where(x => x.Count == 1);
                var undershootSample = bag.Where(x => x.Count < 1);

                if (undershootSample.Count() > iterations * 0.9)
                {
                    // Unsalvageable Hyperplane. It is unclear why this happens in rare cases (Presumably a "cursed" boundaryPoint that lies close to hyperplane intersections), but if it does, no useful result can be gained.
                    return null;
                }
                else if (fineSample.Count() + temporaryPointsOnPlane.Count() >= 2 * spaceDim)
                {
                    foreach (var item in fineSample)
                    {
                        supportPoints.AddRange(item);
                    }
                    supportPoints.AddRange(temporaryPointsOnPlane);
                    temporaryPointsOnPlane.Clear();
                    var ransacResult =  ransacAlgorithm.Ransac(supportPoints, ransacSampleSize, ransacMaxIterations, ransacMaxDeviation, ransacInliersForBreakPercentage, true);
                    this.ransacMaxDeviation = ransacResult.usedMaxDeviation;
                    return ransacResult.Inliers;
                }
                else
                {
                    foreach (var item in fineSample)
                    {
                        temporaryPointsOnPlane.AddRange(item);
                    }

                    if (overshootSample.Count() > undershootSample.Count())
                    {
                        salt += saltIncreasePerRecursion;
                        double normCorrectionFactor = (double)overshootSample.Count() / iterations + 1;
                        return SupportPointsOnBoundary(boundaryPoint, salt, displacementNorm / normCorrectionFactor, directionNorm / normCorrectionFactor, maxMagnitude);
                    }
                    else
                    {
                        salt += saltIncreasePerRecursion;
                        double normCorrectionFactor = (double)undershootSample.Count() / iterations + 1;
                        return SupportPointsOnBoundary(boundaryPoint, salt, displacementNorm * normCorrectionFactor, directionNorm * normCorrectionFactor, maxMagnitude);
                    }
                }

            }
            else
            {
                throw new Exception("FA80");
            }

        }

        public Matrix GenerateRandomPointOnPlane(double approxRadius)
        {
            var xCoords = new Matrix(1, spaceDim - 1);
            xCoords.PopulateAllRandomlyFarFromZero(model.RandomGenerator);
            xCoords = Matrix.NormalizeVector(xCoords, approxRadius);
            var yCoord = Matrix.Multiplication(xCoords, planeIdentity.Parameters);
            Matrix intercept = new Matrix(new double[,] { { (double)planeIdentity.Intercept } });

            var tempPoint = Matrix.ConcatHorizontally(xCoords, Matrix.Addition(yCoord, intercept));
            var norm = Matrix.GetEuclideanNormForVector(tempPoint) / approxRadius;
            xCoords = Matrix.Multiplication(xCoords, 1.0 / (double)norm);
            yCoord = Matrix.Multiplication(xCoords, planeIdentity.Parameters);

            return Matrix.ConcatHorizontally(xCoords, Matrix.Addition(yCoord, intercept));
        }
        public Matrix GenerateRandomNormalizedVectorOnPlane()
        {
            var zeroPoint = new Matrix(1, spaceDim - 1);
            zeroPoint.Zeros();
            Matrix intercept = new Matrix(new double[,] { { (double)planeIdentity.Intercept } });
            var zeroPointWithIntercept = Matrix.ConcatHorizontally(zeroPoint, intercept);

            var point = GenerateRandomPointOnPlane(1);
            var vector = Matrix.Substraction(point, zeroPointWithIntercept);
            return Matrix.NormalizeVector(vector);
        }

        //Probability for a random point to return "true" with accuracy 0.1: ~2%
        public bool? IsPointOnPlane (Matrix point, double accuracy = 0.05)
        {
            if (point.numRow != 1)
            {
                return null;
            }

            double upperLimit = 1 + accuracy;
            double lowerLimit = 1 / upperLimit;

            var flattenPoint = Matrix.FlattenVector(point);
            var xCoords = new Matrix(1, spaceDim - 1);
            for (int i = 0; i < flattenPoint.Length - 1; i++)
            {
                xCoords.SetValue(0, i, flattenPoint[i]);
            }
            var yCoord = flattenPoint[^1];

            var yCoordCalculated = Matrix.Multiplication(xCoords, planeIdentity.Parameters);
            var quotient = yCoord / (Matrix.FlattenVector(yCoordCalculated).First() + planeIdentity.Intercept ?? 0);

            return (lowerLimit < quotient) && (upperLimit > quotient);
        }
        /// <summary>
        /// Gives deviation of a point from plane equation.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Positive double (Smaller ist better), or Double.MaxValue when deviation with changing sign</returns>
        public double? AbsPointDeviationOfPlane(Matrix point)
        {
            if (point.numRow != 1)
            {
                return null;
            }

            var flattenPoint = Matrix.FlattenVector(point);
            var xCoords = new Matrix(1, spaceDim - 1);
            for (int i = 0; i < flattenPoint.Length - 1; i++)
            {
                xCoords.SetValue(0, i, flattenPoint[i]);
            }
            var yCoord = flattenPoint[^1];

            var yCoordCalculated = Matrix.Multiplication(xCoords, planeIdentity.Parameters);
            var quotient = yCoord / (Matrix.FlattenVector(yCoordCalculated).First() + planeIdentity.Intercept ?? 0);
            if (quotient < 0)
            {
                return Double.MaxValue;
            }

            return quotient >= 1 ? quotient - 1 : (1 / quotient) - 1;
        }
        public void Print()
        {
            var printStr = new StringBuilder();
            printStr.AppendLine("// Hyperplane //:");
            printStr.AppendLine("parameters:");
            printStr.Append(Matrix.PrintContent(planeIdentity.Parameters));
            printStr.AppendLine("intercept: " + planeIdentity.Intercept.ToString());
            Console.Write(printStr);
        }
        public static double NormalVectorCosineSimilarity(Hyperplane h1, Hyperplane h2)
        {
            var vectorA = Matrix.FlattenVector(h1.planeIdentity.Parameters);
            var vectorB = Matrix.FlattenVector(h2.planeIdentity.Parameters);
            if (vectorA.Length != vectorB.Length)
            {
                throw new ArgumentException("Hyperplanes must be of the same dimension.");
            }

            double dotProduct = 0.0;
            double magnitudeA = 0.0;
            double magnitudeB = 0.0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                magnitudeA += vectorA[i] * vectorA[i];
                magnitudeB += vectorB[i] * vectorB[i];
            }

            if (magnitudeA == 0.0 || magnitudeB == 0.0)
            {
                throw new ArgumentException("One or both of the vectors contain only zeroes.");
            }

            return dotProduct / (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
        }

    }
}
