namespace Labo.Threading.EventArg
{
    using System;

    /// <summary>
    /// İşçi Thread'i iş parçası hata aldı olay argümanları sınıfı.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadWorkItemExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// İş parçası nesnesini getirir.
        /// </summary>
        public IWorkItem WorkItem { get; private set; }

        /// <summary>
        /// İş parçası hatasını getirir.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// İşçi Thread'i iş parçası hata aldı olay argümanları inşacı metodu.
        /// </summary>
        /// <param name="workItem">İş parçası nesnesi.</param>
        /// <param name="exception">İş parçası hatası.</param>
        public WorkerThreadWorkItemExceptionEventArgs(IWorkItem workItem, Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            if (workItem == null)
            {
                throw new ArgumentNullException("workItem");
            }

            WorkItem = workItem;
            Exception = exception;
        }
    }
}