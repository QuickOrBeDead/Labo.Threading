namespace Labo.Threading
{
    using System;

    /// <summary>
    /// �� par�as� kuyru�u aray�z�.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public interface IWorkItemQueue : IDisposable
    {
        /// <summary>
        /// Kuyruk say�s�n� getirir.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// �� par�as�n� kuyru�a atar.
        /// </summary>
        /// <param name="workItem">�� par�as�.</param>
        void Enqueue(IWorkItem workItem);

        /// <summary>
        /// Kuyrukta bekleyip de s�ras� gelen i� par�as�n� d�nd�r�r.
        /// E�er kuyrukta bir i� par�as� yok ise null d�ner.
        /// </summary>
        /// <returns>�� par�as�.</returns>
        IWorkItem Dequeue();

        /// <summary>
        /// �� par�as� kuyru�unda bekleyen Thread'leri durdurur ve kuyru�a yeni i� par�as� eklemeyi kapat�r.
        /// </summary>
        void ShutDown();
    }
}