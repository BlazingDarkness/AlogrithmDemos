using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using AlogrithmDemos.DataStructures;

namespace AlogrithmDemos.Models
{
    public enum ETriomino : int
    {
        VLine,      /* B
                       B
                       B */

        TopLeft,    /* BB
                       B  */

        TopRight,   /* BB
                        B */

        BottomLeft, /* B
                       BB */

        BottomRight,/*  B
                       BB */

        HLine,       /* BBB */
        NumOfPieces,
        None,
        Mem
    }

    public class TriominosModel : ThreadedModel
    {
        public struct StepData
        {
            public ETriomino triomino;
            public int x;
            public int y;
            public long permutations;
            public bool placed;
        }

        public int Width { private set; get; } = 2;
        public int Height { private set; get; } = 6;
        public long Permutations { private set; get; } = 0;
        public bool UseMemorization { set { m_UseMem = value; Reset(); } get { return m_UseMem; } }
        public StepData[] Steps { private set; get; }

        private DynamicBitset m_State;
        private int m_MaxPieces = 0;
        private bool m_UseMem = false;

        private readonly Dictionary<DynamicBitset, long> m_Dictionary;

        public TriominosModel() : this(2, 6)
        {
        }

        public TriominosModel(int width, int height) : base("Triominos")
        {
            m_UseMem = false;
            m_Dictionary = [];
            Steps = [];
            m_State = new DynamicBitset(width * height, true);

            Resize(width, height);
        }

        public void Resize(int width, int height)
        {
            lock (Lock)
            {
                Width = width;
                Height = height;
                m_MaxPieces = (width * height) / 3;
                Steps = new StepData[m_MaxPieces];
                m_State = new DynamicBitset(width * height, true);

                Reset();
            }
        }

        public override void ResetInfo()
        {
            StepsTaken = 0;
            Permutations = 0;
            Completed = false;
            m_State.SetAll(true);
            m_Dictionary.Clear();

            for (int i = 0; i < Steps.Length; ++i)
            {
                Steps[i].placed = false;
            }
        }

        public override void Calculate()
        {
            Reset();
            lock (Lock)
            {
                Stopwatch s = Stopwatch.StartNew();
                Permutations = Calculate(0, 0);
                s.Stop();
                Trace.WriteLine($"{(s.Elapsed.TotalMilliseconds)}");
                Completed = true;
            }
        }

        private long Calculate(int index, int depth)
        {
            long total = 0;

            if(m_Dictionary.TryGetValue(m_State, out long prevPermutations))
            {
                total += prevPermutations;
            }
            else
            {
                //Find index for next piece to be placed
                while (!m_State[index])
                {
                    ++index;
                }

                //Find next piece to be placed at the index
                int nextX = index % Width;
                int nextY = index / Width;
                int nextDepth = depth + 1;
                ETriomino next = GetNextValidTriomino(ETriomino.None, nextX, nextY);

                while (next != ETriomino.None)
                {
                    if (nextDepth == m_MaxPieces)
                    {
                        total += 1;
                    }
                    else
                    {
                        FlipValidTriomino(next, index);
                        total += Calculate(index, nextDepth);
                        FlipValidTriomino(next, index);
                    }
                    next = GetNextValidTriomino(next, nextX, nextY);
                }

                if(total != 0)
                {
                    m_Dictionary.Add((DynamicBitset)m_State.Clone(), total);
                }
            }

            return total;
        }

        public override IEnumerator CalculateCoroutine()
        {
            ResetInfo();

            IEnumerator e = CalculateCoroutine(0, 0, 0);
            while (e.MoveNext())
                yield return null;

            Completed = true;
        }

