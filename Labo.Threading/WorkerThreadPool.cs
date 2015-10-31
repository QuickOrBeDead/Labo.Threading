namespace Labo.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Threading;

    using Labo.Threading.EventArg;
    using Labo.Threading.Exceptions;

    /// <summary>
    /// Ýþçi thread havuzu sýnýfý.
    /// </summary>
    public sealed class WorkerThreadPool : IWorkerThreadPool, IWorkerThreadManager
    {
        /// <summary>
        /// Çekirdek baþýna düþen havuzda olabilecek en az iþçi thread sayýsý için varsayýlan deðer.
        /// </summary>
        internal const int DEFAULT_MIN_WORKER_THREADS_PER_CORE = 1;

        /// <summary>
        /// Çekirdek baþýna düþen havuda olabilecek en fazla iþçi thread sayýsý için varsayýlan deðer.
        /// </summary>
        internal const int DEFAULT_MAX_WORKER_THREADS_PER_CORE = 10;

        /// <summary>
        /// Ýþçi thread için varsayýlan zaman aþýmmý süresi.
        /// </summary>
        internal const int DEFAULT_WORKER_THREAD_IDLE_TIMEOUT = 60 * 1000;

        /// <summary>
        /// Ýþçi thread listesi.
        /// </summary>
        private readonly IList<WorkerThread> m_WorkerThreads;

        /// <summary>
        /// Ýþ parçasý kuyruðu.
        /// </summary>
        private readonly IWorkItemQueue m_WorkItemQueue;

        /// <summary>
        /// Havuzdaki iþçi thread sayýsýnýn en az olabileceði deðer.
        /// </summary>
        private int m_MinWorkerThreads;

        /// <summary>
        /// Havuzdaki iþçi thread sayýsýnýn en fazla olabileceði deðer.
        /// </summary>
        private int m_MaxWorkerThreads;

        /// <summary>
        /// Ýþçi thread havuzunun sonlanýrýlýyor olarak iþaretlendiðini belirtir.
        /// </summary>
        private bool m_IsShuttingdown;

        /// <summary>
        /// Ýþçi thread numara sayacý.
        /// </summary>
        private long m_WorkerThreadIdCounter;

        /// <summary>
        /// Ýþçi thread havuzu numara sayacý.
        /// </summary>
        private static int s_WorkerThreadPoolIdCounter;

        /// <summary>
        /// Ýþçi thread havuzu numarasý.
        /// </summary>
        private readonly int m_WorkerThreadPoolId;

        /// <summary>
        /// Kuyrukta bekleyen ve iþlemde olan iþ parçasý sayýsý toplamý.
        /// </summary>
        private int m_CurrentWorkItemsCount;

        /// <summary>
        /// Ýþçi Thread havuzu en ekonomik sayýda iþçi Thread'i baþlatýrken tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolStartingOptimumNumberOfThreadsEventArgs> WorkerThreadPoolStartingOptimumNumberOfThreads = delegate { };

        /// <summary>
        /// Ýþçi Thread havuzu yeni bir iþçi Thread'i baþlatýrken tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolStartingNewWorkerThreadEventArgs> WorkerThreadPoolStartingNewWorkerThread = delegate { };

        /// <summary>
        /// Ýþçi Thread havuzu yeni bir iþ parçasýný iþ parcasý kuyruðuna sokmadan önce tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolEnqueuingNewWorkItemEventArgs> WorkerThreadPoolEnqueuingNewWorkItem = delegate { };

        /// <summary>
        /// Ýþçi Thread havuzu yeni bir iþ parçasýný iþ parcasý kuyruðuna sokulduktan sonra tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolEnqueuedNewWorkItemEventArgs> WorkerThreadPoolEnqueuedNewWorkItem = delegate { };

        /// <summary>
        /// Ýþçi Thread havuzu yeni bir iþçi Thread'leri baþlatmayý denerken tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolTryingToStartNewWorkerThreadsEventArgs> WorkerThreadPoolTryingToStartNewWorkerThreads = delegate { };

        /// <summary>
        /// Ýþçi Thread havuzu minimum iþçi thread sayýsý deðiþtiðinde tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolMinimumWorkerThreadsCountChangedEventArgs> WorkerThreadPoolMinimumWorkerThreadsCountChanged = delegate { };

        /// <summary>
        /// Ýþçi Thread havuzu maximum iþçi thread sayýsý deðiþtiðinde tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolMaximumWorkerThreadsCountChangedEventArgs> WorkerThreadPoolMaximumWorkerThreadsCountChanged = delegate { };

        /// <summary>
        /// Ýþçi Thread havuzu kapatýlýrken tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolShuttingdownEventArgs> WorkerThreadPoolShuttingdown = delegate { };

        /// <summary>
        /// Ýþçi Thread havuzu kapatýldýðýnda tetiklenir.
        /// </summary>
        public event EventHandler<EventArgs> WorkerThreadPoolShutdown = delegate { };

        /// <summary>
        /// Ýþçi Thread havuzu kapatýlmak için iþ parçasý kuyruðunda bekleyen iþ parçalarýnýn bitmesini beklerken tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolWaitingForWorkItemsToShutDownEventArgs> WorkerThreadPoolWaitingForWorkItemsToShutDown = delegate { };

        /// <summary>
        /// Ýþçi thread baþlarken tetiklenen olay.
        /// </summary>
        public event EventHandler<EventArgs> WorkerThreadStarting = delegate { };

        /// <summary>
        /// Ýþçi thread baþladýðýnda tetiklenen olay.
        /// </summary>
        public event EventHandler<EventArgs> WorkerThreadStarted = delegate { };

        /// <summary>
        /// Ýþçi thread sonlanýrken tetiklenen olay.
        /// </summary>
        public event EventHandler<WorkerThreadExitingEventArgs> WorkerThreadExiting = delegate { };

        /// <summary>
        /// Ýþçi thread hata aldýðýnda tetiklenen olay.
        /// </summary>
        public event EventHandler<WorkerThreadExceptionEventArgs> WorkerThreadException = delegate { };

        /// <summary>
        /// Ýþçi thread iþ parçasýný çalýþtýrmadan önce tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadWorkItemStartingEventArgs> WorkerThreadWorkItemStarting = delegate { };

        /// <summary>
        /// Ýþçi thread iþ parçasýný çalýþtýrdýktan sonra tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadWorkItemFinishedEventArgs> WorkerThreadWorkItemFinished = delegate { };

        /// <summary>
        /// Ýþçi thread iþ parçasý hata aldýktan sonra tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadWorkItemExceptionEventArgs> WorkerThreadWorkItemException = delegate { };

        /// <summary>
        /// Havuzdaki iþçi thread sayýsýnýn en fazla olabileceði deðer.
        /// </summary>
        public int MaxWorkerThreads
        {
            get
            {
                return m_MaxWorkerThreads;
            }
        }

        /// <summary>
        /// Havuzdaki iþçi thread sayýsýnýn en az olabileceði deðer.
        /// </summary>
        public int MinWorkerThreads
        {
            get
            {
                return m_MinWorkerThreads;
            }
        }

        /// <summary>
        /// Mevcut iþçi thread sayýsý.
        /// </summary>
        public int WorkerThreadsCount
        {
            get
            {
                return m_WorkerThreads.Count;
            }
        }

        /// <summary>
        /// Ýþ parçasý kuyruðunda bekleyen iþ parçasý sayýsý.
        /// </summary>
        public int WorkItemQueueCount
        {
            get
            {
                return m_WorkItemQueue.Count;
            }
        }

        /// <summary>
        /// Ýþ parçasý kuyruðu.
        /// </summary>
        internal IWorkItemQueue WorkItemQueue
        {
            get
            {
                return m_WorkItemQueue;
            }
        }

        /// <summary>
        /// Ýþçi thread havuzu numarasý.
        /// </summary>
        public int WorkerThreadPoolId
        {
            get
            {
                return m_WorkerThreadPoolId;
            }
        }

        /// <summary>
        /// Ýþçi thread listesi.
        /// </summary>
        internal IList<WorkerThread> WorkerThreads
        {
            get
            {
                return new ReadOnlyCollection<WorkerThread>(m_WorkerThreads);
            }
        }

        /// <summary>
        /// Kuyrukta bekleyen ve iþlemde olan iþ parçasý sayýsý toplamý.
        /// </summary>
        public int CurrentWorkItemsCount
        {
            get
            {
                return m_CurrentWorkItemsCount;
            }
        }

        /// <summary>
        /// Ýþçi thread havuzu inþacý metodu.
        /// </summary>
        public WorkerThreadPool()
            : this(new WorkItemQueue(DEFAULT_WORKER_THREAD_IDLE_TIMEOUT), DEFAULT_MIN_WORKER_THREADS_PER_CORE * Environment.ProcessorCount, DEFAULT_MAX_WORKER_THREADS_PER_CORE * Environment.ProcessorCount)
        {
        }

        /// <summary>
        /// Ýþçi thread havuzu inþacý metodu.
        /// </summary>
        /// <param name="minWorkerThreads">Havuzdaki iþçi thread sayýsýnýn en az olabileceði deðer.</param>
        /// <param name="maxWorkerThreads">Havuzdaki iþçi thread sayýsýnýn en fazla olabileceði deðer.</param>
        public WorkerThreadPool(int minWorkerThreads, int maxWorkerThreads)
            : this(DEFAULT_WORKER_THREAD_IDLE_TIMEOUT, minWorkerThreads, maxWorkerThreads)
        {
        }

        /// <summary>
        /// Ýþçi thread havuzu inþacý metodu.
        /// </summary>
        /// <param name="threadIdleTimeoutInMilliSeconds">Thread zaman aþýmý süresinin milisaniye cinsinden deðeri.</param>
        /// <param name="minWorkerThreads">Havuzdaki iþçi thread sayýsýnýn en az olabileceði deðer.</param>
        /// <param name="maxWorkerThreads">Havuzdaki iþçi thread sayýsýnýn en fazla olabileceði deðer.</param>
        public WorkerThreadPool(int threadIdleTimeoutInMilliSeconds, int minWorkerThreads, int maxWorkerThreads)
            : this(new WorkItemQueue(threadIdleTimeoutInMilliSeconds), minWorkerThreads, maxWorkerThreads)
        {
        }

        /// <summary>
        /// Ýþçi thread havuzu inþacý metodu.
        /// </summary>
        /// <param name="workItemQueue">Ýþ parçasý kuyruðu.</param>
        /// <param name="minWorkerThreads">Havuzdaki iþçi thread sayýsýnýn en az olabileceði deðer.</param>
        /// <param name="maxWorkerThreads">Havuzdaki iþçi thread sayýsýnýn en fazla olabileceði deðer.</param>
        internal WorkerThreadPool(IWorkItemQueue workItemQueue, int minWorkerThreads, int maxWorkerThreads)
        {
            if (workItemQueue == null)
            {
                throw new ArgumentNullException("workItemQueue");
            }

            EnsureMaxWorkerThreads(minWorkerThreads, maxWorkerThreads);
            EnsureMinWorkerThreads(minWorkerThreads, maxWorkerThreads);

            m_WorkItemQueue = workItemQueue;
            m_MinWorkerThreads = minWorkerThreads;
            m_MaxWorkerThreads = maxWorkerThreads;
            m_WorkerThreads = new List<WorkerThread>();

            m_WorkerThreadPoolId = Interlocked.Increment(ref s_WorkerThreadPoolIdCounter);
            m_CurrentWorkItemsCount = workItemQueue.Count;

            StartOptimumNumberOfThreads();
        }

        /// <summary>
        /// Maksimum iþçi thread sayýsýný atar.
        /// </summary>
        /// <param name="maxWorkerThreads">Ýþçi thread sayýsý.</param>
        public void SetMaximumWorkerThreadsCount(int maxWorkerThreads)
        {
            EnsureMaxWorkerThreads(maxWorkerThreads);

            int oldCount = m_MaxWorkerThreads;
            m_MaxWorkerThreads = maxWorkerThreads;

            OnWorkerThreadPoolMaximumWorkerThreadsCountChanged(new WorkerThreadPoolMaximumWorkerThreadsCountChangedEventArgs(oldCount, maxWorkerThreads));

            StartOptimumNumberOfThreads();
        }

        /// <summary>
        /// Minimum iþçi thread sayýsýný atar.
        /// </summary>
        /// <param name="minWorkerThreads">Ýþçi thread sayýsý.</param>
        public void SetMinimumWorkerThreadsCount(int minWorkerThreads)
        {
            EnsureMinWorkerThreads(minWorkerThreads);

            int oldCount = m_MinWorkerThreads;
            m_MinWorkerThreads = minWorkerThreads;

            OnWorkerThreadPoolMinimumWorkerThreadsCountChanged(new WorkerThreadPoolMinimumWorkerThreadsCountChangedEventArgs(oldCount, minWorkerThreads));

            StartOptimumNumberOfThreads();
        }

        /// <summary>
        /// Ýþ parçasýný kuyruða sokar.
        /// </summary>
        /// <param name="workItem">Ýþ parçasý.</param>
        public void QueueWorkItem(IWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException("workItem");
            }

            if (m_IsShuttingdown)
            {
                throw new LaboThreadingException("Ýþçi thread havuzu kapatýlýyor. Yeni iþ parçasý ekleyemezsiniz.");
            }

            lock (m_WorkItemQueue)
            {
                // Ýþ parçasýnýn kuyruða atýlmadan önceki ilk durumu: "Yaratýldý".
                workItem.State = WorkItemState.Created;

                OnWorkerThreadPoolEnqueuingNewWorkItem(new WorkerThreadPoolEnqueuingNewWorkItemEventArgs(workItem));

                m_WorkItemQueue.Enqueue(workItem);

                IncrementWorkItemsCount();

                OnWorkerThreadPoolEnqueuedNewWorkItem(new WorkerThreadPoolEnqueuedNewWorkItemEventArgs(workItem));

                // Eðer kuyrukta bekleyen ve iþlenen iþ parçasý sayýsý toplamý iþçi thread sayýsýndan fazla ise 
                // yeni iþçi thread yaratmaya çalýþýyoruz.
                if (m_CurrentWorkItemsCount > m_WorkerThreads.Count)
                {
                    TryStartNewThreads(1);
                }
            }
        }

        /// <summary>
        /// Ýþçi thread havuzunu sonlandýrýr.
        /// </summary>
        /// <param name="waitForWorkItems">
        /// Eðer <c>true</c> deðeri verilirse sonlandýrmak için iþ parçasý kuyruðunda bekleyen iþ parçalarýnýn 
        /// bitmesini bekler. <c>false</c> deðeri verilirse iþlerin bitmesini beklemeden threadleri sonlandýrýr.
        /// </param>
        /// <param name="millisecondsTimeoutForEachWorkerThread">Ýþçi thread baþýna beklenecek milisaniye cinsinden zaman aþýmý süresi.</param>
        public void Shutdown(bool waitForWorkItems = true, int millisecondsTimeoutForEachWorkerThread = 0)
        {
            Thread[] threads;
            
            // Ýþçi thread listesi üzerinde iþlem yapýyoruz, dolayýsýyla baþka thread'lerin listeyi
            // deðiþtirmesini engellemek için listeyi kilitliyoruz.
            lock (m_WorkerThreads)
            {
                // Havuzu kapatýldý olarak iþaretliyoruz
                m_IsShuttingdown = true;

                // Ýþ parçasý kuyruðunu yeni eklemelere kapatýyoruz ve kuyruk için bekleyen Thread'leri durduruyoruz.
                m_WorkItemQueue.ShutDown();

                OnWorkerThreadPoolShuttingdown(new WorkerThreadPoolShuttingdownEventArgs(waitForWorkItems, m_WorkItemQueue.Count, m_WorkerThreads.Count));

                threads = new Thread[m_WorkerThreads.Count];

                for (int i = 0; i < m_WorkerThreads.Count; i++)
                {
                    WorkerThread workerThread = m_WorkerThreads[i];

                    // Ýþ parçasý kuyruðu boþaldýðý anda thread'in sonlanmasýný söylüyoruz.
                    workerThread.StopWhenWorkItemQueueIsEmpty(true);
                    
                    threads[i] = workerThread.Thread;
                }

                m_WorkerThreads.Clear();
            }

            // Eðer iþ parçalarýný beklememiz gerekiyorsa thread'ler iþlerini bitirene kadar bekliyoruz.
            if (waitForWorkItems)
            {
                OnWorkerThreadPoolWaitingForWorkItemsToShutDown(new WorkerThreadPoolWaitingForWorkItemsToShutDownEventArgs(m_WorkItemQueue.Count, m_WorkerThreads.Count));

                for (int i = 0; i < threads.Length; i++)
                {
                    Thread thread = threads[i];
                    if (thread != null)
                    {
                        if (millisecondsTimeoutForEachWorkerThread <= 0)
                        {
                            thread.Join();
                        }
                        else
                        {
                            thread.Join(millisecondsTimeoutForEachWorkerThread);
                        }
                    }
                }
            }

            // Artýk thread'leri sonlandýrabiliriz.
            for (int i = 0; i < threads.Length; i++)
            {
                Thread thread = threads[i];
                if (thread != null)
                {
                    thread.Abort();
                }

                threads[i] = null;
            }

            OnWorkerThreadPoolShutdown();
        }

        /// <summary>
        /// Minimum - maksimum iþçi thread sayýsý ve iþ parçasý kuyruðunda bekleyen iþ parçasý 
        /// deðerlerine bakarak iþçi thread'in sonlandýrýlýp sonlandýrýlmayacaðýný söyler.
        /// </summary>
        /// <param name="workerThread">Ýþçi thread.</param>
        /// <param name="hasNoWorkItemsInQueue"></param>
        /// <returns>
        /// Eðer <c>true</c> dönerse iþçi thread, iþçi thread listesinden çýkartýlmýþ 
        /// demektir ve metodu çaðýran kiþi iþçi thread'i sonlandýrmak zorundýr. 
        /// <c>false</c> ise tersi.
        /// </returns>
        public bool ShouldExitWorkerThread(WorkerThread workerThread, bool hasNoWorkItemsInQueue)
        {
            // Ýþçi thread listesi üzerinde iþlem yapýyoruz, dolayýsýyla baþka thread'lerin listeyi
            // deðiþtirmesini engellemek için listeyi kilitliyoruz.
            lock (m_WorkerThreads)
            {
                // Eðer havuz kapatýlýyor olarak iþaretlendi ise iþçi thread'i hiç düþünmeden sonlandýrmamýz gerekiyor.
                if (m_IsShuttingdown)
                {
                    m_WorkerThreads.Remove(workerThread);

                    return true;
                }

                // Eðer kuyrukta bekleyen iþ parçasý var ise kriterimiz maksimum thread sayýsý, yoksa minimum thread sayýsý.
                // Çünkü boþta bekleyecek thread sayýsýný belirleyen faktör minimum thread sayýsý, meþgul olacak thread sayýsýný 
                // belirleyen faktör ise maksimum thread sayýsý.
                int threadLimit = hasNoWorkItemsInQueue ? m_MinWorkerThreads : m_MaxWorkerThreads;

                // Eðer mevcut iþçi thread sayýsý limiti aþýyor ise thread'i sonlandýrmamýz gerekiyor.
                if (m_WorkerThreads.Count > threadLimit)
                {
                    m_WorkerThreads.Remove(workerThread);

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        ///  Ýþçi thread baþlýyor olayýný tetikler.
        /// </summary>
        /// <param name="sender">Olayý tetikleyen nesne.</param>
        public void OnWorkerThreadStarting(object sender)
        {
            WorkerThreadStarting(sender, EventArgs.Empty);
        }

        /// <summary>
        /// Ýþçi thread baþladý olayýný tetikler.
        /// </summary>
        /// <param name="sender">Olayý tetikleyen nesne.</param>
        public void OnWorkerThreadStarted(object sender)
        {
            WorkerThreadStarted(sender, EventArgs.Empty);
        }

        /// <summary>
        /// Ýþçi thread sonlanýyor olayýný tetikler.
        /// </summary>
        /// <param name="sender">Olayý tetikleyen nesne.</param>
        /// <param name="reason">Ýþçi thread sonlanma nedeni</param>
        public void OnWorkerThreadExiting(object sender, WorkerThreadExitReason reason)
        {
            WorkerThreadExiting(sender, new WorkerThreadExitingEventArgs(reason));
        }

        /// <summary>
        /// Ýþçi thread iþ parçasýný baþlýyor olayýný tetikler.
        /// </summary>
        /// <param name="sender">Olayý tetikleyen nesne.</param>
        /// <param name="e">Olay argümanlarý.</param>
        public void OnWorkerThreadWorkItemStarting(object sender, WorkerThreadWorkItemStartingEventArgs e)
        {
            WorkerThreadWorkItemStarting(sender, e);
        }

        /// <summary>
        /// Ýþçi thread iþ parçasý bitirildi olayýný tetikler.
        /// </summary>
        /// <param name="sender">Olayý tetikleyen nesne.</param>
        /// <param name="e">Olay argümanlarý.</param>
        public void OnWorkerThreadWorkItemFinished(object sender, WorkerThreadWorkItemFinishedEventArgs e)
        {
            WorkerThreadWorkItemFinished(sender, e);

            DecrementWorkItemsCount();
        }

        /// <summary>
        /// Ýþçi thread iþ parçasý hata aldý olayýný tetikler.
        /// </summary>
        /// <param name="sender">Olayý tetikleyen nesne.</param>
        /// <param name="e">Olay argümanlarý.</param>
        public void OnWorkerThreadWorkItemException(object sender, WorkerThreadWorkItemExceptionEventArgs e)
        {
            WorkerThreadWorkItemException(sender, e);
        }

        /// <summary>
        /// Ýþçi thread hata aldý olayýný tetikler.
        /// </summary>
        /// <param name="sender">Olayý tetikleyen nesne.</param>
        /// <param name="e">Olay argümanlarý.</param>
        public void OnWorkerThreadException(object sender, WorkerThreadExceptionEventArgs e)
        {
            WorkerThreadException(sender, e);
        }

        private int IncrementWorkItemsCount()
        {
            return Interlocked.Increment(ref m_CurrentWorkItemsCount);
        }

        private int DecrementWorkItemsCount()
        {
            return Interlocked.Decrement(ref m_CurrentWorkItemsCount);
        }

        private void EnsureMinWorkerThreads(int minWorkerThreads)
        {
           EnsureMinWorkerThreads(minWorkerThreads, m_MaxWorkerThreads);
        }

        private static void EnsureMinWorkerThreads(int minWorkerThreads, int maxWorkerThreads)
        {
            if (minWorkerThreads < 0)
            {
                throw new ArgumentOutOfRangeException("minWorkerThreads", "Minimum iþçi thread sayýsý 0'dan küçük olamaz.");
            }

            if (minWorkerThreads > maxWorkerThreads)
            {
                throw new ArgumentOutOfRangeException("minWorkerThreads", "Minimum iþçi thread sayýsý maksimum iþçi thread sayýsýndan büyük olamaz.");
            }
        }

        private void EnsureMaxWorkerThreads(int maxWorkerThreads)
        {
            EnsureMaxWorkerThreads(m_MinWorkerThreads, maxWorkerThreads);
        }

        private static void EnsureMaxWorkerThreads(int minWorkerThreads, int maxWorkerThreads)
        {
            if (maxWorkerThreads <= 0)
            {
                throw new ArgumentOutOfRangeException("maxWorkerThreads", "Maksimum iþçi thread sayýsý 0'dan büyük olmalýdýr.");
            }

            if (maxWorkerThreads < minWorkerThreads)
            {
                throw new ArgumentOutOfRangeException("maxWorkerThreads", "Maksimum iþçi thread sayýsý minimum iþçi thread sayýsýndan küçük olamaz.");
            }
        }

        /// <summary>
        /// En uygun sayýda iþçi thread'i baþlatýr.
        /// </summary>
        private void StartOptimumNumberOfThreads()
        {
            int workItemsCount = m_CurrentWorkItemsCount;
            int minWorkerThreads = m_MinWorkerThreads;
            int maxWorkerThreads = m_MaxWorkerThreads;

            int optimumNumberOfThreads = CalculateOptimumThreadsCount(workItemsCount, minWorkerThreads, maxWorkerThreads);
            int currentNumberOfWorkerThreads = m_WorkerThreads.Count;
            int threadsCount = optimumNumberOfThreads - currentNumberOfWorkerThreads;

            OnWorkerThreadPoolStartingOptimumNumberOfThreads(new WorkerThreadPoolStartingOptimumNumberOfThreadsEventArgs(workItemsCount, m_WorkItemQueue.Count, currentNumberOfWorkerThreads, optimumNumberOfThreads));

            if (threadsCount > 0)
            {
                TryStartNewThreads(threadsCount);
            }
        }

        /// <summary>
        /// Maksimum iþçi thread sayýsýný geçmeyecek þekilde yeni iþçi thread baþlatýr.
        /// Eðer havuz kapatýlýyor olarak iþaretlendi ise hiç iþçi thread baþlatmaz.
        /// </summary>
        /// <param name="threadsCount">Baþlatýlacak iþçi thread sayýsý.</param>
        private void TryStartNewThreads(int threadsCount)
        {
            OnWorkerThreadPoolTryingToStartNewWorkerThreads(new WorkerThreadPoolTryingToStartNewWorkerThreadsEventArgs(threadsCount));

            // Ýþçi thread listesi üzerinde iþlem yapýyoruz, dolayýsýyla baþka thread'lerin listeyi
            // deðiþtirmesini engellemek için listeyi kilitliyoruz.
            lock (m_WorkerThreads)
            {
                // Eðer havuz kapatýlýyor olarak iþaretlendi ise metoddan çýkýyoruz.
                if (m_IsShuttingdown)
                {
                    return;
                }

                for (int i = 0; i < threadsCount; i++)
                {
                    // Eðer mevcut iþçi thread sayýsý maksimum iþçi thread sayýsýndan fazla oldu ise metoddan çýkýyoruz.
                    if (m_WorkerThreads.Count >= m_MaxWorkerThreads)
                    {
                        return;
                    }

                    // Havuz numarasý ve thread numarasýna göre bir thread ismi belirliyoruz.
                    string threadName = string.Format(CultureInfo.CurrentCulture, "Labo WTP #{0} Thread #{1}", s_WorkerThreadPoolIdCounter, ++m_WorkerThreadIdCounter);

                    WorkerThread workerThread = new WorkerThread(this, m_WorkItemQueue, threadName, false);

                    // Yarattýðýmýz iþçi thread'i iþçi thread listesine ekliyoruz.
                    m_WorkerThreads.Add(workerThread);

                    OnWorkerThreadPoolStartingNewWorkerThread(new WorkerThreadPoolStartingNewWorkerThreadEventArgs(m_WorkerThreads.Count, workerThread));

                    // Ýþçi thread'i hemen baþlatýyoruz.
                    workerThread.Start();
                }
            }
        }

        /// <summary>
        /// Ýþçi thread havuzu numara sayacýný sýfýrlar.
        /// </summary>
        internal static void ResetWorkerThreadPoolIdCounter()
        {
            s_WorkerThreadPoolIdCounter = 0;
        }

        internal static int CalculateOptimumThreadsCount(int workItemsCount, int minWorkerThreads, int maxWorkerThreads)
        {
            // Kuyrukta bekleyen ve iþlenen iþ parçasý sayýsý toplamý ve minimum iþçi thread sayýsýnýn maksimumunu alýyoruz.
            int threadsCount = Math.Max(workItemsCount, minWorkerThreads);

            // Seçtiðimiz thread sayýsý ve maksimum thread sayýsýnýn minimumunu alýyoruz.
            // Çünki maksimum thread sayýsýný geçmememiz gerekiyor.
            return Math.Min(threadsCount, maxWorkerThreads);
        }

        private void OnWorkerThreadPoolStartingOptimumNumberOfThreads(WorkerThreadPoolStartingOptimumNumberOfThreadsEventArgs e)
        {
            WorkerThreadPoolStartingOptimumNumberOfThreads(this, e);
        }

        private void OnWorkerThreadPoolStartingNewWorkerThread(WorkerThreadPoolStartingNewWorkerThreadEventArgs e)
        {
            WorkerThreadPoolStartingNewWorkerThread(this, e);
        }

        private void OnWorkerThreadPoolEnqueuingNewWorkItem(WorkerThreadPoolEnqueuingNewWorkItemEventArgs e)
        {
            WorkerThreadPoolEnqueuingNewWorkItem(this, e);
        }

        private void OnWorkerThreadPoolEnqueuedNewWorkItem(WorkerThreadPoolEnqueuedNewWorkItemEventArgs e)
        {
            WorkerThreadPoolEnqueuedNewWorkItem(this, e);
        }

        private void OnWorkerThreadPoolTryingToStartNewWorkerThreads(WorkerThreadPoolTryingToStartNewWorkerThreadsEventArgs e)
        {
            WorkerThreadPoolTryingToStartNewWorkerThreads(this, e);
        }

        private void OnWorkerThreadPoolMinimumWorkerThreadsCountChanged(WorkerThreadPoolMinimumWorkerThreadsCountChangedEventArgs e)
        {
            WorkerThreadPoolMinimumWorkerThreadsCountChanged(this, e);
        }

        private void OnWorkerThreadPoolMaximumWorkerThreadsCountChanged(WorkerThreadPoolMaximumWorkerThreadsCountChangedEventArgs e)
        {
            WorkerThreadPoolMaximumWorkerThreadsCountChanged(this, e);
        }

        private void OnWorkerThreadPoolShuttingdown(WorkerThreadPoolShuttingdownEventArgs e)
        {
            WorkerThreadPoolShuttingdown(this, e);
        }

        private void OnWorkerThreadPoolWaitingForWorkItemsToShutDown(WorkerThreadPoolWaitingForWorkItemsToShutDownEventArgs e)
        {
            WorkerThreadPoolWaitingForWorkItemsToShutDown(this, e);
        }

        private void OnWorkerThreadPoolShutdown()
        {
            WorkerThreadPoolShutdown(this, EventArgs.Empty);
        }
    }
}