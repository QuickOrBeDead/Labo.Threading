namespace Labo.Threading
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// �� par�as� bekleyici kayd� y���n� s�n�f�.
    /// </summary>
    internal sealed class WorkItemWaiterEntryStack : IWorkItemWaiterEntryStack
    {
        /// <summary>
        /// �� par�as� bekleyici kayd� listesi.
        /// </summary>
        private readonly LinkedList<IWorkItemWaiterEntry> m_WorkItemWaiterEntries;

        /// <summary>
        /// �� par�as� bekleyici kayd� y���n� say�s�n� getirir.
        /// </summary>
        public int Count
        {
            get
            {
                return m_WorkItemWaiterEntries.Count;
            }
        }

        /// <summary>
        /// �� par�as� bekleyici kayd� y���n� in�ac� metodu.
        /// </summary>
        public WorkItemWaiterEntryStack()
        {
            m_WorkItemWaiterEntries = new LinkedList<IWorkItemWaiterEntry>();
        }

        /// <summary>
        /// �� par�as� bekleyici kayd� y���n�ndan bir tane kay�t ��kart�p d�nd�r�r.
        /// </summary>
        /// <returns>Y���n�n en �st�ndeki i� par�as� bekleyici kayd�.</returns>
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
        /// �� par�as� bekleyici kayd� y���n�n�n en �stteki kayd�n� y���ndan ��karmadan d�nd�r�r.
        /// </summary>
        /// <returns>Y���n�n en �st�ndeki i� par�as� bekleyici kayd�.</returns>
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
        /// �� par�as� bekleyici kayd� y���n�na bir tane kay�t ekler.
        /// </summary>
        /// <param name="workItemWaiterEntry">�� par�as� bekleyici kayd�.</param>
        public void Push(IWorkItemWaiterEntry workItemWaiterEntry)
        {
            if (workItemWaiterEntry == null)
            {
                throw new ArgumentNullException("workItemWaiterEntry");
            }

            m_WorkItemWaiterEntries.AddFirst(workItemWaiterEntry);
        }

        /// <summary>
        /// �� par�as� bekleyici kayd� y���n�na bir tane kay�t ��kar�r.
        /// </summary>
        /// <param name="workItemWaiterEntry">�� par�as� bekleyici kayd�.</param>
        /// <returns><c>true</c> e�er i� par�as� bekleyici kayd� ��kar�ld�ysa, yoksa; <c>false</c></returns>
        public bool Remove(IWorkItemWaiterEntry workItemWaiterEntry)
        {
            return m_WorkItemWaiterEntries.Remove(workItemWaiterEntry);
        }
    }
}