namespace Labo.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using Labo.Threading.Exceptions;

    /// <summary>
    /// �� par�as� kuyru�u s�n�f�.
    /// Smart Thread Pool'un kulland��� algoritmadan b�y�k �l��de �rnek al�narak yaz�ld�; http://www.codeproject.com/Articles/7933/Smart-Thread-Pool
    /// </summary>
    internal sealed class WorkItemQueue : IWorkItemQueue
    {
        /// <summary>
        /// �s par�as� bekleyen Thread i�in zaman a��m� s�resi.
        /// </summary>
        private readonly int m_WorkItemWaiterTimeOutInMilliSeconds;

        /// <summary>
        /// �� par�as� kuyru�u.
        /// </summary>
        private readonly Queue<IWorkItem> m_WorkItemsQueue;

        /// <summary>
        /// �� par�as� bekleyici kayd� y���n�.
        /// </summary>
        private readonly IWorkItemWaiterEntryStack m_WorkItemWaiterEntryStack;

        /// <summary>
        /// �� par�as� bekleyen b�t�n Thread'leri durdurmak i�in kullan�lan nesne.
        /// </summary>
        private ManualResetEvent m_ShuttingDownEvent = new ManualResetEvent(false);

        /// <summary>
        /// Mevcut Thread i�in kullan�lacak i� par�as� bekleyici kayd�.
        /// </summary>
        [ThreadStatic]
        private static WorkItemWaiterEntry s_CurrentWorkItemWaiterEntry;

        private bool m_IsShutDown;

        private bool m_Disposed;

        /// <summary>
        /// Mevcut Thread i�in kullan�lacak i� par�as� bekleyici kayd�n� getirir.
        /// </summary>
        internal static IWorkItemWaiterEntry CurrentThreadWaiterEntry
        {
            get
            {
                return s_CurrentWorkItemWaiterEntry ?? (s_CurrentWorkItemWaiterEntry = new WorkItemWaiterEntry());
            }
        }

        /// <summary>
        /// Kuyruk say�s�n� getirir.
        /// </summary>
        public int Count
        {
            get
            {
                lock (this)
                {
                    return m_WorkItemsQueue.Count;                    
                }
            }
        }

        /// <summary>
        /// �� par�as� bekleyici kayd� y���n�.
        /// </summary>
        internal IWorkItemWaiterEntryStack WorkItemWaiterEntryStack
        {
            get
            {
                return m_WorkItemWaiterEntryStack;
            }
        }

        /// <summary>
        /// �s par�as� bekleyen Thread i�in zaman a��m� s�resi.
        /// </summary>
        internal int WorkItemWaiterTimeOutInMilliSeconds
        {
            get
            {
                return m_WorkItemWaiterTimeOutInMilliSeconds;
            }
        }

        /// <summary>
        /// �� par�as� kuyru�u in�ac� metodu.
        /// </summary>
        /// <param name="workItemWaiterTimeOutInMilliSeconds">�s par�as� bekleyen Thread i�in zaman a��m� s�resi.</param>
        public WorkItemQueue(int workItemWaiterTimeOutInMilliSeconds)
            : this(workItemWaiterTimeOutInMilliSeconds, new WorkItemWaiterEntryStack())
        {
        }

        /// <summary>
        /// �� par�as� kuyru�u in�ac� metodu.
        /// </summary>
        /// <param name="workItemWaiterTimeOutInMilliSeconds">�s par�as� bekleyen Thread i�in zaman a��m� s�resi.</param>
        /// <param name="workItemWaiterEntryStack">�� par�as� bekleyici kayd� y���n�.</param>
        internal WorkItemQueue(int workItemWaiterTimeOutInMilliSeconds, IWorkItemWaiterEntryStack workItemWaiterEntryStack)
        {
            if (workItemWaiterEntryStack == null)
            {
                throw new ArgumentNullException("workItemWaiterEntryStack");
            }

            m_WorkItemWaiterTimeOutInMilliSeconds = workItemWaiterTimeOutInMilliSeconds;

            m_WorkItemsQueue = new Queue<IWorkItem>();
            m_WorkItemWaiterEntryStack = workItemWaiterEntryStack;
        }

        /// <summary>
        /// �� par�as� kuyru�u imha metodu.
        /// </summary>
        ~WorkItemQueue()
        {
            Dispose(false);
        }

        /// <summary>
        /// �� par�as�n� kuyru�a atar.
        /// </summary>
        /// <param name="workItem">�� par�as�.</param>
        public void Enqueue(IWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException("workItem");
            }

            lock (this)
            {
                if (m_IsShutDown)
                {
                    throw new LaboThreadingException("�� par�as� kuyru�u i� par�as� eklemeye kapat�lm��t�r. Yeni i� par�as� ekleyemezsiniz.");
                }
                
                bool enqueue = true;

                // E�er bekleyen Thread varsa i� par�as�n� ona yollayaca��z.
                while (m_WorkItemWaiterEntryStack.Count > 0)
                {
                    // Dequeue a waiter.
                    IWorkItemWaiterEntry waiterEntry = m_WorkItemWaiterEntryStack.Pop();

                    // E�er zaman a��m� yoksa i� par�as� bekleyen Thread'e i�lenmesi i�in yolluyoruz ve kuyru�a eklememize gerek kalm�yor.
                    if (waiterEntry.TrySignal(workItem))
                    {
                        workItem.State = WorkItemState.Scheduled;

                        enqueue = false;
                        break;
                    }
                }

                // Hi� bekleyen Thread yoksa i� par�as�n� kuyru�a ekliyoruz.
                if (enqueue)
                {
                    workItem.State = WorkItemState.Queued;

                    // Enqueue the work item
                    m_WorkItemsQueue.Enqueue(workItem);
                }
            }
        }

        /// <summary>
        /// Kuyrukta bekleyip de s�ras� gelen i� par�as�n� d�nd�r�r.
        /// E�er kuyrukta bir i� par�as� yok ise null d�ner.
        /// </summary>
        /// <returns>�� par�as�.</returns>
        public IWorkItem Dequeue()
        {
            IWorkItemWaiterEntry workItemWaiterEntry;
            lock (this)
            {
                // E�er kuyrukta bekleyen i� par�alar� var ise bir tane �ek ve onu d�nd�r.
                if (m_WorkItemsQueue.Count > 0)
                {
                    return m_WorkItemsQueue.Dequeue();
                }

                // Kuyrukta bekleyen bir i� par�as� yok; o zaman mevcut Thread'i bekleme y���n�na ataca��z.
                workItemWaiterEntry = CurrentThreadWaiterEntry;
                workItemWaiterEntry.Reset();

                // E�er i� par�as� bekleme kayd� y���nda mevcut ise y���ndan ��kar�p, y���n�n ba��na koyuyoruz.
                m_WorkItemWaiterEntryStack.Remove(workItemWaiterEntry);
                m_WorkItemWaiterEntryStack.Push(workItemWaiterEntry);
            }

            WaitHandle[] waitHandles = { workItemWaiterEntry.WaitHandle, m_ShuttingDownEvent };

            // Zaman a��m� ge�ene kadar veya kapanma sinyali al�nana kadar ya da sinyal al�nana kadar mevcut Thread'i blokluyoruz.
            int waitAnyIndex = WaitHandle.WaitAny(waitHandles, m_WorkItemWaiterTimeOutInMilliSeconds);
            bool hasSignal = waitAnyIndex == 0;

            lock (this)
            {
                if (!hasSignal)
                {
                    bool isTimedOut = workItemWaiterEntry.TrySetToTimedOut();

                    // E�er mevcut Thread zaman a��ma u�rad�ysa i� par�as� bekleyici kayd� y���n�ndan ��kart�yoruz.
                    if (isTimedOut)
                    {
                        m_WorkItemWaiterEntryStack.Remove(workItemWaiterEntry);
                    }
                    else
                    {
                        hasSignal = true;
                    }
                }

                // E�er sinyal al�nd�ysa i� par�as�n� i�lenmesi i�in d�nd�r�yoruz.
                if (hasSignal)
                {
                    IWorkItem workItem = workItemWaiterEntry.WorkItem;
                    if (workItem != null)
                    {
                        // workItemWaiterEntry.Reset();
                        return workItem;
                    }

                    return m_WorkItemsQueue.Dequeue();
                }
            }

            // E�er kuyrukta i� par�as� yok ise null d�n�yoruz.
            return null;
        }

        /// <summary>
        /// �� par�as� kuyru�unda bekleyen Thread'leri durdurur ve kuyru�a yeni i� par�as� eklemeyi kapat�r.
        /// </summary>
        public void ShutDown()
        {
            lock (this)
            {
                m_IsShutDown = true;

                m_ShuttingDownEvent.Set();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// .Net taraf�ndan y�netilen ya da y�netilmeyan kaynaklar� serbest b�rak�r.
        /// </summary>
        /// <param name="isDisposing"><c>true</c> ise .Net taraf�ndan y�netilen ve y�netilmeyen kaynaklar� serbest b�rak�r; <c>false</c> sadece .Net taraf�ndan y�netilmeyen kaynaklar� serbest b�rak�r.</param>
        private void Dispose(bool isDisposing)
        {
            if (m_Disposed)
            {
                return;
            }

            if (isDisposing)
            {
                if (m_ShuttingDownEvent != null)
                {
                    m_ShuttingDownEvent.Close();
                    m_ShuttingDownEvent = null;
                }

                m_Disposed = true;
            }
        }
    }
}