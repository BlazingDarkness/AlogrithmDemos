using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Models.Sorting
{
    public class OptimisedBubbleSort : SortAlgorithm
    {
        public OptimisedBubbleSort() : this(128)
        {

        }

        public OptimisedBubbleSort(int dataSetSize)
        {
            Name = "Improved Bubble Sort";
            Resize(dataSetSize);
        }

        public override IEnumerator CalculateCoroutine()
        {
            int unsortedArea = Data.Length;

            bool swapsOccurred;
            do
            {
                swapsOccurred = false;
                for (int i = 1; i < unsortedArea; ++i)
                {
                    swapsOccurred |= SwapIfHigher(i - 1, i);
                    EndStep();
                    yield return null;
                }
                --unsortedArea;
            } while (swapsOccurred);

            Completed = true;
        }

        protected override void AdditionalReset()
        {
            // No additional items need to be reset
        }
    }
}
