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
            int numLines = geometricArithmeticMean;
            var initialBundle = layer.DriveLinesThroughSpace(numLines: numLines, minSpacedApartDistance: 100);
            var initialHyperplanesColl = layer.SpaceLinesToHyperplanes(initialBundle);
            var initialDistinctHyperplanes = layer.DistinctHyperplanes(initialHyperplanesColl);
            var firstLayerPlanes = layer.GetFirstLayer(initialDistinctHyperplanes, 1_000);

            if (firstLayerPlanes.Count == firstLayerDim)
            {
                Console.WriteLine("All flp found.");
            }
            else
            {
                Console.WriteLine("Not all flp found.");
            }
            foreach (var flp in firstLayerPlanes)
            {
                flp.Print();
                var aa = model.ReverseEngineeredAccuracy(0, flp.planeIdentity);
                Console.WriteLine(@$"r.e. accuracy: {aa}");
            }

            //var outermostSecondLayerPlanes = initialDistinctHyperplanes.Except(firstLayerPlanes).ToList();

            //var bender = new HyperplaneBending(model);
            //var bb = bender.MoveAlongBend(outermostSecondLayerPlanes[0], firstLayerPlanes);


            clock.Stop();
            Console.WriteLine("Time passed: " + clock.Elapsed.TotalMilliseconds);
            Console.ReadLine();
        }



        

    }
}
