namespace Labo.Threading
{
    using System;

    /// <summary>
    /// Ýþ parçasý arayüzü.
    /// </summary>
    public interface IWorkItem
    {
        /// <summary>
        /// Ýþ parçasý durumu.
        /// </summary>
        WorkItemState State { get; set; }

        /// <summary>
        /// Ýþ parçasýnýn aldýðý son hata.
        /// </summary>
        Exception LastException { get; set; }

        /// <summary>
        /// Ýþ parçasý yaratýlma zamaný.
        /// </summary>
        DateTime CreateDateUtc { get; }

        /// <summary>
        /// Ýþ parçasý baþlangýç zamaný.
        /// </summary>
        DateTime? StartDateUtc { get; }

        /// <summary>
        /// Ýþ parçasý bitiþ zamaný.
        /// </summary>
        DateTime? EndDateUtc { get; }

        /// <summary>
        /// Ýþ parçasýnýn çalýþma süresi.
        /// </summary>
        TimeSpan ExecutionTime { get; }

        /// <summary>
        /// Ýþ parçasýný çalýþtýrýr.
        /// </summary>
        void DoWork();

        /// <summary>
        /// Ýþ parçasýný durdurur.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Stop")]
        void Stop();
    }
}