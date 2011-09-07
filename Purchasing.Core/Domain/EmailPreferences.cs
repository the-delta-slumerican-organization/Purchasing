using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace Purchasing.Core.Domain
{
    public class EmailPreferences : DomainObjectWithTypedId<string>
    {
        public EmailPreferences() { }
        public EmailPreferences(string id) { Id = id; }

        [Display(Name="Order Submission")]
        public virtual bool RequesterOrderSubmission { get; set; }
         
        [Display(Name="Approver Approved")]
        public virtual bool RequesterApproverApproved { get; set; }
        [Display(Name="Approver Updates Request")]
        public virtual bool RequesterApproverChanged { get; set; }

        [Display(Name="Account Manager Reviewed")]
        public virtual bool RequesterAccountManagerApproved { get; set; }
        [Display(Name="Account Manager Updates Request")]
        public virtual bool RequesterAccountManagerChanged { get; set; }

        [Display(Name="Purchaser Action")]
        public virtual bool RequesterPurchaserAction { get; set; }
        [Display(Name="Purchaser Updates Request")]
        public virtual bool RequesterPurchaserChanged { get; set; }

        [Display(Name="Kuali Update")]
        public virtual bool RequesterKualiProcessed { get; set; }
        [Display(Name="Kuali Approved")]
        public virtual bool RequesterKualiApproved { get; set; }

        [Display(Name="Account Manager Reviewed")]
        public virtual bool ApproverAccountManagerApproved { get; set; }
        [Display(Name="Account Manager Denied")]
        public virtual bool ApproverAccountManagerDenied { get; set; }
        [Display(Name="Purchaser Processed")]
        public virtual bool ApproverPurchaserProcessed { get; set; }
        [Display(Name="Kuali Approved")]
        public virtual bool ApproverKualiApproved { get; set; }
        [Display(Name="Request Completed")]
        public virtual bool ApproverOrderCompleted { get; set; }

        [Display(Name="Purchaser Processed")]
        public virtual bool AccountManagerPurchaserProcessed { get; set; }
        [Display(Name="Kuali Approved")]
        public virtual bool AccountManagerKualiApproved { get; set; }
        [Display(Name="Request Completed")]
        public virtual bool AccountManagerOrderCompleted { get; set; }

        [Display(Name="Kuali Approved")]
        public virtual bool PurchaserKualiApproved { get; set; }
        [Display(Name="Request Completed")]
        public virtual bool PurchaserOrderCompleted { get; set; }

        public virtual NotificationTypes NotificationType { get; set; }

        public enum NotificationTypes
        {
            PerEvent,
            Daily,
            Weekly
        }
    }

    public class EmailPreferencesMap : ClassMap<EmailPreferences>
    {
        public EmailPreferencesMap()
        {
            Id(x => x.Id).GeneratedBy.Assigned();

            Map(x => x.RequesterOrderSubmission);
            Map(x => x.RequesterApproverApproved);
            Map(x => x.RequesterApproverChanged);
            Map(x => x.RequesterAccountManagerApproved);
            Map(x => x.RequesterAccountManagerChanged);
            Map(x => x.RequesterPurchaserAction);
            Map(x => x.RequesterPurchaserChanged);
            Map(x => x.RequesterKualiProcessed); 
            Map(x => x.RequesterKualiApproved);

            Map(x => x.ApproverAccountManagerApproved);
            Map(x => x.ApproverAccountManagerDenied);
            Map(x => x.ApproverKualiApproved);
            Map(x => x.ApproverPurchaserProcessed);
            Map(x => x.ApproverOrderCompleted);

            Map(x => x.AccountManagerKualiApproved);
            Map(x => x.AccountManagerOrderCompleted);
            Map(x => x.AccountManagerPurchaserProcessed);

            Map(x => x.PurchaserKualiApproved);
            Map(x => x.PurchaserOrderCompleted);

            Map(x => x.NotificationType).CustomType<NHibernate.Type.EnumStringType<EmailPreferences.NotificationTypes>>()
                .Not.Nullable();
        }
    }
}