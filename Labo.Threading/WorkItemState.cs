namespace Labo.Threading
{
    /// <summary>
    /// �� par�as� durumu.
    /// </summary>
    public enum WorkItemState
    {
        /// <summary>
        /// Yarat�ld�.
        /// </summary>
        Created = 0,

        /// <summary>
        /// Planland�.
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
        /// �al���yor.
        /// </summary>
        Running,

        /// <summary>
        /// Hata ald�.
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