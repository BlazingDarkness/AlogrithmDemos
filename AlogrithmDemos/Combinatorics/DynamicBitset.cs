using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Combinatorics
{
    class DynamicBitset : ICollection, IEnumerable, ICloneable
    {
        private uint[] m_Chunks;
        private uint m_TopMask;

        public DynamicBitset(int bits)
        {
            m_Chunks = new uint[(bits + 31) / 32];
            Count = bits;

            if ((bits & 31) == 0)
                m_TopMask = UInt32.MaxValue;
            else
                m_TopMask = UInt32.MaxValue >> (32 - (bits & 31));
        }


        public DynamicBitset(int bits, bool initVal) : this(bits)
        {
            SetAll(initVal);
        }

        private DynamicBitset(uint[] data)
        {
            m_Chunks = new uint[data.Length];
            for (int i = 0; i < data.Length; ++i)
            {
                m_Chunks[i] = data[i];
            }
        }

        public int Count { get; private set; }

        public object SyncRoot => throw new NotImplementedException();

        public bool IsSynchronized => throw new NotImplementedException();

        public object Clone()
        {
            var bitset = new DynamicBitset(m_Chunks);

            bitset.Count = Count;
            bitset.m_TopMask = m_TopMask;

            return bitset;
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

        public void Flip(int index)
        {
            m_Chunks[index / 32] ^= (uint)(1 << (index & 31));
        }

        public bool this[int index]
        {
            set
            {
                if(value)
                {
                    m_Chunks[index / 32] |= (uint)(1 << (index & 31));
                }
                else
                {
                    m_Chunks[index / 32] &= ~(uint)(1 << (index & 31));
                }
            }
            get
            {
                return (m_Chunks[index / 32] & (uint)(1 << (index & 31))) != 0;
            }
        }
    }
}
