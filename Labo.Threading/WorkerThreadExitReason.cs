namespace Labo.Threading
{
    /// <summary>
    /// İşçi Thread'i sonlanma nedeni.
    /// </summary>
    public enum WorkerThreadExitReason
    {
        /// <summary>
        /// Mevcut Thread sayısı maksimum thread sayısını aştığı için.
        /// </summary>
        MaximumThreadCountExceeded = 0,

        /// <summary>
        /// İş parçası kuyruğu boş ve mevcut thread sayısı minimun thread sayısından fazla olduğu için.
        /// </summary>
        WorkItemQueueIsEmpty,

        /// <summary>
        /// İşçi Thread'i iş parçası kuyruğu boşaldı ise çıkması için zorlandığı için.
        /// </summary>
        StopWhenWorkItemQueueIsEmptyIsTrue,

        /// <summary>
        /// İşçi Thread'in durur metodu çağırıldığı için.
        /// </summary>
        StopMethodIsCalled,

        /// <summary>
        /// İşçi Thread çalıştırılırken hata alındığı için.
        /// </summary>
        ExceptionOccurred,

        /// <summary>
        /// Thread sonlandırıldığı için
        /// </summary>
        ThreadAborted
    }
}
