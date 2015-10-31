namespace Labo.Threading.EventArg
{
    using System;

    /// <summary>
    /// İşçi Thread havuzu yeni bir işçi Thread'i başlatıyor olay argümanları sınıfı.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadPoolStartingNewWorkerThreadEventArgs : EventArgs
    {
        private readonly int m_CurrentNumberOfWorkerThreads;

        private readonly WorkerThread m_WorkerThread;

        /// <summary>
        /// İşçi Thread.
        /// </summary>
        public WorkerThread WorkerThread
        {
            get
            {
                return m_WorkerThread;
            }
        }

        /// <summary>
        /// Mevcut işçi thread sayısı.
        /// </summary>
        public int CurrentNumberOfWorkerThreads
        {
            get
            {
                return m_CurrentNumberOfWorkerThreads;
            }
        }

        /// <summary>
        /// İşçi Thread havuzu yeni bir işçi Thread'i başlatıyor olay argümanları sınıfı inşacı metodu.
        /// </summary>
        /// <param name="currentNumberOfWorkerThreads">Mevcut işçi thread sayısı.</param>
        /// <param name="workerThread">İşçi Thread.</param>
        public WorkerThreadPoolStartingNewWorkerThreadEventArgs(int currentNumberOfWorkerThreads, WorkerThread workerThread)
        {
            if (workerThread == null)
            {
                throw new ArgumentNullException("workerThread");
            }

            m_CurrentNumberOfWorkerThreads = currentNumberOfWorkerThreads;
            m_WorkerThread = workerThread;
        }
    }
}