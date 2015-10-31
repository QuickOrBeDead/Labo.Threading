namespace Labo.Threading
{
    using System;

    /// <summary>
    /// Ýþ parçasý kuyruðu arayüzü.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public interface IWorkItemQueue : IDisposable
    {
        /// <summary>
        /// Kuyruk sayýsýný getirir.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Ýþ parçasýný kuyruða atar.
        /// </summary>
        /// <param name="workItem">Ýþ parçasý.</param>
        void Enqueue(IWorkItem workItem);

        /// <summary>
        /// Kuyrukta bekleyip de sýrasý gelen iþ parçasýný döndürür.
        /// Eðer kuyrukta bir iþ parçasý yok ise null döner.
        /// </summary>
        /// <returns>Ýþ parçasý.</returns>
        IWorkItem Dequeue();

        /// <summary>
        /// Ýþ parçasý kuyruðunda bekleyen Thread'leri durdurur ve kuyruða yeni iþ parçasý eklemeyi kapatýr.
        /// </summary>
        void ShutDown();
    }
}