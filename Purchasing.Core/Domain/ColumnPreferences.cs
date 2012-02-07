﻿using System.ComponentModel.DataAnnotations;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace Purchasing.Core.Domain
{
    public class ColumnPreferences : DomainObjectWithTypedId<string>
    {
        public ColumnPreferences()
        {
            ShowWorkgroup = true;
            ShowVendor = true;
            ShowCreatedBy = true;
            ShowCreatedDate = true;
            ShowStatus = true;
            ShowNeededDate = true;
            ShowDaysNotActedOn = true;
            ShowAccountManager = true;
        }

        public ColumnPreferences(string id) : this() {Id = id;}
        [Display(Name = "Show Request Number")]
        public virtual bool ShowRequestNumber { get; set; }
        [Display(Name = "Show PO #")]
        public virtual bool ShowPurchaseOrderNumber { get; set; }
        [Display(Name = "Show Workgroup")]
        public virtual bool ShowWorkgroup { get; set; }
        [Display(Name = "Show Organization")]
        public virtual bool ShowOrganization { get; set; }
        [Display(Name = "Show Vendor")]
        public virtual bool ShowVendor { get; set; }
        [Display(Name = "Show Ship To")]
        public virtual bool ShowShipTo { get; set; }
        [Display(Name = "Show Allow Backorder")]
        public virtual bool ShowAllowBackorder { get; set; }
        [Display(Name = "Show Restricted Orders")]
        public virtual bool ShowRestrictedOrder { get; set; }
        [Display(Name = "Show Has Splits")]
        public virtual bool ShowHasSplits { get; set; }
        [Display(Name = "Show Has Attachments")]
        public virtual bool ShowHasAttachments { get; set; }
        [Display(Name = "Show # Of Lines")]
        public virtual bool ShowNumberOfLines { get; set; }
        [Display(Name = "Show Total Amount")]
        public virtual bool ShowTotalAmount { get; set; }
        [Display(Name = "Show Created By")]
        public virtual bool ShowCreatedBy { get; set; }
        [Display(Name = "Show Created Date")]
        public virtual bool ShowCreatedDate { get; set; }
        [Display(Name = "Show Status")]
        public virtual bool ShowStatus { get; set; }
        [Display(Name = "Show Needed Date")]
        public virtual bool ShowNeededDate { get; set; }
        [Display(Name = "Show Shipping Type")]
        public virtual bool ShowShippingType { get; set; }
        [Display(Name = "Show Days Not Acted On")]
        public virtual bool ShowDaysNotActedOn { get; set; }
        [Display(Name = "Show Last Acted On By")]
        public virtual bool ShowLastActedOnBy { get; set; }
        [Display(Name = "Show People Pending Action")]
        public virtual bool ShowPeoplePendingAction { get; set; }
        [Display(Name = "Show Account #")]
        public virtual bool ShowAccountNumber { get; set; }
        [Display(Name = "Show Date Ordered")]
        public virtual bool ShowOrderedDate { get; set; }
        [Display(Name = "Show Approver")]
        public virtual bool ShowApprover { get; set; }
        [Display(Name = "Show Account Manager")]
        public virtual bool ShowAccountManager { get; set; }
        [Display(Name = "Show Purchaser")]
        public virtual bool ShowPurchaser { get; set; }
        [Display(Name = "Show Last Acted On Date")]
        public virtual bool ShowLastActedOnDate { get; set; }
        [Display(Name = "Show Last You Acted On Date")]
        public virtual bool ShowLastYouActedOnDate { get; set; }

    }

    public class ColumnPreferencesMap : ClassMap<ColumnPreferences>
    {
        public ColumnPreferencesMap()
        {
            Id(x => x.Id).GeneratedBy.Assigned();

            Map(x => x.ShowRequestNumber);
            Map(x => x.ShowPurchaseOrderNumber);
            Map(x => x.ShowWorkgroup);
            Map(x => x.ShowOrganization);
            Map(x => x.ShowVendor);
            Map(x => x.ShowShipTo);
            Map(x => x.ShowAllowBackorder);
            Map(x => x.ShowRestrictedOrder);
            Map(x => x.ShowHasSplits);
            Map(x => x.ShowHasAttachments);
            Map(x => x.ShowNumberOfLines);
            Map(x => x.ShowTotalAmount);
            Map(x => x.ShowCreatedBy);
            Map(x => x.ShowCreatedDate);
            Map(x => x.ShowStatus);
            Map(x => x.ShowNeededDate);
            Map(x => x.ShowShippingType);
            Map(x => x.ShowDaysNotActedOn);
            Map(x => x.ShowLastActedOnBy);
            Map(x => x.ShowPeoplePendingAction);
            Map(x => x.ShowAccountNumber);
            Map(x => x.ShowOrderedDate);
            Map(x => x.ShowApprover);
            Map(x => x.ShowAccountManager);
            Map(x => x.ShowPurchaser);
            Map(x => x.ShowLastActedOnDate);
            Map(x => x.ShowLastYouActedOnDate);

        }
    }
}
