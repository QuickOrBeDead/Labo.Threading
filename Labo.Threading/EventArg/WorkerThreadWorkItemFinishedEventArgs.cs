namespace Labo.Threading.EventArg
{
    using System;

    /// <summary>
    /// İşçi Thread'i iş parçası işini bitirdi olay argümanları sınıfı.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadWorkItemFinishedEventArgs : EventArgs
    {
        /// <summary>
        /// İş parçası nesnesini getirir.
        /// </summary>
        public IWorkItem WorkItem { get; private set; }

        /// <summary>
        /// İşçi Thread'i iş parçası işini bitirdi olay argümanları inşacı metodu.
        /// </summary>
        /// <param name="workItem">İş parçası nesnesi.</param>
        public WorkerThreadWorkItemFinishedEventArgs(IWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException("workItem");
            }
            WorkItem = workItem;
        }
    }
}