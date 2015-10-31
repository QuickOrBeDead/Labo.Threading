namespace Labo.Threading.EventArg
{
    using System;

    /// <summary>
    /// Ýþçi Thread havuzu yeni bir iþçi Thread'leri baþlatmayý deniyor olay argümanlarý sýnýfý.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadPoolTryingToStartNewWorkerThreadsEventArgs : EventArgs
    {
        private readonly int m_ThreadsCount;

        /// <summary>
        /// Ýþçi thread sayýsý.
        /// </summary>
        public int ThreadsCount
        {
            get
            {
                return m_ThreadsCount;
            }
        }

        /// <summary>
        /// Ýþçi Thread havuzu yeni bir iþçi Thread'leri baþlatmayý deniyor olay argümanlarý sýnýfý inþacý metodu.
        /// </summary>
        /// <param name="threadsCount">Ýþçi thread sayýsý.</param>
        public WorkerThreadPoolTryingToStartNewWorkerThreadsEventArgs(int threadsCount)
        {
            m_ThreadsCount = threadsCount;
        }
    }
}