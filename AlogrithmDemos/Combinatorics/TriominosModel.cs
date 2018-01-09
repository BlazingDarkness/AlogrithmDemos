using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Combinatorics
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
        None
    }

    public class TriominosModel
    {
        public struct StepData
        {
            public ETriomino triomino;
            public int x;
            public int y;
            public long permutations;
            public bool placed;
        }

        public int Width { private set; get; }
        public int Height { private set; get; }
        public long StepsTaken { private set; get; }
        public long Permutations { private set; get; }
        public bool Completed { private set; get; }
        public bool UseMemorization { set { m_UseMem = value; Reset(); } get { return m_UseMem; } }
        public StepData[] Steps { private set; get; }

        private DynamicBitset m_State;
        private int m_MaxPieces;
        private int m_StackIndex;
        private bool m_UseMem;

        private Dictionary<DynamicBitset, long> m_Dictionary;

        public TriominosModel(int width, int height)
        {
            Width = width;
            Height = height;
            Completed = false;
            m_UseMem = false;
            m_MaxPieces = (width * height) / 3;
            m_StackIndex = -1;
            StepsTaken = 0;
            Permutations = 0;
            
            Steps = new StepData[m_MaxPieces];
            m_State = new DynamicBitset(width * height, true);
            m_Dictionary = new Dictionary<DynamicBitset, long>();
        }

        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;
            m_MaxPieces = (width * height) / 3;
            Steps = new StepData[m_MaxPieces];
            m_State = new DynamicBitset(width * height, true);

            Reset();
        }

        public void Reset()
        {
            m_StackIndex = -1;
            StepsTaken = 0;
            Permutations = 0;
            Completed = false;
            m_State.SetAll(true);
            m_Dictionary.Clear();

            for(int i = 0; i < Steps.Length; ++i)
            {
                Steps[i].placed = false;
            }
        }

        /// <summary>
        /// Computes the next step of the dancing links algorithm
        /// </summary>
        public void NextStep()
        {
            if (Completed)
                return;

            ++StepsTaken;

            if (m_StackIndex >= 0)
            {
                ref StepData prevStep = ref Steps[m_StackIndex];

                if (prevStep.placed)
                {
                    if ((m_StackIndex + 1) == m_MaxPieces)
                    {
                        //Can't place another, remove previous instead
                        FlipValidTriomino(prevStep.triomino, prevStep.x, prevStep.y);
                        prevStep.placed = false;
                    }
                    else if(m_UseMem && m_Dictionary.TryGetValue(m_State, out long prevPermutations))
                    {
                        Permutations += prevPermutations;
                        FlipValidTriomino(prevStep.triomino, prevStep.x, prevStep.y);
                        prevStep.placed = false;
                    }
                    else
                    {
                        int index = prevStep.x + prevStep.y * Width;

                        //Find index for next piece to be placed
                        while (!m_State[index])
                        {
                            ++index;
                        }

                        //Find next piece to be placed at the index
                        int nextX = index % Width;
                        int nextY = index / Width;
                        ETriomino next = GetNextValidTriomino(ETriomino.None, nextX, nextY);

                        //If Piece found then place it
                        if (next != ETriomino.None)
                        {
                            ++m_StackIndex;
                            FlipValidTriomino(next, nextX, nextY);

                            Steps[m_StackIndex].triomino = next;
                            Steps[m_StackIndex].x = nextX;
                            Steps[m_StackIndex].y = nextY;
                            Steps[m_StackIndex].permutations = Permutations;
                            Steps[m_StackIndex].placed = true;

                            if ((m_StackIndex + 1) == m_MaxPieces)
                                ++Permutations;
                        }
                        else //No piece found, remove previous piece
                        {
                            FlipValidTriomino(prevStep.triomino, prevStep.x, prevStep.y);
                            prevStep.placed = false;
                        }
                    }
                }
                else
                {
                    //Previous was removed, see if it can be replaced with a different one
                    ETriomino next = GetNextValidTriomino(prevStep.triomino, prevStep.x, prevStep.y);

                    //If a replacement piece was found
                    if (next != ETriomino.None)
                    {
                        prevStep.triomino = next;
                        prevStep.permutations = Permutations;
                        prevStep.placed = true;
                        FlipValidTriomino(prevStep.triomino, prevStep.x, prevStep.y);
                    }
                    else //No replacement piece found
                    {
                        //If no more pieces to be removed
                        if (m_StackIndex == 0)
                        {
                            Completed = true;
                        }
                        else //Remove the next piece
                        {
                            --m_StackIndex;

                            if(m_UseMem)
                            {
                                m_Dictionary.Add((DynamicBitset)m_State.Clone(), Permutations - Steps[m_StackIndex].permutations);
                            }

                            FlipValidTriomino(Steps[m_StackIndex].triomino, Steps[m_StackIndex].x, Steps[m_StackIndex].y);
                            Steps[m_StackIndex].placed = false;
                        }
                    }
                }
            }
            else
            {
                //Get first valid piece
                ETriomino next = GetNextValidTriomino(ETriomino.None, 0, 0);

                //If Piece found then place it
                if (next != ETriomino.None)
                {
                    ++m_StackIndex;
                    FlipValidTriomino(next, 0, 0);

                    Steps[0].triomino = next;
                    Steps[0].x = 0;
                    Steps[0].y = 0;
                    Steps[0].permutations = 0;
                    Steps[0].placed = true;

                    if ((m_StackIndex + 1) == m_MaxPieces)
                        ++Permutations;
                }
                else //No valid starting piece
                {
                    Completed = true;
                }
            }
        }

        private bool State(int x, int y)
        {
            return m_State[x + y * Width];
        }
        
        private void FlipState(int x, int y)
        {
            m_State[x + y * Width] ^= true;
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
                default:
                    return ETriomino.None;
            }
        }

        private bool DoesTriominoFit(ETriomino triomino, int x, int y)
        {
            switch(triomino)
            {
                case ETriomino.None:
                    return false;
                case ETriomino.VLine:
                    return ((y + 2) < Height) &&
                            State(x, y) &&
                            State(x, y + 1) &&
                            State(x, y + 2);
                case ETriomino.TopLeft:
                    return ((y + 1) < Height) &&
                            ((x + 1) < Width) &&
                            State(x, y) &&
                            State(x, y + 1) &&
                            State(x + 1, y);
                case ETriomino.TopRight:
                    return ((y + 1) < Height) &&
                            ((x + 1) < Width) &&
                            State(x, y) &&
                            State(x + 1, y) &&
                            State(x + 1, y + 1);
                case ETriomino.BottomLeft:
                    return ((y + 1) < Height) &&
                            ((x + 1) < Width) &&
                            State(x, y) &&
                            State(x, y + 1) &&
                            State(x + 1, y + 1);
                case ETriomino.BottomRight:
                    return ((y + 1) < Height) &&
                            (x > 0) &&
                            State(x, y) &&
                            State(x, y + 1) &&
                            State(x - 1, y + 1);
                case ETriomino.HLine:
                    return ((x + 2) < Width) &&
                            State(x, y) &&
                            State(x + 1, y) &&
                            State(x + 2, y);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Flips the bits corresponding the the specific triomino piece at the specified location
        /// Does not do any bounds checking for additional performance gains
        /// </summary>
        /// <param name="triomino">The piece being placed or removed</param>
        /// <param name="x">X coordinate of the piece being placed or removed</param>
        /// <param name="y">Y coordinate of the piece being placed or removed</param>
        private void FlipValidTriomino(ETriomino triomino, int x, int y)
        {
            switch (triomino)
            {
                case ETriomino.None:
                    return;
                case ETriomino.VLine:
                    FlipState(x, y);
                    FlipState(x, y + 1);
                    FlipState(x, y + 2);
                    return;
                case ETriomino.TopLeft:
                    FlipState(x, y);
                    FlipState(x, y + 1);
                    FlipState(x + 1, y);
                    return;
                case ETriomino.TopRight:
                    FlipState(x, y);
                    FlipState(x + 1, y);
                    FlipState(x + 1, y + 1);
                    return;
                case ETriomino.BottomLeft:
                    FlipState(x, y);
                    FlipState(x, y + 1);
                    FlipState(x + 1, y + 1);
                    return;
                case ETriomino.BottomRight:
                    FlipState(x, y);
                    FlipState(x, y + 1);
                    FlipState(x - 1, y + 1);
                    return;
                case ETriomino.HLine:
                    FlipState(x, y);
                    FlipState(x + 1, y);
                    FlipState(x + 2, y);
                    return;
                default:
                    return;
            }
        }

        private void FlipTriomino(ETriomino triomino, int x, int y)
        {
            switch (triomino)
            {
                case ETriomino.None:
                    return;
                case ETriomino.VLine:
                    if ((y + 2) < Height)
                    {
                        FlipState(x, y);
                        FlipState(x, y + 1);
                        FlipState(x, y + 2);
                    }
                    break;
                case ETriomino.TopLeft:
                    if (((y + 1) < Height) && ((x + 1) < Width))
                    {
                        FlipState(x, y);
                        FlipState(x, y + 1);
                        FlipState(x + 1, y);
                    }
                    break;
                case ETriomino.TopRight:
                    if (((y + 1) < Height) && ((x + 1) < Width))
                    {
                        FlipState(x, y);
                        FlipState(x + 1, y);
                        FlipState(x + 1, y + 1);
                    }
                    break;
                case ETriomino.BottomLeft:
                    if (((y + 1) < Height) && ((x + 1) < Width))
                    {
                        FlipState(x, y);
                        FlipState(x, y + 1);
                        FlipState(x + 1, y + 1);
                    }
                    break;
                case ETriomino.BottomRight:
                    if (((y + 1) < Height) && (x > 0))
                    {
                        FlipState(x, y);
                        FlipState(x, y + 1);
                        FlipState(x - 1, y + 1);
                    }
                    break;
                case ETriomino.HLine:
                    if ((x + 2) < Width)
                    {
                            FlipState(x, y);
                            FlipState(x + 1, y);
                            FlipState(x + 2, y);
                    }
                    break;
                default:
                    return;
            }
        }
    }
}
