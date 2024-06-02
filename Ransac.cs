using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    public class RansacAlgorithm
    {
        public RansacAlgorithm(Model model)
        {
            this.model = model;
        }

        private Model model { get; }
        private double maxForceConvergenceFactor = 20;
        /// <summary>
        /// Returns an empty List of Inliers if it couldn't converge (i.e. maxIterations were overstepped).
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sampleSize"></param>
        /// <param name="maxIterations"></param>
        /// <param name="maxDeviation"></param>
        /// <param name="ransacInliersForBreakPercentage"></param>
        /// <param name="forceConvergence"></param>
        /// <returns></returns>
        public (double usedMaxDeviation, List<Matrix> Inliers) Ransac(List<Matrix> data, int sampleSize, int maxIterations, double maxDeviation, double ransacInliersForBreakPercentage, bool forceConvergence = false)
        {
            var retVal = new List<Matrix>();
            double scalingFactor = Math.Pow(maxForceConvergenceFactor, 1.0 / maxIterations);

            for (int i = 0; i < maxIterations; i++)
            {
                if(forceConvergence)
                {
                    maxDeviation = maxDeviation * scalingFactor;
                }
                var inliers = new List<Matrix>();
                var sample = Sample(data, sampleSize);
                (var errorThreshold, var hyperplane) = FittingFunction(sample, false, maxDeviation);
                if (errorThreshold > maxDeviation)
                {
                    continue;
                }

                foreach (Matrix item in data)
                {
                    double error = (double)hyperplane.AbsPointDeviationOfPlane(item);
                    if (error < maxDeviation)
                    {
                        inliers.Add(item);
                    }
                }

                if (inliers.Count >= data.Count * ransacInliersForBreakPercentage)
                {
                    retVal = inliers;
                    break;
                }
            }

            return (maxDeviation, retVal);
        }
        public bool Test()
        {
            int spaceDim = 26;
            int parameterDim = spaceDim - 1;
            int numInliers = 1_800;
            int numOutliers = 200;
            bool retVal = false;

            var randomParameters = new Matrix(parameterDim, 1);
            randomParameters.PopulateAllRandomly(this.model.RandomGenerator);
            var planeIdentity = new HyperplaneIdentity();
            planeIdentity.Parameters = randomParameters;
            planeIdentity.Intercept = 0;

            var plane = new Hyperplane(model, planeIdentity, false);
            var inliers = new List<Matrix>();
            var outliers = new List<Matrix>();
            var data = new List<Matrix>();

            for (int i = 0; i < numInliers; i++)
            {
                inliers.Add(plane.GenerateRandomPointOnPlane(200 + i));
            }
            var outlierPoint = new Matrix(1, spaceDim);
            for (int i = 0; i < numOutliers; i++)
            {
                outlierPoint.PopulateAllRandomly(this.model.RandomGenerator);
                outliers.Add(Matrix.Multiplication(outlierPoint, (300 + 2 * i) / Math.Sqrt(spaceDim)));
            }
            data.AddRange(inliers);
            data.AddRange(outliers);

            var result = Ransac(data, 3 * spaceDim, 5_000, 1.0 / 1_000, 2.0 / 3);
            if (result.Inliers.Count == numInliers)
            {
                retVal = !result.Inliers.Any(x => outliers.Contains(x));
            }
            return retVal;
        }

        private List<Matrix> Sample(List<Matrix> data, int sampleSize)
        {
            var sample = new List<Matrix>();
            while (sample.Count < sampleSize)
            {
                int index = model.RandomGenerator.Next(data.Count);
                var item = data[index];
                if (!sample.Contains(item))
                {
                    sample.Add(item);
                }
            }
            return sample;
        }

        private Tuple<double, Hyperplane> FittingFunction(List<Matrix> points, bool hasIntercept, double maxDeviation)
        {
            double retDeviation = -1;
            var plane = new Hyperplane(model, points, hasIntercept);

            foreach (var point in points)
            {
                var currentDeviation = plane.AbsPointDeviationOfPlane(point);
                if (currentDeviation == null)
                {
                    throw new Exception("CE74");
                }

                if (currentDeviation > retDeviation)
                {
                    retDeviation = (double)currentDeviation;
                    if(currentDeviation > maxDeviation)
                    {
                        break;
                    }
                }
            }

            return new Tuple<double, Hyperplane>(retDeviation, plane);
        }

    }

}
