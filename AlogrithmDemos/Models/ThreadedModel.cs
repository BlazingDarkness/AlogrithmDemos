using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace AlogrithmDemos.Models
{
    public abstract class ThreadedModel : ISteppableModel
    {
        public string Name { protected set; get; }
        public long StepsTaken { protected set; get; } = 0;
        public bool Completed { protected set; get; } = false;

        protected Lock Lock { get; } = new();
        private IEnumerator? StepIterator { get; set; }
        private Thread? WorkerThread { get; set; }
        public bool IsRunning { private set; get; } = false;
        private bool ShouldStop { set; get; } = false;

        public abstract void Calculate();
        public abstract IEnumerator CalculateCoroutine();
        public abstract void ResetInfo();

        protected ThreadedModel(string name)
        {
            Name = name;
            StepIterator = CalculateCoroutine();
        }

        public void Reset()
        {
            // Stop any current run
            ShouldStop = true;
            lock(Lock)
            {
                WorkerThread?.Join();
                WorkerThread = null;
                ResetInfo();

                StepIterator = CalculateCoroutine();
            }
        }

        public void Run()
        {
            lock (Lock)
            {
                if (WorkerThread == null)
                {
                    ShouldStop = false;
                    WorkerThread = new Thread(RunThread);
                    WorkerThread.Start();
                }
            }
        }

        public void Pause()
        {
            ShouldStop = true;
            lock (Lock)
            {
                WorkerThread?.Join();
                WorkerThread = null;
            }
        }

        private void RunThread()
        {
            IsRunning = true;
            while (!ShouldStop && !Completed && StepIterator != null)
            {
                lock (Lock)
                {
                    StepIterator.MoveNext();
                }
            }
            IsRunning = false;
        }

        public bool RunStep()
        {
            lock (Lock)
            {
                if(StepIterator == null || Completed)
                {
                    return false;
                }
                return StepIterator.MoveNext();
            }
        }
    }
}
