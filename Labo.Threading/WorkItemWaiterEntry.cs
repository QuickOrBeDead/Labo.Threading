namespace Labo.Threading
{
    using System;
    using System.Threading;

    /// <summary>
    /// Ýþ parçasý bekleyici kaydý.
    /// </summary>
    internal sealed class WorkItemWaiterEntry : IWorkItemWaiterEntry
    {
        /// <summary>
        /// Bekleyen Thread'i tetikleyecek sýnýf.
        /// </summary>
        private AutoResetEvent m_AutoResetEvent;

        /// <summary>
        /// Bekleyen Thread'in iþleyeceði iþ parçasý nesnesi.
        /// </summary>
        private IWorkItem m_WorkItem;

        /// <summary>
        /// Bekleme Thread'e sinyal yollandýðýný ve iþ parçasý nesnesinin atandýðýný gösterir.
        /// </summary>
        private bool m_IsSignaled;

        /// <summary>
        /// Bekleme kaydýnýn zaman aþýmýna uðradýðýný gösterir.
        /// </summary>
        private bool m_IsTimedOut;

        private bool m_Disposed;

        /// <summary>
        /// Mevcut Thread'in ismi.
        /// </summary>
        private readonly string m_CurrentThreadName;

        /// <summary>
        /// Bekleyen Thread'in iþleyeceði iþ parçasý nesnesi.
        /// </summary>
        public IWorkItem WorkItem
        {
            get
            {
                return m_WorkItem;
            }
        }

        /// <summary>
        /// Bekleme Thread'e sinyal yollandýðýný ve iþ parçasý nesnesinin atandýðýný gösterir.
        /// </summary>
        public bool IsSignaled
        {
            get
            {
                return m_IsSignaled;
            }
        }

        /// <summary>
        /// Bekleme kaydýnýn zaman aþýmýna uðradýðýný gösterir.
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
        /// Thread'in zaman aþýmý için kullanýlacak wait handle.
        /// </summary>
        public WaitHandle WaitHandle
        {
            get { return m_AutoResetEvent; }
        }

        /// <summary>
        /// Ýþ parçasý bekleyici inþacý metodu.
        /// </summary>
        public WorkItemWaiterEntry()
        {
            m_CurrentThreadName = Thread.CurrentThread.Name;
            m_AutoResetEvent = new AutoResetEvent(false);
        }

        /// <summary>
        ///  Ýþ parçasý bekleyici imha metodu.
        /// </summary>
        ~WorkItemWaiterEntry()
        {
            Dispose(false);
        }

        /// <summary>
        /// Eðer bekleme kaydý zaman aþýmýna uðramadýysa, iþ kaydý nesnesini alýr ve bekleyen Thread'i tetikler.
        /// </summary>
        /// <returns>Eðer baþarýlý olursa true döner.</returns>
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
        /// Eðer bekleyen Thread iþ kaydýný alýp iþlemeye baþlamadý ise iþ kaydýný zaman aþýmýna uðratýr.
        /// </summary>
        /// <returns>Eðer baþarýlý olursa true döner.</returns>
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
        /// Mevcut Thread'i sinyal alana kadar, zaman aþýmý süresi boyunca bloklar.
        /// </summary>
        /// <param name="millisecondsTimeout">Milisaniye cinsinden zaman aþýmý deðeri</param>
        /// <returns><c>true</c> eðer mevcut Thread sinyal aldýysa; yoksa, <c>false</c>.</returns>
        public bool Wait(int millisecondsTimeout)
        {
            lock (this)
            {
                return m_AutoResetEvent.WaitOne(millisecondsTimeout);                
            }
        }

        /// <summary>
        /// Bekleme kaydýný sýfýrlar.
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