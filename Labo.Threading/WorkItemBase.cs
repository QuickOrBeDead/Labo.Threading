namespace Labo.Threading
{
    using System;

    using Labo.Common.Patterns;

    /// <summary>
    /// Ýþ parçasý taban sýnýfý.
    /// </summary>
    public abstract class WorkItemBase : IWorkItem
    {
        /// <summary>
        /// Tarih / Zaman saðlayýcýsý.
        /// </summary>
        private readonly IDateTimeProvider m_DateTimeProvider;

        /// <summary>
        /// Ýþ parçasý yaratýlma zamaný.
        /// </summary>
        public DateTime CreateDateUtc { get; private set; }

        /// <summary>
        /// Ýþ parçasý baþlangýç zamaný.
        /// </summary>
        public DateTime? StartDateUtc { get; private set; }

        /// <summary>
        /// Ýþ parçasý bitiþ zamaný.
        /// </summary>
        public DateTime? EndDateUtc { get; private set; }

        /// <summary>
        /// Ýþ parçasý durumu.
        /// </summary>
        public WorkItemState State { get; set; }

        /// <summary>
        /// Ýþ parçasýnýn aldýðý son hata.
        /// </summary>
        public Exception LastException { get; set; }

        /// <summary>
        /// Ýþ parçasýnýn çalýþma süresi.
        /// </summary>
        public TimeSpan ExecutionTime
        {
            get
            {
                // Baþlangýç ve bitiþ zamaný varsa birbirinden çýkarýp hesaplýyoruz.
                if (StartDateUtc.HasValue && EndDateUtc.HasValue)
                {
                    return EndDateUtc.Value - StartDateUtc.Value;
                }

                return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Ýþ parçasý taban sýnýfý inþacý metodu.
        /// </summary>
        protected WorkItemBase()
            : this(DateTimeProvider.Current)
        { 
        }

        /// <summary>
        /// Ýþ parçasý taban sýnýfý inþacý metodu.
        /// </summary>
        /// <param name="dateTimeProvider">Tarih / Zaman saðlayýcýsý.</param>
        protected internal WorkItemBase(IDateTimeProvider dateTimeProvider)
        {
            if (dateTimeProvider == null)
            {
                throw new ArgumentNullException("dateTimeProvider");
            }

            m_DateTimeProvider = dateTimeProvider;

            CreateDateUtc = m_DateTimeProvider.UtcNow;
        }

        /// <summary>
        /// Ýþ parçasýný çalýþtýrýr.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void DoWork()
        {
            State = WorkItemState.Running;

            try
            {
                StartDateUtc = m_DateTimeProvider.UtcNow;

                DoWorkInternal();

                EndDateUtc = m_DateTimeProvider.UtcNow;

                State = WorkItemState.Completed;  
            }
            catch (Exception ex)
            {
                EndDateUtc = m_DateTimeProvider.UtcNow;

                LastException = ex;

                State = WorkItemState.CompletedWithException;
            }
        }

        /// <summary>
        /// Ýþ parçasýný durdurur.
        /// </summary>
        public void Stop()
        {
            StopInternal();

            EndDateUtc = m_DateTimeProvider.UtcNow;
            State = WorkItemState.Stopped;
        }

        /// <summary>
        /// Ýþ parçasýný çalýþtýran asýl metod.
        /// </summary>
        protected internal abstract void DoWorkInternal();

        /// <summary>
        /// Ýþ parçasýný durduran asýl metod.
        /// </summary>
        protected internal abstract void StopInternal();
    }
}