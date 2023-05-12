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
            if (result.Count == numInliers)
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
