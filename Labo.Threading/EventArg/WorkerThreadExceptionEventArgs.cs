namespace Labo.Threading.EventArg
{
    using System;

    /// <summary>
    /// ���i Thread hata ald� olay arg�manlar� s�n�f�.
    /// </summary>
    [Serializable]
    public sealed class WorkerThreadExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// �� par�as� hatas�n� getirir.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// ���i Thread hata ald� olay arg�manlar� in�ac� metodu.
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