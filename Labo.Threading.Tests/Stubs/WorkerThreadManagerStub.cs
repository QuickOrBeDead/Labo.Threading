namespace Labo.Threading.Tests.Stubs
{
    using System;

    using Labo.Threading.EventArg;

    using NSubstitute;

    public abstract class WorkerThreadManagerStub : IWorkerThreadManager
    {
        /// <summary>
        /// İşçi thread başlarken tetiklenen olay.
        /// </summary>
        public event EventHandler<EventArgs> WorkerThreadStarting = delegate { };

        /// <summary>
        /// İşçi thread başladığında tetiklenen olay.
        /// </summary>
        public event EventHandler<EventArgs> WorkerThreadStarted = delegate { };

        /// <summary>
        /// İşçi thread sonlanırken tetiklenen olay.
        /// </summary>
        public event EventHandler<WorkerThreadExitingEventArgs> WorkerThreadExiting = delegate { };

        /// <summary>
        /// İşçi thread hata aldığında tetiklenen olay.
        /// </summary>
        public event EventHandler<WorkerThreadExceptionEventArgs> WorkerThreadException = delegate { };

        /// <summary>
        /// İşçi thread iş parçasını çalıştırmadan önce tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadWorkItemStartingEventArgs> WorkerThreadWorkItemStarting = delegate { };

        /// <summary>
        /// İşçi thread iş parçasını çalıştırdıktan sonra tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadWorkItemFinishedEventArgs> WorkerThreadWorkItemFinished = delegate { };

        /// <summary>
        /// İşçi thread iş parçası hata aldıktan sonra tetiklenir.
        /// </summary>
        public event EventHandler<WorkerThreadWorkItemExceptionEventArgs> WorkerThreadWorkItemException = delegate { };

        /// <summary>
        ///  İşçi thread başlıyor olayını tetikler.
        /// </summary>
        /// <param name="sender">Olayı tetikleyen nesne.</param>
        public void OnWorkerThreadStarting(object sender)
        {
            WorkerThreadStarting(sender, EventArgs.Empty);
        }

        /// <summary>
        /// İşçi thread başladı olayını tetikler.
        /// </summary>
        /// <param name="sender">Olayı tetikleyen nesne.</param>
        public void OnWorkerThreadStarted(object sender)
        {
            WorkerThreadStarted(sender, EventArgs.Empty);
        }

        /// <summary>
        /// İşçi thread sonlanıyor olayını tetikler.
        /// </summary>
        /// <param name="sender">Olayı tetikleyen nesne.</param>
        /// <param name="reason">İşçi thread sonlanma nedeni</param>
        public void OnWorkerThreadExiting(object sender, WorkerThreadExitReason reason)
        {
            WorkerThreadExiting(sender, new WorkerThreadExitingEventArgs(reason));
        }

        /// <summary>
        /// İşçi thread iş parçasını başlıyor olayını tetikler.
        /// </summary>
        /// <param name="sender">Olayı tetikleyen nesne.</param>
        /// <param name="e">Olay argümanları.</param>
        public void OnWorkerThreadWorkItemStarting(object sender, WorkerThreadWorkItemStartingEventArgs e)
        {
            WorkerThreadWorkItemStarting(sender, e);
        }

        /// <summary>
        /// İşçi thread iş parçası bitirildi olayını tetikler.
        /// </summary>
        /// <param name="sender">Olayı tetikleyen nesne.</param>
        /// <param name="e">Olay argümanları.</param>
        public void OnWorkerThreadWorkItemFinished(object sender, WorkerThreadWorkItemFinishedEventArgs e)
        {
            WorkerThreadWorkItemFinished(sender, e);
        }

        /// <summary>
        /// İşçi thread iş parçası hata aldı olayını tetikler.
        /// </summary>
        /// <param name="sender">Olayı tetikleyen nesne.</param>
        /// <param name="e">Olay argümanları.</param>
        public void OnWorkerThreadWorkItemException(object sender, WorkerThreadWorkItemExceptionEventArgs e)
        {
            WorkerThreadWorkItemException(sender, e);
        }

        /// <summary>
        /// İşçi thread hata aldı olayını tetikler.
        /// </summary>
        /// <param name="sender">Olayı tetikleyen nesne.</param>
        /// <param name="e">Olay argümanları.</param>
        public void OnWorkerThreadException(object sender, WorkerThreadExceptionEventArgs e)
        {
            WorkerThreadException(sender, e);
        }

        public abstract bool ShouldExitWorkerThread(WorkerThread workerThread, bool hasNoWorkItemsInQueue);

        internal static WorkerThreadManagerStub Create()
        {
            return Substitute.ForPartsOf<WorkerThreadManagerStub>();
        }
    }
}
