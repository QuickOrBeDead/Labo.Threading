namespace Labo.Threading.Tests
{
    using NUnit.Framework;

    [TestFixture(Category = "ActionWorkItem")]
    public sealed class ActionWorkItemFixture
    {
        [Test]
        public void DoWorkShouldCallDoWorkAction()
        {
            bool doWorkActionIsCalledIsCalled = false;
            ActionWorkItem actionWorkItem = new ActionWorkItem(() => doWorkActionIsCalledIsCalled = true);
            actionWorkItem.DoWork();

            Assert.AreEqual(true, doWorkActionIsCalledIsCalled);
        }

        [Test]
        public void StopShouldCallStopWorkAction()
        {
            bool stopWorkActionIsCalled = false;
            ActionWorkItem actionWorkItem = new ActionWorkItem(() => {}, () => stopWorkActionIsCalled = true);
            actionWorkItem.Stop();

            Assert.AreEqual(true, stopWorkActionIsCalled);
        }

        [Test]
        public void StopShouldNotExplodeWhenStopWorkActionIsNull()
        {
            ActionWorkItem actionWorkItem = new ActionWorkItem(() => {});
            Assert.DoesNotThrow(() => actionWorkItem.Stop());
        }
    }
}
