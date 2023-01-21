using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeuronalNetworkReverseEngineering
{
    class Program
    {
        const int inputDim = 7;
        const int firstLayerDim = 13;
        const int secondLayerDim = 14;
        const int outputDim = 4;

        static void Main(string[] args)
        {
            var clock = new Stopwatch();
            clock.Start();


            var model = new Model(new int[4] { inputDim, firstLayerDim, secondLayerDim, outputDim });
            var sampler = new SamplingLine(model);
            var sphere = new SamplingSphere(model);
            var layer = new LayerCalculation(model, sphere);

            int numLines = 5;
            var linesThroughSpace = layer.DriveLinesThroughSpace(numLines: numLines, minSpacedApartDistance: 100);

            int numfirstLayerPlanes = -1;
            var tt = new List<List<Hyperplane>>();
            for (int i = 0; i < linesThroughSpace.Count; i++)
            {
                var tempModel = model.Copy(model.RandomGenerator.Next());

                var hyperPlanes = new List<Hyperplane>();
                foreach (var l in linesThroughSpace[i])
                {
                    hyperPlanes.Add(new Hyperplane(tempModel, l.boundaryPoint, l.safeDistance / 15, hasIntercept: false));
                }

                tt.Add(layer.Old_GetFirstLayer(hyperPlanes, 1_000));
            }

            var gg = new List<Hyperplane>();
            gg.AddRange(tt[0]);
            for (int i = 0; i < tt.Count; i++)
            {
                for (int j = 0; j < tt[i].Count; j++)
                {
                    var temp = tt[i][j];
                    var booli = gg.Any(x => true == Matrix.ApproxEqual(temp.planeIdentity.parameters, x.planeIdentity.parameters, 0.3));
                    if (!booli)
                    {
                        gg.Add(temp);
                    }
                }
            } 

            Console.WriteLine("-------------------");
            Console.WriteLine($@"Highest: {gg.Count}");
             


            //var firstLayerPlanes = layer.GetFirstLayer(hyperPlanes, constRadius);



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
