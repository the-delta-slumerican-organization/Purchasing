﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace Purchasing.Core.Domain
{
    public class User : DomainObjectWithTypedId<string>
    {
        public User()
        {
            Organizations = new List<Organization>();
            Roles = new List<Role>();
        }
        public User(string id) : this()
        {
            Id = id;
            IsActive = true;
        }

        public virtual string FullName { get { return string.Format("{0} {1}", FirstName, LastName); } }
        
        public virtual string FullNameAndId { get { return string.Format("{0} ({1})", FullName, Id); } }
        
        public virtual string FullNameAndIdLastFirst { get { return string.Format("{0}, {1} ({2})", LastName, FirstName, Id); } }

        /// <summary>
        /// User is away if the AwayUntil value is set to sometime in the future
        /// </summary>
        public virtual bool IsAway { get { return AwayUntil.HasValue && AwayUntil.Value > DateTime.Now; } }

        [Required]
        [StringLength(10)]
        [Display(Name = "KerberosID")]
        public override string Id { get; protected set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public virtual string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public virtual string LastName { get; set; }

        [Required]
        [StringLength(50)]
        public virtual string Email { get; set; }

        [Date]
        [Display(Name = "Away Until")]
        public virtual DateTime? AwayUntil { get; set; }
        [Display(Name = "Is Active")]
        public virtual bool IsActive { get; set; }

        //public virtual EmailPreferences EmailPreferences { get; set; }

        public virtual IList<Organization> Organizations { get; set; }
        public virtual IList<Role> Roles { get; set; }
    }

    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Id(x => x.Id).GeneratedBy.Assigned();

            Map(x => x.FirstName);
            Map(x => x.LastName);
            Map(x => x.Email);
            Map(x => x.AwayUntil);
            Map(x => x.IsActive);

            //HasOne(x => x.EmailPreferences).Constrained().LazyLoad().Cascade.SaveUpdate();

            HasManyToMany(x => x.Organizations).Table("UsersXOrganizations").ParentKeyColumn("UserID").ChildKeyColumn("OrganizationID");
            HasManyToMany(x => x.Roles).Table("Permissions").ChildKeyColumn("RoleID").ParentKeyColumn("UserID");
        }
    }
}