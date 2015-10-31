namespace Labo.Threading.EventArg
{
    using System;

    /// <summary>
    /// ���i Thread havuzu yeni bir i��i Thread'leri ba�latmay� deniyor olay arg�manlar� s�n�f�.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadPoolTryingToStartNewWorkerThreadsEventArgs : EventArgs
    {
        private readonly int m_ThreadsCount;

        /// <summary>
        /// ���i thread say�s�.
        /// </summary>
        public int ThreadsCount
        {
            get
            {
                return m_ThreadsCount;
            }
        }

        /// <summary>
        /// ���i Thread havuzu yeni bir i��i Thread'leri ba�latmay� deniyor olay arg�manlar� s�n�f� in�ac� metodu.
        /// </summary>
        /// <param name="threadsCount">���i thread say�s�.</param>
        public WorkerThreadPoolTryingToStartNewWorkerThreadsEventArgs(int threadsCount)
        {
            m_ThreadsCount = threadsCount;
        }
    }
}