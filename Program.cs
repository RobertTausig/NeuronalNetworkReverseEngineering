using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LinAlg = MathNet.Numerics.LinearAlgebra;

namespace NeuronalNetworkReverseEngineering
{
    class Program
    {
        const int inputDim = 7;
        const int firstLayerDim = 5;
        const int secondLayerDim = 6;
        const int outputDim = 4;

        static void Main(string[] args)
        {
            var clock = new Stopwatch();
            clock.Start();


            var model = new Model(new int[4] { inputDim, firstLayerDim, secondLayerDim, outputDim }, hasBias: false);
            //var ransac = new RansacAlgorithm(model);
            //var isTestSuccess = ransac.Test();
            var sampler = new SamplingLine(model);
            var sphere = new SamplingSphere(model);
            var layer = new LayerCalculation(model, sphere);

            int geometricArithmeticMean = (int)((Math.Sqrt(firstLayerDim * secondLayerDim) + (firstLayerDim + secondLayerDim) / 2) / 2);
            int numLines = geometricArithmeticMean + 5;
            var initialBundle = layer.DriveLinesThroughSpace(numLines: numLines);
            // For debugging:
            //var averageLinePoints = initialBundle.SpaceLines.Average(x => x.SpaceLinePoints.Count);
            var initialHyperplanesColl = layer.SpaceLinesToHyperplanes(initialBundle);
            var initialDistinctHyperplanes = layer.DistinctHyperplanes(initialHyperplanesColl);
            var firstLayerPlanes = layer.GetFirstLayer(initialDistinctHyperplanes, 5_000);

            var analyzeResult = model.AnalyzeFirstLayerResults(firstLayerPlanes);

            //var outermostSecondLayerPlanes = initialDistinctHyperplanes.Except(firstLayerPlanes).ToList();
            //var bender = new HyperplaneBending(model);
            //var bb = bender.MoveAlongBend(outermostSecondLayerPlanes[0], firstLayerPlanes);

            clock.Stop();
            Console.WriteLine("Time passed: " + clock.Elapsed.TotalMilliseconds);
            Console.ReadLine();
        }



        

    }
}
