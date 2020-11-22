using CommandLine;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace FloydWarshallConsequtive
{
    class Program
    {
        static int[][] GetMatrix(string filename)
        {
            return File.ReadAllLines(filename)
                   .Select(l => l.Split(' ').Where(k => k.Length > 0).Select(i => int.Parse(i)).ToArray())
                   .ToArray();
        }

        static void SaveMatrix(string filename, int[][] m)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < m.Length; i++)
            {
                for (int j = 0; j < m.Length; j++)
                {
                    sb.Append(m[i][j]);
                    if (j != m.Length - 1)
                    {
                        sb.Append(" ");
                    }
                }
                sb.AppendLine();
            }

            File.WriteAllText(filename, sb.ToString());
        }

        static int MinWeight(int a, int b, int c)
        {
            if (a != int.MaxValue)
            {
                if (b != int.MaxValue && c != int.MaxValue)
                    return Math.Min(a, b + c);
                else
                    return a;
            }
            else
            {
                if (b == int.MaxValue || c == int.MaxValue)
                    return a;
                else
                    return b + c;
            }
        }

        static int[][] Floyd(int[][] m)
        {
            int[][] result = (int[][])m.Clone();
            int rowLength = result.Length;

            for (int k = 0; k < rowLength; k++)
            {
                for (int i = 0; i < rowLength; i++)
                {
                    for (int j = 0; j < rowLength; j++)
                    {
                        result[i][j] = MinWeight(result[i][j], result[i][k], result[k][j]);
                    }
                }
            }

            return result;
        }

        static void Main(string[] args)
        {
            var input = args[0];
            var output = args[1];

            if (!File.Exists(input))
            {
                throw new ArgumentException("Input file doesn't exist");
            }

            int[][] A = GetMatrix(input);

            var sw = new Stopwatch();
            sw.Start();
            int[][] result = Floyd(A);
            sw.Stop();
            SaveMatrix(output, result);
            Console.WriteLine("Done");
            Console.WriteLine($"Total time {sw.ElapsedMilliseconds} ms ({sw.ElapsedTicks} ticks)");
            Console.ReadLine();
        }
    }
}
