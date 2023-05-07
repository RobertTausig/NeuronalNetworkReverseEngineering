using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
            var sampler = new SamplingLine(model);
            var sphere = new SamplingSphere(model);
            var layer = new LayerCalculation(model, sphere);

            int numLines = 5;
            var initialBundle = layer.DriveLinesThroughSpace(numLines: numLines, minSpacedApartDistance: 100);
            var initialHyperplanesColl = layer.SpaceLinesToHyperplanes(initialBundle);
            var firstLayerPlanes = layer.GetFirstLayer(initialHyperplanesColl, 1_000);
            //firstLayerPlanes = sphere.CorrectIntercepts(firstLayerPlanes, 1_000);

            /*var first = firstLayerPlanes[0];
            var second = firstLayerPlanes[1];
            var coll = new List<Matrix>();
            for (int i = 0; i < 107; i++)
            {
                coll.Add(first.GenerateRandomPointOnPlane(20 + i));
            }
            for (int j = 0; j < 8; j++)
            {
                coll.Add(second.GenerateRandomPointOnPlane(10 + 20 * j));
            }
            var ransac = new RansacAlgorithm(model);
            var result = ransac.Ransac(coll, 18, 200, 1.0 / 200, 0.8);*/


            foreach (var flp in firstLayerPlanes)
            {
                flp.Print();
                var aa = model.ReverseEngineeredAccuracy(0, flp.planeIdentity);
                Console.WriteLine(@$"r.e. accuracy: {aa}");
            }

            clock.Stop();
            Console.WriteLine("Time passed: " + clock.Elapsed.TotalMilliseconds);
            Console.ReadLine();
        }



        

    }
}
