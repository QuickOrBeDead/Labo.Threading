namespace Labo.Threading
{
    using System;

    using Labo.Common.Patterns;

    /// <summary>
    /// �� par�as� taban s�n�f�.
    /// </summary>
    public abstract class WorkItemBase : IWorkItem
    {
        /// <summary>
        /// Tarih / Zaman sa�lay�c�s�.
        /// </summary>
        private readonly IDateTimeProvider m_DateTimeProvider;

        /// <summary>
        /// �� par�as� yarat�lma zaman�.
        /// </summary>
        public DateTime CreateDateUtc { get; private set; }

        /// <summary>
        /// �� par�as� ba�lang�� zaman�.
        /// </summary>
        public DateTime? StartDateUtc { get; private set; }

        /// <summary>
        /// �� par�as� biti� zaman�.
        /// </summary>
        public DateTime? EndDateUtc { get; private set; }

        /// <summary>
        /// �� par�as� durumu.
        /// </summary>
        public WorkItemState State { get; set; }

        /// <summary>
        /// �� par�as�n�n ald��� son hata.
        /// </summary>
        public Exception LastException { get; set; }

        /// <summary>
        /// �� par�as�n�n �al��ma s�resi.
        /// </summary>
        public TimeSpan ExecutionTime
        {
            get
            {
                // Ba�lang�� ve biti� zaman� varsa birbirinden ��kar�p hesapl�yoruz.
                if (StartDateUtc.HasValue && EndDateUtc.HasValue)
                {
                    return EndDateUtc.Value - StartDateUtc.Value;
                }

                return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// �� par�as� taban s�n�f� in�ac� metodu.
        /// </summary>
        protected WorkItemBase()
            : this(DateTimeProvider.Current)
        { 
        }

        /// <summary>
        /// �� par�as� taban s�n�f� in�ac� metodu.
        /// </summary>
        /// <param name="dateTimeProvider">Tarih / Zaman sa�lay�c�s�.</param>
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
        /// �� par�as�n� �al��t�r�r.
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
        /// �� par�as�n� durdurur.
        /// </summary>
        public void Stop()
        {
            StopInternal();

            EndDateUtc = m_DateTimeProvider.UtcNow;
            State = WorkItemState.Stopped;
        }

        /// <summary>
        /// �� par�as�n� �al��t�ran as�l metod.
        /// </summary>
        protected internal abstract void DoWorkInternal();

        /// <summary>
        /// �� par�as�n� durduran as�l metod.
        /// </summary>
        protected internal abstract void StopInternal();
    }
}