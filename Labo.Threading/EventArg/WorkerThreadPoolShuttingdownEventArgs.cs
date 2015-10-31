namespace Labo.Threading.EventArg
{
    using System;

    /// <summary>
    /// ���i Thread havuzu kapat�l�yor olay arg�manlar� s�n�f�.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadPoolShuttingdownEventArgs : EventArgs
    {
        private readonly bool m_WaitForWorkItems;

        private readonly int m_WorkItemQueueCount;

        private readonly int m_WorkerThreadsCount;

        /// <summary>
        /// Sonland�rmak i�in i� par�as� kuyru�unda bekleyen i� par�alar�n�n bitmesi beklenecek mi?
        /// </summary>
        public bool WaitForWorkItems
        {
            get
            {
                return m_WaitForWorkItems;
            }
        }

        /// <summary>
        /// �� par�as� kuyru�unda bekleyen i� par�as� say�s�.
        /// </summary>
        public int WorkItemQueueCount
        {
            get
            {
                return m_WorkItemQueueCount;
            }
        }

        /// <summary>
        /// Mevcut i��i thread say�s�.
        /// </summary>
        public int WorkerThreadsCount
        {
            get
            {
                return m_WorkerThreadsCount;
            }
        }

        /// <summary>
        /// ���i Thread havuzu kapat�l�yor olay arg�manlar� s�n�f� in�ac� metodu.
        /// </summary>
        /// <param name="waitForWorkItems">Sonland�rmak i�in i� par�as� kuyru�unda bekleyen i� par�alar�n�n bitmesi beklenecek mi?</param>
        /// <param name="workItemQueueCount">�� par�as� kuyru�unda bekleyen i� par�as� say�s�.</param>
        /// <param name="workerThreadsCount">Mevcut i��i thread say�s�.</param>
        public WorkerThreadPoolShuttingdownEventArgs(bool waitForWorkItems, int workItemQueueCount, int workerThreadsCount)
        {
            m_WaitForWorkItems = waitForWorkItems;
            m_WorkItemQueueCount = workItemQueueCount;
            m_WorkerThreadsCount = workerThreadsCount;
        }
    }
}