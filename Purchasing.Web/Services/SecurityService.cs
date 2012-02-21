﻿using System;
using System.Collections.Generic;
using System.Linq;
using Purchasing.Core;
using Purchasing.Core.Domain;
using UCDArch.Core.PersistanceSupport;
using UCDArch.Core.Utils;

namespace Purchasing.Web.Services
{
    [Flags]
    public enum OrderAccessLevel { None, Readonly, Edit }

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
        /// Checks if the current user has access to the workgroup
        /// </summary>
        /// <param name="workgroup"></param>
        /// <returns></returns>
        bool HasWorkgroupAccess(Workgroup workgroup);

        bool HasWorkgroupEditAccess(int id, out string message);

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

        /// <summary>
        /// Get the current user's access to the order
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        OrderAccessLevel GetAccessLevel(Order order);
        OrderAccessLevel GetAccessLevel(int orderId);

        /// <summary>
        /// Finds or creates a user object as necessary
        /// </summary>
        /// <param name="kerb"></param>
        /// <returns>Null, if kerb does not return result from ldap or db</returns>
        User GetUser(string kerb);
    }

    public class SecurityService :  ISecurityService
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IQueryRepositoryFactory _queryRepositoryFactory;
        private readonly IUserIdentity _userIdentity;
        private readonly IDirectorySearchService _directorySearchService;

        public SecurityService(IRepositoryFactory repositoryFactory, IUserIdentity userIdentity, IDirectorySearchService directorySearchService, IQueryRepositoryFactory queryRepositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
            _userIdentity = userIdentity;
            _directorySearchService = directorySearchService;
            _queryRepositoryFactory = queryRepositoryFactory;
        }

        // ===================================================
        // Workgroup Access Functions
        // ===================================================
        public bool HasWorkgroupOrOrganizationAccess(Workgroup workgroup, Organization organization, out string message)
        {
            if(workgroup == null && organization == null)
            {
                message = "Workgroup and Organization not found.";
                return false;
            }

            var user = _repositoryFactory.UserRepository.Queryable.Where(x => x.Id == _userIdentity.Current).Fetch(x => x.Organizations).Single();

            if(workgroup != null)
            {
                var workgroupIds = user.WorkgroupPermissions.Select(a => a.Workgroup.Id).ToList(); //GetWorkgroups(user).Select(x => x.Id).ToList();
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

        public bool HasWorkgroupAccess(Workgroup workgroup)
        {
            //return true; //TODO: do the actual check once GetWorkgroups is checked?
            string message;

            return HasWorkgroupOrOrganizationAccess(workgroup, null, out message);
        }

        /// <summary>
        /// This is based on the user's organizational access
        /// </summary>
        /// <param name="id">workgroup id</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool HasWorkgroupEditAccess(int id, out string message)
        {
            var workgroup = _repositoryFactory.WorkgroupRepository.GetNullableById(id);
            if(workgroup == null)
            {
                message = "Workgroup not found";
                return false;
            }
            return HasWorkgroupOrOrganizationAccess(null, workgroup.PrimaryOrganization, out message);
        }

        public bool IsInRole(string roleCode, int workgroupId)
        {
            var role = _repositoryFactory.RoleRepository.GetNullableById(roleCode);
            var workgroup = _repositoryFactory.WorkgroupRepository.GetNullableById(workgroupId);

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
            var user = _repositoryFactory.UserRepository.Queryable.Where(x => x.Id == _userIdentity.Current).Fetch(x => x.Organizations).Single();

            return workgroup.Permissions.Where(a => a.User == user && a.Role == role).Any();
        }
        
        private IEnumerable<Workgroup> GetWorkgroups(User user)
        {
            //var orgIds = user.Organizations.Select(x => x.Id).ToArray();

            //return _repository.OfType<Workgroup>().Queryable.Where(x => x.Organizations.Any(a => orgIds.Contains(a.Id)));

            var wrkgrps = user.WorkgroupPermissions.Select(a => a.Workgroup);
            return wrkgrps;

        }

        // ===================================================
        // Order Access Functions
        // ===================================================
        public OrderAccessLevel GetAccessLevel(int orderId)
        {
            var order = _repositoryFactory.OrderRepository.GetById(orderId);
            return GetAccessLevel(order);
        }

        public OrderAccessLevel GetAccessLevel(Order order)
        {
            Check.Require(order != null, "order is required.");

            //var workgroup = order.Workgroup;
            //var user = _repositoryFactory.UserRepository.GetNullableById(_userIdentity.Current);

            //// current order status
            //var currentStatus = order.StatusCode;

            //// get the user's role in the workgroup
            //var permissions = workgroup.Permissions.Where(a => a.User == user && !a.Workgroup.Administrative).ToList();

            //// current approvals
            //var approvals = order.Approvals.Where(a => a.StatusCode.Level == currentStatus.Level && !a.Completed).ToList();

            //// check for edit access
            //if (HasEditAccess(order, approvals, permissions, currentStatus, user))
            //{
            //    return OrderAccessLevel.Edit;
            //}

            //// check for read access
            //if (HasReadAccess(order, order.OrderTrackings, permissions, user))
            //{
            //    return OrderAccessLevel.Readonly;
            //}

            var access = _queryRepositoryFactory.AccessRepository.Queryable.FirstOrDefault(a => a.OrderId == order.Id && a.AccessUserId == _userIdentity.Current);

            if (access != null)
            {
                if (access.EditAccess)
                {
                    return OrderAccessLevel.Edit;
                }
                
                if (access.ReadAccess)
                {
                    return OrderAccessLevel.Readonly;
                }
            }

            // default no access
            return OrderAccessLevel.None;
        }

        /// <summary>
        /// Checks if the user is the current person to review the order
        /// </summary>
        /// <param name="order"></param>
        /// <param name="approvals">Pending approvals for the order's current level</param>
        /// <param name="permissions">User's permissions</param>
        /// <param name="currentStatus">Order's current status</param>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool HasEditAccess(Order order, IEnumerable<Approval> approvals, IEnumerable<WorkgroupPermission> permissions, OrderStatusCode currentStatus, User user)
        {
            // has read access?
            return _queryRepositoryFactory.AccessRepository.Queryable.Any(a => a.OrderId == order.Id && a.AccessUserId == user.Id && a.EditAccess);

            //// is the user explicitely defined at the current level of approval
            //if (approvals.Any(a => a.User == user || a.SecondaryUser == user))
            //{
            //    return true;
            //}

            //// there exists at least one at the current level that is tied to a user
            //if (approvals.Any(a => a.User != null || a.SecondaryUser != null))
            //{
            //    // is one the current user?
            //    if (approvals.Any(a => a.User == user || a.SecondaryUser == user)) return true;

            //    // is the user away? and not the current person
            //    if (approvals.Any(a => (a.User != null && a.User.IsAway) && (a.SecondaryUser == null || (a.SecondaryUser != null && a.SecondaryUser.IsAway))))
            //    {
            //        // user is away for an approval, check workgroup permissions
            //        if (permissions.Any(a => a.Role.Level == currentStatus.Level))
            //        {
            //            return true;
            //        }
            //    }
            //}

            //// there exists at least one at the current level that is not tied to a user
            //if (approvals.Any(a => a.User == null && a.SecondaryUser == null))
            //{
            //    // the user has a matching role level to the current one and qualitfies for workgroup permissions
            //    if (permissions.Any(a => a.Role.Level == currentStatus.Level))
            //    {
            //        return true;
            //    }
            //}

            // do a final check for administrative access
            //return HasAdminAccess(order, user);

            //return false;
        }

        /// <summary>
        /// checks if the user has access to the permissions to the workgroup or performed something in the order 
        /// </summary>
        /// <param name="order"></param>
        /// <param name="trackings"></param>
        /// <param name="permissions"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool HasReadAccess(Order order, IEnumerable<OrderTracking> trackings, IEnumerable<WorkgroupPermission> permissions, User user)
        {
            //return permissions.Count() > 0 || trackings.Any(a => a.User == user);

            // has read access?
            return _queryRepositoryFactory.AccessRepository.Queryable.Any(a => a.OrderId == order.Id && a.AccessUserId == user.Id && a.ReadAccess);
        }

        /// <summary>
        /// checks if the user has administrative access to a particular order
        /// </summary>
        /// <param name="order"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool HasAdminAccess(Order order, User user)
        {
            /*
             * Method using the Admin Order Pending Table
             */
            return _queryRepositoryFactory.AdminOrderPendingRepository.Queryable.Any(a => a.AccessUserId == user.Id && a.OrderId == order.Id);


            /*
             * Method using the descendants table
             */
            //// user's workgroup permissions
            //var permissions = user.WorkgroupPermissions.Where(a => a.Workgroup.Administrative).SelectMany(a => a.Workgroup.Organizations).ToList();

            //// get all orgs and descents, tree
            //var orgs = _queryRepositoryFactory.OrganizationDescendantRepository.Queryable.Where(a => permissions.Select(b => b.Id).Contains(a.RollupParentId));

            //// are any of the order's orgs in the admin tree?
            //return order.Workgroup.Organizations.Any(a => orgs.Any(b => b.OrgId == a.Id));
            
            /*
             * Original Method
             */
            //// get administrative workgroups
            //var permissions = user.WorkgroupPermissions.Where(a => a.Workgroup.Administrative).ToList();

            //foreach (var org in order.Workgroup.Organizations)
            //{
            //    // traverse up the org's parents
            //    var currentOrg = org;

            //    do
            //    {

            //        // check if the current org meets the criteria
            //        var perm = permissions.Where(a => a.Workgroup.Organizations.Contains(currentOrg)).FirstOrDefault();

            //        // there is a permission
            //        if (perm != null)
            //        {
            //            // does the level match?
            //            if (perm.Role.Level == order.StatusCode.Level) return true;
            //        }

            //        // set the next parent
            //        currentOrg = currentOrg.Parent;

            //    } while (currentOrg != null);


            //}

            //return false;
        }

        // ===================================================
        // User Functions
        // ===================================================
        public User GetUser(string kerb)
        {
            var user = _repositoryFactory.UserRepository.GetNullableById(kerb);

            if (user == null)
            {
                var ldapUser = _directorySearchService.FindUser(kerb);

                if (ldapUser != null)
                {
                    user = new User(kerb);
                    user.FirstName = ldapUser.FirstName;
                    user.LastName = ldapUser.LastName;
                    user.Email = ldapUser.EmailAddress;

                    _repositoryFactory.UserRepository.EnsurePersistent(user);
                }
            }

            return user;
        }
    }
}