namespace Labo.Threading.EventArg
{
    using System;

    /// <summary>
    /// ���i Thread havuzu kapat�lmak i�in i� par�as� kuyru�unda bekleyen i� par�alar�n�n bitmesini bekliyor olay arg�manlar� s�n�f�.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadPoolWaitingForWorkItemsToShutDownEventArgs : EventArgs
    {
        private readonly int m_WorkItemQueueCount;

        private readonly int m_WorkerThreadsCount;

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
        /// ���i Thread havuzu kapat�lmak i�in i� par�as� kuyru�unda bekleyen i� par�alar�n�n bitmesini bekliyor olay arg�manlar� s�n�f� in�ac� metodu.
        /// </summary>
        /// <param name="workItemQueueCount">�� par�as� kuyru�unda bekleyen i� par�as� say�s�.</param>
        /// <param name="workerThreadsCount">Mevcut i��i thread say�s�.</param>
        public WorkerThreadPoolWaitingForWorkItemsToShutDownEventArgs(int workItemQueueCount, int workerThreadsCount)
        {
            m_WorkItemQueueCount = workItemQueueCount;
            m_WorkerThreadsCount = workerThreadsCount;
        }
    }
}