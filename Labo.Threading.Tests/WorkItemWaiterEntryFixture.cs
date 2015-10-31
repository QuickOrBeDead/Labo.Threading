namespace Labo.Threading.Tests
{
    using NUnit.Framework;
    using System.Threading;

    using NSubstitute;

    [TestFixture(Category = "WorkItemWaiterEntry")]
    public class WorkItemWaiterEntryFixture
    {
        [Test]
        public void ConstructorShouldSetCurrentThreadName()
        {
            WorkItemWaiterEntry entry = new WorkItemWaiterEntry();

            Assert.AreEqual(Thread.CurrentThread.Name, entry.CurrentThreadName);
        }

        [Test]
        public void WorkItemShouldBeNullWhenInitiated()
        {
            WorkItemWaiterEntry entry = new WorkItemWaiterEntry();

            Assert.AreEqual(null, entry.WorkItem);
        }

        [Test]
        public void IsSignaledShouldBeFasleWhenInitiated()
        {
            WorkItemWaiterEntry entry = new WorkItemWaiterEntry();

            Assert.AreEqual(false, entry.IsSignaled);
        }

        [Test]
        public void IsIsTimedOutShouldBeFasleWhenInitiated()
        {
            WorkItemWaiterEntry entry = new WorkItemWaiterEntry();

            Assert.AreEqual(false, entry.IsTimedOut);
        }

        [Test]
        public void TrySignalShouldSetWorkItemAndSetIsSignaledToTrueWhenIsTimedOutIsFalse()
        {
            WorkItemWaiterEntry entry = new WorkItemWaiterEntry();

            IWorkItem workItem = Substitute.For<IWorkItem>();
            Assert.AreEqual(true, entry.TrySignal(workItem));
            Assert.AreEqual(false, entry.IsTimedOut);
            Assert.AreEqual(true, entry.IsSignaled);
            Assert.AreEqual(workItem, entry.WorkItem);
        }

        [Test]
        public void TrySignalShouldNotSetWorkItemAndSetIsSignaledWhenIsTimedOutIsTrue()
        {
            WorkItemWaiterEntry entry = new WorkItemWaiterEntry();
            Assert.AreEqual(true, entry.TrySetToTimedOut());

            IWorkItem workItem = Substitute.For<IWorkItem>();
            Assert.AreEqual(false, entry.TrySignal(workItem));
            Assert.AreEqual(true, entry.IsTimedOut);
            Assert.AreEqual(false, entry.IsSignaled);
            Assert.AreEqual(null, entry.WorkItem);
        }

        [Test]
        public void TrySetToTimedOutShouldSetIsTimedOutToTrueIfIsNotSignaled()
        {
            WorkItemWaiterEntry entry = new WorkItemWaiterEntry();
            Assert.AreEqual(true, entry.TrySetToTimedOut());
            Assert.AreEqual(false, entry.IsSignaled);
            Assert.AreEqual(true, entry.IsTimedOut);
        }

        [Test]
        public void TrySetToTimedOutShouldNotSetIsTimedOutToTrueIfIsSignalled()
        {
            WorkItemWaiterEntry entry = new WorkItemWaiterEntry();
            Assert.AreEqual(true, entry.TrySignal(Substitute.For<IWorkItem>()));
            Assert.AreEqual(false, entry.TrySetToTimedOut());
            Assert.AreEqual(true, entry.IsSignaled);
            Assert.AreEqual(false, entry.IsTimedOut);
        }

        [Test]
        public void ResetShouldResetTheFields()
        {
            WorkItemWaiterEntry entry = new WorkItemWaiterEntry();
            entry.TrySignal(Substitute.For<IWorkItem>());
            entry.TrySetToTimedOut();

            entry.Reset();
            
            Assert.AreEqual(false, entry.IsSignaled);
            Assert.AreEqual(false, entry.IsTimedOut);
            Assert.AreEqual(null, entry.WorkItem);
        }

        [Test]
        public void DisposeMethodShouldNotExplode()
        {
            WorkItemWaiterEntry entry = new WorkItemWaiterEntry();

            Assert.DoesNotThrow(() => entry.Dispose());
        }
    }
}
