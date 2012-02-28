﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Purchasing.Core.Domain;
using UCDArch.Core.PersistanceSupport;
using System.Text;

namespace Purchasing.Web.Models
{
    public class FilteredOrderListModelDto
    {
        public List<OrderHistoryDto> OrderHistoryDtos { get; set; }

        public class OrderHistoryDto
        {
            public Order Order { get; set; }
            public string Workgroup { get; set; }
            public WorkgroupVendor Vendor { get; set; }
            public string CreatedBy { get; set; }
            public string Status { get; set; }
        }

        // for building dropdown list
        public List<Tuple<string, string>> OrderStatusCodes { get; set; }
        
        public string SelectedOrderStatus { get; set; }
        public bool ShowPending { get; set; }
        [Display(Name = "Created After")]
        public DateTime? StartDate { get; set; }
        [Display(Name = "Created Before")]
        public DateTime? EndDate { get; set; }
        public ColumnPreferences ColumnPreferences { get; set; }
        public string ShowLast { get; set; }

        /// <summary>
        /// Returns true if the user wants to view a column that requires order tracking info
        /// </summary>
        /// <remarks>
        /// Anything that shows "acted on" info will require order tracking history info
        /// </remarks>
        /// <returns></returns>
        public bool RequresOrderTracking()
        {
            return ColumnPreferences.ShowDaysNotActedOn || ColumnPreferences.ShowLastActedOnBy ||
                   ColumnPreferences.ShowLastActedOnDate;
        }

        public void PopulateStatusCodes(IRepositoryWithTypedId<OrderStatusCode, string> statusCodeRepository, List<Tuple<string, string>> orderStatusCodes = null)
        {
            if (orderStatusCodes == null)
            {
                OrderStatusCodes = new List<Tuple<string, string>> {new Tuple<string, string>("All", "All")};
                OrderStatusCodes.AddRange(statusCodeRepository.Queryable
                    .Where(a => a.ShowInFilterList)
                    .OrderBy(a => a.Level)
                    .Select(a => new Tuple<string, string>(a.Id, a.Name))
                    .ToList());
            }
            else
            {
                OrderStatusCodes = orderStatusCodes;
            }
        }

        public List<OrderTracking> OrderTracking { get; set; }

        public bool RequiresSplits()
        {
            return ColumnPreferences.ShowAccountNumber;
        }

        public bool RequiresApprovals()
        {
            return ColumnPreferences.ShowApprover || ColumnPreferences.ShowAccountManager ||
                   ColumnPreferences.ShowPurchaser;
        }

        public string GetNameFromApprovalsForOrder(string orderStatusCodeId, int orderId)
        {
            var approvalList = Approvals.Where(x => x.Order.Id == orderId && x.StatusCode.Id == orderStatusCodeId).ToList();

            var approvals = new StringBuilder();
            var firstApproval = true;

            foreach (var approval in approvalList)
            {
                var token = firstApproval ? string.Empty : ", ";
                approvals.Append(token); //append either nothing for the first approval or a comma/space for future approvals

                if (approval.User == null)
                {
                    approvals.Append("[Workgroup]");
                }else
                {
                    if (approval.User.IsActive && !approval.User.IsAway) //User is not away show them
                    {
                        approvals.Append(approval.User.FullName);
                    }
                    if (approval.SecondaryUser != null && approval.SecondaryUser.IsActive && !approval.SecondaryUser.IsAway) //Primary user is away, show Secondary if active and not away
                    {
                        approvals.Append(approval.SecondaryUser.FullName);
                    }
                }

                firstApproval = false;
            }

            return approvals.ToString();
        }

        public List<Split> Splits { get; set; }

        public List<Approval> Approvals { get; set; }
    }
}