namespace Labo.Threading
{
    /// <summary>
    /// ���i thread havuzu aray�z�.
    /// </summary>
    public interface IWorkerThreadPool
    {
        /// <summary>
        /// �� par�as�n� kuyru�a sokar.
        /// </summary>
        /// <param name="workItem">�� par�as�.</param>
        void QueueWorkItem(IWorkItem workItem);

        /// <summary>
        /// Havuzdaki i��i thread say�s�n�n en fazla olabilece�i de�er.
        /// </summary>
        int MaxWorkerThreads { get; }

        /// <summary>
        /// Havuzdaki i��i thread say�s�n�n en az olabilece�i de�er.
        /// </summary>
        int MinWorkerThreads { get; }

        /// <summary>
        /// Mevcut i��i thread say�s�.
        /// </summary>
        int WorkerThreadsCount { get; }

        /// <summary>
        /// �� par�as� kuyru�unda bekleyen i� par�as� say�s�.
        /// </summary>
        int WorkItemQueueCount { get; }
    }
}