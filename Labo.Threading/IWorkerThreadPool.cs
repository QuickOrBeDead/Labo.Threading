namespace Labo.Threading
{
    /// <summary>
    /// Ýþçi thread havuzu arayüzü.
    /// </summary>
    public interface IWorkerThreadPool
    {
        /// <summary>
        /// Ýþ parçasýný kuyruða sokar.
        /// </summary>
        /// <param name="workItem">Ýþ parçasý.</param>
        void QueueWorkItem(IWorkItem workItem);

        /// <summary>
        /// Havuzdaki iþçi thread sayýsýnýn en fazla olabileceði deðer.
        /// </summary>
        int MaxWorkerThreads { get; }

        /// <summary>
        /// Havuzdaki iþçi thread sayýsýnýn en az olabileceði deðer.
        /// </summary>
        int MinWorkerThreads { get; }

        /// <summary>
        /// Mevcut iþçi thread sayýsý.
        /// </summary>
        int WorkerThreadsCount { get; }

        /// <summary>
        /// Ýþ parçasý kuyruðunda bekleyen iþ parçasý sayýsý.
        /// </summary>
        int WorkItemQueueCount { get; }
    }
}