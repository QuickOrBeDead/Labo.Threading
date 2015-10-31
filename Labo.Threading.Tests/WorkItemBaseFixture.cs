namespace Labo.Threading.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using Labo.Common.Patterns;

    using NSubstitute;

    using NUnit.Framework;

    [TestFixture]
    public class WorkItemBaseFixture
    {
        [Test]
        public void DoWorkShouldCallDoWorkInternal()
        {
            WorkItemBase workItemBase = Substitute.ForPartsOf<WorkItemBase>();
            workItemBase.DoWork();

            workItemBase.Received(1).DoWorkInternal();
        }

        [Test]
        public void DoWorkStateShouldBeRunningWhenDoWorkInternalIsCalled()
        {
            WorkItemBase workItemBase = Substitute.ForPartsOf<WorkItemBase>();
            workItemBase.When(x => x.DoWorkInternal()).Do(
                x =>
                    {
                        Assert.AreEqual(WorkItemState.Running, workItemBase.State);
                    });
            workItemBase.DoWork();
        }

        [Test]
        public void DoWorkStartDateShouldBeSetWhenDoWorkInternalIsCalled()
        {
            WorkItemBase workItemBase = Substitute.ForPartsOf<WorkItemBase>();
            workItemBase.When(x => x.DoWorkInternal()).Do(
                x =>
                {
                    Assert.AreEqual(true, workItemBase.StartDateUtc.HasValue);
                });
            workItemBase.DoWork();
        }

        [Test]
        public void DoWorkEndDateShouldNotBeSetWhenDoWorkInternalIsCalled()
        {
            WorkItemBase workItemBase = Substitute.ForPartsOf<WorkItemBase>();
            workItemBase.When(x => x.DoWorkInternal()).Do(
                x =>
                {
                    Assert.AreEqual(false, workItemBase.EndDateUtc.HasValue);
                });
            workItemBase.DoWork();
        }

        [Test]
        public void DoWorkShouldSetEndDate()
        {
            WorkItemBase workItemBase = Substitute.ForPartsOf<WorkItemBase>();
            workItemBase.DoWork();   
         
            Assert.AreEqual(true, workItemBase.EndDateUtc.HasValue);
        }

        [Test]
        public void DoWorkStateShouldBeCompletedAfterExecutionCompleted()
        {
            WorkItemBase workItemBase = Substitute.ForPartsOf<WorkItemBase>();
            workItemBase.DoWork();

            Assert.AreEqual(WorkItemState.Completed, workItemBase.State);
        }

        [Test]
        public void DoWorkStateShouldBeCompletedWithExceptionWhenExceptionOccurs()
        {
            Exception invalidCastException = new InvalidCastException();

            WorkItemBase workItemBase = Substitute.ForPartsOf<WorkItemBase>();
            workItemBase.When(x => x.DoWorkInternal()).Do(
                x =>
                    {
                        throw invalidCastException;
                    });
            workItemBase.DoWork();

            Assert.AreEqual(WorkItemState.CompletedWithException, workItemBase.State);
            Assert.AreSame(invalidCastException, workItemBase.LastException);
        }

        [Test]
        public void DoWorkShouldSetLastExceptionWhenExceptionOccurs()
        {
            Exception invalidCastException = new InvalidCastException();

            WorkItemBase workItemBase = Substitute.ForPartsOf<WorkItemBase>();
            workItemBase.When(x => x.DoWorkInternal()).Do(
                x =>
                {
                    throw invalidCastException;
                });
            workItemBase.DoWork();

            Assert.AreSame(invalidCastException, workItemBase.LastException);
        }

        [Test]
        public void ExecutionTime()
        {
            IList<DateTime> timeList =new List<DateTime>();
            IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
            dateTimeProvider.UtcNow.Returns(
                x =>
                    {
                        DateTime date = DateTime.UtcNow;
                        timeList.Add(date);
                        return date;
                    });
            WorkItemBase workItemBase = Substitute.ForPartsOf<WorkItemBase>(dateTimeProvider);
            workItemBase.When(x => x.DoWorkInternal()).Do(
                x =>
                    {
                        Thread.Sleep(100);
                    });
            workItemBase.DoWork();

            Assert.AreEqual(3, timeList.Count);
            Assert.AreEqual(timeList[2] - timeList[1], workItemBase.ExecutionTime);
        }

        [Test]
        public void ExecutionTimeShouldBeZeroWhenStartDateOrEndDateIsNull()
        {
            WorkItemBase workItemBase = Substitute.ForPartsOf<WorkItemBase>();
            Assert.AreEqual(TimeSpan.Zero, workItemBase.ExecutionTime);
        }

        [Test]
        public void StopShouldCallStopInternal()
        {
            WorkItemBase workItemBase = Substitute.ForPartsOf<WorkItemBase>();
            workItemBase.Stop();

            workItemBase.Received(1).StopInternal();
        }

        [Test]
        public void StopShouldSetEndDate()
        {
            WorkItemBase workItemBase = Substitute.ForPartsOf<WorkItemBase>();
            workItemBase.Stop();

            Assert.AreEqual(true, workItemBase.EndDateUtc.HasValue);
        }

        [Test]
        public void StopShouldSetStateToStopped()
        {
            WorkItemBase workItemBase = Substitute.ForPartsOf<WorkItemBase>();
            workItemBase.Stop();

            Assert.AreEqual(WorkItemState.Stopped, workItemBase.State);
        }
    }
}
