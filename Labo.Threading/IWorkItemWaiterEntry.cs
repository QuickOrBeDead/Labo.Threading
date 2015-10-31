namespace Labo.Threading
{
    using System;
    using System.Threading;

    /// <summary>
    /// Ýþ parçasý bekleyici kaydý arayüzü.
    /// </summary>
    internal interface IWorkItemWaiterEntry : IDisposable
    {
        /// <summary>
        /// Bekleyen Thread'in iþleyeceði iþ parçasý nesnesi.
        /// </summary>
        IWorkItem WorkItem { get; }

        /// <summary>
        /// Bekleme Thread'e sinyal yollandýðýný ve iþ parçasý nesnesinin atandýðýný gösterir.
        /// </summary>
        bool IsSignaled { get; }

        /// <summary>
        /// Bekleme kaydýnýn zaman aþýmýna uðradýðýný gösterir.
        /// </summary>
        bool IsTimedOut { get; }

        /// <summary>
        /// Mevcut Thread'in ismi.
        /// </summary>
        string CurrentThreadName { get; }

        /// <summary>
        /// Eðer bekleme kaydý zaman aþýmýna uðramadýysa, iþ kaydý nesnesini alýr ve bekleyen Thread'i tetikler.
        /// </summary>
        /// <returns>Eðer baþarýlý olursa true döner.</returns>
        bool TrySignal(IWorkItem workItem);

        /// <summary>
        /// Eðer bekleyen Thread iþ kaydýný alýp iþlemeye baþlamadý ise iþ kaydýný zaman aþýmýna uðratýr.
        /// </summary>
        /// <returns>Eðer baþarýlý olursa true döner.</returns>
        bool TrySetToTimedOut();

        /// <summary>
        /// Thread'in zaman aþýmý için kullanýlacak wait handle.
        /// </summary>
        WaitHandle WaitHandle { get; }

        /// <summary>
        /// Mevcut Thread'i sinyal alana kadar, zaman aþýmý süresi boyunca bloklar.
        /// </summary>
        /// <param name="millisecondsTimeout">Milisaniye cinsinden zaman aþýmý deðeri</param>
        /// <returns><c>true</c> eðer mevcut Thread sinyal aldýysa; yoksa, <c>false</c>.</returns>
        bool Wait(int millisecondsTimeout);

        /// <summary>
        /// Bekleme kaydýný sýfýrlar.
        /// </summary>
        void Reset();
    }
}