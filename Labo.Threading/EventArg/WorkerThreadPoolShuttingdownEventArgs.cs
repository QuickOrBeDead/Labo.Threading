namespace Labo.Threading.EventArg
{
    using System;

    /// <summary>
    /// Ýþçi Thread havuzu kapatýlýyor olay argümanlarý sýnýfý.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadPoolShuttingdownEventArgs : EventArgs
    {
        private readonly bool m_WaitForWorkItems;

        private readonly int m_WorkItemQueueCount;

        private readonly int m_WorkerThreadsCount;

        /// <summary>
        /// Sonlandýrmak için iþ parçasý kuyruðunda bekleyen iþ parçalarýnýn bitmesi beklenecek mi?
        /// </summary>
        public bool WaitForWorkItems
        {
            get
            {
                return m_WaitForWorkItems;
            }
        }

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
        /// Ýþçi Thread havuzu kapatýlýyor olay argümanlarý sýnýfý inþacý metodu.
        /// </summary>
        /// <param name="waitForWorkItems">Sonlandýrmak için iþ parçasý kuyruðunda bekleyen iþ parçalarýnýn bitmesi beklenecek mi?</param>
        /// <param name="workItemQueueCount">Ýþ parçasý kuyruðunda bekleyen iþ parçasý sayýsý.</param>
        /// <param name="workerThreadsCount">Mevcut iþçi thread sayýsý.</param>
        public WorkerThreadPoolShuttingdownEventArgs(bool waitForWorkItems, int workItemQueueCount, int workerThreadsCount)
        {
            m_WaitForWorkItems = waitForWorkItems;
            m_WorkItemQueueCount = workItemQueueCount;
            m_WorkerThreadsCount = workerThreadsCount;
        }
    }
}