﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Maths
{
    //Modified 3rd Party code
    //Source: https://gist.github.com/Flafla2/f0260a861be0ebdeef76

    public class PerlinNoise
    {
        private int m_Seed = 0;
        private readonly int[] m_Permutations = new int[512];

        public int Seed
        {
            get { return m_Seed; }
            set { m_Seed = value; Reseed(); }
        }

        public PerlinNoise()
        {
            Reseed();
        }

        public void Reseed()
        {
            int i = 0;
            Random rng = new Random(m_Seed);
            foreach(int n in Enumerable.Range(0, 256).ToArray().OrderBy(x => rng.Next()))
            {
                m_Permutations[i] = n;
                m_Permutations[i + 256] = n;
                ++i;
            }
        }

        public double OctaveNoise(double x, double y, double z, int octaves, double persistence)
        {
            double total = 0;
            double frequency = 1;
            double amplitude = 1;
            double maxValue = 0;            // Used for normalizing result to 0.0 - 1.0
            for (int i = 0; i < octaves; i++)
            {
                total += Noise(x * frequency, y * frequency, z * frequency) * amplitude;

                maxValue += amplitude;

                amplitude *= persistence;
                frequency *= 2;
            }

            return total / maxValue;
        }

        public double Noise(double x, double y, double z)
        {
            int xi = (int)x & 255;                              // Calculate the "unit cube" that the point asked will be located in
            int yi = (int)y & 255;                              // The left bound is ( |_x_|,|_y_|,|_z_| ) and the right bound is that
            int zi = (int)z & 255;                              // plus 1.  Next we calculate the location (from 0.0 to 1.0) in that cube.
            double xf = x - (int)x;                             // We also fade the location to smooth the result.
            double yf = y - (int)y;
            double zf = z - (int)z;
            double u = Fade(xf);
            double v = Fade(yf);
            double w = Fade(zf);

            int a  = m_Permutations[xi] + yi;                             // This here is Perlin's hash function.  We take our x value (remember,
            int aa = m_Permutations[a] + zi;                             // between 0 and 255) and get a random value (from our p[] array above) between
            int ab = m_Permutations[a + 1] + zi;                             // 0 and 255.  We then add y to it and plug that into p[], and add z to that.
            int b  = m_Permutations[xi + 1] + yi;                             // Then, we get another random value by adding 1 to that and putting it into p[]
            int ba = m_Permutations[b] + zi;                             // and add z to it.  We do the whole thing over again starting with x+1.  Later
            int bb = m_Permutations[b + 1] + zi;                             // we plug aa, ab, ba, and bb back into p[] along with their +1's to get another set.
                                                                // in the end we have 8 values between 0 and 255 - one for each vertex on the unit cube.
                                                                // These are all interpolated together using u, v, and w below.

            double x1, x2, y1, y2;
            x1 = Lerp(Grad(m_Permutations[aa],     xf, yf, zf),              // This is where the "magic" happens.  We calculate a new set of p[] values and use that to get
                      Grad(m_Permutations[ba], xf - 1, yf, zf),              // our final Gradient values.  Then, we interpolate between those Gradients with the u value to get
                        u);                                     // 4 x-values.  Next, we interpolate between the 4 x-values with v to get 2 y-values.  Finally,
            x2 = Lerp(Grad(m_Permutations[ab],     xf, yf - 1, zf),          // we interpolate between the y-values to get a z-value.
                      Grad(m_Permutations[bb], xf - 1, yf - 1, zf),
                        u);                                     // When calculating the p[] values, remember that above, p[a+1] expands to p[xi]+yi+1 -- so you are
            y1 = Lerp(x1, x2, v);                               // essentially adding 1 to yi.  Likewise, p[ab+1] expands to p[p[xi]+yi+1]+zi+1] -- so you are adding
                                                                // to zi.  The other 3 parameters are your possible return values (see Grad()), which are actually
            x1 = Lerp(Grad(m_Permutations[aa + 1],     xf, yf, zf - 1),      // the vectors from the edges of the unit cube to the point in the unit cube itself.
                      Grad(m_Permutations[ba + 1], xf - 1, yf, zf - 1),
                        u);
            x2 = Lerp(Grad(m_Permutations[ab + 1],     xf, yf - 1, zf - 1),
                      Grad(m_Permutations[bb + 1], xf - 1, yf - 1, zf - 1),
                          u);
            y2 = Lerp(x1, x2, v);

            return (Lerp(y1, y2, w) + 1) / 2;                       // For convenience we bound it to 0 - 1 (theoretical min/max before is -1 - 1)
        }

        private static double Grad(int hash, double x, double y, double z)
        {
            int h = hash & 15;                                  // Take the hashed value and take the first 4 bits of it (15 == 0b1111)
            double u = h < 8 /* 0b1000 */ ? x : y;              // If the most significant bit (MSB) of the hash is 0 then set u = x.  Otherwise y.

            double v;                                           // In Ken Perlin's original implementation this was another conditional operator (?:).  I
                                                                // expanded it for readability.

            if (h < 4 /* 0b0100 */)                             // If the first and second significant bits are 0 set v = y
                v = y;
            else if (h == 12 /* 0b1100 */ || h == 14 /* 0b1110*/)// If the first and second significant bits are 1 set v = x
                v = x;
            else                                                // If the first and second significant bits are not equal (0/1, 1/0) set v = z
                v = z;

            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v); // Use the last 2 bits to decide if u and v are positive or negative.  Then return their addition.
        }

        private static double Fade(double t)
        {
            // Fade function as defined by Ken Perlin.  This eases coordinate values
            // so that they will "ease" towards integral values.  This ends up smoothing
            // the final output.
            return t * t * t * (t * (t * 6 - 15) + 10);         // 6t^5 - 15t^4 + 10t^3
        }

        private static double Lerp(double a, double b, double x)
        {
            return a + x * (b - a);
        }
    }
}
