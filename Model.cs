using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using analyzeResult = (bool allFound, double recoveryRatio, double medianAccuracy, double meanAccuracy, double stdDeviationAccuracy);

namespace NeuronalNetworkReverseEngineering
{
    public class Model
    {
        public Model(int[] topology, bool hasBias)
        {
            if (topology.Length < 3)
            {
                return;
            }

            RandomGenerator = new Random(0);
            for (int i = 0; i < topology.Length - 1; i++)
            {
                var tempMat = new Matrix(topology[i], topology[i + 1]);
                tempMat.PopulateAllRandomly(RandomGenerator);
                weigthMatrices.Add(tempMat);

                var tempVec = new Matrix(1, topology[i + 1]);
                if (hasBias)
                {
                    tempVec.PopulateAllRandomly(RandomGenerator);
                }
                else
                {
                    tempVec.Zeros();
                }
                biasVectors.Add(tempVec);
            }
            this.topology = topology;
        }
        private Model(Model model, int? randomSeed = null)
        {
            this.topology = model.topology;
            this.weigthMatrices = model.weigthMatrices;
            this.biasVectors = model.biasVectors;
            this.RandomGenerator = randomSeed == null ? model.RandomGenerator : new Random((int)randomSeed);
        }

        private List<Matrix> weigthMatrices = new List<Matrix>();
        private List<Matrix> biasVectors = new List<Matrix>();
        public int[] topology { get; }
        public List<Matrix> neuronValues { get; } = new List<Matrix>();
        public Random RandomGenerator { get; }



        public Matrix Use(Matrix input)
        {
            if(input.numRow != 1 || input.numCol != this.topology.First())
            {
                return null;
            }
            neuronValues.Clear();

            neuronValues.Add(input);
            for (int i = 0; i < topology.Length - 1; i++)
            {
                var temp = Matrix.Addition(Matrix.Multiplication(neuronValues[i], weigthMatrices[i]), biasVectors[i]);
                if (i != topology.Length - 2)
                {
                    temp.ReLuOnSelf();
                }
                neuronValues.Add(temp);
            }

            return neuronValues.Last();
        }

        public Model Copy(int? randomSeed = null)
        {
            return new Model(this, randomSeed);
        }

        public analyzeResult AnalyzeFirstLayerResults(List<Hyperplane> firstLayerPlanes)
        {
            analyzeResult retVal;
            int firstLayerDim = topology[1];
            var weightMatrix = weigthMatrices[0];
            var weights = weightMatrix.content;
            var reducedWeightVectors = new List<double[]>();
            for (int j = 0; j < weightMatrix.numCol; j++)
            {
                var tempVector = new double[weightMatrix.numRow - 1];
                for (int i = 0; i < weightMatrix.numRow - 1; i++)
                {
                    tempVector[i] = weights[i, j];
                }
                reducedWeightVectors.Add(tempVector);
            }

            var accuracies = new List<double?>();
            foreach (var plane in firstLayerPlanes)
            {
                accuracies.Add(ReverseEngineeredAccuracy(reducedWeightVectors, plane.planeIdentity));
            }

            var notNullCount = accuracies.Count(x => x != null);

            retVal.allFound = (notNullCount == accuracies.Count) && (firstLayerPlanes.Count == firstLayerDim);
            retVal.recoveryRatio = (double)notNullCount / firstLayerDim;
            retVal.medianAccuracy = accuracies.Median();
            retVal.meanAccuracy = accuracies.Mean();
            retVal.stdDeviationAccuracy = accuracies.StandardDeviation();

            return retVal;
        }

        /// <summary>
        ///  Only for verification purposes. Checks how closely the weights and bias were calculated.
        /// </summary>
        /// <param name="layerNum">Zero-indexed number of layer to check the identity against</param>
        /// <returns>Accuracy as factor. Null, when far off.</returns>
        private double? ReverseEngineeredAccuracy(List<double[]> weightVectors, HyperplaneIdentity identity)
        {
            double similarityTreshold = 0.999;
            var paramVector = Matrix.FlattenVector(identity.Parameters);

            var ratios = new List<double>();
            foreach (var vec in weightVectors)
            {
                var aa = Hyperplane.NormalVectorCosineSimilarity(vec, paramVector);
                if (Math.Abs(aa) > similarityTreshold)
                {
                    ratios = vec.Zip(paramVector, (x, y) =>
                    {
                        return x / y;
                    }).ToList();
                    break;
                }
            }

            if (ratios.Count == 0)
            {
                return null;
            }
            else
            {
                return ratios.MaximumAbsolute() / ratios.MinimumAbsolute();
            }
        }



    }
}
