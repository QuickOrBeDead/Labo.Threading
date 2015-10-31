namespace Labo.Threading
{
    using System;

    /// <summary>
    /// Action iþ parçasý.
    /// Ýþ parçasýný çalýþtýrmak ve durdurmak için dýþarýdan Action delegesini alýr.
    /// </summary>
    public sealed class ActionWorkItem : WorkItemBase
    {
        private readonly Action m_DoWorkAction;

        private readonly Action m_StopWorkAction;

        /// <summary>
        /// Action iþ parçasý inþacý metodu.
        /// </summary>
        /// <param name="doWorkAction">Ýþ parçasý çalýþtýðýnda tetiklenecek olan delege.</param>
        /// <param name="stopWorkAction">Ýþ parçasý durdurulduðunda tetiklenecek olan delege.</param>
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
        /// Ýþ parçasýný çalýþtýran asýl metod.
        /// </summary>
        protected internal override void DoWorkInternal()
        {
            m_DoWorkAction();
        }

        /// <summary>
        /// Ýþ parçasýný durduran asýl metod.
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