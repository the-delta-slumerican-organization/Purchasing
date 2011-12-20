﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Purchasing.Core.Domain;
using Purchasing.Web.Controllers;
using UCDArch.Core.PersistanceSupport;
using MvcContrib;
using System.Linq;
using System.Linq.Expressions;
using UCDArch.Core.Utils;

namespace Purchasing.Web.Services
{
    public interface ISecurityService
    {
        /// <summary>
        /// Checks if the current user has access to the workgroup or the organization.
        /// </summary>
        /// <param name="workgroup"></param>
        /// <param name="organization"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool HasWorkgroupOrOrganizationAccess(Workgroup workgroup, Organization organization, out string message);

        /// <summary>
        /// Checks if a user is in a particular role for a workgroup
        /// </summary>
        /// <param name="roleCode">Role Id</param>
        /// <param name="workgroupId">Workgroup Id</param>
        /// <returns>True, if in role, False, if not.</returns>
        bool IsInRole(string roleCode, int workgroupId);

        /// <summary>
        /// Checks if a user is in a particular role for a workgroup
        /// </summary>
        /// <param name="role">Role</param>
        /// <param name="workgroup">Workgroup</param>
        /// <returns>True, if in role, False, if not.</returns>
        bool IsInRole(Role role, Workgroup workgroup);
    }

    public class SecurityService :  ISecurityService
    {
        private readonly IUserIdentity _userIdentity;
        private readonly IRepositoryWithTypedId<Role, string> _roleRepository;
        private readonly IRepository<Workgroup> _workgroupRepository;
        private readonly IRepository _repository;    

        public SecurityService(IRepository repository, IUserIdentity userIdentity, IRepositoryWithTypedId<Role, string> roleRepository, IRepository<Workgroup> workgroupRepository )
        {
            _userIdentity = userIdentity;
            _roleRepository = roleRepository;
            _workgroupRepository = workgroupRepository;
            _repository = repository;
        }

        public bool HasWorkgroupOrOrganizationAccess(Workgroup workgroup, Organization organization, out string message)
        {
            if(workgroup == null && organization == null)
            {
                message = "Workgroup and Organization not found.";
                return false;
            }

            var user = _repository.OfType<User>().Queryable.Where(x => x.Id == _userIdentity.Current).Fetch(x => x.Organizations).Single();

            if(workgroup != null)
            {
                var workgroupIds = GetWorkgroups(user).Select(x => x.Id).ToList();
                if(!workgroupIds.Contains(workgroup.Id))
                {
                    message = "No access to that workgroup";
                    return false;
                }
            }
            else
            {
                var orgIds = user.Organizations.Select(x => x.Id).ToList();
                if(!orgIds.Contains(organization.Id))
                {
                    message = "No access to that organization";
                    return false;
                }
            }

            message = null;
            return true;
        }

        public bool IsInRole(string roleCode, int workgroupId)
        {
            var role = _roleRepository.GetNullableById(roleCode);
            var workgroup = _workgroupRepository.GetNullableById(workgroupId);

            // invalid role code was provided
            if (role == null)
            {
                throw new ArgumentException("Role code not found.");
            }

            // invalid workgroup
            if (workgroup == null)
            {
                throw new ArgumentException("Workgroup was not found.");
            }

            return IsInRole(role, workgroup);
        }

        public bool IsInRole(Role role, Workgroup workgroup)
        {
            Check.Require(role != null, "role is required.");
            Check.Require(workgroup != null, "workgroup is required.");
            var user = _repository.OfType<User>().Queryable.Where(x => x.Id == _userIdentity.Current).Fetch(x => x.Organizations).Single();

            return workgroup.Permissions.Where(a => a.User == user && a.Role == role).Any();
        }


        private IQueryable<Workgroup> GetWorkgroups(User user)
        {
            var orgIds = user.Organizations.Select(x => x.Id).ToArray();

            return _repository.OfType<Workgroup>().Queryable.Where(x => x.Organizations.Any(a => orgIds.Contains(a.Id)));

        }
    }
}