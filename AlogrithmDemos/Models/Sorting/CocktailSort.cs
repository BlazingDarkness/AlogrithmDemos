using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Models.Sorting
{
    public class CocktailSort : SortAlgorithm
    {
        public CocktailSort() : this(128)
        {

        }

        public CocktailSort(int dataSetSize)
        {
            Name = "Cocktail Sort";
            Resize(dataSetSize);
        }


        public override void Calculate()
        {
            EnableHistory = false;
            int start = 1;
            int end = Data.Length;

            do
            {
                int lastSwapIndex = 0;

                for (int i = start; i < end; ++i)
                {
                    if(SwapIfHigher(i - 1, i))
                    {
                        lastSwapIndex = i;
                    }
                    EndStep();
                }
                end = lastSwapIndex;

                for (int i = end; i >= start; --i)
                {
                    if(SwapIfHigher(i - 1, i))
                    {
                        lastSwapIndex = i;
                    }
                    EndStep();
                }
                start = lastSwapIndex;

            } while (end > start);

            EnableHistory = true;
            Completed = true;
        }

        public override IEnumerator CalculateCoroutine()
        {
            int start = 1;
            int end = Data.Length;

            do
            {
                int lastSwapIndex = 0;

                for (int i = start; i < end; ++i)
                {
                    if (SwapIfHigher(i - 1, i))
                    {
                        lastSwapIndex = i;
                    }
                    EndStep();
                    yield return null;
                }
                end = lastSwapIndex;

                for (int i = end; i >= start; --i)
                {
                    if (SwapIfHigher(i - 1, i))
                    {
                        lastSwapIndex = i;
                    }
                    EndStep();
                    yield return null;
                }
                start = lastSwapIndex;

            } while (end > start);

            Completed = true;
        }

        protected override void AdditionalReset()
        {
            // No additional items need to be reset
        }
    }
}
