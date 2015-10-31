namespace Labo.Threading
{
    /// <summary>
    /// Ýþ parçasý bekleyici kaydý yýðýný arayüzü.
    /// </summary>
    internal interface IWorkItemWaiterEntryStack
    {
        /// <summary>
        /// Ýþ parçasý bekleyici kaydý yýðýný sayýsýný getirir.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Ýþ parçasý bekleyici kaydý yýðýnýndan bir tane kayýt çýkartýp döndürür.
        /// </summary>
        /// <returns>Yýðýnýn en üstündeki iþ parçasý bekleyici kaydý.</returns>
        IWorkItemWaiterEntry Pop();

        /// <summary>
        /// Ýþ parçasý bekleyici kaydý yýðýnýnýn en üstteki kaydýný yýðýndan çýkarmadan döndürür.
        /// </summary>
        /// <returns>Yýðýnýn en üstündeki iþ parçasý bekleyici kaydý.</returns>
        IWorkItemWaiterEntry Peek();

        /// <summary>
        /// Ýþ parçasý bekleyici kaydý yýðýnýna bir tane kayýt ekler.
        /// </summary>
        /// <param name="workItemWaiterEntry">Ýþ parçasý bekleyici kaydý.</param>
        void Push(IWorkItemWaiterEntry workItemWaiterEntry);

        /// <summary>
        /// Ýþ parçasý bekleyici kaydý yýðýnýna bir tane kayýt çýkarýr.
        /// </summary>
        /// <param name="workItemWaiterEntry">Ýþ parçasý bekleyici kaydý.</param>
        /// <returns><c>true</c> eðer iþ parçasý bekleyici kaydý çýkarýldýysa, yoksa; <c>false</c></returns>
        bool Remove(IWorkItemWaiterEntry workItemWaiterEntry);
    }
}