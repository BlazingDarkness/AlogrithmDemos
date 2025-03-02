using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Models.Sorting
{
    public class QuickSort : SortAlgorithm
    {
        // A hacky work around
        private static int pivotReturn = 0;

        public QuickSort() : this(128)
        {

        }

        public QuickSort(int dataSetSize)
        {
            Name = "Quick Sort";
            Resize(dataSetSize);
        }

        public override IEnumerator CalculateCoroutine()
        {
            IEnumerator e = SortCoroutine(0, Data.Length - 1);
            while (e.MoveNext())
                yield return null;

            Completed = true;
        }

        private IEnumerator SortCoroutine(int low, int high)
        {
            if (low < high)
            {
                IEnumerator e = PartitionCoroutine(low, high);
                while (e.MoveNext())
                    yield return null;

                int pivot = pivotReturn;

                e = SortCoroutine(low, pivot - 1);
                while (e.MoveNext())
                    yield return null;

                e = SortCoroutine(pivot + 1, high);
                while (e.MoveNext())
                    yield return null;
            }
        }

        private IEnumerator PartitionCoroutine(int low, int high)
        {
            int pivotIndex = (low + high) / 2;

            Swap(pivotIndex, high); // Store pivot at high
            EndStep();
            yield return null;

            pivotIndex = low;
            for (int i = low; i < high; i++)
            {
                if (CompareHigher(high, i)) // Check if pivot is higher
                {
                    Swap(pivotIndex, i);
                    ++pivotIndex;
                }
                EndStep();
                yield return null;
            }

            Swap(pivotIndex, high); // Retrieve the pivot from high
            EndStep();
            yield return null;

            pivotReturn = pivotIndex;
        }

        protected override void AdditionalReset()
        {
            // No additional items need to be reset
        }
    }
}
