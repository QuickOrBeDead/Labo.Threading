namespace Labo.Threading.Tests
{
    using System;
    using System.Threading;

    using Labo.Threading.Tests.Stubs;

    using NSubstitute;

    using NUnit.Framework;

    [TestFixture(Category = "WorkerThread")]
    public class WorkerThreadFixture
    {
        [Test]
        public void ConstructorShouldNotStartThread()
        {
            WorkerThread workerThread = new WorkerThread(Substitute.For<IWorkerThreadManager>(), Substitute.For<IWorkItemQueue>(), "Test", false);
            Assert.AreEqual(false, workerThread.Thread.IsAlive);
        }

        [Test, Sequential]
        public void ConstructorShouldSetIsBackgroundPropertyOfThread([Values(true, false)]bool isBackground)
        {
            WorkerThread workerThread = new WorkerThread(Substitute.For<IWorkerThreadManager>(), Substitute.For<IWorkItemQueue>(), "Test", isBackground);
            Assert.AreEqual(isBackground, workerThread.Thread.IsBackground);
        }

        [Test]
        public void StartShouldStartThread()
        {
            WorkerThreadManagerStub workerThreadManager = WorkerThreadManagerStub.Create(); 

            IWorkItemQueue workItemQueue = Substitute.For<IWorkItemQueue>();
            workItemQueue.Dequeue().Returns((IWorkItem)null);

            WorkerThread workerThread = new WorkerThread(workerThreadManager, workItemQueue, "Test", false);
            workerThreadManager.ShouldExitWorkerThread(workerThread, false).Returns(false);            
            workerThread.Start();

            Wait.While(() => !workerThread.Thread.IsAlive, 300);

            Assert.AreEqual(true, workerThread.Thread.IsAlive);

            workerThread.Stop();

            Wait.While(() => workerThread.Thread.IsAlive, 300);

            Assert.AreEqual(false, workerThread.Thread.IsAlive);
        }

        [Test]
        public void DoWorkShouldNotCallWorkItemDequeueWhenWorkerThreadManagerShouldExitWorkerThreadReturnsTrue()
        {
            WorkerThreadManagerStub workerThreadManager = WorkerThreadManagerStub.Create(); 
            
            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            IWorkItemQueue workItemQueue = Substitute.For<IWorkItemQueue>();
            WorkerThread workerThread = new WorkerThread(workerThreadManager, workItemQueue, "Test", false);
            workerThreadManager.WorkerThreadExiting += (sender, args) =>
                {
                    if (args.WorkerThreadExitReason == WorkerThreadExitReason.MaximumThreadCountExceeded)
                    {
                        manualResetEvent.Set();
                    }
                };
            workerThreadManager.ShouldExitWorkerThread(workerThread, false).ReturnsForAnyArgs(true);
            workerThread.Start();

            manualResetEvent.WaitOne(500);

            workerThread.Stop();

            Wait.While(() => workerThread.Thread.IsAlive, 300);

            Assert.AreEqual(false, workerThread.Thread.IsAlive);

            workItemQueue.DidNotReceive().Dequeue();
        }

        [Test]
        public void DoWorkShouldCallWorkItemDequeueWhenWorkerThreadManagerShouldExitWorkerThreadReturnsFalse()
        {
            WorkerThreadManagerStub workerThreadManager = WorkerThreadManagerStub.Create(); 

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            IWorkItemQueue workItemQueue = Substitute.For<IWorkItemQueue>();
            workItemQueue.When(x => x.Dequeue()).Do(x => { manualResetEvent.Set(); });
           
            WorkerThread workerThread = new WorkerThread(workerThreadManager, workItemQueue, "Test", false);
            workerThreadManager.ShouldExitWorkerThread(workerThread, true).Returns(false);
            workerThread.StopWhenWorkItemQueueIsEmpty(true);
            workerThread.Start();

            manualResetEvent.WaitOne(500);
            workerThread.Stop();

            Wait.While(() => workerThread.Thread.IsAlive, 300);

            Assert.AreEqual(false, workerThread.Thread.IsAlive);

            workItemQueue.Received(1).Dequeue();
        }

        [Test]
        public void DoWorkShouldExitWorkerThreadWhenWorkerThreadManagerShouldExitWorkerThreadReturnsTrueAndWorkItemQueueIsEmpty()
        {
            WorkerThreadManagerStub workerThreadManager = WorkerThreadManagerStub.Create(); 

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            IWorkItemQueue workItemQueue = Substitute.For<IWorkItemQueue>();
            workItemQueue.Dequeue().Returns((IWorkItem)null);

            bool exitedBecaseWorkItemQueueIsEmpty = false;
            WorkerThread workerThread = new WorkerThread(workerThreadManager, workItemQueue, "Test", false);
            workerThreadManager.WorkerThreadExiting += (sender, args) =>
            {
                if (args.WorkerThreadExitReason == WorkerThreadExitReason.WorkItemQueueIsEmpty)
                {
                    exitedBecaseWorkItemQueueIsEmpty = true;
                    manualResetEvent.Set();
                }
            };
            workerThreadManager.ShouldExitWorkerThread(workerThread, false).Returns(false);
            workerThreadManager.ShouldExitWorkerThread(workerThread, true).Returns(true);

            workerThread.Start();

            manualResetEvent.WaitOne(500);
            workerThread.Stop();

            Wait.While(() => workerThread.Thread.IsAlive, 300);

            Assert.AreEqual(false, workerThread.Thread.IsAlive);

            workItemQueue.Received(1).Dequeue();
            Assert.AreEqual(true, exitedBecaseWorkItemQueueIsEmpty);
        }

        [Test]
        public void DoWorkShouldExitWorkerThreadWhenStopWhenWorkItemQueueIsEmptyIsTrueAndWorkItemQueueIsEmpty()
        {
            WorkerThreadManagerStub workerThreadManager = WorkerThreadManagerStub.Create(); 

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            IWorkItemQueue workItemQueue = Substitute.For<IWorkItemQueue>();
            workItemQueue.Dequeue().Returns((IWorkItem)null);

            bool exitedBecaseStopWhenWorkItemQueueIsEmptyFlagIsTrue = false;
            WorkerThread workerThread = new WorkerThread(workerThreadManager, workItemQueue, "Test", false);
            workerThreadManager.WorkerThreadExiting += (sender, args) =>
            {
                if (args.WorkerThreadExitReason == WorkerThreadExitReason.StopWhenWorkItemQueueIsEmptyIsTrue)
                {
                    exitedBecaseStopWhenWorkItemQueueIsEmptyFlagIsTrue = true;
                    manualResetEvent.Set();
                }
            };
            workerThreadManager.ShouldExitWorkerThread(workerThread, false).Returns(false);
            workerThreadManager.ShouldExitWorkerThread(workerThread, true).Returns(false);

            workerThread.StopWhenWorkItemQueueIsEmpty(true);
            workerThread.Start();

            manualResetEvent.WaitOne(500);
            workerThread.Stop();

            Wait.While(() => workerThread.Thread.IsAlive, 300);

            Assert.AreEqual(false, workerThread.Thread.IsAlive);

            workItemQueue.Received(1).Dequeue();
            Assert.AreEqual(true, exitedBecaseStopWhenWorkItemQueueIsEmptyFlagIsTrue);
        }

        [Test]
        public void DoWorkShouldProcessWorkItemWhenWorkItemQueueDequeueReturnsNotNullWorkItem()
        {
            WorkerThreadManagerStub workerThreadManager = WorkerThreadManagerStub.Create(); 

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            IWorkItemQueue workItemQueue = Substitute.For<IWorkItemQueue>();
            IWorkItem workItem = Substitute.For<IWorkItem>();
            workItemQueue.Dequeue().Returns(workItem);

            Exception otherThreadsException = null;
            WorkerThread workerThread = new WorkerThread(workerThreadManager, workItemQueue, "Test", false);
            workerThreadManager.WorkerThreadWorkItemFinished += (sender, args) =>
            {
                try
                {
                    Assert.AreEqual(workItem, args.WorkItem);
                }
                catch (Exception ex)
                {
                    otherThreadsException = ex;
                }
                workerThread.Stop();
                manualResetEvent.Set();
            };
            workerThread.Start();

            manualResetEvent.WaitOne(500);
            workerThread.Stop();

            Wait.While(() => workerThread.Thread.IsAlive, 300);

            Assert.AreEqual(false, workerThread.Thread.IsAlive);

            workItem.Received(1).DoWork();

            if (otherThreadsException != null)
            {
                throw otherThreadsException;
            }
        }

        [Test]
        public void DoWorkShouldCallWorkerThreadWorkItemStartingBeforeProcessingWorkItem()
        {
            WorkerThreadManagerStub workerThreadManager = WorkerThreadManagerStub.Create(); 

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            IWorkItemQueue workItemQueue = Substitute.For<IWorkItemQueue>();
            IWorkItem workItem = Substitute.For<IWorkItem>();
            workItemQueue.Dequeue().Returns(workItem);

            Exception otherThreadsException = null;
            WorkerThread workerThread = new WorkerThread(workerThreadManager, workItemQueue, "Test", false);
            workerThreadManager.WorkerThreadWorkItemStarting += (sender, args) =>
            {
                try
                {
                    Assert.AreEqual(workItem, args.WorkItem);

                    workItem.DidNotReceive().DoWork();
                }
                catch (Exception ex)
                {
                    otherThreadsException = ex;
                }
               
                workerThread.Stop();
                manualResetEvent.Set();
            };
            workerThread.Start();

            manualResetEvent.WaitOne(500);
            workerThread.Stop();

            Wait.While(() => workerThread.Thread.IsAlive, 300);

            Assert.AreEqual(false, workerThread.Thread.IsAlive);

            if (otherThreadsException != null)
            {
                throw otherThreadsException;
            }
        }

        [Test]
        public void DoWorkShouldCallWorkerThreadWorkItemExceptionEventWhenWorkItemIsCompletedWithException()
        {
            WorkerThreadManagerStub workerThreadManager = WorkerThreadManagerStub.Create(); 

            Exception invalidCastException = new InvalidCastException();
            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            IWorkItemQueue workItemQueue = Substitute.For<IWorkItemQueue>();
            IWorkItem workItem = Substitute.For<IWorkItem>();
            workItem.When(x => x.DoWork()).Do(
                x =>
                    {
                        throw invalidCastException;
                    });
            workItemQueue.Dequeue().Returns(workItem);

            Exception otherThreadsException = null;
            bool workerThreadWorkItemExceptionEventIsCalled = false;
            WorkerThread workerThread = new WorkerThread(workerThreadManager, workItemQueue, "Test", false);
            workerThreadManager.WorkerThreadWorkItemException += (sender, args) =>
                {
                    workerThreadWorkItemExceptionEventIsCalled = true;

                    try
                    {
                        Assert.AreEqual(WorkItemState.CompletedWithException, workItem.State);
                        Assert.AreEqual(workItem, args.WorkItem);
                        Assert.AreEqual(workItem.LastException, args.Exception);
                        Assert.AreEqual(invalidCastException, args.Exception);
                        workItem.Received(1).DoWork();
                    }
                    catch (Exception ex)
                    {
                        otherThreadsException = ex;
                    }

                    workerThread.Stop();
                    manualResetEvent.Set();

                };
            workerThread.Start();

            manualResetEvent.WaitOne(500);
            workerThread.Stop();

            Wait.While(() => workerThread.Thread.IsAlive, 300);

            Assert.AreEqual(false, workerThread.Thread.IsAlive);

            Assert.AreEqual(true, workerThreadWorkItemExceptionEventIsCalled);

            if (otherThreadsException != null)
            {
                throw otherThreadsException;
            }
        }

        [Test]
        public void DoWorkShouldCallWorkerThreadExitingEventWhenThreadIsAborted()
        {
            WorkerThreadManagerStub workerThreadManager = WorkerThreadManagerStub.Create(); 

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            IWorkItemQueue workItemQueue = Substitute.For<IWorkItemQueue>();
            IWorkItem workItem = Substitute.For<IWorkItem>();
            workItemQueue.Dequeue().Returns(workItem);

            Exception otherThreadsException = null;
            bool workerThreadExitingEventIsCalled = false;
            WorkerThread workerThread = new WorkerThread(workerThreadManager, workItemQueue, "Test", false);
            workerThreadManager.WorkerThreadExiting += (sender, args) =>
            {
                workerThreadExitingEventIsCalled = true;

                try
                {
                    Assert.AreEqual(WorkerThreadExitReason.ThreadAborted, args.WorkerThreadExitReason);
                }
                catch (Exception ex)
                {
                    otherThreadsException = ex;
                }

                manualResetEvent.Set();
            };
            workerThread.Start();
            
            Wait.While(() => !workerThread.Thread.IsAlive, 300);
            
            workerThread.Thread.Abort();

            manualResetEvent.WaitOne(500);
            workerThread.Stop();

            Wait.While(() => workerThread.Thread.IsAlive, 300);

            Assert.AreEqual(false, workerThread.Thread.IsAlive);

            Assert.AreEqual(true, workerThreadExitingEventIsCalled);

            if (otherThreadsException != null)
            {
                throw otherThreadsException;
            }
        }

        [Test]
        public void DoWorkShouldCallWorkerThreadExceptionEventWhenExceptionOccursOutsideOfWorkItemDoWorkMethod()
        {
            WorkerThreadManagerStub workerThreadManager = WorkerThreadManagerStub.Create(); 

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            IWorkItemQueue workItemQueue = Substitute.For<IWorkItemQueue>();
            IWorkItem workItem = Substitute.For<IWorkItem>();
            workItemQueue.Dequeue().Returns(workItem);

            Exception invalidCastException = new InvalidCastException();
            Exception otherThreadsException = null;
            bool workerThreadExitingEventIsCalled = false;
            WorkerThread workerThread = new WorkerThread(workerThreadManager, workItemQueue, "Test", false);
            workerThreadManager.WorkerThreadStarted += (sender, args) =>
                {
                    throw invalidCastException;
                };
            workerThreadManager.WorkerThreadException += (sender, args) =>
                {
                    try
                    {
                        Assert.AreEqual(invalidCastException, args.Exception);
                    }
                    catch (Exception ex)
                    {
                        otherThreadsException = ex;
                    }
                };
            workerThreadManager.WorkerThreadExiting += (sender, args) =>
            {
                workerThreadExitingEventIsCalled = true;

                try
                {
                    Assert.AreEqual(WorkerThreadExitReason.ExceptionOccurred, args.WorkerThreadExitReason);
                }
                catch (Exception ex)
                {
                    otherThreadsException = ex;
                }

                manualResetEvent.Set();
            };
            workerThread.Start();

            Wait.While(() => !workerThread.Thread.IsAlive, 300);

            manualResetEvent.WaitOne(500);
            workerThread.Stop();

            Wait.While(() => workerThread.Thread.IsAlive, 300);

            Assert.AreEqual(false, workerThread.Thread.IsAlive);

            Assert.AreEqual(true, workerThreadExitingEventIsCalled);

            if (otherThreadsException != null)
            {
                throw otherThreadsException;
            }
        }

        [Test]
        public void StopShouldCallCurrentWorkItemStopMethodWhenCalledDuringAnyWorkItemIsInProgress()
        {
            WorkerThreadManagerStub workerThreadManager = WorkerThreadManagerStub.Create();

            ManualResetEvent startWaitHandle = new ManualResetEvent(false);
            ManualResetEvent stopWaitHandle = new ManualResetEvent(false);
            IWorkItemQueue workItemQueue = Substitute.For<IWorkItemQueue>();
            IWorkItem workItem = Substitute.For<IWorkItem>();
            workItem.When(x => x.DoWork()).Do(
                x =>
                    {
                        startWaitHandle.Set();
                        stopWaitHandle.WaitOne(500);
                    });
            workItemQueue.Dequeue().Returns(workItem);

            WorkerThread workerThread = new WorkerThread(workerThreadManager, workItemQueue, "Test", false);
            workerThread.Start();

            startWaitHandle.WaitOne(500);
            workerThread.Stop();
            stopWaitHandle.Set();

            Wait.While(() => workerThread.Thread.IsAlive, 500);

            Assert.AreEqual(false, workerThread.Thread.IsAlive);

            workItem.Received(1).Stop();
        }
    }
}
