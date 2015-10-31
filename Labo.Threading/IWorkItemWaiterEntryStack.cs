namespace Labo.Threading
{
    /// <summary>
    /// �� par�as� bekleyici kayd� y���n� aray�z�.
    /// </summary>
    internal interface IWorkItemWaiterEntryStack
    {
        /// <summary>
        /// �� par�as� bekleyici kayd� y���n� say�s�n� getirir.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// �� par�as� bekleyici kayd� y���n�ndan bir tane kay�t ��kart�p d�nd�r�r.
        /// </summary>
        /// <returns>Y���n�n en �st�ndeki i� par�as� bekleyici kayd�.</returns>
        IWorkItemWaiterEntry Pop();

        /// <summary>
        /// �� par�as� bekleyici kayd� y���n�n�n en �stteki kayd�n� y���ndan ��karmadan d�nd�r�r.
        /// </summary>
        /// <returns>Y���n�n en �st�ndeki i� par�as� bekleyici kayd�.</returns>
        IWorkItemWaiterEntry Peek();

        /// <summary>
        /// �� par�as� bekleyici kayd� y���n�na bir tane kay�t ekler.
        /// </summary>
        /// <param name="workItemWaiterEntry">�� par�as� bekleyici kayd�.</param>
        void Push(IWorkItemWaiterEntry workItemWaiterEntry);

        /// <summary>
        /// �� par�as� bekleyici kayd� y���n�na bir tane kay�t ��kar�r.
        /// </summary>
        /// <param name="workItemWaiterEntry">�� par�as� bekleyici kayd�.</param>
        /// <returns><c>true</c> e�er i� par�as� bekleyici kayd� ��kar�ld�ysa, yoksa; <c>false</c></returns>
        bool Remove(IWorkItemWaiterEntry workItemWaiterEntry);
    }
}