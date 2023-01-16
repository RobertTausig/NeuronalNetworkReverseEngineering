﻿using MathNet.Numerics;
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


            var model = new Model(new int[4] { inputDim, firstLayerDim, secondLayerDim, outputDim });
            var sampler = new SamplingLine(model);
            var sphere = new SamplingSphere(model);
            var layer = new LayerCalculation(model, sphere);

            int numLines = inputDim * 5;
            var linesThroughSpace = layer.DriveLinesThroughSpace(numLines: numLines, minSpacedApartDistance: 100);

            var hyperPlanes = new List<Hyperplane>();
            foreach (var l in linesThroughSpace.First())
            {
                hyperPlanes.Add(new Hyperplane(model, l.boundaryPoint, l.safeDistance / 15, hasIntercept: false));
            }

            var firstLayerPlanes = new List<Hyperplane>();
            var bb = new List<Matrix>();
            foreach (var plane in hyperPlanes)
            {
                var temp = new List<Matrix>();
                foreach (var line in linesThroughSpace)
                {
                    foreach (var point in line)
                    {
                        if((bool)plane.IsPointOnPlane(point.boundaryPoint, 0.1))
                        {
                            temp.Add(point.boundaryPoint);
                            break;
                        }
                    }
                }

                Console.WriteLine(temp.Count());
                if (temp.Count() > numLines * 0.8)
                {
                    firstLayerPlanes.Add(plane);
                }
                else if (temp.Count() > numLines * 0.5)
                {
                    bb = temp;
                }
            }

            var cc = new Hyperplane(model, bb, hasIntercept: true);
            var tempo = new List<Matrix>();
            foreach (var line in linesThroughSpace)
            {
                foreach (var point in line)
                {
                    if ((bool)cc.IsPointOnPlane(point.boundaryPoint, 0.1))
                    {
                        tempo.Add(point.boundaryPoint);
                        break;
                    }
                }
            }



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
