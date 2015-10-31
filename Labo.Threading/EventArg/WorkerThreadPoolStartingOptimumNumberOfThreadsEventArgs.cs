using System;

namespace Labo.Threading.EventArg
{
    /// <summary>
    /// İşçi Thread havuzu en ekonomik sayıda işçi Thread'i başlatıyor olay argümanları sınıfı.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadPoolStartingOptimumNumberOfThreadsEventArgs : EventArgs
    {
        private readonly int m_WorkItemQueueCount;

        private readonly int m_CurrentWorkItemsCount;

        private readonly int m_CurrentNumberOfWorkerThreads;

        private readonly int m_OptimumNumberOfThreads;

        /// <summary>
        /// İşçi threadlerin mevcut sayısı.
        /// </summary>
        public int CurrentNumberOfWorkerThreads
        {
            get
            {
                return m_CurrentNumberOfWorkerThreads;
            }
        }

        /// <summary>
        /// İşçi threadleri için en ekonomik sayı.
        /// </summary>
        public int OptimumNumberOfThreads
        {
            get
            {
                return m_OptimumNumberOfThreads;
            }
        }

        /// <summary>
        /// İş parçası kuyruğunda bekleyen iş parçası sayısı.
        /// </summary>
        public int WorkItemQueueCount
        {
            get
            {
                return m_WorkItemQueueCount;
            }
        }

        /// <summary>
        /// İş parçası kuyruğunda bekleyen ve işlenen iş parçası sayısı toplamı.
        /// </summary>
        public int CurrentWorkItemsCount
        {
            get
            {
                return m_CurrentWorkItemsCount;
            }
        }

        /// <summary>
        /// İşçi Thread havuzu en ekonomik sayıda işçi Thread'i başlatıyor olay argümanları inşacı metodu.
        /// </summary>
        /// <param name="workItemQueueCount">İş parçası kuyruğunda bekleyen iş parçası sayısı.</param>
        /// <param name="currentWorkItemsCount">İş parçası kuyruğunda bekleyen ve işlenen iş parçası sayısı toplamı.</param>
        /// <param name="currentNumberOfWorkerThreads">İşçi threadlerin mevcut sayısı.</param>
        /// <param name="optimumNumberOfThreads">İşçi threadleri için en ekonomik sayı.</param>
        public WorkerThreadPoolStartingOptimumNumberOfThreadsEventArgs(int workItemQueueCount, int currentWorkItemsCount, int currentNumberOfWorkerThreads, int optimumNumberOfThreads)
        {
            m_WorkItemQueueCount = workItemQueueCount;
            m_CurrentWorkItemsCount = currentWorkItemsCount;
            m_CurrentNumberOfWorkerThreads = currentNumberOfWorkerThreads;
            m_OptimumNumberOfThreads = optimumNumberOfThreads;
        }
    }
}
