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

        private DataRecord[] _Data = [];
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

        protected Queue<int>[] DataBins { private set; get; } = [];

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

            Random rng = new(seed);

            for (int i = Data.Length - 1; i > 0; --i)
            {
                int pos = rng.Next(i + 1);
                (Data[i].data, Data[pos].data) = (Data[pos].data, Data[i].data);
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

        private void UpdateHistory(int element, SortAction action, int bin = 0)
        {
            if(EnableHistory)
            {
                Data[element].bin = bin;
                Data[element].step = StepsTaken;
                Data[element].action = action;
            }
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
            UpdateHistory(element, SortAction.Bin, bin);
        }
        
        // Places the element into the first bin
        protected void Bin(int element)
        {
            ++ElementCopies;
            DataBins[0].Enqueue(Data[element].data);
            UpdateHistory(element, SortAction.Bin, 0);
        }

        protected void UnbinNext(int i)
        {
            try
            {
                Queue<int> bin = DataBins.First(b => b.Count > 0);
                Data[i].data = bin.Dequeue();

                ++ElementCopies;

                UpdateHistory(i, SortAction.Swap);
            }
            catch (InvalidOperationException)
            {
            }
        }

        protected void Swap(int first, int second)
        {
            (Data[second].data, Data[first].data) = (Data[first].data, Data[second].data);
            ++Swaps;
            ElementCopies += 3;

            UpdateHistory(first, SortAction.Swap);
            UpdateHistory(second, SortAction.Swap);
        }

        /// <summary>
        /// Compares if the first is higher than the second
        /// </summary>
        /// <returns>Whether the first is higher</returns>
        protected bool CompareHigher(int first, int second)
        {
            ++Comparisons;

            UpdateHistory(first, SortAction.Compare);
            UpdateHistory(second, SortAction.Compare);

            return Data[first].data > Data[second].data;
        }

        /// <summary>
        /// Compares if the first is lower than the second
        /// </summary>
        /// <returns>Whether the first is lower</returns>
        protected bool CompareLower(int first, int second)
        {
            ++Comparisons;

            UpdateHistory(first, SortAction.Compare);
            UpdateHistory(second, SortAction.Compare);

            return Data[first].data < Data[second].data;
        }


        /// <summary>
        /// Swaps the two items if the first is higher than the second
        /// </summary>
        /// <returns>Whether a swap occurred</returns>
        protected bool SwapIfHigher(int first, int second)
        {
            if(CompareHigher(first, second))
            {
                Swap(first, second);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Swaps the two items if the first is lower than the second
        /// </summary>
        /// <returns>Whether a swap occurred</returns>
        protected bool SwapIfLower(int first, int second)
        {
            if (CompareLower(first, second))
            {
                Swap(first, second);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Cleans up recent actions and increments the step counter
        /// </summary>
        protected void EndStep()
        {
            StepsTaken++;
        }


        // Algorithm dependant

        public void Calculate()
        {
            EnableHistory = false;
            
            var iter = CalculateCoroutine();
            while (iter.MoveNext()) { }

            EnableHistory = true;
            Completed = true;
        }

        abstract public IEnumerator CalculateCoroutine();

        /// <summary>
        /// Called after the mandatory fields are cleaned up and resetted
        /// This should then reset any algorithm specific data
        /// </summary>
        abstract protected void AdditionalReset();
    }
}
