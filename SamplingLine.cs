using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronalNetworkReverseEngineering
{
    class SamplingLine
    {
        public SamplingLine(Model model)
        {
            this.model = model;
        }

        private Model model { get; }

        private const double stdRadius = 60;
        private const double stdlineLength = 3_000;
        private const double stdMinPointDistance = 0.25;

        public List<Matrix> RandomSecantLine(double radius = stdRadius, double lineLength = stdlineLength, double minPointDistance = stdMinPointDistance)
        {
            var retVal = new List<Matrix>();

            var firstVector = new Matrix(1, model.topology.First());
            firstVector.PopulateAllRandomly(model.RandomGenerator);
            firstVector = Matrix.NormalizeVector(firstVector, radius);

            var secondVector = new Matrix(1, model.topology.First());
            secondVector.PopulateAllRandomly(model.RandomGenerator);
            secondVector = Matrix.NormalizeVector(secondVector, radius);

            var directionVector = Matrix.Substraction(secondVector, firstVector);
            var midPoint = Matrix.Addition(firstVector, Matrix.Multiplication(directionVector, 0.5));

            directionVector = Matrix.NormalizeVector(directionVector, minPointDistance);
            for (int i = -(int)(lineLength / minPointDistance) / 2; i < (int)(lineLength / minPointDistance) / 2; i++)
            {
                retVal.Add(Matrix.Addition(midPoint, Matrix.Multiplication(directionVector, i)));
            }

            return retVal;
        }

        public List<Matrix> LinearRegionChanges (List<Matrix> inputs)
        {
            var retVal = new List<Matrix>();
            bool fastMode = false;
            const int fastSteps = 10;
            int fastTrigger = fastSteps;

            var outputDiff = Matrix.Substraction(model.Use(inputs[1]), model.Use(inputs[0]));
            for (int i = 2; i < inputs.Count; i++)
            {
                if (fastMode && (i + fastSteps + 1 < inputs.Count))
                {
                    var temp = Matrix.Substraction(model.Use(inputs[i + fastSteps]), model.Use(inputs[i + fastSteps - 1]));
                    if(Matrix.ApproxEqual(temp, outputDiff) == true)
                    {
                        i += fastSteps + 1;
                    }
                    else
                    {
                        fastTrigger = fastSteps;
                        fastMode = false;
                    }
                }
                var newOutPutDiff = Matrix.Substraction(model.Use(inputs[i]), model.Use(inputs[i - 1]));
                switch(Matrix.ApproxEqual(newOutPutDiff, outputDiff))
                {
                    case null:
                        throw new Exception();
                    case true:
                        fastTrigger--;
                        if (fastTrigger < 1)
                        {
                            fastMode = true;
                        }
                        continue;
                    case false:
                        retVal.Add(inputs[i]);
                        outputDiff = Matrix.Substraction(model.Use(inputs[i + 1]), model.Use(inputs[i]));
                        //Console.WriteLine(i);
                        i++;
                        fastTrigger = fastSteps;
                        fastMode = false;
                        break;
                }
            }

            return retVal;
        }

    }
}
