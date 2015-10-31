namespace Labo.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using Labo.Threading.Exceptions;

    /// <summary>
    /// Ýþ parçasý kuyruðu sýnýfý.
    /// Smart Thread Pool'un kullandýðý algoritmadan büyük ölçüde örnek alýnarak yazýldý; http://www.codeproject.com/Articles/7933/Smart-Thread-Pool
    /// </summary>
    internal sealed class WorkItemQueue : IWorkItemQueue
    {
        /// <summary>
        /// Ýs parçasý bekleyen Thread için zaman aþýmý süresi.
        /// </summary>
        private readonly int m_WorkItemWaiterTimeOutInMilliSeconds;

        /// <summary>
        /// Ýþ parçasý kuyruðu.
        /// </summary>
        private readonly Queue<IWorkItem> m_WorkItemsQueue;

        /// <summary>
        /// Ýþ parçasý bekleyici kaydý yýðýný.
        /// </summary>
        private readonly IWorkItemWaiterEntryStack m_WorkItemWaiterEntryStack;

        /// <summary>
        /// Ýþ parçasý bekleyen bütün Thread'leri durdurmak için kullanýlan nesne.
        /// </summary>
        private ManualResetEvent m_ShuttingDownEvent = new ManualResetEvent(false);

        /// <summary>
        /// Mevcut Thread için kullanýlacak iþ parçasý bekleyici kaydý.
        /// </summary>
        [ThreadStatic]
        private static WorkItemWaiterEntry s_CurrentWorkItemWaiterEntry;

        private bool m_IsShutDown;

        private bool m_Disposed;

        /// <summary>
        /// Mevcut Thread için kullanýlacak iþ parçasý bekleyici kaydýný getirir.
        /// </summary>
        internal static IWorkItemWaiterEntry CurrentThreadWaiterEntry
        {
            get
            {
                return s_CurrentWorkItemWaiterEntry ?? (s_CurrentWorkItemWaiterEntry = new WorkItemWaiterEntry());
            }
        }

        /// <summary>
        /// Kuyruk sayýsýný getirir.
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
        /// Ýþ parçasý bekleyici kaydý yýðýný.
        /// </summary>
        internal IWorkItemWaiterEntryStack WorkItemWaiterEntryStack
        {
            get
            {
                return m_WorkItemWaiterEntryStack;
            }
        }

        /// <summary>
        /// Ýs parçasý bekleyen Thread için zaman aþýmý süresi.
        /// </summary>
        internal int WorkItemWaiterTimeOutInMilliSeconds
        {
            get
            {
                return m_WorkItemWaiterTimeOutInMilliSeconds;
            }
        }

        /// <summary>
        /// Ýþ parçasý kuyruðu inþacý metodu.
        /// </summary>
        /// <param name="workItemWaiterTimeOutInMilliSeconds">Ýs parçasý bekleyen Thread için zaman aþýmý süresi.</param>
        public WorkItemQueue(int workItemWaiterTimeOutInMilliSeconds)
            : this(workItemWaiterTimeOutInMilliSeconds, new WorkItemWaiterEntryStack())
        {
        }

        /// <summary>
        /// Ýþ parçasý kuyruðu inþacý metodu.
        /// </summary>
        /// <param name="workItemWaiterTimeOutInMilliSeconds">Ýs parçasý bekleyen Thread için zaman aþýmý süresi.</param>
        /// <param name="workItemWaiterEntryStack">Ýþ parçasý bekleyici kaydý yýðýný.</param>
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
        /// Ýþ parçasý kuyruðu imha metodu.
        /// </summary>
        ~WorkItemQueue()
        {
            Dispose(false);
        }

        /// <summary>
        /// Ýþ parçasýný kuyruða atar.
        /// </summary>
        /// <param name="workItem">Ýþ parçasý.</param>
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
                    throw new LaboThreadingException("Ýþ parçasý kuyruðu iþ parçasý eklemeye kapatýlmýþtýr. Yeni iþ parçasý ekleyemezsiniz.");
                }
                
                bool enqueue = true;

                // Eðer bekleyen Thread varsa iþ parçasýný ona yollayacaðýz.
                while (m_WorkItemWaiterEntryStack.Count > 0)
                {
                    // Dequeue a waiter.
                    IWorkItemWaiterEntry waiterEntry = m_WorkItemWaiterEntryStack.Pop();

                    // Eðer zaman aþýmý yoksa iþ parçasý bekleyen Thread'e iþlenmesi için yolluyoruz ve kuyruða eklememize gerek kalmýyor.
                    if (waiterEntry.TrySignal(workItem))
                    {
                        workItem.State = WorkItemState.Scheduled;

                        enqueue = false;
                        break;
                    }
                }

                // Hiç bekleyen Thread yoksa iþ parçasýný kuyruða ekliyoruz.
                if (enqueue)
                {
                    workItem.State = WorkItemState.Queued;

                    // Enqueue the work item
                    m_WorkItemsQueue.Enqueue(workItem);
                }
            }
        }

        /// <summary>
        /// Kuyrukta bekleyip de sýrasý gelen iþ parçasýný döndürür.
        /// Eðer kuyrukta bir iþ parçasý yok ise null döner.
        /// </summary>
        /// <returns>Ýþ parçasý.</returns>
        public IWorkItem Dequeue()
        {
            IWorkItemWaiterEntry workItemWaiterEntry;
            lock (this)
            {
                // Eðer kuyrukta bekleyen iþ parçalarý var ise bir tane çek ve onu döndür.
                if (m_WorkItemsQueue.Count > 0)
                {
                    return m_WorkItemsQueue.Dequeue();
                }

                // Kuyrukta bekleyen bir iþ parçasý yok; o zaman mevcut Thread'i bekleme yýðýnýna atacaðýz.
                workItemWaiterEntry = CurrentThreadWaiterEntry;
                workItemWaiterEntry.Reset();

                // Eðer iþ parçasý bekleme kaydý yýðýnda mevcut ise yýðýndan çýkarýp, yýðýnýn baþýna koyuyoruz.
                m_WorkItemWaiterEntryStack.Remove(workItemWaiterEntry);
                m_WorkItemWaiterEntryStack.Push(workItemWaiterEntry);
            }

            WaitHandle[] waitHandles = { workItemWaiterEntry.WaitHandle, m_ShuttingDownEvent };

            // Zaman aþýmý geçene kadar veya kapanma sinyali alýnana kadar ya da sinyal alýnana kadar mevcut Thread'i blokluyoruz.
            int waitAnyIndex = WaitHandle.WaitAny(waitHandles, m_WorkItemWaiterTimeOutInMilliSeconds);
            bool hasSignal = waitAnyIndex == 0;

            lock (this)
            {
                if (!hasSignal)
                {
                    bool isTimedOut = workItemWaiterEntry.TrySetToTimedOut();

                    // Eðer mevcut Thread zaman aþýma uðradýysa iþ parçasý bekleyici kaydý yýðýnýndan çýkartýyoruz.
                    if (isTimedOut)
                    {
                        m_WorkItemWaiterEntryStack.Remove(workItemWaiterEntry);
                    }
                    else
                    {
                        hasSignal = true;
                    }
                }

                // Eðer sinyal alýndýysa iþ parçasýný iþlenmesi için döndürüyoruz.
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

            // Eðer kuyrukta iþ parçasý yok ise null dönüyoruz.
            return null;
        }

        /// <summary>
        /// Ýþ parçasý kuyruðunda bekleyen Thread'leri durdurur ve kuyruða yeni iþ parçasý eklemeyi kapatýr.
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
        /// .Net tarafýndan yönetilen ya da yönetilmeyan kaynaklarý serbest býrakýr.
        /// </summary>
        /// <param name="isDisposing"><c>true</c> ise .Net tarafýndan yönetilen ve yönetilmeyen kaynaklarý serbest býrakýr; <c>false</c> sadece .Net tarafýndan yönetilmeyen kaynaklarý serbest býrakýr.</param>
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