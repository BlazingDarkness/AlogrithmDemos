using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.DataStructures
{
    class DynamicBitset : ICollection, IEnumerable, ICloneable
    {
        private const int MAX_BITS_SHIFT = 5; //log2 of MAX_BITS 
        private const int MAX_BITS = 32; //Must be a power of 2
        private const int MOD_MAX_BITS_MASK = MAX_BITS - 1;

        private uint[] m_Chunks;
        private uint m_TopMask;
        
        public DynamicBitset(int bits)
        {
            m_Chunks = new uint[(bits + MOD_MAX_BITS_MASK) / MAX_BITS];
            Count = bits;

            if ((bits & MOD_MAX_BITS_MASK) == 0)
                m_TopMask = UInt32.MaxValue;
            else
                m_TopMask = UInt32.MaxValue >> (MAX_BITS - (bits & MOD_MAX_BITS_MASK));
        }


        public DynamicBitset(int bits, bool initVal) : this(bits)
        {
            SetAll(initVal);
        }

        //Creates a deep copy of the dynamic bitset
        public DynamicBitset(DynamicBitset bitset)
        {
            m_Chunks = new uint[bitset.m_Chunks.Length];
            for (int i = 0; i < bitset.m_Chunks.Length; ++i)
            {
                m_Chunks[i] = bitset.m_Chunks[i];
            }
            Count = bitset.Count;
            m_TopMask = bitset.m_TopMask;
        }

        public int Count { get; private set; }

        public object SyncRoot => throw new NotImplementedException();

        public bool IsSynchronized => throw new NotImplementedException();

        public object Clone()
        {
            return new DynamicBitset(this);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            DynamicBitset bitset = (DynamicBitset)obj;

            if (bitset.Count != Count)
                return false;

            for(int i = 0; i < m_Chunks.Length; ++i)
            {
                if (m_Chunks[i] != bitset.m_Chunks[i])
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            foreach (uint data in m_Chunks)
            {
                hash = hash * 23 + data.GetHashCode();
            }
            return hash;
        }

        public void SetAll(bool val)
        {
            if (val)
            {
                for (int i = 0; i < (m_Chunks.Length - 1); ++i)
                {
                    m_Chunks[i] = UInt32.MaxValue;
                }
                m_Chunks[m_Chunks.Length - 1] = m_TopMask;
            }
            else
            {
                for (int i = 0; i < m_Chunks.Length; ++i)
                {
                    m_Chunks[i] = 0;
                }
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Flip(int index)
        {
            m_Chunks[index >> MAX_BITS_SHIFT] ^= (uint)(1L << (index & MOD_MAX_BITS_MASK));
        }

        public bool this[int index]
        {
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if(value)
                {
                    m_Chunks[index >> MAX_BITS_SHIFT] |= (uint)(1L << (index & MOD_MAX_BITS_MASK));
                }
                else
                {
                    m_Chunks[index >> MAX_BITS_SHIFT] &= ~(uint)(1L << (index & MOD_MAX_BITS_MASK));
                }
            }
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (m_Chunks[index >> MAX_BITS_SHIFT] & (uint)(1L << (index & MOD_MAX_BITS_MASK))) != 0;
            }
        }
    }
}
