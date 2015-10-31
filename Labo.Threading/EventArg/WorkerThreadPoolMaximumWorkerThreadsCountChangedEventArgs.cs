namespace Labo.Threading.EventArg
{
    using System;

    /// <summary>
    /// ���i Thread havuzu maximum i��i thread say�s� de�i�ti olay arg�manlar� s�n�f�.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadPoolMaximumWorkerThreadsCountChangedEventArgs : EventArgs
    {
        private readonly int m_OldCount;

        private readonly int m_NewCount;

        /// <summary>
        /// Eski maximum i��i thread say�s�.
        /// </summary>
        public int OldCount
        {
            get
            {
                return m_OldCount;
            }
        }

        /// <summary>
        /// Yeni maximum i��i thread say�s�.
        /// </summary>
        public int NewCount
        {
            get
            {
                return m_NewCount;
            }
        }

        /// <summary>
        /// ���i Thread havuzu maximum i��i thread say�s� de�i�ti olay arg�manlar� s�n�f� in�ac� metodu. 
        /// </summary>
        /// <param name="oldCount">Eski maximum i��i thread say�s�.</param>
        /// <param name="newCount">Yeni maximum i��i thread say�s�.</param>
        public WorkerThreadPoolMaximumWorkerThreadsCountChangedEventArgs(int oldCount, int newCount)
        {
            m_OldCount = oldCount;
            m_NewCount = newCount;
        }
    }
}