namespace Labo.Threading.Tests.Stubs
{
    using System.Threading;

    internal sealed class ManualFinishWorkItem : WorkItemBase
    {
        private readonly ManualResetEvent m_StopWaitHandle = new ManualResetEvent(false);
        private readonly ManualResetEvent m_StartWaitHandle = new ManualResetEvent(false);

        protected internal override void DoWorkInternal()
        {
            m_StartWaitHandle.Set();
            m_StopWaitHandle.WaitOne();
        }

        protected internal override void StopInternal()
        {
            m_StopWaitHandle.Set();
        }

        public void WaitStart(int millisecondsTimeout = -1)
        {
            m_StartWaitHandle.WaitOne(millisecondsTimeout);
        }
    }
}
