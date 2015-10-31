namespace Labo.Threading
{
    /// <summary>
    /// Ýþ parçasý durumu.
    /// </summary>
    public enum WorkItemState
    {
        /// <summary>
        /// Yaratýldý.
        /// </summary>
        Created = 0,

        /// <summary>
        /// Planlandý.
        /// </summary>
        Scheduled,

        /// <summary>
        /// Kuyrukta bekliyor.
        /// </summary>
        Queued,

        /// <summary>
        /// Durduruldu.
        /// </summary>
        Stopped,

        /// <summary>
        /// Çalýþýyor.
        /// </summary>
        Running,

        /// <summary>
        /// Hata aldý.
        /// </summary>
        Failing,

        /// <summary>
        /// Hata alarak bitirildi.
        /// </summary>
        CompletedWithException,

        /// <summary>
        /// Bitirildi.
        /// </summary>
        Completed
    }
}