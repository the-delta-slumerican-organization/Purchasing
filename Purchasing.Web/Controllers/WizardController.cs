﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using MvcContrib;
using Purchasing.Core.Domain;
using Purchasing.Web.Helpers;
using Purchasing.Web.Models;
using Purchasing.Web.Services;
using Purchasing.Web.Utility;
using UCDArch.Core.PersistanceSupport;
using UCDArch.Core.Utils;
using UCDArch.Web.ActionResults;
using UCDArch.Web.Helpers;

namespace Purchasing.Web.Controllers
{
    /// <summary>
    /// Controller for the Wizard class
    /// </summary>
    [Authorize(Roles = Role.Codes.DepartmentalAdmin)]
    public class WizardController : ApplicationController
    {
        private readonly IRepository<Workgroup> _workgroupRepository;
        private readonly IRepositoryWithTypedId<User, string> _userRepository;
        private readonly IRepositoryWithTypedId<Role, string> _roleRepository;
        private readonly IRepository<WorkgroupPermission> _workgroupPermissionRepository;
        private readonly ISecurityService _securityService;
        private readonly IDirectorySearchService _searchService;
        private readonly IRepository<WorkgroupVendor> _workgroupVendorRepository;
        private readonly IRepositoryWithTypedId<Vendor, string> _vendorRepository;
        private readonly IRepositoryWithTypedId<VendorAddress, Guid> _vendorAddressRepository;
        private readonly IRepositoryWithTypedId<State, string> _stateRepository;
        private readonly IRepository<WorkgroupAccount> _workgroupAccountRepository;
        private readonly IWorkgroupAddressService _workgroupAddressService;
        private readonly IWorkgroupService _workgroupService;
        public const string WorkgroupType = "Workgroup";
        public const string OrganizationType = "Organization";


        public WizardController(IRepository<Workgroup> workgroupRepository,
            IRepositoryWithTypedId<User, string> userRepository,
            IRepositoryWithTypedId<Role, string> roleRepository,
            IRepository<WorkgroupPermission> workgroupPermissionRepository,
            ISecurityService securityService,
            IDirectorySearchService searchService,
            IRepository<WorkgroupVendor> workgroupVendorRepository,
            IRepositoryWithTypedId<Vendor, string> vendorRepository,
            IRepositoryWithTypedId<VendorAddress, Guid> vendorAddressRepository,
            IRepositoryWithTypedId<State, string> stateRepository,
            IRepository<WorkgroupAccount> workgroupAccountRepository,
            IWorkgroupAddressService workgroupAddressService,
            IWorkgroupService workgroupService)
        {
            _workgroupRepository = workgroupRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _workgroupPermissionRepository = workgroupPermissionRepository;
            _securityService = securityService;
            _searchService = searchService;
            _workgroupVendorRepository = workgroupVendorRepository;
            _vendorRepository = vendorRepository;
            _vendorAddressRepository = vendorAddressRepository;
            _stateRepository = stateRepository;
            _workgroupAccountRepository = workgroupAccountRepository;
            _workgroupAddressService = workgroupAddressService;
            _workgroupService = workgroupService;
        }


        /// <summary>
        /// GET: /Wizard/
        /// View To start the Workgroup create wizard
        /// Test #1
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {

            return View();
        }

        //NOTE: This wizard will only allow adding stuff. No delete, edit or view. For that they will complete the wizard and go back to the Workgroup management

        /// <summary>
        /// Step 1
        /// Test #2
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateWorkgroup()
        {
            ViewBag.StepNumber = 1;
            var user = _userRepository.Queryable.Single(x => x.Id == CurrentUser.Identity.Name);

            var model = WorkgroupModifyModel.Create(user);

            return View(model);
        }

        /// <summary>
        /// Step #1
        /// Test #3
        /// </summary>
        /// <param name="workgroup"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateWorkgroup(Workgroup workgroup)
        {
            ViewBag.StepNumber = 1;

            var user = GetCurrentUser();
            if(!user.Organizations.Any(a => a.Id == workgroup.PrimaryOrganization.Id))
            {
                ModelState.AddModelError("Workgroup.PrimaryOrganization", "You do not have access to the selected organization");
            }

            if(!ModelState.IsValid)
            {
                var model = WorkgroupModifyModel.Create(user);
                model.Workgroup = workgroup;

                return View(model);
            }

            var createdWorkgroup = _workgroupService.CreateWorkgroup(workgroup, null);

            Message = string.Format("{0} workgroup was created",
                                    createdWorkgroup.Name);

            return this.RedirectToAction(a => a.AddSubOrganizations(createdWorkgroup.Id));
        }

