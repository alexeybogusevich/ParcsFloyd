using Parcs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FloydWarshallParcs
{
    class FloydWarshallModule : IModule
    {
        private int number;
        private int[][] chunk;


        static void PrintMatrix(int[][] m)
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
        public void Run(ModuleInfo info, CancellationToken token = default(CancellationToken))
        {
            number = info.Parent.ReadInt();
            Console.WriteLine($"Current number {number}");
            chunk = info.Parent.ReadObject<int[][]>();

            int n = chunk[0].Length;
            int c = chunk.Length;
            int p = n / c;
            Console.WriteLine($"Chunk {c}x{n}");

            for (int k = 0; k < n; k++)
            {
                int[] currentRow;

                if (k >= number * c && k < number*c + c)
                {
                    currentRow = chunk[k % c];
                    info.Parent.WriteObject(chunk[k % c]);                    
                }
                else
                {
                    currentRow = info.Parent.ReadObject<int[]>();
                }

                for (int i = 0; i < c; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        chunk[i][j] = MinWeight(chunk[i][j], chunk[i][k], currentRow[j]);
                    }
                }
            }

            info.Parent.WriteObject(chunk);
            Console.WriteLine("Done!");
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
    }
}
