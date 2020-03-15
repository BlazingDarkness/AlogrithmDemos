using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlogrithmDemos.Models
{
    public interface ISteppableModel
    {
        string Name { get; }

        long StepsTaken { get; }

        bool Completed { get; }

        void Calculate();

        IEnumerator CalculateCoroutine();

        void Reset();
    }
}
