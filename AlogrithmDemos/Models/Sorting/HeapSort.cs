using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Models.Sorting
{
    public class HeapSort : SortAlgorithm
    {
        public HeapSort() : this(128)
        {

        }

        public HeapSort(int dataSetSize)
        {
            Name = "Heap Sort";
            Resize(dataSetSize);
        }


        public override void Calculate()
        {
            //Build Heap
            int start = (Data.Length - 2) / 2;
            while (start >= 0)
            {
                ShiftDown(start, Data.Length - 1);
                start--;
            }

            //Run Sort
            int end = Data.Length - 1;

            while (end > 0)
            {
                Swap(0, end);
                EndStep();
                end--;
                ShiftDown(0, end);
            }
            Completed = true;
        }

        private void ShiftDown(int start, int end)
        {
            int root = start;

            // Bounds check for first child
            while (root * 2 < end)
            {
                int child = root * 2 + 1;
                int swap = root;

                // Check first child
                if (Compare(child, swap))
                {
                    swap = child;
                }
                EndStep();

                // Bounds check for second child
                if (child < end) 
                {
                    // Check second child
                    if (Compare(child + 1, swap))
                    {
                        swap = child + 1;
                    }
                    EndStep();
                }

                // Check if swap needs to occur
                if (swap != root)
                {
                    Swap(root, swap);
                    EndStep();
                    root = swap;
                }
                else
                {
                    return;
                }
            }
        }

        public override IEnumerator CalculateCoroutine()
        {
            //Build Heap
            int start = (Data.Length - 2) / 2;
            while (start >= 0)
            {
                IEnumerator e = ShiftDownCoroutine(start, Data.Length - 1);
                while (e.MoveNext())
                    yield return null;
                start--;
            }

            //Run Sort
            int end = Data.Length - 1;

            while (end > 0)
            {
                Swap(0, end);
                EndStep();
                end--;
                yield return null;

                IEnumerator e = ShiftDownCoroutine(0, end);
                while (e.MoveNext())
                    yield return null;
            }
            Completed = true;
        }

        private IEnumerator ShiftDownCoroutine(int start, int end)
        {
            int root = start;

            // Bounds check for first child
            while (root * 2 < end)
            {
                int child = root * 2 + 1;
                int swap = root;

                // Check first child
                if (Compare(child, swap))
                {
                    swap = child;
                }
                EndStep();
                yield return null;

                // Bounds check for second child
                if (child < end)
                {
                    // Check second child
                    if (Compare(child + 1, swap))
                    {
                        swap = child + 1;
                    }
                    EndStep();
                    yield return null;
                }

                // Check if swap needs to occur
                if (swap != root)
                {
                    Swap(root, swap);
                    root = swap;
                    EndStep();
                    yield return null;
                }
                else
                {
                    break;
                }
            }
        }


        protected override void AdditionalReset()
        {
            // No additional items need to be reset
        }
    }
}
