﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace Purchasing.Core.Domain
{
    public class OrderStatusCode : DomainObjectWithTypedId<string>
    {
        [StringLength(50)]
        [Required]
        public virtual string Name { get; set; }

        public virtual int Level { get; set; }
        public virtual bool IsComplete { get; set; }
        public virtual bool KfsStatus { get; set; }
        public virtual bool ShowInFilterList { get; set; }

        public static class Codes
        {
            public const string AccountManager = Role.Codes.AccountManager;
            public const string Approver = "AP";
            public const string ConditionalApprover = "CA";
            public const string CompleteNotUploadedKfs = "CN";
            public const string Complete = "CP";
            public const string Purchaser = Role.Codes.Purchaser;
            public const string Requester = Role.Codes.Requester;
        }
    }

    public class OrderStatusCodeMap : ClassMap<OrderStatusCode>
    {
        public OrderStatusCodeMap()
        {
            ReadOnly();
            
            Id(x => x.Id).GeneratedBy.Assigned();

            Map(x => x.Name);
            Map(x => x.Level);
            Map(x => x.IsComplete);
            Map(x => x.KfsStatus);
            Map(x => x.ShowInFilterList);
        }
    }
}
