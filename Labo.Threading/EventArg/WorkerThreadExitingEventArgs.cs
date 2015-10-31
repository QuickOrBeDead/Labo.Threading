using System;

namespace Labo.Threading.EventArg
{
    /// <summary>
    /// İşçi thread'i çıkış olay argümanları sınıfı.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadExitingEventArgs : EventArgs
    {
        /// <summary>
        /// İşçi thread'i çıkış nedenini getirir.
        /// </summary>
        public WorkerThreadExitReason WorkerThreadExitReason { get; private set; }

        /// <summary>
        /// İşçi thread'i çıkış olay argümanları inşacı metodu.
        /// </summary>
        /// <param name="workerThreadExitReason">İşçi Thread'i çıkış nedeni.</param>
        public WorkerThreadExitingEventArgs(WorkerThreadExitReason workerThreadExitReason)
        {
            WorkerThreadExitReason = workerThreadExitReason;
        }
    }
}
