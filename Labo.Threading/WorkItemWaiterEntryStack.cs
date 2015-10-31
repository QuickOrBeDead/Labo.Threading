namespace Labo.Threading
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Ýþ parçasý bekleyici kaydý yýðýný sýnýfý.
    /// </summary>
    internal sealed class WorkItemWaiterEntryStack : IWorkItemWaiterEntryStack
    {
        /// <summary>
        /// Ýþ parçasý bekleyici kaydý listesi.
        /// </summary>
        private readonly LinkedList<IWorkItemWaiterEntry> m_WorkItemWaiterEntries;

        /// <summary>
        /// Ýþ parçasý bekleyici kaydý yýðýný sayýsýný getirir.
        /// </summary>
        public int Count
        {
            get
            {
                return m_WorkItemWaiterEntries.Count;
            }
        }

        /// <summary>
        /// Ýþ parçasý bekleyici kaydý yýðýný inþacý metodu.
        /// </summary>
        public WorkItemWaiterEntryStack()
        {
            m_WorkItemWaiterEntries = new LinkedList<IWorkItemWaiterEntry>();
        }

        /// <summary>
        /// Ýþ parçasý bekleyici kaydý yýðýnýndan bir tane kayýt çýkartýp döndürür.
        /// </summary>
        /// <returns>Yýðýnýn en üstündeki iþ parçasý bekleyici kaydý.</returns>
        public IWorkItemWaiterEntry Pop()
        {
            LinkedListNode<IWorkItemWaiterEntry> firstNode = m_WorkItemWaiterEntries.First;
            if (firstNode == null)
            {
                return null;
            }

            IWorkItemWaiterEntry first = firstNode.Value;
            
            m_WorkItemWaiterEntries.RemoveFirst();

            return first;
        }

        /// <summary>
        /// Ýþ parçasý bekleyici kaydý yýðýnýnýn en üstteki kaydýný yýðýndan çýkarmadan döndürür.
        /// </summary>
        /// <returns>Yýðýnýn en üstündeki iþ parçasý bekleyici kaydý.</returns>
        public IWorkItemWaiterEntry Peek()
        {
            LinkedListNode<IWorkItemWaiterEntry> firstNode = m_WorkItemWaiterEntries.First;
            if (firstNode == null)
            {
                return null;
            }

            IWorkItemWaiterEntry first = firstNode.Value;

            return first;
        }

        /// <summary>
        /// Ýþ parçasý bekleyici kaydý yýðýnýna bir tane kayýt ekler.
        /// </summary>
        /// <param name="workItemWaiterEntry">Ýþ parçasý bekleyici kaydý.</param>
        public void Push(IWorkItemWaiterEntry workItemWaiterEntry)
        {
            if (workItemWaiterEntry == null)
            {
                throw new ArgumentNullException("workItemWaiterEntry");
            }

            m_WorkItemWaiterEntries.AddFirst(workItemWaiterEntry);
        }

        /// <summary>
        /// Ýþ parçasý bekleyici kaydý yýðýnýna bir tane kayýt çýkarýr.
        /// </summary>
        /// <param name="workItemWaiterEntry">Ýþ parçasý bekleyici kaydý.</param>
        /// <returns><c>true</c> eðer iþ parçasý bekleyici kaydý çýkarýldýysa, yoksa; <c>false</c></returns>
        public bool Remove(IWorkItemWaiterEntry workItemWaiterEntry)
        {
            return m_WorkItemWaiterEntries.Remove(workItemWaiterEntry);
        }
    }
}