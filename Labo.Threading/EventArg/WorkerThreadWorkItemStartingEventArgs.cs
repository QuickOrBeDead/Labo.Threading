using System;

namespace Labo.Threading.EventArg
{
    /// <summary>
    /// İşçi Thread'i iş parçası başlatılıyor olay argümanları sınıfı.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadWorkItemStartingEventArgs : EventArgs
    {
        /// <summary>
        /// İş parçası nesnesini getirir.
        /// </summary>
        public IWorkItem WorkItem { get; private set; }

        /// <summary>
        /// İşçi Thread'i iş parçası başlatılıyor olay argümanları inşacı metodu.
        /// </summary>
        /// <param name="workItem">İş parçası nesnesi.</param>
        public WorkerThreadWorkItemStartingEventArgs(IWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException("workItem");
            }
            WorkItem = workItem;
        }
    }
}
