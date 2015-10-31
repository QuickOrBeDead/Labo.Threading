namespace Labo.Threading.EventArg
{
    using System;

    /// <summary>
    /// Ýþçi Thread hata aldý olay argümanlarý sýnýfý.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Ýþ parçasý hatasýný getirir.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Ýþçi Thread hata aldý olay argümanlarý inþacý metodu.
        /// </summary>
        /// <param name="exception">Hata.</param>
        public WorkerThreadExceptionEventArgs(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Exception = exception;
        }
    }
}