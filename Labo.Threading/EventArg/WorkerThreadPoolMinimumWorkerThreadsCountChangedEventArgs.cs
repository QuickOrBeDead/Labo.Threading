namespace Labo.Threading.EventArg
{
    using System;

    /// <summary>
    /// ���i Thread havuzu minimum i��i thread say�s� de�i�ti olay arg�manlar� s�n�f�.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadPoolMinimumWorkerThreadsCountChangedEventArgs : EventArgs
    {
        private readonly int m_OldCount;

        private readonly int m_NewCount;

        /// <summary>
        /// Eski minimum i��i thread say�s�.
        /// </summary>
        public int OldCount
        {
            get
            {
                return m_OldCount;
            }
        }

        /// <summary>
        /// Yeni minimum i��i thread say�s�.
        /// </summary>
        public int NewCount
        {
            get
            {
                return m_NewCount;
            }
        }
        
        /// <summary>
        /// ���i Thread havuzu minimum i��i thread say�s� de�i�ti olay arg�manlar� s�n�f� in�ac� metodu. 
        /// </summary>
        /// <param name="oldCount">Eski minimum i��i thread say�s�.</param>
        /// <param name="newCount">Yeni minimum i��i thread say�s�.</param>
        public WorkerThreadPoolMinimumWorkerThreadsCountChangedEventArgs(int oldCount, int newCount)
        {
            m_OldCount = oldCount;
            m_NewCount = newCount;
        }
    }
}