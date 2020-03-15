using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Models.Sorting
{
    public class OddEvenSort : SortAlgorithm
    {
        public OddEvenSort() : this(128)
        {

        }

        public OddEvenSort(int dataSetSize)
        {
            Name = "Odd Even Sort";
            Resize(dataSetSize);
        }


        public override void Calculate()
        {
            EnableHistory = false;
            bool swapsOccurred = false;

            do
            {
                swapsOccurred = false;
                for (int i = 2; i < Data.Length; i += 2)
                {
                    swapsOccurred |= SwapIfHigher(i - 1, i);
                    EndStep();
                }
                for (int i = 1; i < Data.Length; i += 2)
                {
                    swapsOccurred |= SwapIfHigher(i - 1, i);
                    EndStep();
                }
            } while (swapsOccurred);

            EnableHistory = true;
            Completed = true;
        }

        public override IEnumerator CalculateCoroutine()
        {
            bool swapsOccurred = false;

            do
            {
                swapsOccurred = false;
                for (int i = 2; i < Data.Length; i += 2)
                {
                    swapsOccurred |= SwapIfHigher(i - 1, i);
                    EndStep();
                    yield return null;
                }
                for (int i = 1; i < Data.Length; i += 2)
                {
                    swapsOccurred |= SwapIfHigher(i - 1, i);
                    EndStep();
                    yield return null;
                }
            } while (swapsOccurred);
            
            Completed = true;
        }

        protected override void AdditionalReset()
        {
            // No additional items need to be reset
        }
    }
}
