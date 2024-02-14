using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LinAlg = MathNet.Numerics.LinearAlgebra;

namespace NeuronalNetworkReverseEngineering
{
    class Program
    {
        static void Main(string[] args)
        {
            int inputDim = 7;
            int firstLayerDim = 5;
            int secondLayerDim = 6;
            int outputDim = 4;

#if !DEBUG
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: Program <inputDim> <firstLayerDim> <secondLayerDim> <outputDim>");
                return;
            }
            inputDim = int.Parse(args[0]);
            firstLayerDim = int.Parse(args[1]);
            secondLayerDim = int.Parse(args[2]);
            outputDim = int.Parse(args[3]);
#endif
            var clock = new Stopwatch();
            clock.Start();

            var model = new Model([inputDim, firstLayerDim, secondLayerDim, outputDim], hasBias: false);
            var sampler = new SamplingLine(model);
            var sphere = new SamplingSphere(model);
            var layer = new LayerCalculation(model, sphere);

            int geometricArithmeticMean = (int)((Math.Sqrt(firstLayerDim * secondLayerDim) + (firstLayerDim + secondLayerDim) / 2) / 2);
            int numLines = geometricArithmeticMean + 5;
            var initialBundle = layer.DriveLinesThroughSpace(numLines: numLines);
            var initialHyperplanesColl = layer.SpaceLinesToHyperplanes(initialBundle);
            var initialDistinctHyperplanes = layer.DistinctHyperplanes(initialHyperplanesColl);
            var firstLayerPlanes = layer.GetFirstLayer(initialDistinctHyperplanes);

            var analyzeResult = model.AnalyzeFirstLayerResults(firstLayerPlanes);

            //var outermostSecondLayerPlanes = initialDistinctHyperplanes.Except(firstLayerPlanes).ToList();
            //var bender = new HyperplaneBending(model);
            //var bb = bender.MoveAlongBend(outermostSecondLayerPlanes[0], firstLayerPlanes);

            clock.Stop();
            Console.WriteLine("Time passed: " + clock.Elapsed.TotalSeconds);

            var nowTime = System.DateTime.Now;
            string fileName = $"{nowTime.ToString("yyMMdd")}_{nowTime.ToString("HHmmss")}_NNRE-Result_{inputDim}-{firstLayerDim}-{secondLayerDim}-{outputDim}.txt";
            var logger = new VariablesLogger(fileName);
            Dictionary<string, string> output = new Dictionary<string, string>
            {
                { "Time [seconds]", $"{clock.Elapsed.TotalSeconds}" },
                { nameof(inputDim), $"{inputDim}" },
                { nameof(firstLayerDim), $"{firstLayerDim}" },
                { nameof(secondLayerDim), $"{secondLayerDim}" },
                { nameof(outputDim), $"{outputDim}" },
                { nameof(firstLayerPlanes) + "." + nameof(firstLayerPlanes.Count), $"{firstLayerPlanes.Count}" },
                { nameof(analyzeResult.allFound), $"{analyzeResult.allFound}" },
                { nameof(analyzeResult.recoveryRatio), $"{analyzeResult.recoveryRatio}" },
                { nameof(analyzeResult.medianAccuracy), $"{analyzeResult.medianAccuracy}" },
                { nameof(analyzeResult.meanAccuracy), $"{analyzeResult.meanAccuracy}" },
                { nameof(analyzeResult.stdDeviationAccuracy), $"{analyzeResult.stdDeviationAccuracy}" },
            };
            logger.LogVariables(output);
        }


        

    }
}
