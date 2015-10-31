namespace Labo.Threading.EventArg
{
    using System;

    /// <summary>
    /// Ýþçi Thread havuzu kapatýlmak için iþ parçasý kuyruðunda bekleyen iþ parçalarýnýn bitmesini bekliyor olay argümanlarý sýnýfý.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadPoolWaitingForWorkItemsToShutDownEventArgs : EventArgs
    {
        private readonly int m_WorkItemQueueCount;

        private readonly int m_WorkerThreadsCount;

        /// <summary>
        /// Ýþ parçasý kuyruðunda bekleyen iþ parçasý sayýsý.
        /// </summary>
        public int WorkItemQueueCount
        {
            get
            {
                return m_WorkItemQueueCount;
            }
        }

        /// <summary>
        /// Mevcut iþçi thread sayýsý.
        /// </summary>
        public int WorkerThreadsCount
        {
            get
            {
                return m_WorkerThreadsCount;
            }
        }

        /// <summary>
        /// Ýþçi Thread havuzu kapatýlmak için iþ parçasý kuyruðunda bekleyen iþ parçalarýnýn bitmesini bekliyor olay argümanlarý sýnýfý inþacý metodu.
        /// </summary>
        /// <param name="workItemQueueCount">Ýþ parçasý kuyruðunda bekleyen iþ parçasý sayýsý.</param>
        /// <param name="workerThreadsCount">Mevcut iþçi thread sayýsý.</param>
        public WorkerThreadPoolWaitingForWorkItemsToShutDownEventArgs(int workItemQueueCount, int workerThreadsCount)
        {
            m_WorkItemQueueCount = workItemQueueCount;
            m_WorkerThreadsCount = workerThreadsCount;
        }
    }
}