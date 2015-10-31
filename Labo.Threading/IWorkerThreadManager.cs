namespace Labo.Threading
{
    using Labo.Threading.EventArg;

    /// <summary>
    /// ���i thread y�netici aray�z�.
    /// </summary>
    internal interface IWorkerThreadManager
    {
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
        bool ShouldExitWorkerThread(WorkerThread workerThread, bool hasNoWorkItemsInQueue);

        /// <summary>
        ///  ���i thread ba�l�yor olay�n� tetikler.
        /// </summary>
        /// <param name="sender">Olay� tetikleyen nesne.</param>
        void OnWorkerThreadStarting(object sender);

        /// <summary>
        /// ���i thread ba�lad� olay�n� tetikler.
        /// </summary>
        /// <param name="sender">Olay� tetikleyen nesne.</param>
        void OnWorkerThreadStarted(object sender);

        /// <summary>
        /// ���i thread sonlan�yor olay�n� tetikler.
        /// </summary>
        /// <param name="sender">Olay� tetikleyen nesne.</param>
        /// <param name="reason">���i thread sonlanma nedeni</param>
        void OnWorkerThreadExiting(object sender, WorkerThreadExitReason reason);

        /// <summary>
        /// ���i thread i� par�as�n� ba�l�yor olay�n� tetikler.
        /// </summary>
        /// <param name="sender">Olay� tetikleyen nesne.</param>
        /// <param name="e">Olay arg�manlar�.</param>
        void OnWorkerThreadWorkItemStarting(object sender, WorkerThreadWorkItemStartingEventArgs e);

        /// <summary>
        /// ���i thread i� par�as� bitirildi olay�n� tetikler.
        /// </summary>
        /// <param name="sender">Olay� tetikleyen nesne.</param>
        /// <param name="e">Olay arg�manlar�.</param>
        void OnWorkerThreadWorkItemFinished(object sender, WorkerThreadWorkItemFinishedEventArgs e);

        /// <summary>
        /// ���i thread i� par�as� hata ald� olay�n� tetikler.
        /// </summary>
        /// <param name="sender">Olay� tetikleyen nesne.</param>
        /// <param name="e">Olay arg�manlar�.</param>
        void OnWorkerThreadWorkItemException(object sender, WorkerThreadWorkItemExceptionEventArgs e);

        /// <summary>
        /// ���i thread hata ald� olay�n� tetikler.
        /// </summary>
        /// <param name="sender">Olay� tetikleyen nesne.</param>
        /// <param name="e">Olay arg�manlar�.</param>
        void OnWorkerThreadException(object sender, WorkerThreadExceptionEventArgs e);
    }
}