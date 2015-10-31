namespace Labo.Threading.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Labo.Threading.Exceptions;

    using NSubstitute;

    using NUnit.Framework;

    [TestFixture(Category = "WorkItemQueue")]
    public class WorkItemQueueFixture
    {
        [Test]
        public void EnqueueShouldNotAddWorkItemToQueueWhenWaiterIsSignaled()
        {
            IWorkItem workItem = Substitute.For<IWorkItem>();

            IWorkItemWaiterEntry workItemWaiterEntry = Substitute.For<IWorkItemWaiterEntry>();
            workItemWaiterEntry.TrySignal(workItem).Returns(true);

            IWorkItemWaiterEntryStack workItemWaiterEntryStack = Substitute.For<IWorkItemWaiterEntryStack>();
            workItemWaiterEntryStack.Count.Returns(1);
            workItemWaiterEntryStack.Pop().Returns(workItemWaiterEntry);

            WorkItemQueue workItemQueue = new WorkItemQueue(1 * 1000, workItemWaiterEntryStack);
            workItemQueue.Enqueue(workItem);

            Assert.AreEqual(0, workItemQueue.Count);
        }

        [Test]
        public void EnqueueShouldAddWorkItemToQueueWhenAllWaitersTimedOut()
        {
            Stack<IWorkItemWaiterEntry> workItemWaiterEntries = new Stack<IWorkItemWaiterEntry>();
            workItemWaiterEntries.Push(CreateNotSignalableWorkItemWaiterEntry());
            workItemWaiterEntries.Push(CreateNotSignalableWorkItemWaiterEntry());

            IWorkItemWaiterEntryStack workItemWaiterEntryStack = Substitute.For<IWorkItemWaiterEntryStack>();
            workItemWaiterEntryStack.Count.Returns(x => workItemWaiterEntries.Count);
            workItemWaiterEntryStack.Pop().Returns(x => workItemWaiterEntries.Pop());

            WorkItemQueue workItemQueue = new WorkItemQueue(1 * 1000, workItemWaiterEntryStack);

            IWorkItem workItem = Substitute.For<IWorkItem>();
            workItemQueue.Enqueue(workItem);

            Assert.AreEqual(1, workItemQueue.Count);
        }

        [Test]
        public void EnqueueShouldAddWorkItemsToQueueWhenThereAreNoWaiters()
        {
            IWorkItem workItem = Substitute.For<IWorkItem>();

            WorkItemQueue workItemQueue = new WorkItemQueue(1 * 100);
            IList<Task> tasks = new List<Task>
                                    {
                                        Task.Factory.StartNew(() => workItemQueue.Enqueue(workItem)),
                                        Task.Factory.StartNew(() => workItemQueue.Enqueue(workItem)),
                                        Task.Factory.StartNew(() => workItemQueue.Enqueue(workItem))
                                    };
            Task.WaitAll(tasks.ToArray());

            Assert.AreEqual(3, workItemQueue.Count);
        }

        [Test]
        public void EnqueueTrySignalMethodShouldBeCalledWhenWaitingStackIsNotEmpty()
        {
            IWorkItemWaiterEntry workItemWaiterEntry = Substitute.For<IWorkItemWaiterEntry>();
            IWorkItemWaiterEntryStack workItemWaiterEntryStack = Substitute.For<IWorkItemWaiterEntryStack>();
           
            int count = 1;
            workItemWaiterEntryStack.Count.Returns(x => count);
            workItemWaiterEntryStack.Pop().Returns(
                x =>
                    {
                        if (count == 1)
                        {
                            count = 0;
                            return workItemWaiterEntry;
                        }

                        return null;
                    });

            WorkItemQueue workItemQueue = new WorkItemQueue(1 * 1000, workItemWaiterEntryStack);

            IWorkItem workItem = Substitute.For<IWorkItem>();            
            workItemQueue.Enqueue(workItem);

            workItemWaiterEntry.Received(1).TrySignal(workItem);
        }

        [Test]
        public void EnqueueTrySignalMethodShouldNotBeCalledWhenWaitingStackIsEmpty()
        {
            IWorkItemWaiterEntry workItemWaiterEntry = Substitute.For<IWorkItemWaiterEntry>();
            IWorkItemWaiterEntryStack workItemWaiterEntryStack = Substitute.For<IWorkItemWaiterEntryStack>();
            workItemWaiterEntryStack.Count.Returns(0);

            WorkItemQueue workItemQueue = new WorkItemQueue(1 * 1000, workItemWaiterEntryStack);

            IWorkItem workItem = Substitute.For<IWorkItem>();
            workItemQueue.Enqueue(workItem);

            workItemWaiterEntry.DidNotReceiveWithAnyArgs().TrySignal(workItem);
        }

        [Test]
        public void EnqueueShouldThrowExceptionWhenWorkItemIsEmpty()
        {
            WorkItemQueue workItemQueue = new WorkItemQueue(1 * 1000, Substitute.For<IWorkItemWaiterEntryStack>());

            Assert.Throws<ArgumentNullException>(() => workItemQueue.Enqueue(null));
        }

        [Test]
        public void EnqueueShouldThrowExceptionWhenItIsCalledAfterShutDown()
        {
            WorkItemQueue workItemQueue = new WorkItemQueue(1 * 300);
            workItemQueue.ShutDown();
            
            Assert.Throws<LaboThreadingException>(() => workItemQueue.Enqueue(Substitute.For<IWorkItem>()));
        }

        [Test]
        public void DequeueShouldReturnQueuedWorkItemWhenQueueIsNotEmpty()
        {
            WorkItemQueue workItemQueue = new WorkItemQueue(1 * 1000);

            IWorkItem workItemFirst = Substitute.For<IWorkItem>();
            IWorkItem workItemLast = Substitute.For<IWorkItem>();

            workItemQueue.Enqueue(workItemFirst);
            workItemQueue.Enqueue(workItemLast);

            Assert.AreEqual(workItemFirst, workItemQueue.Dequeue());
            Assert.AreEqual(workItemLast, workItemQueue.Dequeue());
        }

        [Test]
        public void DequeueShouldReturnQueuedWorkItemWhenTheWaiterIsNotTimedOut()
        {
            IWorkItem workItem = Substitute.For<IWorkItem>();

            WorkItemQueue workItemQueue = new WorkItemQueue(1 * 1000);
            new Thread(
                () =>
                    {
                        Thread.Sleep(200);

                        workItemQueue.Enqueue(workItem);
                    }).Start();
            

            Assert.AreEqual(workItem, workItemQueue.Dequeue());
            Assert.AreEqual(0, workItemQueue.Count);
            Assert.AreEqual(0, workItemQueue.WorkItemWaiterEntryStack.Count);
        }

        [Test]
        public void DequeueShouldReturnNullWhenTheWaiterIsTimedOut()
        {
            IWorkItem workItem = Substitute.For<IWorkItem>();

            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            WorkItemQueue workItemQueue = new WorkItemQueue(1 * 300);
            new Thread(
                () =>
                    {
                        Thread.Sleep(1000);

                        workItemQueue.Enqueue(workItem);
                        autoResetEvent.Set();
                    }).Start();

            Assert.AreEqual(null, workItemQueue.Dequeue());

            autoResetEvent.WaitOne();

            Assert.AreEqual(1, workItemQueue.Count);
            Assert.AreEqual(0, workItemQueue.WorkItemWaiterEntryStack.Count);
        }

        [Test]
        public void ShutDownShouldNotWaitMoreThanWaiterTimeOutWhenThereIsNoWorkItemsInTheQueue()
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            const int workItemWaiterTimeOutInMilliSeconds = 1000;
            WorkItemQueue workItemQueue = new WorkItemQueue(workItemWaiterTimeOutInMilliSeconds);
            new Thread(
                () =>
                {
                    Thread.Sleep(100);

                    autoResetEvent.Set();

                    IWorkItem workItem = workItemQueue.Dequeue();
                   
                    autoResetEvent.Set();

                    Assert.AreEqual(null, workItem);
                }).Start();

            autoResetEvent.WaitOne(500);

            Stopwatch stopwatch = Stopwatch.StartNew();
            workItemQueue.ShutDown();
            autoResetEvent.WaitOne(workItemWaiterTimeOutInMilliSeconds + 100);
            stopwatch.Stop();

            Assert.Less(stopwatch.ElapsedMilliseconds, workItemWaiterTimeOutInMilliSeconds);
        }

        [Test]
        public void ShutDownShouldNotBlockDequeueWhenThereAreWorkItemsInTheQueue()
        {
            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            const int workItemWaiterTimeOutInMilliSeconds = 1000;
            WorkItemQueue workItemQueue = new WorkItemQueue(workItemWaiterTimeOutInMilliSeconds);

            IWorkItem workItem = Substitute.For<IWorkItem>();
            workItemQueue.Enqueue(workItem);
            
            new Thread(
                () =>
                {
                    manualResetEvent.Set();
                    manualResetEvent.Reset();
                    manualResetEvent.WaitOne(300);

                    IWorkItem dequeuedWorkItem = workItemQueue.Dequeue();

                    Assert.AreEqual(workItem, dequeuedWorkItem);
                }).Start();

            manualResetEvent.WaitOne(300);

            workItemQueue.ShutDown();

            manualResetEvent.Set();
        }

        [Test]
        public void DisposeShouldNotExplode()
        {
            WorkItemQueue workItemQueue = new WorkItemQueue(300);
            Assert.DoesNotThrow(() => workItemQueue.Dispose());
        }

        private static IWorkItemWaiterEntry CreateNotSignalableWorkItemWaiterEntry()
        {
            IWorkItemWaiterEntry workItemWaiterEntry = Substitute.For<IWorkItemWaiterEntry>();
            workItemWaiterEntry.TrySignal(null).ReturnsForAnyArgs(false);
            return workItemWaiterEntry;
        }
    }
}