        /// <summary>
        /// Step #2
        /// Test #4
        /// </summary>
        /// <param name="id">Workgroup Id</param>
        /// <returns></returns>
        public ActionResult AddSubOrganizations(int id)
        {
            if(id == 0)
            {
                Message = "Workgroup must be created before proceeding";
                return this.RedirectToAction(a => a.CreateWorkgroup());
            }
            ViewBag.StepNumber = 2;

            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }

            var user = _userRepository.Queryable.Single(x => x.Id == CurrentUser.Identity.Name);

            var model = WorkgroupModifyModel.Create(user);
            model.Workgroup = workgroup;
            ViewBag.WorkgroupId = workgroup.Id;

            return View(model);
        }

        /// <summary>
        /// Step #2
        /// Test #4
        /// </summary>
        /// <param name="id">Workgroup Id</param>
        /// <param name="selectedOrganizations"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddSubOrganizations(int id, string[] selectedOrganizations)
        {
            ViewBag.StepNumber = 2;
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }


            if(selectedOrganizations != null)
            {
                var user = GetCurrentUser();

                foreach(var selectedOrganization in selectedOrganizations)
                {
                    Check.Require(user.Organizations.Any(a => a.Id == selectedOrganization), string.Format("The organization '{0}' was being added by {1} to a workgroup's subOrganizations.", selectedOrganization, user.FullNameAndId));
                }

                var existingOrganizations = workgroup.Organizations.Select(a => a.Id).ToList();
                var organizationsToAdd =
                    Repository.OfType<Organization>().Queryable.Where(a => selectedOrganizations.Contains(a.Id)).Select(
                        b => b.Id).ToList().Union(existingOrganizations).ToList();

                workgroup.Organizations =
                    Repository.OfType<Organization>().Queryable.Where(a => organizationsToAdd.Contains(a.Id)).ToList();
            }

            _workgroupRepository.EnsurePersistent(workgroup);

