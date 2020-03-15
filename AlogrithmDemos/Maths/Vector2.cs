using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Maths
{
    public class Vector2<T> where T : IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
    {
        T X { get; set; }
        T Y { get; set; }
    }

    public class Vector2i : Vector2<int>
    {
        int X { get; set; }
        int Y { get; set; }
    }

    
}
