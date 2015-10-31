namespace Labo.Threading.EventArg
{
    using System;

    /// <summary>
    /// İşçi Thread havuzu yeni bir iş parçasını iş parcası kuyruğuna soktu olay argümanları sınıfı.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadPoolEnqueuedNewWorkItemEventArgs : EventArgs
    {
        private readonly IWorkItem m_WorkItem;

        /// <summary>
        /// İş parçası.
        /// </summary>
        public IWorkItem WorkItem
        {
            get
            {
                return m_WorkItem;
            }
        }

        /// <summary>
        /// İşçi Thread havuzu yeni bir iş parçasını iş parcası kuyruğuna soktu olay argümanları sınıfı inşacı metodu.
        /// </summary>
        /// <param name="workItem">İş parçası.</param>
        public WorkerThreadPoolEnqueuedNewWorkItemEventArgs(IWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException("workItem");
            }

            m_WorkItem = workItem;
        }
    }
}