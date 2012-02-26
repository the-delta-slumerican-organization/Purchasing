﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Purchasing.Core.Domain;
using UCDArch.Core.PersistanceSupport;

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
    }
}