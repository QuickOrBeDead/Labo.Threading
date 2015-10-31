namespace Labo.Threading.Tests.Stubs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    internal sealed class ManualFinishWorkItemQueueStub : IWorkItemQueue
    {
        private readonly ConcurrentQueue<ManualFinishWorkItem> m_Queue = new ConcurrentQueue<ManualFinishWorkItem>();

        private readonly IList<ManualFinishWorkItem> m_WorkItems = new List<ManualFinishWorkItem>();

        public int Count
        {
            get { return m_Queue.Count; }
        }

        public IList<ManualFinishWorkItem> WorkItems
        {
            get
            {
                return m_WorkItems;
            }
        }

        public ConcurrentQueue<ManualFinishWorkItem> Queue
        {
            get
            {
                return m_Queue;
            }
        }

        public void Enqueue(IWorkItem workItem)
        {
            ManualFinishWorkItem manualFinishWorkItem = (ManualFinishWorkItem)workItem;
            m_Queue.Enqueue(manualFinishWorkItem);
            m_WorkItems.Add(manualFinishWorkItem);
        }

        public IWorkItem Dequeue()
        {
            ManualFinishWorkItem workItem;
            m_Queue.TryDequeue(out workItem);

            return workItem;
        }

        public void ShutDown()
        {
        }

        public void Dispose()
        {
        }

        public void WaitAllStart(int millisecondsTimeout = -1)
        {
            m_WorkItems.ForEach(x => x.WaitStart(millisecondsTimeout));
        }

        public void StopAll()
        {
            m_WorkItems.ForEach(x => x.Stop());
        }
    }
}
