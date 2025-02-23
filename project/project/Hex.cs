using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project
{
    public class Hex
    {
        static (int, int)[] axial_addjacent = new (int, int)[]
        {
                (1,0),
                (1,-1),
                (0,-1),
                (-1,0),
                (-1,1),
                (0,1),
        };


        public static Random rand = new Random();
        public static List<Hex> hexes = new List<Hex>();

        //axial coords

        public int q;
        public int r;
        public Hex(int Q, int R)
        {
            q = Q;
            r = R;
        }
        public Hex((int, int) a)
        {
            q = a.Item1;
            r = a.Item2;
        }

        static public bool isEqual(Hex a, Hex b)
        {
            return (a.q == b.q && a.r == b.r);
        }
        public bool isEqual(Hex a)
        {
            return (a.q == this.q && a.r == this.r);
        }
        public static (int, int) add((int, int) A, (int, int) B)
        {
            return (A.Item1 + B.Item1, A.Item2 + B.Item2);
        }
        public (int, int) add((int, int) B)
        {
            return (this.q + B.Item1, this.r + B.Item2);
        }
        public static (int, int) subtract((int, int) A, (int, int) B)
        {
            return (A.Item1 - B.Item1, A.Item2 - B.Item2);
        }
        public static int distance((int, int) A, (int, int) B)
        {
            return (Math.Abs(A.Item1 - B.Item1) + Math.Abs(A.Item1 + A.Item2 - B.Item1 - B.Item2) + Math.Abs(A.Item2 - B.Item2)) / 2;
        }

        public static (int, int) direction(int direction)
        {//in range 0->5
            return axial_addjacent[direction];
        }
        public static (int, int) neighbor(Hex A, (int, int) direction)
        {
            return A.add(direction);
        }
        public static (int, int) neighbor((int, int) A, (int, int) direction)
        {
            return add(direction, A);
        }
        (int, int) neighbor((int, int) direction)
        {
            return this.add(direction);
        }
        public static List<(int, int)> RangeN((int, int) centre, int N)
        {
            List<(int, int)> A = new List<(int, int)>();
            for (int i = -N; i <= N; i++)
            {
                for (int j = Math.Max(-N, -i - N); j <= Math.Min(N, -i + N); j++)
                {
                    A.Add(add(centre, (i, j)));
                }
            }
            return A;
        }
    }

}
