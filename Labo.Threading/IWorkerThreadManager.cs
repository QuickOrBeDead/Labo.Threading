namespace Labo.Threading
{
    using Labo.Threading.EventArg;

    /// <summary>
    /// Ýþçi thread yönetici arayüzü.
    /// </summary>
    internal interface IWorkerThreadManager
    {
        /// <summary>
        /// Minimum - maksimum iþçi thread sayýsý ve iþ parçasý kuyruðunda bekleyen iþ parçasý 
        /// deðerlerine bakarak iþçi thread'in sonlandýrýlýp sonlandýrýlmayacaðýný söyler.
        /// </summary>
        /// <param name="workerThread">Ýþçi thread.</param>
        /// <param name="hasNoWorkItemsInQueue"></param>
        /// <returns>
        /// Eðer <c>true</c> dönerse iþçi thread, iþçi thread listesinden çýkartýlmýþ 
        /// demektir ve metodu çaðýran kiþi iþçi thread'i sonlandýrmak zorundýr. 
        /// <c>false</c> ise tersi.
        /// </returns>
        bool ShouldExitWorkerThread(WorkerThread workerThread, bool hasNoWorkItemsInQueue);

        /// <summary>
        ///  Ýþçi thread baþlýyor olayýný tetikler.
        /// </summary>
        /// <param name="sender">Olayý tetikleyen nesne.</param>
        void OnWorkerThreadStarting(object sender);

        /// <summary>
        /// Ýþçi thread baþladý olayýný tetikler.
        /// </summary>
        /// <param name="sender">Olayý tetikleyen nesne.</param>
        void OnWorkerThreadStarted(object sender);

        /// <summary>
        /// Ýþçi thread sonlanýyor olayýný tetikler.
        /// </summary>
        /// <param name="sender">Olayý tetikleyen nesne.</param>
        /// <param name="reason">Ýþçi thread sonlanma nedeni</param>
        void OnWorkerThreadExiting(object sender, WorkerThreadExitReason reason);

        /// <summary>
        /// Ýþçi thread iþ parçasýný baþlýyor olayýný tetikler.
        /// </summary>
        /// <param name="sender">Olayý tetikleyen nesne.</param>
        /// <param name="e">Olay argümanlarý.</param>
        void OnWorkerThreadWorkItemStarting(object sender, WorkerThreadWorkItemStartingEventArgs e);

        /// <summary>
        /// Ýþçi thread iþ parçasý bitirildi olayýný tetikler.
        /// </summary>
        /// <param name="sender">Olayý tetikleyen nesne.</param>
        /// <param name="e">Olay argümanlarý.</param>
        void OnWorkerThreadWorkItemFinished(object sender, WorkerThreadWorkItemFinishedEventArgs e);

        /// <summary>
        /// Ýþçi thread iþ parçasý hata aldý olayýný tetikler.
        /// </summary>
        /// <param name="sender">Olayý tetikleyen nesne.</param>
        /// <param name="e">Olay argümanlarý.</param>
        void OnWorkerThreadWorkItemException(object sender, WorkerThreadWorkItemExceptionEventArgs e);

        /// <summary>
        /// Ýþçi thread hata aldý olayýný tetikler.
        /// </summary>
        /// <param name="sender">Olayý tetikleyen nesne.</param>
        /// <param name="e">Olay argümanlarý.</param>
        void OnWorkerThreadException(object sender, WorkerThreadExceptionEventArgs e);
    }
}