using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEGFileFormatLib
{
    class Program
    {
        static void Main(string[] args)
        {
            CalculationFunction();
            //JPEGFile j = new JPEGFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "kiss-smiley-pillow.jpg"));
            //JPEGFile j = new JPEGFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "huff_simple0.jpg"));
            JPEGFile j = new JPEGFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "fig2.jpg"));
            //JPEGFile j = new JPEGFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "test.jpg"));
            //JPEGFile j = new JPEGFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "iron_man.jpg"));
            //JPEGFile j = new JPEGFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "car.jpg"));
        }

        private static void CalculationFunction()
        {
            byte[,] data = new byte[,] {
                { 52, 55, 61, 66, 70, 61, 64, 73 },
                { 63,59,55,90,109,85,69,72 },
                { 62,59,68,113,144,104,66,73 },
                { 63,58,71,122,154,106,70,69 },
                { 67,61,68,104,126,88,68,70 },
                { 79,65,60,70,77,68,58,75 },
                { 85,71,64,59,55,61,65,83 },
                { 87,79,69,68,65,76,78,94 },
            };
            double[,] destination = new double[8, 8];

            for (int i = 0; i < data.Length; i++)
                destination[i % 8, i / 8] = (double)(data[i % 8, i / 8] - 128);

            Print2DArray<double>(destination, "First Transformation");

            destination = ForwardDCT(destination);

            Print2DArray<double>(destination, "DCT Matrix");
            byte[,] quantizationMatrix = new byte[,] {
                { 16,11,10,16,24,40,51,61},
                { 12,12,14,19,26,58,60,55},
                { 14,13,16,24,40,57,69,56},
                { 14,17,22,29,51,87,80,62},
                { 18,22,37,56,68,109,103,77},
                { 24,35,55,64,81,104,113,92},
                { 49,64,78,87,103,121,120,101},
                { 72,92,95,98,112,100,103,99},
            };

            int[,] bMatrix = new int[8, 8];
            for (int i = 0; i < data.Length; i++)
                bMatrix[i % 8, i / 8] = (int)(Math.Round(destination[i % 8, i / 8] / quantizationMatrix[i % 8, i / 8]));

            Print2DArray<int>(bMatrix, "B Matrix");

            bMatrix = ZigZag(bMatrix);

            Print2DArray<int>(bMatrix, "B Matrix Zigzag");

            int[] rle = RunLevelEncoding(bMatrix);
        }

        private static int[] RunLevelEncoding(int[,] bMatrix)
        {
            Console.WriteLine("\"Run level encoding\"");
            int n = bMatrix.GetLength(1);
            List<int> result = new List<int>();

            int run = 0;
            for (int i = 0; i < bMatrix.Length; i++)
            {
                int level = bMatrix[i / n, i % n];
                if (level == 0)
                {
                    run++;
                    continue;
                }

                Console.Write($"({run},{level})\t");
                result.Add(run);
                result.Add(level);

                run = 0;
            }

            return result.ToArray();
        }

        public static Double[,] ForwardDCT(Double[,] input)
        {
            double sqrtOfLength = System.Math.Sqrt(input.Length);

            int N = input.GetLength(0);

            double[,] coefficientsMatrix = InitCoefficientsMatrix(N);
            Double[,] output = new Double[N, N];

            for (int u = 0; u <= N - 1; u++)
            {
                for (int v = 0; v <= N - 1; v++)
                {
                    double sum = 0.0;
                    for (int x = 0; x <= N - 1; x++)
                    {
                        for (int y = 0; y <= N - 1; y++)
                        {
                            sum += input[x, y] * System.Math.Cos(((2.0 * x + 1.0) / (2.0 * N)) * u * System.Math.PI) * System.Math.Cos(((2.0 * y + 1.0) / (2.0 * N)) * v * System.Math.PI);
                        }
                    }
                    sum *= coefficientsMatrix[u, v];
                    //output[u, v] = System.Math.Round(sum);
                    output[u, v] = sum;
                }
            }
            return output;
        }

        private static Double[,] InitCoefficientsMatrix(int dim)
        {
            Double[,] coefficientsMatrix = new double[dim, dim];

            for (int i = 0; i < dim; i++)
            {
                coefficientsMatrix[i, 0] = System.Math.Sqrt(2.0) / dim;
                coefficientsMatrix[0, i] = System.Math.Sqrt(2.0) / dim;
            }

            coefficientsMatrix[0, 0] = 1.0 / dim;

            for (int i = 1; i < dim; i++)
            {
                for (int j = 1; j < dim; j++)
                {
                    coefficientsMatrix[i, j] = 2.0 / dim;
                }
            }
            return coefficientsMatrix;
        }

        // Program for zig-zag conversion of array
        private static int[,] ZigZag(int[,] matrix)
        {
            int n = matrix.GetLength(1);
            int rows = n, cols = n;
            int[,] result = new int[n, n];
            //Tuple<int, int>[] a = new Tuple<int, int>[rows * cols];
            int i = 0;
            int j = 0;
            //a[p++] = Tuple.Create(i, j);
            int current = 0;
            result[current / n, current % n] = matrix[i, j];
            current++;
            while (current < rows * cols)
            {
                if (j < cols - 1)
                    j++;
                else
                    i++;

                while (i < rows && j >= 0)
                {
                    result[current / n, current % n] = matrix[i, j];
                    current++;
                    i++;
                    j--;
                }
                i--;
                j++;

                if (i < rows - 1)
                    i++;
                else
                    j++;

                while (i >= 0 && j < cols)
                {
                    result[current / n, current % n] = matrix[i, j];
                    current++;
                    i--;
                    j++;
                }
                i++;
                j--;
            }
            return result;
        }

        private static void Print2DArray<T>(T[,] data, string name)
        {
            Console.WriteLine($"\"{name}\"");
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (typeof(T) == typeof(int))
                        Console.Write(Convert.ToInt32(data[i, j]).ToString() + "\t");
                    else if (typeof(T) == typeof(byte))
                        Console.Write(Convert.ToByte(data[i, j]).ToString("X") + "\t");
                    else if (typeof(T) == typeof(double))
                        Console.Write(Convert.ToDouble(data[i, j]).ToString("0") + "\t");
                }
                Console.WriteLine();
            }
        }
    }
}
