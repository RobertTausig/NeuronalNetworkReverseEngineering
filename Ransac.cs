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
                var tempModel = model.Copy(i);
                var inliers = new List<Matrix>();
                var sample = Sample(data, sampleSize);
                (var errorThreshold, var hyperplane) = StdFittingFunction(tempModel, sample, false);
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

        private static List<Matrix> Sample(List<Matrix> data, int sampleSize)
        {
            var sample = new List<Matrix>();
            Random random = new Random();
            while (sample.Count < sampleSize)
            {
                int index = random.Next(data.Count);
                var item = data[index];
                if (!sample.Contains(item))
                {
                    sample.Add(item);
                }
            }
            return sample;
        }

        private Tuple<double, Hyperplane> StdFittingFunction(Model model, List<Matrix> points, bool hasIntercept)
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
