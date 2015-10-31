namespace Labo.Threading
{
    using System;

    /// <summary>
    /// �� par�as� aray�z�.
    /// </summary>
    public interface IWorkItem
    {
        /// <summary>
        /// �� par�as� durumu.
        /// </summary>
        WorkItemState State { get; set; }

        /// <summary>
        /// �� par�as�n�n ald��� son hata.
        /// </summary>
        Exception LastException { get; set; }

        /// <summary>
        /// �� par�as� yarat�lma zaman�.
        /// </summary>
        DateTime CreateDateUtc { get; }

        /// <summary>
        /// �� par�as� ba�lang�� zaman�.
        /// </summary>
        DateTime? StartDateUtc { get; }

        /// <summary>
        /// �� par�as� biti� zaman�.
        /// </summary>
        DateTime? EndDateUtc { get; }

        /// <summary>
        /// �� par�as�n�n �al��ma s�resi.
        /// </summary>
        TimeSpan ExecutionTime { get; }

        /// <summary>
        /// �� par�as�n� �al��t�r�r.
        /// </summary>
        void DoWork();

        /// <summary>
        /// �� par�as�n� durdurur.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Stop")]
        void Stop();
    }
}