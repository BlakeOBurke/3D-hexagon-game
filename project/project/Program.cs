using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using OpenTK;
using System.Text.Json.Serialization;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;

namespace project
{
    internal class Program
    {


        public static List<Ientity> vehicles = new List<Ientity>();


        static void Main(string[] args)
        {
            for (int i = -100; i <= 100; i++)
            {
                for (int j = -100; j <= 100; j++)
                {
                    if (Hex.distance((i, j), (0, 0)) <= 35)
                    {
                        Hex.hexes.Add(new Hex(i, j));
                    }

                }
            }

            Game a = new Game(1200, 900);
            a.Run(60);
           
        }



        
    }
}
