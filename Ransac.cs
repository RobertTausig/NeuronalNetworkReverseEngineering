using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    class RansacAlgorithm
    {
        public RansacAlgorithm(Model model)
        {
            this.model = model;
        }

        private Model model { get; }


        public List<Matrix> Ransac(List<Matrix> data, int sampleSize, int maxIterations, double maxDeviation, double percentageInliersForBreak)
        {
            var retVal = new List<Matrix>();

            for (int i = 0; i < maxIterations; i++)
            {
                var inliers = new List<Matrix>();
                var sample = Sample(data, sampleSize);
                (var errorThreshold, var hyperplane) = StdFittingFunction(sample, false);
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

                if (inliers.Count >= data.Count * percentageInliersForBreak)
                {
                    retVal = inliers;
                    break;
                }
            }

            return retVal;
        }
        public bool Test()
        {
            int spaceDim = 18;
            int parameterDim = spaceDim - 1;
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

            for (int i = 0; i < 1_800; i++)
            {
                inliers.Add(plane.GenerateRandomPointOnPlane(200 + i));
            }
            var outlierPoint = new Matrix(1, spaceDim);
            for (int i = 0; i < 200; i++)
            {
                outlierPoint.PopulateAllRandomly(this.model.RandomGenerator);
                outliers.Add(Matrix.Multiplication(outlierPoint, (300 + 2 * i) / Math.Sqrt(spaceDim)));
            }
            data.AddRange(inliers);
            data.AddRange(outliers);

            var result = Ransac(data, 3 * spaceDim, 5_000, 1.0 / 1_000, 2.0 / 3);
            if (result.Count > 0)
            {
                retVal = !result.Any(x => outliers.Contains(x));
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

        private Tuple<double, Hyperplane> StdFittingFunction(List<Matrix> points, bool hasIntercept)
        {
            double maxDeviation = -1;
            var plane = new Hyperplane(model, points, hasIntercept);

            foreach (var point in points)
            {
                var currentDeviation = plane.AbsPointDeviationOfPlane(point);
                if (currentDeviation == null)
                {
                    throw new Exception("CE74");
                }

                if (currentDeviation > maxDeviation)
                {
                    maxDeviation = (double)currentDeviation;
                }
            }

            return new Tuple<double, Hyperplane>(maxDeviation, plane);
        }

    }

}
