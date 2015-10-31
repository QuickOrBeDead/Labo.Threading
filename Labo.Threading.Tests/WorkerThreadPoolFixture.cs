namespace Labo.Threading.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Labo.Threading.Exceptions;
    using Labo.Threading.Tests.Stubs;

    using NSubstitute;

    using NUnit.Framework;

    using Timer = System.Timers.Timer;

    [TestFixture(Category = "WorkerThreadPool")]
    public class WorkerThreadPoolFixture
    {
        [SetUp]
        public void Setup()
        {
            WorkerThreadPool.ResetWorkerThreadPoolIdCounter();
        }

        [Test, Ignore]
        public void A()
        {
            Random random = new Random();
            WorkerThreadPool workerThreadPool = new WorkerThreadPool(6 * 10 * 1000, 8, 8);
            const int workItemsCount = 100;
            int counter = 0;
            int totalSleepMilliseconds = 0;
            
            IList<IWorkItem> workItems = new List<IWorkItem>(workItemsCount);
            for (int i = 0; i < workItemsCount; i++)
            {
                ActionWorkItem workItem = new ActionWorkItem(
                    () =>
                        {
                            int sleepMilliseconds = random.Next(1000, 2000);
                            Interlocked.Add(ref totalSleepMilliseconds, sleepMilliseconds);
                            Thread.Sleep(sleepMilliseconds);
                            Interlocked.Increment(ref counter);
                        });
                workItems.Add(workItem);                
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            for (int i = 0; i < workItems.Count; i++)
            {
                IWorkItem workItem = workItems[i];
                workerThreadPool.QueueWorkItem(workItem);
            }

            workerThreadPool.Shutdown(true, 500);

            stopwatch.Stop();

            TimeSpan totalExecutionTime = TimeSpan.Zero;
            for (int i = 0; i < workItems.Count; i++)
            {
                IWorkItem workItem = workItems[i];
                totalExecutionTime += workItem.ExecutionTime;
            }

            totalExecutionTime.ToString();
        }

        [Test]
        public void DefaultConstructorShouldSetTheDefaultWorkerThreadIdleTimeout()
        {
            WorkerThreadPool workerThreadPool = new WorkerThreadPool();
            WorkItemQueue workItemQueue = (WorkItemQueue)workerThreadPool.WorkItemQueue;

            Assert.AreEqual(WorkerThreadPool.DEFAULT_WORKER_THREAD_IDLE_TIMEOUT, workItemQueue.WorkItemWaiterTimeOutInMilliSeconds);

            workerThreadPool.Shutdown(true, 500);
        }

        [Test]
        public void DefaultrConstructorShouldSetTheDefaultMinWorkerThreadsCount()
        {
            WorkerThreadPool workerThreadPool = new WorkerThreadPool();
            Assert.AreEqual(WorkerThreadPool.DEFAULT_MIN_WORKER_THREADS_PER_CORE * Environment.ProcessorCount, workerThreadPool.MinWorkerThreads);

            workerThreadPool.Shutdown(true, 500);
        }

        [Test]
        public void DefaultConstructorShouldSetTheDefaultMaxWorkerThreadsCount()
        {
            WorkerThreadPool workerThreadPool = new WorkerThreadPool();
            Assert.AreEqual(WorkerThreadPool.DEFAULT_MAX_WORKER_THREADS_PER_CORE * Environment.ProcessorCount, workerThreadPool.MaxWorkerThreads);

            workerThreadPool.Shutdown(true, 500);
        }

        [Test]
        public void ConstructorShouldThrowExceptionWhenWorkItemQueueIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkerThreadPool(null, 1, 1));
        }

        [Test]
        public void ConstructorShouldThrowExceptionWhenMaxWorkerThreadsCountIsLessThanOrEqualToZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WorkerThreadPool(Substitute.For<IWorkItemQueue>(), 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WorkerThreadPool(Substitute.For<IWorkItemQueue>(), 0, -1));
        }

        [Test]
        public void ConstructorShouldThrowExceptionWhenMaxWorkerThreadsCountIsLessMinWorkerThreadsCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WorkerThreadPool(Substitute.For<IWorkItemQueue>(), 2, 1));
        }

        [Test]
        public void ConstructorShouldThrowExceptionWhenMinWorkerThreadsCountIsLessThanZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WorkerThreadPool(Substitute.For<IWorkItemQueue>(), -1, 1));
        }

        [Test]
        public void ConstructorShouldThrowExceptionWhenMinWorkerThreadsCountIsBiggerThanMaxWorkerThreadsCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WorkerThreadPool(Substitute.For<IWorkItemQueue>(), 3, 2));
        }

        [Test]
        public void ConstructorShouldSetWorkerThreadPoolIdThreadSafely()
        {
            const int count = 1000;
            WorkerThreadPool[] pools = new WorkerThreadPool[count];

            ParallelLoopResult parallelLoopResult = Parallel.For(0, count, x => pools[x] = new WorkerThreadPool(0, 1));
            Wait.While(() => !parallelLoopResult.IsCompleted, 500);

            Assert.AreEqual(count, pools.Select(x => x.WorkerThreadPoolId).Distinct().Count());
            Assert.AreEqual(count * (count + 1) / 2, pools.Sum(x => x.WorkerThreadPoolId));

            pools.ForEach(x => x.Shutdown(true, 500));
        }

        [Test, Sequential]
        [TestCase(1, 1, 4, 1)]
        [TestCase(10, 3, 10, 10)]
        [TestCase(0, 0, 1, 0)]
        [TestCase(0, 1, 1, 1)]
        [TestCase(0, 1, 2, 1)]
        [TestCase(0, 1, 1, 1)]
        [TestCase(1, 2, 3, 2)]
        [TestCase(3, 2, 2, 2)]
        public void ConstructorTestInitialWorkerThreadsCount(int workItemQueueCount, int minWorkerThreads, int maxWorkerThreads, int expected)
        {
            IWorkItemQueue workItemQueue = Substitute.For<IWorkItemQueue>();
            workItemQueue.Count.Returns(workItemQueueCount);
            WorkerThreadPool workerThreadPool = new WorkerThreadPool(workItemQueue, minWorkerThreads, maxWorkerThreads);
            Assert.AreEqual(expected, workerThreadPool.WorkerThreadsCount);

            workerThreadPool.Shutdown(true, 500);
        }

        [Test]
        public void ConstructorShoulStartMinimumCountWorkerThreadsAlthoughThereIsNoWorkItemsInTheQueue()
        {
            IWorkItemQueue workItemQueue = Substitute.For<IWorkItemQueue>();
            workItemQueue.Count.Returns(0);
            workItemQueue.Dequeue().Returns((IWorkItem)null);

            WorkerThreadPool workerThreadPool = new WorkerThreadPool(workItemQueue, 2, 2);
            
            Assert.AreEqual(2, workerThreadPool.WorkerThreadsCount);

            Wait.While(() => !workerThreadPool.WorkerThreads.All(x => x.Thread.IsAlive), 500);
            
            Assert.AreEqual(0, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            workerThreadPool.Shutdown(true, 500);
        }

        [Test, Sequential]
        [TestCase(1, 1, 4, 1)]
        [TestCase(10, 3, 10, 10)]
        [TestCase(0, 0, 1, 0)]
        [TestCase(0, 1, 1, 1)]
        [TestCase(0, 1, 2, 1)]
        [TestCase(0, 1, 1, 1)]
        [TestCase(1, 2, 3, 2)]
        [TestCase(3, 2, 2, 2)]
        public void CalculateOptimumThreadsCount(int workItemQueueCount, int minWorkerThreads, int maxWorkerThreads, int expected)
        {
            Assert.AreEqual(expected, WorkerThreadPool.CalculateOptimumThreadsCount(workItemQueueCount, minWorkerThreads, maxWorkerThreads));
        }

        /// <summary>
        /// I. Maksimum işçi thread sayısı ile ilgili durumlar şunlar olabilir;
        /// A. Başlangıç durumu:
        /// 1. Kuyrukta bekleyen iş parçası olmayabilir;
        ///     - Minimum işçi thread sayısı kadar işçi thread yaratılır.
        /// 2. Kuyrukta bekleyen iş parçası olabilir;
        ///     - Maksimum kuyruktaki iş parçası sayısı ile maksimum işçi thread sayısı arasında ve minimum minimum işçi thread sayısı ile kuyruktaki iş parçası sayısı arasında işçi thread yaratılır.
        /// B. Maksimum işçi thread sonradan sayısının değişmesi halinde:
        /// 1. Azaltılıyor ise;
        ///     - İşini ilk bitiren işçi thread, işçi thread listesinden çıkartılır. Bu işlem işçi thread sayısı minimum işçi thread sayısına erişene kadar devam eder.
        /// 2. Arttırılıyor ise;
        ///     - Kuyrukta bekleyen iş parçası yoksa; yeni işçi thread yaratılmaz.
        ///     - Kuyrukta bekleyen iş parçası varsa; maksimum işçi thread sayısına ulaşana kadar yeni işçi threadleri yaratılır.
        /// 
        /// II. Minimum işçi thread sayısı ile ilgili durumlar şunlar olabilir;
        /// A. Başlangıç durumu:
        /// 1. Kuyrukta bekleyen iş parçası olmayabilir;
        ///     - Mevcut işçi thread sayısı minimum işçi thread sayısı kadar yaratılır.
        /// 2. Kuyrukta bekleyen iş parçası olabilir;
        ///     - İşçi thread sayısı maksimuma ulaşmış demektir. Kuyrukta bekleyen iş parçası bittikten sonra mevcut işçi thread sayısı minimum işçi thread sayısına kadar iner. 
        /// B. Minimum işçi thread sonradan sayısının değişmesi halinde:
        /// 1. Azaltılıyor ise;
        ///     - Kuyrukta bekleyen iş parçası yoksa; mevcut işçi thread sayısı minimum işçi thread sayısına inene kadar boşta olan işçi threadler çıkartılır.
        /// 2. Arttırılıyor ise;
        ///     - Kuyrukta bekleyen iş parçası yoksa; mevcut işçi thread sayısı minimum işçi thread sayısına çıkana kadar yeni işçi thread yaratılır.
        ///     - Kuyrukta bekleyen iş parçası varsa; zaten maksimum işçi thread sayısına erişilmiş demektir, dolayısıyla yeni işçi thread yaratılmaz.
        /// </summary>

        // Test I.A.1
        [Test]
        public void ConstructorShouldCreateMinimumWorkerThreadsWhenThereIsNoWorkItemInTheQueue()
        {
            WorkerThreadPool workerThreadPool = new WorkerThreadPool(2, 4);

            Assert.AreEqual(2, workerThreadPool.WorkerThreadsCount);

            workerThreadPool.Shutdown(true, 500);
        }

        // Test I.A.2
        [Test, Sequential]
        [TestCase(3, 3)]
        [TestCase(2, 2)]
        [TestCase(4, 4)]
        [TestCase(1, 2)]
        [TestCase(5, 4)]
        public void ConstructorShouldCreateOptimumWorkerThreadsWhenThereAreWorkItemsInTheQueue(int workItemCount, int expected)
        {
            IWorkItemQueue workItemQueue = Substitute.For<IWorkItemQueue>();
            workItemQueue.Count.Returns(workItemCount);

            WorkerThreadPool workerThreadPool = new WorkerThreadPool(workItemQueue, 2, 4);

            Assert.AreEqual(expected, workerThreadPool.WorkerThreadsCount);

            workerThreadPool.Shutdown(true, 500);
        }

        // Test I.B.1
        [Test]
        public void SetMaximumWorkerThreadsCountShouldRemoveWorkerThreadsWhenMaximumThreadsCountDecreases()
        {
            ManualFinishWorkItemQueueStub manualFinishWorkItemQueueStub = new ManualFinishWorkItemQueueStub();
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());

            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            WorkerThreadPool workerThreadPool = new WorkerThreadPool(manualFinishWorkItemQueueStub, 1, 3);
            workerThreadPool.WorkerThreadExiting += (sender, args) => { autoResetEvent.Set(); };

            manualFinishWorkItemQueueStub.WaitAllStart(500);

            Assert.AreEqual(3, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(3, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            workerThreadPool.SetMaximumWorkerThreadsCount(2);

            manualFinishWorkItemQueueStub.WorkItems[0].Stop();
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());

            autoResetEvent.WaitOne(500);

            Assert.AreEqual(2, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(2, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            manualFinishWorkItemQueueStub.WorkItems[1].Stop();

            autoResetEvent.WaitOne(500);

            Assert.AreEqual(2, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(2, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            manualFinishWorkItemQueueStub.WorkItems[2].Stop();

            autoResetEvent.WaitOne(500);

            Assert.AreEqual(1, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(1, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            manualFinishWorkItemQueueStub.WorkItems[3].Stop();

            autoResetEvent.WaitOne(500);

            Assert.AreEqual(1, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(0, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            manualFinishWorkItemQueueStub.StopAll();

            workerThreadPool.Shutdown(true, 500);
        }

        // Test I.B.2.1
        [Test]
        public void SetMaximumWorkerThreadsCountShouldAddNoWorkerThreadsWhenMaximumThreadsCountIncreasesAndThereIsNoWorkItemsInTheQueue()
        {
            ManualFinishWorkItemQueueStub manualFinishWorkItemQueueStub = new ManualFinishWorkItemQueueStub();

            WorkerThreadPool workerThreadPool = new WorkerThreadPool(manualFinishWorkItemQueueStub, 1, 3);

            Assert.AreEqual(1, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(0, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            workerThreadPool.SetMaximumWorkerThreadsCount(4);

            Assert.AreEqual(1, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(0, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            manualFinishWorkItemQueueStub.StopAll();

            workerThreadPool.Shutdown(true, 500);
        }

        // Test I.B.2.2
        [Test]
        public void SetMaximumWorkerThreadsCountShouldAddNoWorkerThreadsToReachTheMaximumThreadCountWhenMaximumThreadsCountIncreasesAndThereAreWorkItemsInTheQueue()
        {
            ManualFinishWorkItemQueueStub manualFinishWorkItemQueueStub = new ManualFinishWorkItemQueueStub();
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());

            WorkerThreadPool workerThreadPool = new WorkerThreadPool(manualFinishWorkItemQueueStub, 1, 2);

            manualFinishWorkItemQueueStub.WaitAllStart(500);

            Assert.AreEqual(2, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(2, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            workerThreadPool.QueueWorkItem(new ManualFinishWorkItem());
            workerThreadPool.QueueWorkItem(new ManualFinishWorkItem());

            Assert.AreEqual(2, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(2, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            Assert.AreEqual(2, manualFinishWorkItemQueueStub.Count);

            workerThreadPool.SetMaximumWorkerThreadsCount(4);

            manualFinishWorkItemQueueStub.WaitAllStart(500);

            Assert.AreEqual(4, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(4, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            Assert.AreEqual(0, manualFinishWorkItemQueueStub.Count);

            manualFinishWorkItemQueueStub.StopAll();

            workerThreadPool.Shutdown(true, 500);
        }

        // Test II.A.1
        [Test]
        public void ConstructorShouldCreateMinimumNumberWorkerThreadsWhenThereIsNoWorkItemInTheQueue()
        {
            WorkerThreadPool workerThreadPool = new WorkerThreadPool(3, 4);

            Assert.AreEqual(3, workerThreadPool.WorkerThreadsCount);

            workerThreadPool.Shutdown(true, 500);
        }

        // Test II.A.2
        [Test]
        public void ConstructorShouldCreateOptimumWorkerThreadsWhenThereAreWorkItemsInTheQueue()
        {
            ManualFinishWorkItemQueueStub workItemQueue = new ManualFinishWorkItemQueueStub();
            for (int i = 0; i < 5; i++)
            {
                workItemQueue.Enqueue(new ManualFinishWorkItem());
            }

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            int exitedWorkerThreadCount = 0;
            WorkerThreadPool workerThreadPool = new WorkerThreadPool(workItemQueue, 2, 4);
            workerThreadPool.WorkerThreadExiting += (sender, args) =>
                {
                    if (Interlocked.Increment(ref exitedWorkerThreadCount) >= 2)
                    {
                        manualResetEvent.Set();
                    }
                };

            workItemQueue.WaitAllStart(500);

            Assert.AreEqual(4, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(4, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            workItemQueue.StopAll();

            manualResetEvent.WaitOne(500);

            Assert.AreEqual(2, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(0, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            workerThreadPool.Shutdown(true, 500);
        }

        // Test II.B.1
        [Test]
        public void SetMinimumWorkerThreadsCountShouldRemoveWorkerThreadsWhenMinimumThreadsCountDecreasesAndThereIsNoWorkItemInTheQueue()
        {
            ManualFinishWorkItemQueueStub manualFinishWorkItemQueueStub = new ManualFinishWorkItemQueueStub();
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            int exitedWorkerThreadCount = 0;
            WorkerThreadPool workerThreadPool = new WorkerThreadPool(manualFinishWorkItemQueueStub, 2, 3);
            workerThreadPool.WorkerThreadExiting += (sender, args) =>
                {
                    if (Interlocked.Increment(ref exitedWorkerThreadCount) >= 2)
                    {
                        manualResetEvent.Set();
                    }
                };

            manualFinishWorkItemQueueStub.WaitAllStart(500);

            Assert.AreEqual(3, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(3, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));
            Assert.AreEqual(0, manualFinishWorkItemQueueStub.Count);

            workerThreadPool.SetMinimumWorkerThreadsCount(1);

            manualFinishWorkItemQueueStub.StopAll();

            manualResetEvent.WaitOne(500);

            Assert.AreEqual(1, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(0, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));
            Assert.AreEqual(0, manualFinishWorkItemQueueStub.Count);

            workerThreadPool.Shutdown(true, 500);
        }

        // Test II.B.2.1
        [Test]
        public void SetMinimumWorkerThreadsCountShouldAddWorkerThreadsWhenMinimumThreadsCountIncreasesAndThereIsNoWorkItemInTheQueue()
        {
            ManualFinishWorkItemQueueStub manualFinishWorkItemQueueStub = new ManualFinishWorkItemQueueStub();
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            int exitedWorkerThreadCount = 0;
            WorkerThreadPool workerThreadPool = new WorkerThreadPool(manualFinishWorkItemQueueStub, 2, 4);
            workerThreadPool.WorkerThreadExiting += (sender, args) =>
            {
                if (Interlocked.Increment(ref exitedWorkerThreadCount) >= 3)
                {
                    manualResetEvent.Set();
                }
            };

            manualFinishWorkItemQueueStub.WaitAllStart(500);

            Assert.AreEqual(4, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(4, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));
            Assert.AreEqual(0, manualFinishWorkItemQueueStub.Count);

            manualFinishWorkItemQueueStub.StopAll();

            workerThreadPool.SetMinimumWorkerThreadsCount(1);

            manualResetEvent.WaitOne();

            Assert.AreEqual(1, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(0, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));
            Assert.AreEqual(0, manualFinishWorkItemQueueStub.Count);

            workerThreadPool.Shutdown(true, 500);
        }

        // Test II.B.2.2
        [Test]
        public void SetMinimumWorkerThreadsCountShouldNotAddWorkerThreadsWhenMinimumThreadsCountIncreasesAndThereIsWorkItemsInTheQueue()
        {
            ManualFinishWorkItemQueueStub manualFinishWorkItemQueueStub = new ManualFinishWorkItemQueueStub();
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());
            manualFinishWorkItemQueueStub.Enqueue(new ManualFinishWorkItem());

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            int exitedWorkerThreadCount = 0;
            WorkerThreadPool workerThreadPool = new WorkerThreadPool(manualFinishWorkItemQueueStub, 2, 4);
            workerThreadPool.WorkerThreadExiting += (sender, args) =>
            {
                if (Interlocked.Increment(ref exitedWorkerThreadCount) >= 1)
                {
                    manualResetEvent.Set();
                }
            };

            manualFinishWorkItemQueueStub.WaitAllStart(500);

            Assert.AreEqual(4, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(4, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));
            Assert.AreEqual(1, manualFinishWorkItemQueueStub.Count);

            workerThreadPool.SetMinimumWorkerThreadsCount(3);

            Assert.AreEqual(4, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(4, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));
            Assert.AreEqual(1, manualFinishWorkItemQueueStub.Count);

            manualFinishWorkItemQueueStub.StopAll();

            manualResetEvent.WaitOne();

            Assert.AreEqual(3, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(0, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            workerThreadPool.Shutdown(true, 500);
        }

        [Test]
        public void SetMaximumWorkerThreadsCountShouldThrowExceptionWhenMaximumThreadsCountIsLessThanZero()
        {
            WorkerThreadPool workerThreadPool = new WorkerThreadPool(0, 1);

            Assert.Throws<ArgumentOutOfRangeException>(() => workerThreadPool.SetMaximumWorkerThreadsCount(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => workerThreadPool.SetMaximumWorkerThreadsCount(-1));

            workerThreadPool.Shutdown(true, 500);
        }

        [Test]
        public void SetMaximumWorkerThreadsCountShouldThrowExceptionWhenMaximumThreadsCountIsLessThanMinimumCount()
        {
            WorkerThreadPool workerThreadPool = new WorkerThreadPool(2, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() => workerThreadPool.SetMaximumWorkerThreadsCount(1));

            workerThreadPool.Shutdown(true, 500);
        }

        [Test]
        public void SetMaximumWorkerThreadsCountShouldAddNoNewWorkerThreadWhenMaxThreadCountIncreasedAndThereIsNoWorkItemsInTheQueue()
        {
            ManualFinishWorkItemQueueStub workItemQueue = new ManualFinishWorkItemQueueStub();
            workItemQueue.Enqueue(new ManualFinishWorkItem());

            WorkerThreadPool workerThreadPool = new WorkerThreadPool(workItemQueue, 0, 1);

            Assert.AreEqual(1, workerThreadPool.WorkerThreadsCount);

            workItemQueue.WaitAllStart(500);
            workerThreadPool.SetMaximumWorkerThreadsCount(2);

            Assert.AreEqual(1, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(1, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            workItemQueue.StopAll();

            workerThreadPool.Shutdown(true, 500);
        }

        [Test]
        public void SetMaximumWorkerThreadsCountShouldAddANewWorkerThreadWhenMaxThreadCountIncreasedAndThereAreWorkItemsInTheQueue()
        {
            ManualFinishWorkItemQueueStub workItemQueue = new ManualFinishWorkItemQueueStub();
            workItemQueue.Enqueue(new ManualFinishWorkItem());
            workItemQueue.Enqueue(new ManualFinishWorkItem());

            WorkerThreadPool workerThreadPool = new WorkerThreadPool(workItemQueue, 0, 1);

            Assert.AreEqual(1, workerThreadPool.WorkerThreadsCount);

            workerThreadPool.SetMaximumWorkerThreadsCount(2);

            workItemQueue.WaitAllStart(500);

            Assert.AreEqual(2, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(2, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            workItemQueue.StopAll();

            workerThreadPool.Shutdown(true, 500);
        }

        [Test]
        public void SetMaximumWorkerThreadsCountShouldRemoveIdleWorkerThreadsWhenDecreased()
        {
            ManualFinishWorkItemQueueStub workItemQueue = new ManualFinishWorkItemQueueStub();
            workItemQueue.Enqueue(new ManualFinishWorkItem());
            workItemQueue.Enqueue(new ManualFinishWorkItem());

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            WorkerThreadPool workerThreadPool = new WorkerThreadPool(workItemQueue, 0, 2);
            workerThreadPool.WorkerThreadExiting += (sender, args) =>
                {
                    manualResetEvent.Set();
                };

            workItemQueue.WaitAllStart(500);

            Assert.AreEqual(2, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(2, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            workerThreadPool.SetMaximumWorkerThreadsCount(1);

            Assert.AreEqual(2, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(2, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            workItemQueue.WorkItems[0].Stop();
            workItemQueue.Enqueue(new ManualFinishWorkItem());

            Assert.AreEqual(1, workItemQueue.Count);

            manualResetEvent.WaitOne(300);

            Assert.AreEqual(1, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(1, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            workItemQueue.StopAll();

            workerThreadPool.Shutdown(true, 500);
        }
        
        [Test]
        public void SetMinimumWorkerThreadsCountShouldThrowExceptionWhenMinimumThreadsCountIsLessThanZero()
        {
            WorkerThreadPool workerThreadPool = new WorkerThreadPool(0, 1);

            Assert.Throws<ArgumentOutOfRangeException>(() => workerThreadPool.SetMinimumWorkerThreadsCount(-1));

            workerThreadPool.Shutdown(true, 500);
        }

        [Test]
        public void SetMinimumWorkerThreadsCountShouldThrowExceptionWhenMinimumThreadsCountIsBiggerThanMaximumCount()
        {
            WorkerThreadPool workerThreadPool = new WorkerThreadPool(2, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() => workerThreadPool.SetMinimumWorkerThreadsCount(4));

            workerThreadPool.Shutdown(true, 500);
        }

        /// <summary>
        /// İşçi threadlerin zaman aşımı durumu;
        /// SÜRE        İŞ PARÇASI SÜRESİ   BEKLEYEN İŞÇİ THREADLER     İŞ PARÇASINI İŞLEYEN İŞÇİ THREAD
        /// 00:00:00    1.5                 A, B, C, D                  A
        /// 00:00:01    1.5                 B, C, D                     B
        /// 00:00:02    1.5                 A, C, D                     A
        /// 00:00:03    1.5                 B, C, D                     B
        /// 
        /// Süre zaman aşımı süresine ulaştığında C ve D işçi thread listesinden çıkarılacaktır.
        /// </summary>
        [Test]
        public void WorkerThreadIdleTimeOutTest()
        {
            WorkerThreadPool workerThreadPool = new WorkerThreadPool(800, 2, 4);
            
            // İş parçası süresi / İş parçası eklenme süresi = 400 / 100 => 4 İşçi Thread gerekli.
            int workItemDuration = 400;
            Timer timer = new Timer(100);
            timer.AutoReset = true;
            timer.Elapsed += (sender, args) =>
                {
                    workerThreadPool.QueueWorkItem(
                        new ActionWorkItem(
                            () =>
                                {
                                    Thread.Sleep(workItemDuration);
                                }));
                };
            timer.Start();

            Thread.Sleep(3000);

            Assert.AreEqual(4, workerThreadPool.WorkerThreadsCount);

            // İş parçası süresi / İş parçası eklenme süresi = 150 / 100 => 2 İşçi Thread gerekli.
            Interlocked.Add(ref workItemDuration, -250); // 400 - 250 = 150

            Thread.Sleep(3000);

            Assert.AreEqual(2, workerThreadPool.WorkerThreadsCount);

            timer.Stop();

            workerThreadPool.Shutdown(true, 3000);
        }

        [Test]
        public void QueueWorkItemShouldAddANewWorkerThreadWhenMaxThreadCountIsNotReached()
        {
            ManualFinishWorkItemQueueStub workItemQueue = new ManualFinishWorkItemQueueStub();
            workItemQueue.Enqueue(new ManualFinishWorkItem());

            WorkerThreadPool workerThreadPool = new WorkerThreadPool(workItemQueue, 0, 2);

            Assert.AreEqual(1, workerThreadPool.WorkerThreadsCount);

            workItemQueue.WaitAllStart(500);

            Assert.AreEqual(1, workerThreadPool.WorkerThreadsCount);

            ManualFinishWorkItem newWorkItem = new ManualFinishWorkItem();
            workerThreadPool.QueueWorkItem(newWorkItem);
            newWorkItem.WaitStart(500);

            Assert.AreEqual(2, workerThreadPool.WorkerThreadsCount);
            Assert.AreEqual(2, workerThreadPool.WorkerThreads.Count(x => x.IsBusy));

            workItemQueue.StopAll();

            workerThreadPool.Shutdown(true, 500);
        }

        [Test]
        public void QueueWorkItemShouldThrowExceptionWhenWorkerThreadPoolIsShutDown()
        {
            WorkerThreadPool workerThreadPool = new WorkerThreadPool();
            workerThreadPool.Shutdown();

            Assert.Throws<LaboThreadingException>(() => workerThreadPool.QueueWorkItem(Substitute.For<IWorkItem>()));
        }

        [Test]
        public void QueueWorkItemShouldThrowExceptionWhenWorkItemIsNull()
        {
            WorkerThreadPool workerThreadPool = new WorkerThreadPool();
            Assert.Throws<ArgumentNullException>(() => workerThreadPool.QueueWorkItem(null));
        }
    }
}
