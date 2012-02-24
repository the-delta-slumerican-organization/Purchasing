﻿using Purchasing.Core.Domain;

namespace Purchasing.WS
{
    public interface IFinancialSystemService
    {
        /// <summary>
        /// Submits the order to the campus financial system
        /// </summary>
        /// <param name="order">Order to be submitted</param>
        /// <param name="userId">Kerb id of user to save the document to</param>
        SubmitResult SubmitOrder(Order order, string userId);

        /// <summary>
        /// Gets updated information from the campus financial system
        /// </summary>
        /// <param name="docNumber"></param>
        /// <returns></returns>
        FinancialDocumentStatus GetOrderStatus(string docNumber);

        /// <summary>
        /// Gets a document url to link into campus financial system
        /// </summary>
        /// <param name="docNumber"></param>
        /// <returns></returns>
        string GetDocumentUrl(string docNumber);
    }
}
