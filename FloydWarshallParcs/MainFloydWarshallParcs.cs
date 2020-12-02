using CommandLine;
using Parcs;
using Parcs.Module.CommandLine;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace FloydWarshallParcs
{
    class MainFloydWarshallParcs : MainModule
    {
        private static CLIOptions options;

        class CLIOptions : BaseModuleOptions
        {
            [Option("input", Required = true, HelpText = "File path to the input array.")]
            public string InputFile { get; set; }
            [Option("output", Required = true, HelpText = "File path to the sorted array.")]
            public string OutputFile { get; set; }
            [Option("p", Required = true, HelpText = "Number of points.")]
            public int PointsCount { get; set; }
        }

        private IChannel[] channels;
        private IPoint[] points;
        private int[][] matrix;

        static void Main(string[] args)
        {
            options = new CLIOptions();

            if (args != null)
            {
                if (!Parser.Default.ParseArguments(args, options))
                {
                    throw new ArgumentException($@"Cannot parse the arguments. Possible usages: {options.GetUsage()}");
                }
            }

            (new MainFloydWarshallParcs()).RunModule(options);
        }

        public override void Run(ModuleInfo info, CancellationToken token = default)
        {
            int pointsNum = options.PointsCount;
            Stopwatch sw = new Stopwatch();
            matrix = GetMatrix(options.InputFile);

            if (matrix.Length % pointsNum != 0)
            {
                throw new Exception($"Matrix size (now {matrix.Length}) should be divided by {pointsNum}!");
            }

            channels = new IChannel[pointsNum];
            points = new IPoint[pointsNum];

            for (int i = 0; i < pointsNum; ++i)
            {
                points[i] = info.CreatePoint();
                channels[i] = points[i].CreateChannel();
                points[i].ExecuteClass("FloydWarshallParcs.ModuleFloydWarshall");
            }

            DistributeAllData();

            sw.Start();
            RunParallelFloyd();
            sw.Stop();

            int[][] result = GatherAllData();

            SaveMatrix(options.OutputFile, result);
            Console.WriteLine("Done");
            Console.WriteLine($"Total time {sw.ElapsedMilliseconds} ms ({sw.ElapsedTicks} ticks)");
            Console.ReadLine();
        }

        static int[][] GetMatrix(string filename)
        {
            return File.ReadAllLines(filename)
                   .Select(l => l.Split(' ').Where(k => k.Length > 0).Select(i => int.Parse(i)).ToArray())
                   .ToArray();
        }

        static void SaveMatrix(string filename, int[][] m)
        {
            using (var file = File.CreateText(filename))
            {
                for (int i = 0; i < m.Length; i++)
                {
                    for (int j = 0; j < m.Length; j++)
                    {
                        file.Write(m[i][j]);
                        if (j != m.Length - 1)
                        {
                            file.Write(" ");
                        }
                    }
                    file.WriteLine();
                }
            }
        }

        private static int[][] ReadMatrix()
        {
            Console.WriteLine("Matrix size: ");
            string input = Console.ReadLine();
            var m = int.Parse(input);
            var result = new int[m][];
            for (int i = 0; i < m; i++)
            {
                result[i] = Console.ReadLine()
                    .Replace("-1", int.MaxValue.ToString())
                    .Split(' ')
                    .Select(a => int.Parse(a))
                    .ToArray();
            }
            return result;
        }

        private void PrintMatrix(int[][] m)
        {
            int rowLength = m.Length;

            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < m[i].Length; j++)
                {
                    Console.Write(m[i][j] + " ");
                }
                Console.WriteLine();
            }
        }

        private int[][] GatherAllData()
        {
            int chunkSize = matrix.Length / options.PointsCount;
            int[][] result = new int[matrix.Length][];

            for (int i = 0; i < channels.Length; i++)
            {
                int[][] chunk = channels[i].ReadObject<int[][]>();
                for (int j = 0; j < chunkSize; j++)
                {
                    result[i * chunkSize + j] = chunk[j];
                }
            }

            return result;
        }

        private void RunParallelFloyd()
        {
            object locker = new object();
            int chunkSize = matrix.Length / options.PointsCount;
            for (int k = 0; k < matrix.Length; k++)
            {
                lock (locker)
                {
                    int currentSupplier = k / chunkSize;
                    int[] currentRow = channels[currentSupplier].ReadObject<int[]>();

                    for (int ch = 0; ch < channels.Length; ch++)
                    {
                        if (ch != currentSupplier)
                        {
                            channels[ch].WriteObject(currentRow);
                        }
                    }
                }
            }
        }

        private void DistributeAllData()
        {
            for (int i = 0; i < channels.Length; i++)
            {
                Console.WriteLine($"Sent to channel: {i}");
                channels[i].WriteData(i);
                int chunkSize = matrix.Length / options.PointsCount;

                int[][] chunk = new int[chunkSize][];

                for (int j = 0; j < chunkSize; j++)
                {
                    chunk[j] = matrix[i * chunkSize + j];
                }

                channels[i].WriteObject(chunk);
            }
        }
    }
}
