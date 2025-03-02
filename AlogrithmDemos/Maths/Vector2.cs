using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Maths
{
    public class Vector2<T> where T : INumber<T>
    {
        public required T X { get; set; }
        public required T Y { get; set; }
    }
}
