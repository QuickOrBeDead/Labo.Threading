namespace Labo.Threading.Tests
{
    using System;

    using NSubstitute;

    using NUnit.Framework;

    [TestFixture(Category = "WorkItemWaiterEntryStack")]
    public class WorkItemWaiterEntryStackFixture
    {
        [Test]
        public void InitialCountShoudBeZero()
        {
            WorkItemWaiterEntryStack stack = new WorkItemWaiterEntryStack();
            Assert.AreEqual(0, stack.Count);
        }

        [Test]
        public void PopShouldReturnNullWhenStackIsEmpty()
        {
            WorkItemWaiterEntryStack stack = new WorkItemWaiterEntryStack();
            Assert.AreEqual(null, stack.Pop());
        }

        [Test]
        public void PopShouldReturnTheOnlyItemAndSetCountToZeroWhenStackHasOneItems()
        {
            WorkItemWaiterEntryStack stack = new WorkItemWaiterEntryStack();
            IWorkItemWaiterEntry entry = Substitute.For<IWorkItemWaiterEntry>();
            stack.Push(entry);

            Assert.AreEqual(entry, stack.Pop());
            Assert.AreEqual(0, stack.Count);

            Assert.AreEqual(null, stack.Pop());
            Assert.AreEqual(0, stack.Count);
        }

        [Test]
        public void PopShouldReturnLastAddedItemAndDecrementCountWhenStackHasMoreThanOneItems()
        {
            WorkItemWaiterEntryStack stack = new WorkItemWaiterEntryStack();
            IWorkItemWaiterEntry firstEntry = Substitute.For<IWorkItemWaiterEntry>();
            IWorkItemWaiterEntry lastEntry = Substitute.For<IWorkItemWaiterEntry>();

            stack.Push(firstEntry);
            stack.Push(lastEntry);

            Assert.AreEqual(2, stack.Count);

            Assert.AreEqual(lastEntry, stack.Pop());
            Assert.AreEqual(1, stack.Count);

            Assert.AreEqual(firstEntry, stack.Pop());
            Assert.AreEqual(0, stack.Count);

            Assert.AreEqual(null, stack.Pop());
            Assert.AreEqual(0, stack.Count);
        }

        [Test]
        public void PeekShouldReturnNullIfStackIsEmpty()
        {
            WorkItemWaiterEntryStack stack = new WorkItemWaiterEntryStack();
            Assert.AreEqual(null, stack.Peek());
        }

        [Test]
        public void PeekShouldReturnTheOnlyItemAndShouldNotRemovwItemWhenStackHasOneItems()
        {
            WorkItemWaiterEntryStack stack = new WorkItemWaiterEntryStack();
            IWorkItemWaiterEntry entry = Substitute.For<IWorkItemWaiterEntry>();
            stack.Push(entry);

            Assert.AreEqual(entry, stack.Peek());
            Assert.AreEqual(1, stack.Count);

            Assert.AreEqual(entry, stack.Peek());
            Assert.AreEqual(1, stack.Count);
        }

        [Test]
        public void PeekShouldReturnLastAddedItemAndShouldNotRemoveItemWhenStackHasMoreThanOneItems()
        {
            WorkItemWaiterEntryStack stack = new WorkItemWaiterEntryStack();
            IWorkItemWaiterEntry firstEntry = Substitute.For<IWorkItemWaiterEntry>();
            IWorkItemWaiterEntry lastEntry = Substitute.For<IWorkItemWaiterEntry>();

            stack.Push(firstEntry);
            stack.Push(lastEntry);

            Assert.AreEqual(2, stack.Count);

            Assert.AreEqual(lastEntry, stack.Peek());
            Assert.AreEqual(2, stack.Count);

            Assert.AreEqual(lastEntry, stack.Peek());
            Assert.AreEqual(2, stack.Count);
        }

        [Test]
        public void PushShouldAddTheItemToFirstPlace()
        {
            WorkItemWaiterEntryStack stack = new WorkItemWaiterEntryStack();
            IWorkItemWaiterEntry firstEntry = Substitute.For<IWorkItemWaiterEntry>();
            IWorkItemWaiterEntry lastEntry = Substitute.For<IWorkItemWaiterEntry>();

            stack.Push(firstEntry);

            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(firstEntry, stack.Peek());

            stack.Push(lastEntry);

            Assert.AreEqual(lastEntry, stack.Peek());
            Assert.AreEqual(2, stack.Count);
        }

        [Test]
        public void PushShouldThrowExceptionWhenItemIsEmpty()
        {
            WorkItemWaiterEntryStack stack = new WorkItemWaiterEntryStack();

            Assert.Throws<ArgumentNullException>(() => stack.Push(null));
        }

        [Test]
        public void RemoveShouldRemoveItemFromStack()
        {
            WorkItemWaiterEntryStack stack = new WorkItemWaiterEntryStack();
            IWorkItemWaiterEntry firstEntry = Substitute.For<IWorkItemWaiterEntry>();
            IWorkItemWaiterEntry lastEntry = Substitute.For<IWorkItemWaiterEntry>();

            stack.Push(firstEntry);
            stack.Push(lastEntry);

            Assert.AreEqual(2, stack.Count);

            Assert.AreEqual(lastEntry, stack.Peek());
            Assert.AreEqual(true, stack.Remove(lastEntry));
            Assert.AreEqual(firstEntry, stack.Peek());

            Assert.AreEqual(1, stack.Count);

            Assert.AreEqual(true, stack.Remove(firstEntry));
            Assert.AreEqual(0, stack.Count);
        }

        public void RemoveShouldReturnFalseIfTheSpecifiedItemIsNotFoundInTheStack()
        {
            WorkItemWaiterEntryStack stack = new WorkItemWaiterEntryStack();
            IWorkItemWaiterEntry firstEntry = Substitute.For<IWorkItemWaiterEntry>();
            IWorkItemWaiterEntry lastEntry = Substitute.For<IWorkItemWaiterEntry>();

            Assert.AreEqual(false, stack.Remove(firstEntry));

            stack.Push(firstEntry);

            Assert.AreEqual(1, stack.Count);

            Assert.AreEqual(false, stack.Remove(lastEntry));
        }
    }
}
