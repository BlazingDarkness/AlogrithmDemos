using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Maths
{
    public static class MathExtensions
    {
        public static int Clamp(this int source, int min, int max)
        {
            return Math.Max(Math.Min(source, max), min);
        }
    }
}
