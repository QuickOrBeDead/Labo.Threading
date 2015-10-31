namespace Labo.Threading
{
    using System;
    using System.Threading;

    using Labo.Threading.EventArg;

    /// <summary>
    /// İşçi Thread sınıfı.
    /// </summary>
    public sealed class WorkerThread
    {
        /// <summary>
        /// Thread nesnesi.
        /// </summary>
        private readonly Thread m_Thread;

        /// <summary>
        /// Thread'in çalışmasının durdurulup durdurulmayacağını söyler. Birden fazla Thread'in aynı anda erişebilmesi için volatile yapıldı.
        /// </summary>
        private volatile bool m_ShouldStop;

        /// <summary>
        /// İşçi Thread yönetici nesnesi.
        /// </summary>
        private readonly IWorkerThreadManager m_WorkerThreadManager;

        /// <summary>
        /// İş havuzu kuyruğu.
        /// </summary>
        private readonly IWorkItemQueue m_WorkItemQueue;

        /// <summary>
        /// İşlenmekte olan mevcut iş parçası.
        /// </summary>
        private IWorkItem m_CurrentWorkItem;

        /// <summary>
        /// Senkronizasyon için kullanılan kilit nesnesi.
        /// </summary>
        private readonly object m_SyncLock = new object();

        /// <summary>
        /// İşlenecek iş parçası kalmadığında Thread'in durmasını sağlar.
        /// </summary>
        private bool m_StopWhenWorkItemQueueIsEmpty;

        /// <summary>
        /// İş parçasını işlemek için kullanılan Thread nesnesi.
        /// </summary>
        public Thread Thread
        {
            get
            {
                return m_Thread;
            }
        }

        /// <summary>
        /// İşçi thread'in iş parçasını işlemekte olup olmadığını belirtir.
        /// </summary>
        public bool IsBusy
        {
            get
            {
                lock (m_SyncLock)
                {
                    return m_CurrentWorkItem != null;
                }
            }
        }

        /// <summary>
        /// İşçi Thread'in yapıcı metodu.
        /// </summary>
        /// <param name="workerThreadManager">İşçi Thread yönetici sınıfı.</param>
        /// <param name="workItemQueue">İş parçası kuyruğu.</param>
        /// <param name="threadName">Thread ismi.</param>
        /// <param name="isBackground">Thread arkaplan Thread'i mi?</param>
        internal WorkerThread(IWorkerThreadManager workerThreadManager, IWorkItemQueue workItemQueue, string threadName, bool isBackground)
        {
            if (workerThreadManager == null)
            {
                throw new ArgumentNullException("workerThreadManager");
            }

            if (workItemQueue == null)
            {
                throw new ArgumentNullException("workItemQueue");
            }

            if (string.IsNullOrWhiteSpace(threadName))
            {
                throw new ArgumentNullException("threadName");
            }

            m_WorkerThreadManager = workerThreadManager;
            m_WorkItemQueue = workItemQueue;
            m_Thread = new Thread(DoWork);
            m_Thread.IsBackground = isBackground;
            m_Thread.Name = threadName;
        }

        /// <summary>
        /// Thread'i başlatır.
        /// </summary>
        public void Start()
        {
            OnWorkerThreadStarting();

            m_Thread.Start();
        }

        /// <summary>
        /// İş parçalarını iş parçası kuyruğundan çekerek işler. 
        /// Eğer durduruldu olarak işaretlendiyse elinde iş parçası varsa onu işleyip, 
        /// yoksa yeni iş parçası işlemeden metoddan çıkar.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void DoWork()
        {
            try
            {
                OnWorkerThreadStarted();

                while (!m_ShouldStop)
                {
                    if (m_WorkerThreadManager.ShouldExitWorkerThread(this, false))
                    {
                        OnWorkerThreadExiting(WorkerThreadExitReason.MaximumThreadCountExceeded);
                        return;
                    }

                    IWorkItem workItem = m_WorkItemQueue.Dequeue();

                    if (workItem == null)
                    {
                        if (m_WorkerThreadManager.ShouldExitWorkerThread(this, true))
                        {
                            OnWorkerThreadExiting(WorkerThreadExitReason.WorkItemQueueIsEmpty);
                            return;
                        }
                        else
                        {
                            if (m_StopWhenWorkItemQueueIsEmpty)
                            {
                                OnWorkerThreadExiting(WorkerThreadExitReason.StopWhenWorkItemQueueIsEmptyIsTrue);
                                return;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                    lock (m_SyncLock)
                    {
                        m_CurrentWorkItem = workItem;
                    }

                    try
                    {
                        OnWorkerThreadWorkItemStarting(new WorkerThreadWorkItemStartingEventArgs(workItem));

                        try
                        {
                            workItem.DoWork();
                        }
                        catch (Exception ex)
                        {
                            workItem.State = WorkItemState.CompletedWithException;
                            workItem.LastException = ex;
                            
                            OnWorkerThreadWorkItemException(new WorkerThreadWorkItemExceptionEventArgs(workItem, ex));
                        }
                        
                        if (workItem.State == WorkItemState.CompletedWithException)
                        {
                            OnWorkerThreadWorkItemException(new WorkerThreadWorkItemExceptionEventArgs(workItem, workItem.LastException));
                        }
                    }
                    finally
                    {
                        lock (m_SyncLock)
                        {
                            m_CurrentWorkItem = null;

                            OnWorkerThreadWorkItemFinished(new WorkerThreadWorkItemFinishedEventArgs(workItem));
                        }
                    }
                }

                OnWorkerThreadExiting(WorkerThreadExitReason.StopMethodIsCalled);
            }
            catch (ThreadAbortException)
            {
                Stop();

                // ThreadAbortException'ın tekrar fırlatılmamasını sağlar.
                Thread.ResetAbort();

                OnWorkerThreadExiting(WorkerThreadExitReason.ThreadAborted);
            }
            catch (Exception ex)
            {
                OnWorkerThreadException(new WorkerThreadExceptionEventArgs(ex));

                OnWorkerThreadExiting(WorkerThreadExitReason.ExceptionOccurred);
            }
        }

        /// <summary>
        /// İşçi Thread'i ve mevcutta işlenen bir iş parçası varsa onu durudurldu olarak işaretler.
        /// </summary>
        public void Stop()
        {
            lock (m_SyncLock)
            {
                if (m_CurrentWorkItem != null)
                {
                    m_CurrentWorkItem.Stop();
                }

                m_ShouldStop = true;
            }
        }

        /// <summary>
        /// İş parçası kuyruğu boş ise Thread'in durdurulup durdurulmayacağını belirler.
        /// </summary>
        /// <param name="stop">Durdurmak için <c>true</c> yoksa <c>false</c>.</param>
        public void StopWhenWorkItemQueueIsEmpty(bool stop)
        {
            m_StopWhenWorkItemQueueIsEmpty = stop;
        }

        private void OnWorkerThreadStarted()
        {
            m_WorkerThreadManager.OnWorkerThreadStarted(this);
        }

        private void OnWorkerThreadExiting(WorkerThreadExitReason reason)
        {
            m_WorkerThreadManager.OnWorkerThreadExiting(this, reason);
        }

        private void OnWorkerThreadWorkItemStarting(WorkerThreadWorkItemStartingEventArgs e)
        {
            m_WorkerThreadManager.OnWorkerThreadWorkItemStarting(this, e);
        }

        private void OnWorkerThreadWorkItemFinished(WorkerThreadWorkItemFinishedEventArgs e)
        {
            m_WorkerThreadManager.OnWorkerThreadWorkItemFinished(this, e);
        }

        private void OnWorkerThreadStarting()
        {
            m_WorkerThreadManager.OnWorkerThreadStarting(this);
        }

        private void OnWorkerThreadWorkItemException(WorkerThreadWorkItemExceptionEventArgs e)
        {
            m_WorkerThreadManager.OnWorkerThreadWorkItemException(this, e);
        }

        private void OnWorkerThreadException(WorkerThreadExceptionEventArgs e)
        {
           m_WorkerThreadManager.OnWorkerThreadException(this, e);
        }
    }
}
