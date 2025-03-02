using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Models.Sorting
{
    class MergeSort : SortAlgorithm
    {
        private int[] TempData { get; set; } = [];

        public MergeSort() : this(128)
        {

        }

        public MergeSort(int dataSetSize)
        {
            Name = "Merge Sort";
            Resize(dataSetSize);
        }

        public override IEnumerator CalculateCoroutine()
        {
            for (int segLen = 1; segLen < Data.Length; segLen *= 2)
            {
                int mergeLen = segLen * 2;
                for (int start = 0; start < Data.Length; start += mergeLen)
                {
                    int start1 = start;
                    int end1 = start + segLen;
                    int start2 = start + segLen;
                    int end2 = Math.Min(start + mergeLen, Data.Length);  // Prevent out of bounds

                    while (start1 != end1 && start2 != end2)
                    {
                        if (CompareHigher(start1, start2))
                        {
                            Bin(start2);
                            ++start2;
                            EndStep();
                            yield return null;
                        }
                        else
                        {
                            Bin(start1);
                            ++start1;
                            EndStep();
                            yield return null;
                        }
                    }

                    // Add the rest of the first segment that still has elements
                    while (start1 != end1)
                    {
                        Bin(start1);
                        ++start1;
                        EndStep();
                        yield return null;
                    }

                    // Add the rest of the second segment that still has elements
                    while (start2 != end2)
                    {
                        Bin(start2);
                        ++start2;
                        EndStep();
                        yield return null;
                    }

                    // Unpack the elements from the additional memory
                    for (int i = start; i < end2; ++i)
                    {
                        UnbinNext(i);
                        EndStep();
                        yield return null;
                    }

                    EndStep();
                }
            }
            Completed = true;
        }

        protected override void AdditionalReset()
        {
            if (TempData == null || TempData.Length != Data.Length)
            {
                TempData = new int[Data.Length];
            }
            else
            {
                for (int i = 0; i < TempData.Length; ++i)
                {
                    TempData[i] = 0;
                }
            }
        }
    }
}
