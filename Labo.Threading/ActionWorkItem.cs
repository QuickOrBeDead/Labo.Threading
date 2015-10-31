namespace Labo.Threading
{
    using System;

    /// <summary>
    /// Action i� par�as�.
    /// �� par�as�n� �al��t�rmak ve durdurmak i�in d��ar�dan Action delegesini al�r.
    /// </summary>
    public sealed class ActionWorkItem : WorkItemBase
    {
        private readonly Action m_DoWorkAction;

        private readonly Action m_StopWorkAction;

        /// <summary>
        /// Action i� par�as� in�ac� metodu.
        /// </summary>
        /// <param name="doWorkAction">�� par�as� �al��t���nda tetiklenecek olan delege.</param>
        /// <param name="stopWorkAction">�� par�as� durduruldu�unda tetiklenecek olan delege.</param>
        public ActionWorkItem(Action doWorkAction, Action stopWorkAction = null)
        {
            if (doWorkAction == null)
            {
                throw new ArgumentNullException("doWorkAction");
            }

            m_DoWorkAction = doWorkAction;
            m_StopWorkAction = stopWorkAction;
        }

        /// <summary>
        /// �� par�as�n� �al��t�ran as�l metod.
        /// </summary>
        protected internal override void DoWorkInternal()
        {
            m_DoWorkAction();
        }

        /// <summary>
        /// �� par�as�n� durduran as�l metod.
        /// </summary>
        protected internal override void StopInternal()
        {
            if (m_StopWorkAction != null)
            {
                m_StopWorkAction();
            }
        }
    }
}