        private IEnumerator CalculateCoroutine(int x, int y, int depth)
        {
            if (m_UseMem && m_Dictionary.TryGetValue(m_State, out long permutations))
            {
                Permutations += permutations;
                //FlipValidTriominoAndSetStep(ETriomino.Mem, 0, 0, depth);
                yield return null;
            }
            else
            {
                long prevPermutations = Permutations;
                int index = x + y * Width;

                //Find index for next piece to be placed
                while (!m_State[index])
                {
                    ++index;
                }

                //Find next piece to be placed at the index
                int nextX = index % Width;
                int nextY = index / Width;
                int nextDepth = depth + 1;
                ETriomino next = GetNextValidTriomino(ETriomino.None, nextX, nextY);

                while (next != ETriomino.None)
                {
                    FlipValidTriominoAndSetStep(next, nextX, nextY, depth);
                    yield return null;
                    if (nextDepth == m_MaxPieces)
                    {
                        Permutations += 1L;
                    }
                    else
                    {
                        IEnumerator e = CalculateCoroutine(nextX, nextY, nextDepth);
                        while (e.MoveNext())
                            yield return null;
                    }
                    FlipValidTriominoAndSetStep(next, nextX, nextY, depth);
                    yield return null;
                    next = GetNextValidTriomino(next, nextX, nextY);
                }

                if (m_UseMem && (prevPermutations != Permutations))
                {
                    m_Dictionary.Add((DynamicBitset)m_State.Clone(), Permutations - prevPermutations);
                }
            }
        }
        
        private bool State(int x, int y)
        {
            return m_State[x + y * Width];
        }
        
        private ETriomino GetNextValidTriomino(ETriomino triomino, int x, int y)
        {
            bool b01 = ((y + 1) < Height) && State(x, y + 1);
            bool b10 = ((x + 1) < Width) && State(x + 1, y);
            bool b11 = ((y + 1) < Height) && ((x + 1) < Width) && State(x + 1, y + 1);

            switch (triomino)
            {
                case ETriomino.None:
                    if (b01 && ((y + 2) < Height) && State(x, y + 2))
                        return ETriomino.VLine;
                    else
                        goto case ETriomino.VLine;
                case ETriomino.VLine:
                    if (b10 && b01)
                        return ETriomino.TopLeft;
                    else
                        goto case ETriomino.TopLeft;
                case ETriomino.TopLeft:
                    if (b10 && b11)
                        return ETriomino.TopRight;
                    else
                        goto case ETriomino.TopRight;
                case ETriomino.TopRight:
                    if (b01 && b11)
                        return ETriomino.BottomLeft;
                    else
                        goto case ETriomino.BottomLeft;
                case ETriomino.BottomLeft:
                    if (b01 && (x > 0) && State(x - 1, y + 1))
                        return ETriomino.BottomRight;
                    else
                        goto case ETriomino.BottomRight;
                case ETriomino.BottomRight:
                    if (b10 && ((x + 2) < Width) && State(x + 2, y))
                        return ETriomino.HLine;
                    else
                        return ETriomino.None;
                case ETriomino.HLine:
                    return ETriomino.None;
                case ETriomino.Mem:
                    return ETriomino.None;
                default:
                    return ETriomino.None;
            }
        }
        
        private void FlipValidTriominoAndSetStep(ETriomino triomino, int x, int y, int depth)
        {
            Steps[depth].triomino = triomino;
            Steps[depth].x = x;
            Steps[depth].y = y;
            Steps[depth].placed = !Steps[depth].placed;
            ++StepsTaken;
            FlipValidTriomino(triomino, x + y * Width);
        }

        /// <summary>
        /// Flips the bits corresponding the the specific triomino piece at the specified location
        /// Does not do any bounds checking for additional performance gains
        /// </summary>
        /// <param name="triomino">The piece being placed or removed</param>
        /// <param name="index">The location in the bitset the piece is being placed or removed/param>
        private void FlipValidTriomino(ETriomino triomino, int index)
        {
            switch (triomino)
            {
                case ETriomino.None:
                    return;
                case ETriomino.VLine:
                    m_State.Flip(index);
                    m_State.Flip(index + Width);
                    m_State.Flip(index + (Width << 1));
                    return;
                case ETriomino.TopLeft:
                    m_State.Flip(index);
                    m_State.Flip(index + Width);
                    m_State.Flip(index + 1);
                    return;
                case ETriomino.TopRight:
                    m_State.Flip(index);
                    m_State.Flip(index + 1);
                    m_State.Flip(index + 1 + Width);
                    return;
                case ETriomino.BottomLeft:
                    m_State.Flip(index);
                    m_State.Flip(index + Width);
                    m_State.Flip(index + 1 + Width);
                    return;
                case ETriomino.BottomRight:
                    m_State.Flip(index);
                    m_State.Flip(index + Width);
                    m_State.Flip(index + Width - 1);
                    return;
                case ETriomino.HLine:
                    m_State.Flip(index);
                    m_State.Flip(index + 1);
                    m_State.Flip(index + 2);
                    return;
                default:
                    return;
            }
        }
    }
}
