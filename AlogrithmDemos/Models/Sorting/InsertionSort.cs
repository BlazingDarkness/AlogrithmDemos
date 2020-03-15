using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Models.Sorting
{
    public class InsertionSort : SortAlgorithm
    {
        public InsertionSort() : this(128)
        {

        }

        public InsertionSort(int dataSetSize)
        {
            Name = "Insertion Sort";
            Resize(dataSetSize);
        }


        public override void Calculate()
        {
            EnableHistory = false;
            for (int j = 1; j < Data.Length; ++j)
            {
                int i = j - 1;
                while (i >= 0 && SwapIfHigher(i, i + 1))
                {
                    --i;
                    EndStep();
                }
                EndStep();
            }
            EnableHistory = true;
            Completed = true;
        }

        public override IEnumerator CalculateCoroutine()
        {
            for (int j = 1; j < Data.Length; ++j)
            {
                int i = j - 1;
                while (i >= 0 && SwapIfHigher(i, i + 1))
                {
                    --i;
                    EndStep();
                    yield return null;
                }
                EndStep();
                yield return null;
            }
            Completed = true;
        }

        protected override void AdditionalReset()
        {
            // No additional items need to be reset
        }
    }
}