using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NeuronalNetworkReverseEngineering
{
    class Program
    {
        const double constRadius = 100;
        const double constMinDistance = 10;

        static void Main(string[] args)
        {
            var clock = new Stopwatch();
            clock.Start();


            var model = new Model(new int[4] { 7, 5, 6, 4 });
            var sampler = new SamplingLine(model);

            var linesThroughSpace = new List<List<Matrix>>();
            // Why doing this:
            // To make sure to not accidentally get a "bad" line.
            while (linesThroughSpace.Count < 10)
            {
                var (midPoint, directionVector) = sampler.RandomSecantLine(radius: constRadius);
                var firstBoundaryPointsSuggestion = sampler.BidirectionalLinearRegionChanges(midPoint, directionVector);
                var secondBoundaryPointsSuggestion = sampler.BidirectionalLinearRegionChanges(Matrix.Multiplication(midPoint, 1.01), directionVector);
                if (firstBoundaryPointsSuggestion.Count == secondBoundaryPointsSuggestion.Count && IsSpacedApart(firstBoundaryPointsSuggestion, constMinDistance))
                {
                    linesThroughSpace.Add(firstBoundaryPointsSuggestion);
                }
            }

            var hyperPlaneCollection = SampleLinePointsToHyperplanes(linesThroughSpace, model);
            


            clock.Stop();
            Console.WriteLine("Time passed: " + clock.Elapsed.TotalMilliseconds);
            Console.ReadLine();
        }



        public static bool IsSpacedApart(List<Matrix> list, double minDistance)
        {
            for (int i = 1; i < list.Count; i++)
            {
                var norm = Matrix.GetEuclideanNormForVector(Matrix.Substraction(list[i], list[i - 1]));
                if (norm == null || norm < minDistance)
                {
                    return false;
                }
            }

            return true;
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
