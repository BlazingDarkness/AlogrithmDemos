using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Models.Sorting
{
    public class BubbleSort : SortAlgorithm
    {
        public BubbleSort() : this(128)
        {

        }

        public BubbleSort(int dataSetSize)
        {
            Name = "Bubble Sort";
            Resize(dataSetSize);
        }

        public override IEnumerator CalculateCoroutine()
        {
            for (int pass = 0; pass < Data.Length; ++pass)
            {
                for (int i = 1; i < Data.Length; ++i)
                {
                    SwapIfHigher(i - 1, i);
                    EndStep();
                    yield return null;
                }
            }
            Completed = true;
        }

        protected override void AdditionalReset()
        {
            // No additional items need to be reset
        }
    }
}
