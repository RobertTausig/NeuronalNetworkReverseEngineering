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

            var gg = new List<Hyperplane>();
            var precisionBundle = layer.DriveLinesThroughSpace(numLines: 3*firstLayerDim, minSpacedApartDistance: 100, enableSafeDistance: false);
            foreach (var plane in firstLayerPlanes)
            {
                var zzz = new List<Matrix>();
                for (int i = 0; i < precisionBundle.SpaceLines.Count; i++)
                {
                    var tempi = precisionBundle.SpaceLines[i].SpaceLinePoints.Where(y => true == plane.IsPointOnPlane(y.BoundaryPoint, accuracy: 0.05));
                    if (tempi.Count() == 1)
                    {
                        zzz.Add(tempi.First().BoundaryPoint);
                    }
                }
                gg.Add(new Hyperplane(model, zzz, hasIntercept: false));


                /*var aa = precisionBundle.SpaceLines.SelectMany(x => x.SpaceLinePoints.Where(y => true == plane.IsPointOnPlane(y.BoundaryPoint, accuracy: 0.02))).Select(z => z.BoundaryPoint).ToList();
                gg.Add(new Hyperplane(model, aa, hasIntercept: false));*/
            }


            foreach (var flp in firstLayerPlanes)
            {
                flp.Print();
                var aa = model.ReverseEngineeredAccuracy(0, flp.planeIdentity);
                Console.WriteLine(@$"r.e. accuracy: {aa}");
            }
            Console.WriteLine("###########################");
            foreach (var flp in gg)
            {
                flp.Print();
                var aa = model.ReverseEngineeredAccuracy(0, flp.planeIdentity);
                Console.WriteLine(@$"r.e. accuracy: {aa}");
            }


            clock.Stop();
            Console.WriteLine("Time passed: " + clock.Elapsed.TotalMilliseconds);
            Console.ReadLine();
        }



        

        public static List<List<Hyperplane>> SampleLinePointsToHyperplanes(List<List<Matrix>> lineCollection, Model model)
        {
            var retVal = new List<List<Hyperplane>>();
            foreach (var lines in lineCollection)
            {
                var temp = new List<Hyperplane>();
                foreach (var point in lines)
                {
                    temp.Add(new Hyperplane(model, point));
                }
                retVal.Add(temp);
            }
            return retVal;
        }

    }
}