            return this.RedirectToAction(a => a.SubOrganizations(workgroup.Id));
        }

        /// <summary>
        /// Step 2
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SubOrganizations(int id)
        {
            ViewBag.StepNumber = 2;
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }

            return View(workgroup);
        }

        /// <summary>
        /// Step 3, 4, 5, 6
        /// </summary>
        /// <returns></returns>
        public ActionResult AddPeople(int id, string roleFilter)
        {
            if(id == 0)
            {
                Message = "Workgroup must be created before proceeding";
                return this.RedirectToAction(a => a.CreateWorkgroup());
            }
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }

            var viewModel = WorgroupPeopleCreateModel.Create(_roleRepository, workgroup);
            if(!string.IsNullOrWhiteSpace(roleFilter))
            {
                viewModel.Role = _roleRepository.Queryable.SingleOrDefault(a => a.Level >= 1 && a.Level <= 4 && a.Id == roleFilter);
            }

            Check.Require(viewModel.Roles.Single(a => a.Level == 1).Id == "RQ"); //Used for navigation in _StatusBar.cshtml
            Check.Require(viewModel.Roles.Single(a => a.Level == 2).Id == "AP"); //Used for navigation in _StatusBar.cshtml
            Check.Require(viewModel.Roles.Single(a => a.Level == 3).Id == "AM"); //Used for navigation in _StatusBar.cshtml
            Check.Require(viewModel.Roles.Single(a => a.Level == 4).Id == "PR"); //Used for navigation in _StatusBar.cshtml

            ViewBag.rolefilter = roleFilter;
            ViewBag.StepNumber = 3 + (viewModel.Role.Level - 1);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddPeople(int id, WizardWorkgroupPeoplePostModel workgroupPeoplePostModel, string roleFilter, string bulkEmail, string bulkKerb)
        {
            var notAddedKvp = new List<KeyValuePair<string, string>>();

            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }
            if(workgroupPeoplePostModel.Users == null)
            {
                workgroupPeoplePostModel.Users = new List<string>();
            }

            ModelState.Clear();

            workgroupPeoplePostModel.Role = _roleRepository.Queryable.FirstOrDefault(a => a.Id == roleFilter);
            //Ensure role picked is valid.
            if(workgroupPeoplePostModel.Role != null)
            {
                if(!_roleRepository.Queryable.Any(a => a.Level >= 1 && a.Level <= 4 && a.Id == workgroupPeoplePostModel.Role.Id))
                {
                    ModelState.AddModelError("Role", "Invalid Role Selected - don't mess with the query string!");
                }

            }

            if(!ModelState.IsValid)
            {
                var viewModel = WorgroupPeopleCreateModel.Create(_roleRepository, workgroup);

                if(workgroupPeoplePostModel.Role != null)
                {
                    viewModel.Role = workgroupPeoplePostModel.Role;
                }
                if(workgroupPeoplePostModel.Users != null && workgroupPeoplePostModel.Users.Count > 0)
                {
                    var users = new List<IdAndName>();
                    foreach(var user in workgroupPeoplePostModel.Users)
                    {
                        var temp = _userRepository.GetNullableById(user);
                        if(temp != null)
                        {
                            users.Add(new IdAndName(temp.Id, temp.FullName));
                        }
                        else
                        {
                            var ldapuser = _searchService.FindUser(user);
                            if(ldapuser != null)
                            {
                                users.Add(new IdAndName(ldapuser.LoginId, string.Format("{0} {1}", ldapuser.FirstName, ldapuser.LastName)));
                            }
                        }
                    }
                    viewModel.Users = users;
                }
                ViewBag.rolefilter = roleFilter;
                if(!string.IsNullOrWhiteSpace(roleFilter))
                {
                    viewModel.Role = _roleRepository.Queryable.SingleOrDefault(a => a.Level >= 1 && a.Level <= 4 && a.Id == roleFilter);
                }
                ViewBag.StepNumber = 3 + (viewModel.Role.Level - 1);
                return View(viewModel);
            }

            int successCount = 0;
            int failCount = 0;
            foreach(var u in workgroupPeoplePostModel.Users)
            {
                successCount = _workgroupService.TryToAddPeople(id, workgroupPeoplePostModel.Role, workgroup, successCount, u, ref failCount, notAddedKvp);
            }



            #region Bulk Load Email
            successCount = _workgroupService.TryBulkLoadPeople(bulkEmail, true, id, workgroupPeoplePostModel.Role, workgroup, successCount, ref failCount, notAddedKvp);

            //const string regexPattern = @"\b[A-Z0-9._-]+@[A-Z0-9][A-Z0-9.-]{0,61}[A-Z0-9]\.[A-Z.]{2,6}\b";

            //// Find matches
            //System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(bulkEmail, regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            //foreach(System.Text.RegularExpressions.Match match in matches)
            //{
            //    var temp = match.ToString().ToLower();
            //    successCount = _workgroupService.TryToAddPeople(id, workgroupPeoplePostModel.Role, workgroup, successCount, temp,  ref failCount, notAddedKvp);
            //}
            #endregion

            #region Bulk Load Kerb

            successCount = _workgroupService.TryBulkLoadPeople(bulkKerb, false, id, workgroupPeoplePostModel.Role, workgroup, successCount, ref failCount, notAddedKvp);

            //const string regexPattern4Kerb = @"\b[A-Z0-9]{2,10}\b";

            //// Find matches
            //System.Text.RegularExpressions.MatchCollection matchesKerb = System.Text.RegularExpressions.Regex.Matches(bulkKerb, regexPattern4Kerb, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            //foreach(System.Text.RegularExpressions.Match match in matchesKerb)
            //{
            //    var temp = match.ToString().ToLower();
            //    successCount = _workgroupService.TryToAddPeople(id, workgroupPeoplePostModel.Role, workgroup, successCount, temp, ref failCount, notAddedKvp);
            //}
            #endregion

            Message = string.Format("Successfully added {0} people to workgroup as {1}. {2} not added because of duplicated role or not found.", successCount,
                                    workgroupPeoplePostModel.Role.Name, failCount);

            if(failCount > 0 || successCount == 0)
            {
                if(failCount == 0)
                {
                    ModelState.AddModelError("Users", "Must add at least 1 user or Skip");
                }

                var viewModel = WorgroupPeopleCreateModel.Create(_roleRepository, workgroup);
                viewModel.ErrorDetails = notAddedKvp;
                // ViewBag.DetailedMessage = notAddedSb.ToString();

                ViewBag.rolefilter = roleFilter;
                if(!string.IsNullOrWhiteSpace(roleFilter))
                {
                    viewModel.Role = _roleRepository.Queryable.SingleOrDefault(a => a.Level >= 1 && a.Level <= 4 && a.Id == roleFilter);
                }
                ViewBag.StepNumber = 3 + (viewModel.Role.Level - 1);
                return View(viewModel);
            }

            Message = string.Format("Successfully added {0} people to workgroup as {1}.", successCount,
                                   workgroupPeoplePostModel.Role.Name);
            return this.RedirectToAction(a => a.People(id, workgroupPeoplePostModel.Role.Id));
        }


        /// <summary>
        /// Step 3, 4, 5, 6
        /// </summary>
        /// <param name="id"></param>
        /// <param name="roleFilter"></param>
        /// <returns></returns>
        public ActionResult People(int id, string roleFilter)
        {
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }

            var viewModel = WorgroupPeopleListModel.Create(_workgroupPermissionRepository, _roleRepository, workgroup, roleFilter);
            viewModel.CurrentRole = _roleRepository.Queryable.SingleOrDefault(a => a.Level >= 1 && a.Level <= 4 && a.Id == roleFilter);
            ViewBag.rolefilter = roleFilter;
            ViewBag.StepNumber = 3 + (viewModel.CurrentRole.Level - 1);
            return View(viewModel);
        }


        /// <summary>
        /// Step 7
        /// </summary>
        /// <returns></returns>
        public ActionResult AddAccounts(int id)
        {
            if(id == 0)
            {
                Message = "Workgroup must be created before proceeding";
                return this.RedirectToAction(a => a.CreateWorkgroup());
            }
            ViewBag.StepNumber = 7;
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }

            var viewModel = WorkgroupAccountModel.Create(Repository, workgroup);

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult AddAccounts(int id, WorkgroupAccount workgroupAccount)
        {
            ViewBag.StepNumber = 7;
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }

            var workgroupAccountToCreate = new WorkgroupAccount { Workgroup = workgroup };

            Mapper.Map(workgroupAccount, workgroupAccountToCreate);

            ModelState.Clear();
            workgroupAccountToCreate.TransferValidationMessagesTo(ModelState);

            if(_workgroupAccountRepository.Queryable.Any(a => a.Workgroup.Id == workgroup.Id && a.Account.Id == workgroupAccountToCreate.Account.Id))
            {
                ModelState.AddModelError("WorkgroupAccount.Account", "Account already exists for this workgroup.");
            }
            if(ModelState.IsValid)
            {
                _workgroupAccountRepository.EnsurePersistent(workgroupAccountToCreate);
                Message = "Workgroup account saved.";
                //return this.RedirectToAction("Accounts", new {id = id});
                return this.RedirectToAction(a => a.Accounts(id));
            }

            var viewModel = WorkgroupAccountModel.Create(Repository, workgroup, workgroupAccountToCreate);
            return View(viewModel);
        }

        public ActionResult Accounts(int id)
        {
            ViewBag.StepNumber = 7;
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }

            return View(workgroup);
        }

        /// <summary>
        /// Step 8
        /// </summary>
        /// <returns></returns>
        public ActionResult AddKfsVendor(int id)
        {
            ViewBag.StepNumber = 8;
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }

            var viewModel = WorkgroupVendorViewModel.Create(_vendorRepository, new WorkgroupVendor { Workgroup = workgroup });

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult AddKfsVendor(int id, WorkgroupVendor workgroupVendor)
        {
            ViewBag.StepNumber = 8;
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }

            var workgroupVendorToCreate = new WorkgroupVendor();

            _workgroupService.TransferValues(workgroupVendor, ref workgroupVendorToCreate);

            workgroupVendorToCreate.Workgroup = workgroup;

            ModelState.Clear();
            workgroupVendorToCreate.TransferValidationMessagesTo(ModelState);

            if(ModelState.IsValid)
            {
                _workgroupVendorRepository.EnsurePersistent(workgroupVendorToCreate);

                Message = "WorkgroupVendor Created Successfully";

                return this.RedirectToAction(a => a.Vendors(id));
            }

            WorkgroupVendorViewModel viewModel;


            viewModel = WorkgroupVendorViewModel.Create(_vendorRepository, workgroupVendorToCreate, newVendor: true);

            return View(viewModel);
        }

        public ActionResult AddNewVendor(int id)
        {
            ViewBag.StepNumber = 8;
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }

            var viewModel = WorkgroupVendorViewModel.Create(_vendorRepository, new WorkgroupVendor { Workgroup = workgroup });

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult AddNewVendor(int id, WorkgroupVendor workgroupVendor)
        {
            ViewBag.StepNumber = 8;
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }

            var workgroupVendorToCreate = new WorkgroupVendor();

            _workgroupService.TransferValues(workgroupVendor, ref workgroupVendorToCreate);

            workgroupVendorToCreate.Workgroup = workgroup;

            ModelState.Clear();
            workgroupVendorToCreate.TransferValidationMessagesTo(ModelState);

            if(ModelState.IsValid)
            {
                _workgroupVendorRepository.EnsurePersistent(workgroupVendorToCreate);

                Message = "WorkgroupVendor Created Successfully";

                return this.RedirectToAction(a => a.Vendors(id));
            }

            WorkgroupVendorViewModel viewModel;


            var vendor = _vendorRepository.GetNullableById(workgroupVendor.VendorId);
            var vendorAddress = _vendorAddressRepository.Queryable.FirstOrDefault(a => a.Vendor == vendor && a.TypeCode == workgroupVendor.VendorAddressTypeCode);
            viewModel = WorkgroupVendorViewModel.Create(_vendorRepository, workgroupVendorToCreate, vendor, vendorAddress, true);



            return View(viewModel);
        }
        public ActionResult Vendors(int id)
        {
            if(id == 0)
            {
                Message = "Workgroup must be created before proceeding";
                return this.RedirectToAction(a => a.CreateWorkgroup());
            }
            ViewBag.StepNumber = 8;

            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }

            var workgroupVendorList = _workgroupVendorRepository.Queryable.Where(a => a.Workgroup == workgroup && a.IsActive);
            ViewBag.WorkgroupId = id;
            ViewBag.Title = workgroup.Name;
            return View(workgroupVendorList.ToList());
        }

        /// <summary>
        /// Step 9
        /// </summary>
        /// <returns></returns>
        public ActionResult AddAddresses(int id)
        {
            if(id == 0)
            {
                Message = "Workgroup must be created before proceeding";
                return this.RedirectToAction(a => a.CreateWorkgroup());
            }
            ViewBag.StepNumber = 9;
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }

            var viewModel = WorkgroupAddressViewModel.Create(workgroup, _stateRepository, true);
            viewModel.WorkgroupAddress = new WorkgroupAddress();
            viewModel.WorkgroupAddress.Workgroup = workgroup;
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult AddAddresses(int id, WorkgroupAddress workgroupAddress)
        {
            ViewBag.StepNumber = 9;
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }

            workgroupAddress.Workgroup = workgroup;
            ModelState.Clear();
            workgroupAddress.TransferValidationMessagesTo(ModelState);
            if(!ModelState.IsValid)
            {
                ErrorMessage = "Address not valid";
                var viewModel = WorkgroupAddressViewModel.Create(workgroup, _stateRepository, true);
                viewModel.WorkgroupAddress = workgroupAddress;
                viewModel.WorkgroupAddress.Workgroup = workgroup;
                return View(viewModel);
            }
            var matchFound = 0;
            foreach(var address in workgroup.Addresses)
            {
                matchFound = _workgroupAddressService.CompareAddress(workgroupAddress, address);
                if(matchFound > 0)
                {
                    break;
                }
            }
            if(matchFound > 0)
            {
                var matchedAddress = workgroup.Addresses.Single(a => a.Id == matchFound);
                if(!matchedAddress.IsActive)
                {
                    Message = "Address created.";
                    matchedAddress.IsActive = true;
                    _workgroupRepository.EnsurePersistent(workgroup);
                }
                else
                {
                    Message = "This Address already exists.";
                }
            }
            else
            {
                Message = "Address created";
                workgroup.AddAddress(workgroupAddress);
                _workgroupRepository.EnsurePersistent(workgroup);
            }
            return this.RedirectToAction(a => a.Addresses(id));
        }

        public ActionResult Addresses(int id)
        {
            ViewBag.StepNumber = 9;
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }
            var viewModel = WorkgroupAddressListModel.Create(workgroup);
            return View(viewModel);
        }

        /// <summary>
        /// Step 10
        /// </summary>
        /// <returns></returns>
        public ActionResult AddConditionalApproval(int id)
        {
            if(id == 0)
            {
                Message = "Workgroup must be created before proceeding";
                return this.RedirectToAction(a => a.CreateWorkgroup());
            }
            ViewBag.StepNumber = 10;
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }
            var model = new ConditionalApproval();
            model.Workgroup = workgroup;

            return View(model);
        }

        [HttpPost]
        public ActionResult AddConditionalApproval(int id, ConditionalApproval conditionalApproval, string primaryApproverParm, string secondaryApproverParm)
        {
            ViewBag.StepNumber = 10;
            // TODO Check that primary and secondary approvers are in db, add if not. Double check against ConditionalApprover controller
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }

            var primaryApproverInDb = GetUserBySearchTerm(primaryApproverParm);
            var secondaryApproverInDb = string.IsNullOrWhiteSpace(secondaryApproverParm)
                                            ? null
                                            : GetUserBySearchTerm(secondaryApproverParm);

            if(primaryApproverInDb == null)
            {
                DirectoryUser primaryApproverInLdap = _searchService.FindUser(primaryApproverParm);

                if(primaryApproverInLdap == null)
                {
                    ModelState.AddModelError("primaryApproverParm",
                         "No user could be found with the kerberos or email address entered");
                }
                else //found the primary approver in ldap
                {
                    primaryApproverInDb = new User(primaryApproverInLdap.LoginId)
                    {
                        FirstName = primaryApproverInLdap.FirstName,
                        LastName = primaryApproverInLdap.LastName,
                        Email = primaryApproverInLdap.EmailAddress,
                        IsActive = true
                    };

                    _userRepository.EnsurePersistent(primaryApproverInDb, forceSave: true);
                }
            }

            if(!string.IsNullOrWhiteSpace(secondaryApproverParm)) //only check if a value was provided
            {
                if(secondaryApproverInDb == null)
                {
                    DirectoryUser secondaryApproverInLdap = _searchService.FindUser(secondaryApproverParm);

                    if(secondaryApproverInLdap == null)
                    {
                        ModelState.AddModelError("secondaryApproverParm",
                                                 "No user could be found with the kerberos or email address entered");
                    }
                    else //found the secondary approver in ldap
                    {
                        secondaryApproverInDb = new User(secondaryApproverInLdap.LoginId)
                        {
                            FirstName = secondaryApproverInLdap.FirstName,
                            LastName = secondaryApproverInLdap.LastName,
                            Email = secondaryApproverInLdap.EmailAddress,
                            IsActive = true
                        };

                        _userRepository.EnsurePersistent(secondaryApproverInDb, forceSave: true);
                    }
                }
            }

            conditionalApproval.PrimaryApprover = primaryApproverInDb;
            conditionalApproval.SecondaryApprover = secondaryApproverInDb;
            conditionalApproval.Workgroup = workgroup;
            ModelState.Clear();
            conditionalApproval.TransferValidationMessagesTo(ModelState);

            if(ModelState.IsValid)
            {
                var newConditionalApproval = new ConditionalApproval
                                                 {
                                                     Question = conditionalApproval.Question,
                                                     Organization = conditionalApproval.Organization,
                                                     Workgroup = workgroup,
                                                     PrimaryApprover = primaryApproverInDb,
                                                     SecondaryApprover = secondaryApproverInDb
                                                 };
                Repository.OfType<ConditionalApproval>().EnsurePersistent(newConditionalApproval);
                return this.RedirectToAction(a => a.ConditionalApprovals(id));
            }
            conditionalApproval.Workgroup = workgroup;
            return View(conditionalApproval);
        }

        public ActionResult ConditionalApprovals(int id)
        {
            ViewBag.StepNumber = 10;
            if(id == 0)
            {
                Message = "Workgroup must be created before proceeding";
                return this.RedirectToAction(a => a.CreateWorkgroup());
            }

            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }

            var workgroupConditionalApprovals =
                Repository.OfType<ConditionalApproval>().Queryable.Where(
                    a => a.Workgroup != null && a.Workgroup.Id == workgroup.Id);
            ViewBag.WorkgroupId = id;
            ViewBag.Title = workgroup.Name;
            return View(workgroupConditionalApprovals.ToList());

        }

        /// <summary>
        /// Step 11
        /// </summary>
        /// <returns></returns>
        public ActionResult Details(int id)
        {
            ViewBag.StepNumber = 9;
            ActionResult redirectToAction;
            var workgroup = GetWorkgroupAndCheckAccess(id, out redirectToAction);
            if(workgroup == null)
            {
                return redirectToAction;
            }
            var viewModel = WorkgroupDetailModel.Create(_workgroupPermissionRepository, workgroup);
            return View(viewModel);
        }

        #region Private Helpers
        private Workgroup GetWorkgroupAndCheckAccess(int id, out ActionResult redirectToAction)
        {
            Workgroup workgroup;
            workgroup = _workgroupRepository.Queryable.Where(a => a.Id == id).Single();

            string message;
            if(!_securityService.HasWorkgroupOrOrganizationAccess(workgroup, null, out message))
            {
                Message = message;
                {
                    redirectToAction = this.RedirectToAction<ErrorController>(a => a.NotAuthorized());
                    return null;
                }
            }
            redirectToAction = null;
            return workgroup;
        }
        #endregion

        public JsonNetResult SearchUsers(string searchTerm)
        {
            searchTerm = searchTerm.ToLower().Trim();

            var users = _userRepository.Queryable.Where(a => a.Email == searchTerm || a.Id == searchTerm).ToList();
            if(users.Count == 0)
            {
                var ldapuser = _searchService.FindUser(searchTerm);
                if(ldapuser != null)
                {
                    Check.Require(!string.IsNullOrWhiteSpace(ldapuser.LoginId));
                    Check.Require(!string.IsNullOrWhiteSpace(ldapuser.EmailAddress));

                    var user = new User(ldapuser.LoginId);
                    user.Email = ldapuser.EmailAddress;
                    user.FirstName = ldapuser.FirstName;
                    user.LastName = ldapuser.LastName;

                    users.Add(user);
                }
            }

            //We don't want to show users that are not active
            var results =
                users.Where(a => a.IsActive).Select(a => new IdAndName(a.Id, string.Format("{0} {1}", a.FirstName, a.LastName))).ToList();
            return new JsonNetResult(results.Select(a => new { Id = a.Id, Label = a.Name }));
        }

        private ConditionalApprovalModifyModel CreateModifyModel(string approvalType, ConditionalApprovalModifyModel existingModel = null)
        {
            var model = existingModel ?? new ConditionalApprovalModifyModel { ApprovalType = approvalType };

            var userWithOrgs = GetUserWithOrgs();

            if(approvalType == WorkgroupType)
            {
                model.Workgroups = GetWorkgroups(userWithOrgs).ToList();

                if(!model.Workgroups.Any())
                {
                    return null;
                }
            }
            else if(approvalType == OrganizationType)
            {
                model.Organizations = userWithOrgs.Organizations.ToList();

                if(!model.Organizations.Any())
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            return model;
        }

        private User GetUserWithOrgs()
        {
            return
                _userRepository.Queryable.Where(x => x.Id == CurrentUser.Identity.Name).Fetch(x => x.Organizations).
                    Single();
        }

        private IQueryable<Workgroup> GetWorkgroups(User user)
        {
            var orgIds = user.Organizations.Select(x => x.Id).ToArray();

            return _workgroupRepository.Queryable.Where(x => x.Organizations.Any(a => orgIds.Contains(a.Id)));

        }

        private User GetUserBySearchTerm(string searchTerm)
        {
            return _userRepository.Queryable.SingleOrDefault(x => x.Id == searchTerm || x.Email == searchTerm);
        }


    }




}
