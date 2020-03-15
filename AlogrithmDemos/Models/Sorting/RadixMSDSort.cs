using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Models.Sorting
{
    public class RadixMSDSort : SortAlgorithm
    {
        public RadixMSDSort() : this(128)
        {

        }

        public RadixMSDSort(int dataSetSize)
        {
            Name = "Radix MSD Sort";
            Resize(dataSetSize);
        }


        public override void Calculate()
        {
            // Find highest value

            int highestIndex = 0;
            for (int i = 1; i < Data.Length; ++i)
            {
                if (Compare(i, highestIndex))
                {
                    highestIndex = i;
                }
                EndStep();
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

            SortPartition(0, Data.Length, digits - 1);

            Completed = true;
        }

        public void SortPartition(int start, int end, int digit)
        {
            int[] binCounts = new int[DataBins.Length];

            // Bin the data
            for (int i = start; i < end; ++i)
            {
                Bin(i, digit);
                EndStep();
            }

            // Store bin counts
            for (int i = 0; i < DataBins.Length; ++i)
            {
                binCounts[i] = DataBins[i].Count;
            }

            // Unbin the data
            for (int i = start; i < end; ++i)
            {
                UnbinNext(i);
                EndStep();
            }

            // Sort the bins
            if (digit > 0)
            {
                int nextStart = start;
                for (int i = 0; i < binCounts.Length; ++i)
                {
                    int nextEnd = nextStart + binCounts[i];

                    SortPartition(nextStart, nextEnd, digit - 1);

                    nextStart = nextEnd;
                }
            }
        }

        public override IEnumerator CalculateCoroutine()
        {
            // Find highest value

            int highestIndex = 0;
            for (int i = 1; i < Data.Length; ++i)
            {
                if (Compare(i, highestIndex))
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

            IEnumerator e = SortPartitionCoroutine(0, Data.Length, digits - 1);
            while (e.MoveNext())
                yield return null;

            Completed = true;
        }

        public IEnumerator SortPartitionCoroutine(int start, int end, int digit)
        {
            int[] binCounts = new int[DataBins.Length];

            // Bin the data
            for (int i = start; i < end; ++i)
            {
                Bin(i, digit);
                EndStep();
                yield return null;
            }

            // Store bin counts
            for(int i = 0; i < DataBins.Length; ++i)
            {
                binCounts[i] = DataBins[i].Count;
            }

            // Unbin the data
            for (int i = start; i < end; ++i)
            {
                UnbinNext(i);
                EndStep();
                yield return null;
            }

            // Sort the bins
            if (digit > 0)
            {
                int nextStart = start;
                for (int i = 0; i < binCounts.Length; ++i)
                {
                    int nextEnd = nextStart + binCounts[i];

                    IEnumerator e = SortPartitionCoroutine(nextStart, nextEnd, digit - 1);
                    while (e.MoveNext())
                        yield return null;

                    nextStart = nextEnd;
                }
            }
        }

        protected override void AdditionalReset()
        {
            // No additional items need to be reset
        }
    }
}
