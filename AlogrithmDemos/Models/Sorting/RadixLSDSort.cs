using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Models.Sorting
{
    class RadixLSDSort : SortAlgorithm
    {
        public RadixLSDSort() : this(128)
        {

        }

        public RadixLSDSort(int dataSetSize)
        {
            Name = "Radix LSD Sort";
            Resize(dataSetSize);
        }

        public override IEnumerator CalculateCoroutine()
        {
            // Find highest value

            int highestIndex = 0;
            for (int i = 1; i < Data.Length; ++i)
            {
                if (CompareHigher(i, highestIndex))
                {
                    highestIndex = i;
                }
                EndStep();
                yield return null;
            }

            // Calc the number of digits

            int digits = 0;
            int digitTest = Data[highestIndex].data;

            do
            {
                digits++;
                digitTest /= BinCount;
            } while (digitTest > 0);

            // Do a pass per digit

            for (int pass = 0; pass < digits; ++pass)
            {
                // Bin the data
                for (int i = 0; i < Data.Length; ++i)
                {
                    Bin(i, pass);
                    EndStep();
                    yield return null;
                }

                // Unbin the data
                for (int i = 0; i < Data.Length; ++i)
                {
                    UnbinNext(i);
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
