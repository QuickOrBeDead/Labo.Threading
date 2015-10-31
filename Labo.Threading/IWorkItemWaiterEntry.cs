namespace Labo.Threading
{
    using System;
    using System.Threading;

    /// <summary>
    /// �� par�as� bekleyici kayd� aray�z�.
    /// </summary>
    internal interface IWorkItemWaiterEntry : IDisposable
    {
        /// <summary>
        /// Bekleyen Thread'in i�leyece�i i� par�as� nesnesi.
        /// </summary>
        IWorkItem WorkItem { get; }

        /// <summary>
        /// Bekleme Thread'e sinyal yolland���n� ve i� par�as� nesnesinin atand���n� g�sterir.
        /// </summary>
        bool IsSignaled { get; }

        /// <summary>
        /// Bekleme kayd�n�n zaman a��m�na u�rad���n� g�sterir.
        /// </summary>
        bool IsTimedOut { get; }

        /// <summary>
        /// Mevcut Thread'in ismi.
        /// </summary>
        string CurrentThreadName { get; }

        /// <summary>
        /// E�er bekleme kayd� zaman a��m�na u�ramad�ysa, i� kayd� nesnesini al�r ve bekleyen Thread'i tetikler.
        /// </summary>
        /// <returns>E�er ba�ar�l� olursa true d�ner.</returns>
        bool TrySignal(IWorkItem workItem);

        /// <summary>
        /// E�er bekleyen Thread i� kayd�n� al�p i�lemeye ba�lamad� ise i� kayd�n� zaman a��m�na u�rat�r.
        /// </summary>
        /// <returns>E�er ba�ar�l� olursa true d�ner.</returns>
        bool TrySetToTimedOut();

        /// <summary>
        /// Thread'in zaman a��m� i�in kullan�lacak wait handle.
        /// </summary>
        WaitHandle WaitHandle { get; }

        /// <summary>
        /// Mevcut Thread'i sinyal alana kadar, zaman a��m� s�resi boyunca bloklar.
        /// </summary>
        /// <param name="millisecondsTimeout">Milisaniye cinsinden zaman a��m� de�eri</param>
        /// <returns><c>true</c> e�er mevcut Thread sinyal ald�ysa; yoksa, <c>false</c>.</returns>
        bool Wait(int millisecondsTimeout);

        /// <summary>
        /// Bekleme kayd�n� s�f�rlar.
        /// </summary>
        void Reset();
    }
}