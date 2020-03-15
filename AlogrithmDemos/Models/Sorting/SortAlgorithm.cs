using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlogrithmDemos.Models.Sorting
{
    public abstract class SortAlgorithm : ISteppableModel
    {
        public enum SortAction
        {
            None,
            Swap,
            Compare,
            Bin
        }

        public struct DataRecord
        {
            public int data;
            public int bin;
            public long step;
            public SortAction action;
        }

        private DataRecord[] _Data = null;
        private int _BinCount = 8;

        // Mandatory data fields

        public string Name { protected set; get; } = "Sort";

        public long Swaps { protected set; get; } = 0;

        public long ElementCopies { protected set; get; } = 0;

        public long Comparisons { protected set; get; } = 0;

        public long StepsTaken { protected set; get; } = 0;

        public long StepHistoryDisplayed { set; get; } = 16;

        public bool EnableHistory { set; get; } = true;

        public bool Completed { protected set; get; } = false;

        public DataRecord[] Data {
            protected set
            {
                _Data = value;
                ResetBins();
                Reset();
            }
            get { return _Data; }
        }

        protected Queue<int>[] DataBins { private set; get; }

        public int BinCount
        {
            protected set
            {
                _BinCount = value;
                ResetBins();
            }
            get { return _BinCount; }
        }

        // Bins

        private void ResetBins()
        {
            DataBins = new Queue<int>[_BinCount];

            for(int i = 0; i < DataBins.Length; ++i)
            {
                DataBins[i] = new Queue<int>();
            }
        }


        // Ultility

        public void Shuffle(int seed)
        {
            Reset();

            Random rng = new Random(seed);

            for (int i = Data.Length - 1; i > 0; --i)
            {
                int pos = rng.Next(i + 1);
                int val = Data[pos].data;
                Data[pos].data = Data[i].data;
                Data[i].data = val;
            }
        }

        public void Resize(int size)
        {
            Data = new DataRecord[size];
            Reset();
        }

        public void Reset()
        {
            StepsTaken = 0;
            ElementCopies = 0;
            Swaps = 0;
            Comparisons = 0;
            EnableHistory = true;
            Completed = false;

            for(int i = 0; i < Data.Length; ++i)
            {
                Data[i].data = i + 1;
                Data[i].step = 0;
                Data[i].action = SortAction.None;
            }

            foreach (Queue<int> q in DataBins)
            {
                q.Clear();
            }

            AdditionalReset();
        }

        private int Pow(int a, int b)
        {
            int result = 1;

            while(b > 0)
            {
                result *= a;
            }

            return result;
        }

        /// <summary>
        /// Places the element into the bin corresponding to the value of the digit
        /// </summary>
        /// <param name="element">The index of the element to be binned</param>
        /// <param name="digit">Zero indexed digit where the digit is base n with n being the number of bins</param>
        protected void Bin(int element, int digit)
        {
            ++ElementCopies;
            int bin = (Data[element].data / ((int)Math.Pow(BinCount, digit))) % BinCount;

            DataBins[bin].Enqueue(Data[element].data);

            if (EnableHistory)
            {
                Data[element].bin = bin;
                Data[element].step = StepsTaken;
                Data[element].action = SortAction.Bin;
            }
        }
        
        // Places the element into the first bin
        protected void Bin(int element)
        {
            ++ElementCopies;
            DataBins[0].Enqueue(Data[element].data);

            if (EnableHistory)
            {
                Data[element].step = StepsTaken;
                Data[element].action = SortAction.Bin;
            }
        }

        protected void UnbinNext(int i)
        {
            try
            {
                Queue<int> bin = DataBins.First(b => b.Count > 0);
                Data[i].data = bin.Dequeue();

                ++ElementCopies;

                if (EnableHistory)
                {
                    Data[i].step = StepsTaken;
                    Data[i].action = SortAction.Swap;
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        protected void Swap(int first, int second)
        {
            int val = Data[first].data;
            Data[first].data = Data[second].data;
            Data[second].data = val;
            ++Swaps;
            ElementCopies += 3;

            if (EnableHistory)
            {
                Data[first].step = StepsTaken;
                Data[first].action = SortAction.Swap;
                Data[second].step = StepsTaken;
                Data[second].action = SortAction.Swap;
            }
        }

        /// <summary>
        /// Swaps the two items if the first is higher than the second
        /// </summary>
        /// <returns>Whether a swap occurred</returns>
        protected bool Compare(int first, int second)
        {
            ++Comparisons;

            if (EnableHistory)
            {
                Data[first].step = StepsTaken;
                Data[first].action = SortAction.Compare;
                Data[second].step = StepsTaken;
                Data[second].action = SortAction.Compare;
            }

            return Data[first].data > Data[second].data;
        }


        /// <summary>
        /// Swaps the two items if the first is higher than the second
        /// </summary>
        /// <returns>Whether a swap occurred</returns>
        protected bool SwapIfHigher(int first, int second)
        {
            ++Comparisons;
            if (Data[first].data > Data[second].data)
            {
                int val = Data[first].data;
                Data[first].data = Data[second].data;
                Data[second].data = val;
                Swaps++;
                ElementCopies += 3;

                if (EnableHistory)
                {
                    Data[first].step = StepsTaken;
                    Data[first].action = SortAction.Swap;
                    Data[second].step = StepsTaken;
                    Data[second].action = SortAction.Swap;
                }
                return true;
            }
            else
            {
                if (EnableHistory)
                {
                    Data[first].step = StepsTaken;
                    Data[first].action = SortAction.Compare;
                    Data[second].step = StepsTaken;
                    Data[second].action = SortAction.Compare;
                }
                return false;
            }
        }

        /// <summary>
        /// Swaps the two items if the first is lower than the second
        /// </summary>
        /// <returns>Whether a swap occurred</returns>
        protected bool SwapIfLower(int first, int second)
        {
            ++Comparisons;
            if (Data[first].data < Data[second].data)
            {
                int val = Data[first].data;
                Data[first].data = Data[second].data;
                Data[second].data = val;
                Swaps++;
                ElementCopies += 3;

                if (EnableHistory)
                {
                    Data[first].step = StepsTaken;
                    Data[first].action = SortAction.Swap;
                    Data[second].step = StepsTaken;
                    Data[second].action = SortAction.Swap;
                }
                return true;
            }
            else
            {
                if (EnableHistory)
                {
                    Data[first].step = StepsTaken;
                    Data[first].action = SortAction.Compare;
                    Data[second].step = StepsTaken;
                    Data[second].action = SortAction.Compare;
                }
                return false;
            }
        }

        /// <summary>
        /// Cleans up recent actions and increments the step counter
        /// </summary>
        protected void EndStep()
        {
            StepsTaken++;

            long StepCutoff = StepsTaken - StepHistoryDisplayed;
        }


        // Algorithm dependant

        abstract public void Calculate();

        abstract public IEnumerator CalculateCoroutine();

        /// <summary>
        /// Called after the mandatory fields are cleaned up and resetted
        /// This should then reset any algorithm specific data
        /// </summary>
        abstract protected void AdditionalReset();
    }
}
