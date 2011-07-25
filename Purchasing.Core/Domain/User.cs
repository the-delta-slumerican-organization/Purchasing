﻿using System.Collections.Generic;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace Purchasing.Core.Domain
{
    public class User : DomainObjectWithTypedId<string>
    {
        protected User() { }
        public User(string id)
        {
            Id = id;
            Departments = new List<Department>();
        }

        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string Email { get; set; }

        public virtual IList<Department> Departments { get; set; }
    }

    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Id(x => x.Id).GeneratedBy.Assigned();

            Map(x => x.FirstName);
            Map(x => x.LastName);
            Map(x => x.Email);

            HasMany(x => x.Departments).Table("UsersXDepartments"); //TODO: Possibly make many-to-many
        }
    }
}