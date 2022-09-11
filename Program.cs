using System;

namespace NeuronalNetworkReverseEngineering
{
    class Program
    {
        static void Main(string[] args)
        {
            //var Mat1 = new Matrix(5, 6);
            //var Mat2 = new Matrix(5, 6);
            ////Mat1.PopulateAllRandomly(200);
            ////Mat2.PopulateAllRandomly(450);
            //for (int i = 0; i < 5; i++)
            //{
            //    for (int j = 0; j < 6; j++)
            //    {
            //        Mat1.SetValue(i, j, i * j - 3 + 2 * i);
            //        Mat2.SetValue(i, j, i * j + 2 - 3 * j);
            //    }
            //}

            //var Mat4 = Matrix.Addition(Mat1, Mat2);

            var model = new Model(new int[4] { 7, 5, 6, 4 });
            for (int i = 0; i < 20_000; i++)
            {
                var input = new Matrix(1, 7);
                input.PopulateAllRandomly(600+i);
                model.Use(input);
            }


            Console.WriteLine("Hello World!");
        }
    }
}
