using System;

namespace NeuronalNetworkReverseEngineering
{
    class Program
    {
        static void Main(string[] args)
        {
            var Mat1 = new Matrix(1, 7);
            var Mat2 = new Matrix(7, 5);
            Mat1.PopulateAllRandomly(200);
            Mat2.PopulateAllRandomly(450);
            //for (int i = 0; i < 2; i++)
            //{
            //    for (int j = 0; j < 3; j++)
            //    {
            //        Mat1.SetValue(i, j, i * j - 3 + 2 * i);
            //        Mat2.SetValue(j, i, i * j + 2 - 3 * j);
            //    }
            //}

            var Mat3 = Matrix.Multiplication(Mat1, Mat2);

            var model = new Model(new int[4] { 7, 5, 6, 4 });


            Console.WriteLine("Hello World!");
        }
    }
}
