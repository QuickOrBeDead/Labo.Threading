namespace Labo.Threading
{
    using System;
    using System.Threading;

    /// <summary>
    /// �� par�as� bekleyici kayd�.
    /// </summary>
    internal sealed class WorkItemWaiterEntry : IWorkItemWaiterEntry
    {
        /// <summary>
        /// Bekleyen Thread'i tetikleyecek s�n�f.
        /// </summary>
        private AutoResetEvent m_AutoResetEvent;

        /// <summary>
        /// Bekleyen Thread'in i�leyece�i i� par�as� nesnesi.
        /// </summary>
        private IWorkItem m_WorkItem;

        /// <summary>
        /// Bekleme Thread'e sinyal yolland���n� ve i� par�as� nesnesinin atand���n� g�sterir.
        /// </summary>
        private bool m_IsSignaled;

        /// <summary>
        /// Bekleme kayd�n�n zaman a��m�na u�rad���n� g�sterir.
        /// </summary>
        private bool m_IsTimedOut;

        private bool m_Disposed;

        /// <summary>
        /// Mevcut Thread'in ismi.
        /// </summary>
        private readonly string m_CurrentThreadName;

        /// <summary>
        /// Bekleyen Thread'in i�leyece�i i� par�as� nesnesi.
        /// </summary>
        public IWorkItem WorkItem
        {
            get
            {
                return m_WorkItem;
            }
        }

        /// <summary>
        /// Bekleme Thread'e sinyal yolland���n� ve i� par�as� nesnesinin atand���n� g�sterir.
        /// </summary>
        public bool IsSignaled
        {
            get
            {
                return m_IsSignaled;
            }
        }

        /// <summary>
        /// Bekleme kayd�n�n zaman a��m�na u�rad���n� g�sterir.
        /// </summary>
        public bool IsTimedOut
        {
            get
            {
                return m_IsTimedOut;
            }
        }

        /// <summary>
        /// Mevcut Thread'in ismi.
        /// </summary>
        public string CurrentThreadName
        {
            get
            {
                return m_CurrentThreadName;
            }
        }

        /// <summary>
        /// Thread'in zaman a��m� i�in kullan�lacak wait handle.
        /// </summary>
        public WaitHandle WaitHandle
        {
            get { return m_AutoResetEvent; }
        }

        /// <summary>
        /// �� par�as� bekleyici in�ac� metodu.
        /// </summary>
        public WorkItemWaiterEntry()
        {
            m_CurrentThreadName = Thread.CurrentThread.Name;
            m_AutoResetEvent = new AutoResetEvent(false);
        }

        /// <summary>
        ///  �� par�as� bekleyici imha metodu.
        /// </summary>
        ~WorkItemWaiterEntry()
        {
            Dispose(false);
        }

        /// <summary>
        /// E�er bekleme kayd� zaman a��m�na u�ramad�ysa, i� kayd� nesnesini al�r ve bekleyen Thread'i tetikler.
        /// </summary>
        /// <returns>E�er ba�ar�l� olursa true d�ner.</returns>
        public bool TrySignal(IWorkItem workItem)
        {
            lock (this)
            {
                if (!m_IsTimedOut)
                {
                    m_IsSignaled = true;
                    m_WorkItem = workItem;
                    m_AutoResetEvent.Set();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// E�er bekleyen Thread i� kayd�n� al�p i�lemeye ba�lamad� ise i� kayd�n� zaman a��m�na u�rat�r.
        /// </summary>
        /// <returns>E�er ba�ar�l� olursa true d�ner.</returns>
        public bool TrySetToTimedOut()
        {
            lock (this)
            {
                if (!m_IsSignaled)
                {
                    m_IsTimedOut = true;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Mevcut Thread'i sinyal alana kadar, zaman a��m� s�resi boyunca bloklar.
        /// </summary>
        /// <param name="millisecondsTimeout">Milisaniye cinsinden zaman a��m� de�eri</param>
        /// <returns><c>true</c> e�er mevcut Thread sinyal ald�ysa; yoksa, <c>false</c>.</returns>
        public bool Wait(int millisecondsTimeout)
        {
            lock (this)
            {
                return m_AutoResetEvent.WaitOne(millisecondsTimeout);                
            }
        }

        /// <summary>
        /// Bekleme kayd�n� s�f�rlar.
        /// </summary>
        public void Reset()
        {
            m_WorkItem = null;
            m_IsTimedOut = false;
            m_IsSignaled = false;
            m_AutoResetEvent.Reset();
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
                if (m_AutoResetEvent != null)
                {
                    m_AutoResetEvent.Close();
                    m_AutoResetEvent = null;
                }

                m_Disposed = true;
            }
        }
    }
}