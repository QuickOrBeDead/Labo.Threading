namespace Labo.Threading.EventArg
{
    using System;

    /// <summary>
    /// Ýþçi Thread havuzu maximum iþçi thread sayýsý deðiþti olay argümanlarý sýnýfý.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadPoolMaximumWorkerThreadsCountChangedEventArgs : EventArgs
    {
        private readonly int m_OldCount;

        private readonly int m_NewCount;

        /// <summary>
        /// Eski maximum iþçi thread sayýsý.
        /// </summary>
        public int OldCount
        {
            get
            {
                return m_OldCount;
            }
        }

        /// <summary>
        /// Yeni maximum iþçi thread sayýsý.
        /// </summary>
        public int NewCount
        {
            get
            {
                return m_NewCount;
            }
        }

        /// <summary>
        /// Ýþçi Thread havuzu maximum iþçi thread sayýsý deðiþti olay argümanlarý sýnýfý inþacý metodu. 
        /// </summary>
        /// <param name="oldCount">Eski maximum iþçi thread sayýsý.</param>
        /// <param name="newCount">Yeni maximum iþçi thread sayýsý.</param>
        public WorkerThreadPoolMaximumWorkerThreadsCountChangedEventArgs(int oldCount, int newCount)
        {
            m_OldCount = oldCount;
            m_NewCount = newCount;
        }
    }
}