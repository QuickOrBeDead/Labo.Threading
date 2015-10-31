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
    /// ���i thread havuzu s�n�f�.
    /// </summary>
    public sealed class WorkerThreadPool : IWorkerThreadPool, IWorkerThreadManager
    {
        /// <summary>
        /// �ekirdek ba��na d��en havuzda olabilecek en az i��i thread say�s� i�in varsay�lan de�er.
        /// </summary>
        internal const int DEFAULT_MIN_WORKER_THREADS_PER_CORE = 1;

        /// <summary>
        /// �ekirdek ba��na d��en havuda olabilecek en fazla i��i thread say�s� i�in varsay�lan de�er.
        /// </summary>
        internal const int DEFAULT_MAX_WORKER_THREADS_PER_CORE = 10;

        /// <summary>
        /// ���i thread i�in varsay�lan zaman a��mm� s�resi.
        /// </summary>
        internal const int DEFAULT_WORKER_THREAD_IDLE_TIMEOUT = 60 * 1000;

        /// <summary>
        /// ���i thread listesi.
        /// </summary>
        private readonly IList<WorkerThread> m_WorkerThreads;

        /// <summary>
        /// �� par�as� kuyru�u.
        /// </summary>
        private readonly IWorkItemQueue m_WorkItemQueue;

        /// <summary>
        /// Havuzdaki i��i thread say�s�n�n en az olabilece�i de�er.
        /// </summary>
        private int m_MinWorkerThreads;

        /// <summary>
        /// Havuzdaki i��i thread say�s�n�n en fazla olabilece�i de�er.
        /// </summary>
        private int m_MaxWorkerThreads;

        /// <summary>
        /// ���i thread havuzunun sonlan�r�l�yor olarak i�aretlendi�ini belirtir.
        /// </summary>
        private bool m_IsShuttingdown;

        /// <summary>
        /// ���i thread numara sayac�.
        /// </summary>
        private long m_WorkerThreadIdCounter;

        /// <summary>
        /// ���i thread havuzu numara sayac�.
        /// </summary>
        private static int s_WorkerThreadPoolIdCounter;

        /// <summary>
        /// ���i thread havuzu numaras�.
        /// </summary>
        private readonly int m_WorkerThreadPoolId;

        /// <summary>
        /// Kuyrukta bekleyen ve i�lemde olan i� par�as� say�s� toplam�.
        /// </summary>
        private int m_CurrentWorkItemsCount;

        /// <summary>
        /// ���i Thread havuzu en ekonomik say�da i��i Thread'i ba�lat�rken tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolStartingOptimumNumberOfThreadsEventArgs> WorkerThreadPoolStartingOptimumNumberOfThreads = delegate { };

        /// <summary>
        /// ���i Thread havuzu yeni bir i��i Thread'i ba�lat�rken tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolStartingNewWorkerThreadEventArgs> WorkerThreadPoolStartingNewWorkerThread = delegate { };

        /// <summary>
        /// ���i Thread havuzu yeni bir i� par�as�n� i� parcas� kuyru�una sokmadan �nce tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolEnqueuingNewWorkItemEventArgs> WorkerThreadPoolEnqueuingNewWorkItem = delegate { };

        /// <summary>
        /// ���i Thread havuzu yeni bir i� par�as�n� i� parcas� kuyru�una sokulduktan sonra tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolEnqueuedNewWorkItemEventArgs> WorkerThreadPoolEnqueuedNewWorkItem = delegate { };

        /// <summary>
        /// ���i Thread havuzu yeni bir i��i Thread'leri ba�latmay� denerken tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolTryingToStartNewWorkerThreadsEventArgs> WorkerThreadPoolTryingToStartNewWorkerThreads = delegate { };

        /// <summary>
        /// ���i Thread havuzu minimum i��i thread say�s� de�i�ti�inde tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolMinimumWorkerThreadsCountChangedEventArgs> WorkerThreadPoolMinimumWorkerThreadsCountChanged = delegate { };

        /// <summary>
        /// ���i Thread havuzu maximum i��i thread say�s� de�i�ti�inde tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolMaximumWorkerThreadsCountChangedEventArgs> WorkerThreadPoolMaximumWorkerThreadsCountChanged = delegate { };

        /// <summary>
        /// ���i Thread havuzu kapat�l�rken tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolShuttingdownEventArgs> WorkerThreadPoolShuttingdown = delegate { };

        /// <summary>
        /// ���i Thread havuzu kapat�ld���nda tetiklenir.
        /// </summary>
        public event EventHandler<EventArgs> WorkerThreadPoolShutdown = delegate { };

        /// <summary>
        /// ���i Thread havuzu kapat�lmak i�in i� par�as� kuyru�unda bekleyen i� par�alar�n�n bitmesini beklerken tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadPoolWaitingForWorkItemsToShutDownEventArgs> WorkerThreadPoolWaitingForWorkItemsToShutDown = delegate { };

        /// <summary>
        /// ���i thread ba�larken tetiklenen olay.
        /// </summary>
        public event EventHandler<EventArgs> WorkerThreadStarting = delegate { };

        /// <summary>
        /// ���i thread ba�lad���nda tetiklenen olay.
        /// </summary>
        public event EventHandler<EventArgs> WorkerThreadStarted = delegate { };

        /// <summary>
        /// ���i thread sonlan�rken tetiklenen olay.
        /// </summary>
        public event EventHandler<WorkerThreadExitingEventArgs> WorkerThreadExiting = delegate { };

        /// <summary>
        /// ���i thread hata ald���nda tetiklenen olay.
        /// </summary>
        public event EventHandler<WorkerThreadExceptionEventArgs> WorkerThreadException = delegate { };

        /// <summary>
        /// ���i thread i� par�as�n� �al��t�rmadan �nce tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadWorkItemStartingEventArgs> WorkerThreadWorkItemStarting = delegate { };

        /// <summary>
        /// ���i thread i� par�as�n� �al��t�rd�ktan sonra tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadWorkItemFinishedEventArgs> WorkerThreadWorkItemFinished = delegate { };

        /// <summary>
        /// ���i thread i� par�as� hata ald�ktan sonra tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadWorkItemExceptionEventArgs> WorkerThreadWorkItemException = delegate { };

        /// <summary>
        /// Havuzdaki i��i thread say�s�n�n en fazla olabilece�i de�er.
        /// </summary>
        public int MaxWorkerThreads
        {
            get
            {
                return m_MaxWorkerThreads;
            }
        }

        /// <summary>
        /// Havuzdaki i��i thread say�s�n�n en az olabilece�i de�er.
        /// </summary>
        public int MinWorkerThreads
        {
            get
            {
                return m_MinWorkerThreads;
            }
        }

        /// <summary>
        /// Mevcut i��i thread say�s�.
        /// </summary>
        public int WorkerThreadsCount
        {
            get
            {
                return m_WorkerThreads.Count;
            }
        }

        /// <summary>
        /// �� par�as� kuyru�unda bekleyen i� par�as� say�s�.
        /// </summary>
        public int WorkItemQueueCount
        {
            get
            {
                return m_WorkItemQueue.Count;
            }
        }

        /// <summary>
        /// �� par�as� kuyru�u.
        /// </summary>
        internal IWorkItemQueue WorkItemQueue
        {
            get
            {
                return m_WorkItemQueue;
            }
        }

        /// <summary>
        /// ���i thread havuzu numaras�.
        /// </summary>
        public int WorkerThreadPoolId
        {
            get
            {
                return m_WorkerThreadPoolId;
            }
        }

        /// <summary>
        /// ���i thread listesi.
        /// </summary>
        internal IList<WorkerThread> WorkerThreads
        {
            get
            {
                return new ReadOnlyCollection<WorkerThread>(m_WorkerThreads);
            }
        }

        /// <summary>
        /// Kuyrukta bekleyen ve i�lemde olan i� par�as� say�s� toplam�.
        /// </summary>
        public int CurrentWorkItemsCount
        {
            get
            {
                return m_CurrentWorkItemsCount;
            }
        }

        /// <summary>
        /// ���i thread havuzu in�ac� metodu.
        /// </summary>
        public WorkerThreadPool()
            : this(new WorkItemQueue(DEFAULT_WORKER_THREAD_IDLE_TIMEOUT), DEFAULT_MIN_WORKER_THREADS_PER_CORE * Environment.ProcessorCount, DEFAULT_MAX_WORKER_THREADS_PER_CORE * Environment.ProcessorCount)
        {
        }

        /// <summary>
        /// ���i thread havuzu in�ac� metodu.
        /// </summary>
        /// <param name="minWorkerThreads">Havuzdaki i��i thread say�s�n�n en az olabilece�i de�er.</param>
        /// <param name="maxWorkerThreads">Havuzdaki i��i thread say�s�n�n en fazla olabilece�i de�er.</param>
        public WorkerThreadPool(int minWorkerThreads, int maxWorkerThreads)
            : this(DEFAULT_WORKER_THREAD_IDLE_TIMEOUT, minWorkerThreads, maxWorkerThreads)
        {
        }

        /// <summary>
        /// ���i thread havuzu in�ac� metodu.
        /// </summary>
        /// <param name="threadIdleTimeoutInMilliSeconds">Thread zaman a��m� s�resinin milisaniye cinsinden de�eri.</param>
        /// <param name="minWorkerThreads">Havuzdaki i��i thread say�s�n�n en az olabilece�i de�er.</param>
        /// <param name="maxWorkerThreads">Havuzdaki i��i thread say�s�n�n en fazla olabilece�i de�er.</param>
        public WorkerThreadPool(int threadIdleTimeoutInMilliSeconds, int minWorkerThreads, int maxWorkerThreads)
            : this(new WorkItemQueue(threadIdleTimeoutInMilliSeconds), minWorkerThreads, maxWorkerThreads)
        {
        }

        /// <summary>
        /// ���i thread havuzu in�ac� metodu.
        /// </summary>
        /// <param name="workItemQueue">�� par�as� kuyru�u.</param>
        /// <param name="minWorkerThreads">Havuzdaki i��i thread say�s�n�n en az olabilece�i de�er.</param>
        /// <param name="maxWorkerThreads">Havuzdaki i��i thread say�s�n�n en fazla olabilece�i de�er.</param>
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
        /// Maksimum i��i thread say�s�n� atar.
        /// </summary>
        /// <param name="maxWorkerThreads">���i thread say�s�.</param>
        public void SetMaximumWorkerThreadsCount(int maxWorkerThreads)
        {
            EnsureMaxWorkerThreads(maxWorkerThreads);

            int oldCount = m_MaxWorkerThreads;
            m_MaxWorkerThreads = maxWorkerThreads;

            OnWorkerThreadPoolMaximumWorkerThreadsCountChanged(new WorkerThreadPoolMaximumWorkerThreadsCountChangedEventArgs(oldCount, maxWorkerThreads));

            StartOptimumNumberOfThreads();
        }

        /// <summary>
        /// Minimum i��i thread say�s�n� atar.
        /// </summary>
        /// <param name="minWorkerThreads">���i thread say�s�.</param>
        public void SetMinimumWorkerThreadsCount(int minWorkerThreads)
        {
            EnsureMinWorkerThreads(minWorkerThreads);

            int oldCount = m_MinWorkerThreads;
            m_MinWorkerThreads = minWorkerThreads;

            OnWorkerThreadPoolMinimumWorkerThreadsCountChanged(new WorkerThreadPoolMinimumWorkerThreadsCountChangedEventArgs(oldCount, minWorkerThreads));

            StartOptimumNumberOfThreads();
        }

        /// <summary>
        /// �� par�as�n� kuyru�a sokar.
        /// </summary>
        /// <param name="workItem">�� par�as�.</param>
        public void QueueWorkItem(IWorkItem workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException("workItem");
            }

            if (m_IsShuttingdown)
            {
                throw new LaboThreadingException("���i thread havuzu kapat�l�yor. Yeni i� par�as� ekleyemezsiniz.");
            }

            lock (m_WorkItemQueue)
            {
                // �� par�as�n�n kuyru�a at�lmadan �nceki ilk durumu: "Yarat�ld�".
                workItem.State = WorkItemState.Created;

                OnWorkerThreadPoolEnqueuingNewWorkItem(new WorkerThreadPoolEnqueuingNewWorkItemEventArgs(workItem));

                m_WorkItemQueue.Enqueue(workItem);

                IncrementWorkItemsCount();

                OnWorkerThreadPoolEnqueuedNewWorkItem(new WorkerThreadPoolEnqueuedNewWorkItemEventArgs(workItem));

                // E�er kuyrukta bekleyen ve i�lenen i� par�as� say�s� toplam� i��i thread say�s�ndan fazla ise 
                // yeni i��i thread yaratmaya �al���yoruz.
                if (m_CurrentWorkItemsCount > m_WorkerThreads.Count)
                {
                    TryStartNewThreads(1);
                }
            }
        }

        /// <summary>
        /// ���i thread havuzunu sonland�r�r.
        /// </summary>
        /// <param name="waitForWorkItems">
        /// E�er <c>true</c> de�eri verilirse sonland�rmak i�in i� par�as� kuyru�unda bekleyen i� par�alar�n�n 
        /// bitmesini bekler. <c>false</c> de�eri verilirse i�lerin bitmesini beklemeden threadleri sonland�r�r.
        /// </param>
        /// <param name="millisecondsTimeoutForEachWorkerThread">���i thread ba��na beklenecek milisaniye cinsinden zaman a��m� s�resi.</param>
        public void Shutdown(bool waitForWorkItems = true, int millisecondsTimeoutForEachWorkerThread = 0)
        {
            Thread[] threads;
            
            // ���i thread listesi �zerinde i�lem yap�yoruz, dolay�s�yla ba�ka thread'lerin listeyi
            // de�i�tirmesini engellemek i�in listeyi kilitliyoruz.
            lock (m_WorkerThreads)
            {
                // Havuzu kapat�ld� olarak i�aretliyoruz
                m_IsShuttingdown = true;

                // �� par�as� kuyru�unu yeni eklemelere kapat�yoruz ve kuyruk i�in bekleyen Thread'leri durduruyoruz.
                m_WorkItemQueue.ShutDown();

                OnWorkerThreadPoolShuttingdown(new WorkerThreadPoolShuttingdownEventArgs(waitForWorkItems, m_WorkItemQueue.Count, m_WorkerThreads.Count));

                threads = new Thread[m_WorkerThreads.Count];

                for (int i = 0; i < m_WorkerThreads.Count; i++)
                {
                    WorkerThread workerThread = m_WorkerThreads[i];

                    // �� par�as� kuyru�u bo�ald��� anda thread'in sonlanmas�n� s�yl�yoruz.
                    workerThread.StopWhenWorkItemQueueIsEmpty(true);
                    
                    threads[i] = workerThread.Thread;
                }

                m_WorkerThreads.Clear();
            }

            // E�er i� par�alar�n� beklememiz gerekiyorsa thread'ler i�lerini bitirene kadar bekliyoruz.
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

            // Art�k thread'leri sonland�rabiliriz.
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
        /// Minimum - maksimum i��i thread say�s� ve i� par�as� kuyru�unda bekleyen i� par�as� 
        /// de�erlerine bakarak i��i thread'in sonland�r�l�p sonland�r�lmayaca��n� s�yler.
        /// </summary>
        /// <param name="workerThread">���i thread.</param>
        /// <param name="hasNoWorkItemsInQueue"></param>
        /// <returns>
        /// E�er <c>true</c> d�nerse i��i thread, i��i thread listesinden ��kart�lm�� 
        /// demektir ve metodu �a��ran ki�i i��i thread'i sonland�rmak zorund�r. 
        /// <c>false</c> ise tersi.
        /// </returns>
        public bool ShouldExitWorkerThread(WorkerThread workerThread, bool hasNoWorkItemsInQueue)
        {
            // ���i thread listesi �zerinde i�lem yap�yoruz, dolay�s�yla ba�ka thread'lerin listeyi
            // de�i�tirmesini engellemek i�in listeyi kilitliyoruz.
            lock (m_WorkerThreads)
            {
                // E�er havuz kapat�l�yor olarak i�aretlendi ise i��i thread'i hi� d���nmeden sonland�rmam�z gerekiyor.
                if (m_IsShuttingdown)
                {
                    m_WorkerThreads.Remove(workerThread);

                    return true;
                }

                // E�er kuyrukta bekleyen i� par�as� var ise kriterimiz maksimum thread say�s�, yoksa minimum thread say�s�.
                // ��nk� bo�ta bekleyecek thread say�s�n� belirleyen fakt�r minimum thread say�s�, me�gul olacak thread say�s�n� 
                // belirleyen fakt�r ise maksimum thread say�s�.
                int threadLimit = hasNoWorkItemsInQueue ? m_MinWorkerThreads : m_MaxWorkerThreads;

                // E�er mevcut i��i thread say�s� limiti a��yor ise thread'i sonland�rmam�z gerekiyor.
                if (m_WorkerThreads.Count > threadLimit)
                {
                    m_WorkerThreads.Remove(workerThread);

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        ///  ���i thread ba�l�yor olay�n� tetikler.
        /// </summary>
        /// <param name="sender">Olay� tetikleyen nesne.</param>
        public void OnWorkerThreadStarting(object sender)
        {
            WorkerThreadStarting(sender, EventArgs.Empty);
        }

        /// <summary>
        /// ���i thread ba�lad� olay�n� tetikler.
        /// </summary>
        /// <param name="sender">Olay� tetikleyen nesne.</param>
        public void OnWorkerThreadStarted(object sender)
        {
            WorkerThreadStarted(sender, EventArgs.Empty);
        }

        /// <summary>
        /// ���i thread sonlan�yor olay�n� tetikler.
        /// </summary>
        /// <param name="sender">Olay� tetikleyen nesne.</param>
        /// <param name="reason">���i thread sonlanma nedeni</param>
        public void OnWorkerThreadExiting(object sender, WorkerThreadExitReason reason)
        {
            WorkerThreadExiting(sender, new WorkerThreadExitingEventArgs(reason));
        }

        /// <summary>
        /// ���i thread i� par�as�n� ba�l�yor olay�n� tetikler.
        /// </summary>
        /// <param name="sender">Olay� tetikleyen nesne.</param>
        /// <param name="e">Olay arg�manlar�.</param>
        public void OnWorkerThreadWorkItemStarting(object sender, WorkerThreadWorkItemStartingEventArgs e)
        {
            WorkerThreadWorkItemStarting(sender, e);
        }

        /// <summary>
        /// ���i thread i� par�as� bitirildi olay�n� tetikler.
        /// </summary>
        /// <param name="sender">Olay� tetikleyen nesne.</param>
        /// <param name="e">Olay arg�manlar�.</param>
        public void OnWorkerThreadWorkItemFinished(object sender, WorkerThreadWorkItemFinishedEventArgs e)
        {
            WorkerThreadWorkItemFinished(sender, e);

            DecrementWorkItemsCount();
        }

        /// <summary>
        /// ���i thread i� par�as� hata ald� olay�n� tetikler.
        /// </summary>
        /// <param name="sender">Olay� tetikleyen nesne.</param>
        /// <param name="e">Olay arg�manlar�.</param>
        public void OnWorkerThreadWorkItemException(object sender, WorkerThreadWorkItemExceptionEventArgs e)
        {
            WorkerThreadWorkItemException(sender, e);
        }

        /// <summary>
        /// ���i thread hata ald� olay�n� tetikler.
        /// </summary>
        /// <param name="sender">Olay� tetikleyen nesne.</param>
        /// <param name="e">Olay arg�manlar�.</param>
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
                throw new ArgumentOutOfRangeException("minWorkerThreads", "Minimum i��i thread say�s� 0'dan k���k olamaz.");
            }

            if (minWorkerThreads > maxWorkerThreads)
            {
                throw new ArgumentOutOfRangeException("minWorkerThreads", "Minimum i��i thread say�s� maksimum i��i thread say�s�ndan b�y�k olamaz.");
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
                throw new ArgumentOutOfRangeException("maxWorkerThreads", "Maksimum i��i thread say�s� 0'dan b�y�k olmal�d�r.");
            }

            if (maxWorkerThreads < minWorkerThreads)
            {
                throw new ArgumentOutOfRangeException("maxWorkerThreads", "Maksimum i��i thread say�s� minimum i��i thread say�s�ndan k���k olamaz.");
            }
        }

        /// <summary>
        /// En uygun say�da i��i thread'i ba�lat�r.
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
        /// Maksimum i��i thread say�s�n� ge�meyecek �ekilde yeni i��i thread ba�lat�r.
        /// E�er havuz kapat�l�yor olarak i�aretlendi ise hi� i��i thread ba�latmaz.
        /// </summary>
        /// <param name="threadsCount">Ba�lat�lacak i��i thread say�s�.</param>
        private void TryStartNewThreads(int threadsCount)
        {
            OnWorkerThreadPoolTryingToStartNewWorkerThreads(new WorkerThreadPoolTryingToStartNewWorkerThreadsEventArgs(threadsCount));

            // ���i thread listesi �zerinde i�lem yap�yoruz, dolay�s�yla ba�ka thread'lerin listeyi
            // de�i�tirmesini engellemek i�in listeyi kilitliyoruz.
            lock (m_WorkerThreads)
            {
                // E�er havuz kapat�l�yor olarak i�aretlendi ise metoddan ��k�yoruz.
                if (m_IsShuttingdown)
                {
                    return;
                }

                for (int i = 0; i < threadsCount; i++)
                {
                    // E�er mevcut i��i thread say�s� maksimum i��i thread say�s�ndan fazla oldu ise metoddan ��k�yoruz.
                    if (m_WorkerThreads.Count >= m_MaxWorkerThreads)
                    {
                        return;
                    }

                    // Havuz numaras� ve thread numaras�na g�re bir thread ismi belirliyoruz.
                    string threadName = string.Format(CultureInfo.CurrentCulture, "Labo WTP #{0} Thread #{1}", s_WorkerThreadPoolIdCounter, ++m_WorkerThreadIdCounter);

                    WorkerThread workerThread = new WorkerThread(this, m_WorkItemQueue, threadName, false);

                    // Yaratt���m�z i��i thread'i i��i thread listesine ekliyoruz.
                    m_WorkerThreads.Add(workerThread);

                    OnWorkerThreadPoolStartingNewWorkerThread(new WorkerThreadPoolStartingNewWorkerThreadEventArgs(m_WorkerThreads.Count, workerThread));

                    // ���i thread'i hemen ba�lat�yoruz.
                    workerThread.Start();
                }
            }
        }

        /// <summary>
        /// ���i thread havuzu numara sayac�n� s�f�rlar.
        /// </summary>
        internal static void ResetWorkerThreadPoolIdCounter()
        {
            s_WorkerThreadPoolIdCounter = 0;
        }

        internal static int CalculateOptimumThreadsCount(int workItemsCount, int minWorkerThreads, int maxWorkerThreads)
        {
            // Kuyrukta bekleyen ve i�lenen i� par�as� say�s� toplam� ve minimum i��i thread say�s�n�n maksimumunu al�yoruz.
            int threadsCount = Math.Max(workItemsCount, minWorkerThreads);

            // Se�ti�imiz thread say�s� ve maksimum thread say�s�n�n minimumunu al�yoruz.
            // ��nki maksimum thread say�s�n� ge�mememiz gerekiyor.
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