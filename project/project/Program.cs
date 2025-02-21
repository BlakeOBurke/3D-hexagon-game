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


        static void Main(string[] args)
        {
            for (int i = -10; i <= 10; i++)
            {
                for (int j = -10; j <= 10; j++)
                {
                    Hex.hexes.Add(new Hex(i, j));
                }
            }

            Game a = new Game(1200, 900);
            a.Run(60);
           
        }



        
    }
}
