﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Purchasing.Core.Domain;
using Purchasing.Core.Queries;
using Purchasing.Web.App_GlobalResources;
using Purchasing.Web.Attributes;
using Purchasing.Web.Models;
using Purchasing.Web.Services;
using UCDArch.Core.PersistanceSupport;
using UCDArch.Core.Utils;
using MvcContrib;
using Purchasing.Core;
using UCDArch.Web.ActionResults;
using UCDArch.Web.Attributes;

namespace Purchasing.Web.Controllers
{
    /// <summary>
    /// Controller for the Order class
    /// </summary>
    public class OrderController : ApplicationController
    {
        private readonly IOrderService _orderAccessService;
        private readonly IOrderService _orderService;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ISecurityService _securityService;
        private readonly IDirectorySearchService _directorySearchService;

        public OrderController(IRepositoryFactory repositoryFactory, IOrderService orderAccessService, IOrderService orderService, ISecurityService securityService, IDirectorySearchService directorySearchService)
        {
            _orderAccessService = orderAccessService;
            _orderService = orderService;
            _repositoryFactory = repositoryFactory;
            _securityService = securityService;
            _directorySearchService = directorySearchService;
        }

        /// <summary>
        /// List of orders
        /// </summary>
        /// <param name="selectedOrderStatus"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="showAll">Matches AllActive in GetListOfOrders</param>
        /// <param name="showCompleted">Matches All in GetListOfOrders</param>
        /// <param name="showOwned"></param>
        /// <param name="hideOrdersYouCreated">Hide orders which you have created</param>
        /// <param name="showLast"></param>
        /// <returns></returns>
        public ActionResult Index(string selectedOrderStatus, DateTime? startDate, DateTime? endDate, bool showPending = false, string showLast = null) //, bool showAll = false, bool showCompleted = false, bool showOwned = false, bool hideOrdersYouCreated = false)
        {
            //TODO: Review even/odd display of table once Trish has look at it. (This page is a single, and the background color is the same as the even background color.
            if (selectedOrderStatus == "All")
            {
                selectedOrderStatus = null;
            }
            // TODO: get rid of hard coded "complete" status in favor of looking at StatusCode.IsComplete attribute
            var isComplete = selectedOrderStatus == "CP";

            IList<Order> orders;
            if (string.IsNullOrWhiteSpace(showLast))
            {
                orders = _orderAccessService.GetListofOrders(isComplete, showPending, selectedOrderStatus, startDate, endDate);
            } else
            {
                if (showLast== "month")
                {
                    orders =
                        Repository.OfType<CompletedOrdersThisMonth>().Queryable.Where(
                            a => a.OrderTrackingUser == CurrentUser.Identity.Name).Select(b => b.Order).ToList();
                } else
                {
                    orders = Repository.OfType<CompletedOrdersThisWeek>().Queryable.Where(
                            a => a.OrderTrackingUser == CurrentUser.Identity.Name).Select(b => b.Order).ToList();
                }
            }
            var viewModel = FilteredOrderListModel.Create(Repository, orders);
            viewModel.ShowLast = showLast;
            viewModel.SelectedOrderStatus = selectedOrderStatus;
            viewModel.StartDate = startDate;
            viewModel.EndDate = endDate;
            viewModel.ShowPending = showPending;
            viewModel.ColumnPreferences = _repositoryFactory.ColumnPreferencesRepository.GetNullableById(CurrentUser.Identity.Name) ??
                                          new ColumnPreferences(CurrentUser.Identity.Name);

            return View(viewModel);

        }

        /// <summary>
        /// Page to view Administrative Workgroup Orders
        /// </summary>
        /// <returns></returns>
        public ActionResult AdminOrders(string selectedOrderStatus, DateTime? startDate, DateTime? endDate, bool showPending = false)
        {
            //TODO: Review even/odd display of table once Trish has look at it. (This page is a single, and the background color is the same as the even background color.
            if (selectedOrderStatus == "All")
            {
                selectedOrderStatus = null;
            }
            // TODO: get rid of hard coded "complete" status in favor of looking at StatusCode.IsComplete attribute
            var isComplete = selectedOrderStatus == "CP";

            //TODO: replace/update this so it gets the admin list of orders.
            //var orders = _orderAccessService.GetListofOrders(showAll, showCompleted, showOwned, hideOrdersYouCreated, list, startDate, endDate);
            var orders = _orderAccessService.GetAdministrativeListofOrders(isComplete, showPending, selectedOrderStatus, startDate, endDate);
            var viewModel = FilteredOrderListModel.Create(Repository, orders);
            viewModel.SelectedOrderStatus = selectedOrderStatus;
            viewModel.StartDate = startDate;
            viewModel.EndDate = endDate;
            viewModel.ShowPending = showPending;
            viewModel.ColumnPreferences = _repositoryFactory.ColumnPreferencesRepository.GetNullableById(CurrentUser.Identity.Name) ??
                                          new ColumnPreferences(CurrentUser.Identity.Name);

            return View(viewModel);

        }

        /// <summary>
        /// If user has more than one workgroup, they select it for their order
        /// </summary>
        /// <returns></returns>
        public ActionResult SelectWorkgroup()
        {
            var user = GetCurrentUser();
            var role = _repositoryFactory.RoleRepository.GetNullableById(Role.Codes.Requester);
            var workgroups = user.WorkgroupPermissions.Where(a => a.Role == role && !a.Workgroup.Administrative).Select(a=>a.Workgroup);

            // only one workgroup, automatically redirect
            if (workgroups.Count() == 1)
            {
                var workgroup = workgroups.Single();
                return this.RedirectToAction(a => a.Request(workgroup.Id));
            }
            
            return View(workgroups.ToList());
        }

        /// <summary>
        /// Make an order request
        /// </summary>
        /// <param name="id">Workgroup Id</param>
        /// <returns></returns>
        public new ActionResult Request(int id)
        {
            var workgroup = _repositoryFactory.WorkgroupRepository.GetNullableById(id);

            if (workgroup == null)
            {
                return RedirectToAction("SelectWorkgroup");
            }

            if (!_securityService.HasWorkgroupAccess(workgroup))
            {
                return new HttpUnauthorizedResult(Resources.NoAccess_Workgroup);
            }

            var model = CreateOrderModifyModel(workgroup);

            return View(model);
        }

        [HttpPost]
        public new ActionResult Request(int id, OrderViewModel model)
        {
            var canCreateOrderInWorkgroup =
                _securityService.HasWorkgroupAccess(_repositoryFactory.WorkgroupRepository.GetById(model.Workgroup));

            Check.Require(canCreateOrderInWorkgroup);

            //TODO: no validation will be done!
            var order = new Order();

            BindOrderModel(order, model, includeLineItemsAndSplits: true);

            _orderService.CreateApprovalsForNewOrder(order, accountId: model.Account, approverId: model.Approvers, accountManagerId: model.AccountManagers, conditionalApprovalIds: model.ConditionalApprovals);

            _repositoryFactory.OrderRepository.EnsurePersistent(order);

            Message = Resources.NewOrder_Success;

            return RedirectToAction("Review", new { id = order.Id });
        }

        /// <summary>
        /// Edit the given order
        /// </summary>
        [AuthorizeEditOrder]
        public ActionResult Edit(int id)
        {
            var order = _repositoryFactory.OrderRepository.GetNullableById(id);
            Check.Require(order != null);
            
            var model = CreateOrderModifyModel(order.Workgroup);
            model.Order = order;

            var inactiveAccounts = GetInactiveAccountsForOrder(id);

            if (inactiveAccounts.Any())
            {
                ErrorMessage = Resources.InactiveAccounts_Warning +
                               string.Join(", ", inactiveAccounts);
            }

            return View(model);
        }

        [HttpPost]
        [AuthorizeEditOrder]
        public ActionResult Edit(int id, OrderViewModel model)
        {
            var order = _repositoryFactory.OrderRepository.GetNullableById(id);

            Check.Require(order != null);

            var adjustRouting = model.AdjustRouting.HasValue && model.AdjustRouting.Value;

            BindOrderModel(order, model, includeLineItemsAndSplits: adjustRouting);

            if (adjustRouting)
            {
                //TODO: Add expense validation
                //order.ValidateExpenses().ToArray();

                _orderService.ReRouteApprovalsForExistingOrder(order, approverId: model.Approvers, accountManagerId: model.AccountManagers);
            }
            else
            {
                _orderService.EditExistingOrder(order);
            }

            _repositoryFactory.OrderRepository.EnsurePersistent(order);

            Message = Resources.OrderEdit_Success;

            return RedirectToAction("Review", new { id });
        }
        
        /// <summary>
        /// Copy the existing order given 
        /// </summary>
        [AuthorizeReadOrEditOrder]
        public ActionResult Copy(int id)
        {
            var order = _repositoryFactory.OrderRepository.GetNullableById(id);
            Check.Require(order != null);

            var model = CreateOrderModifyModel(order.Workgroup);
            model.IsCopyOrder = true;
            model.Order = order;
            model.Order.Attachments.Clear(); //Clear out attachments so they don't get included w/ copied order

            var inactiveAccounts = GetInactiveAccountsForOrder(id);
            
            if (inactiveAccounts.Any())
            {
                ErrorMessage = Resources.InactiveAccounts_Warning +
                               string.Join(", ", inactiveAccounts);
            }

            return View(model);
        }

        [HttpPost]
        [AuthorizeReadOrEditOrder]
        public ActionResult Copy(int id, OrderViewModel model)
        {
            var order = new Order();

            BindOrderModel(order, model, includeLineItemsAndSplits: true);

            _orderService.CreateApprovalsForNewOrder(order, accountId: model.Account, approverId: model.Approvers, accountManagerId: model.AccountManagers, conditionalApprovalIds: model.ConditionalApprovals);

            _repositoryFactory.OrderRepository.EnsurePersistent(order);

            Message = Resources.OrderCopy_Success;

            return RedirectToAction("Review", new { id = order.Id });
        }

        /// <summary>
        /// Page to review an order and for approving/denying the order.
        /// </summary>
        /// <remarks>
        /// This page should be used by ad hoc account managers too, but without the link to edit
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthorizeReadOrEditOrder]
        public ActionResult Review(int id)
        {
            //TODO: so anyone can view any order?
            //TODO: eager fetch or fetch related collections separately to avoid a ton of queries
            var model = new ReviewOrderViewModel {Order = _repositoryFactory.OrderRepository.GetNullableById(id)};
            
            if (model.Order == null)
            {
                Message = Resources.NotFound_Order;
                //TODO: Workout a way to get a return to where the person came from, rather than just redirecting to the generic index
                //TODO: you can use the UrlReferrer for that //Scott
                return RedirectToAction("index");
            }

            if (model.Order.StatusCode.IsComplete)
            {   //complete orders can't ever be edited or cancelled so just return now
                return View(model);
            }

            model.CanEditOrder = _securityService.GetAccessLevel(model.Order) == OrderAccessLevel.Edit;
            model.CanCancelOrder = model.Order.CreatedBy.Id == CurrentUser.Identity.Name; //Can cancel the order if you are the one who created it
            model.IsRequesterInWorkgroup =
                model.Order.Workgroup.Permissions.Any(x => x.Role.Id == Role.Codes.Requester && x.User.Id == CurrentUser.Identity.Name);
            model.IsPurchaser = model.Order.StatusCode.Id == OrderStatusCode.Codes.Purchaser;
            model.IsAccountManager = model.Order.StatusCode.Id == OrderStatusCode.Codes.AccountManager;

            if (model.CanEditOrder)
            {
                if (model.IsAccountManager)
                {
                    model.HasAssociatedAccounts =
                        _repositoryFactory.SplitRepository.Queryable
                            .Any(s => s.Order.Id == model.Order.Id && s.Account != null);
                }

                if (model.IsPurchaser)
                {
                    model.OrderTypes = _repositoryFactory.OrderTypeRepository.Queryable.Where(x => x.PurchaserAssignable).ToList();
                }

                var app = from a in _repositoryFactory.ApprovalRepository.Queryable
                          where a.Order.Id == id && a.StatusCode.Level == a.Order.StatusCode.Level 
                                && a.Split != null && a.StatusCode.Id != OrderStatusCode.Codes.ConditionalApprover
                                && (!_repositoryFactory.WorkgroupAccountRepository.Queryable.Any(
                                  x => x.Workgroup.Id == model.Order.Workgroup.Id && x.Account.Id == a.Split.Account))
                          select a;

                model.ExternalApprovals = app.ToList();
            }

            return View(model);
        }

        /// <summary>
        /// Find an order by request number and redirect to the review page
        /// </summary>
        public ActionResult Lookup(string id)
        {
            var relatedOrderId =
                _repositoryFactory.OrderRepository.Queryable
                    .Where(x => x.RequestNumber == id)
                    .Select(x => x.Id)
                    .SingleOrDefault();

            if (relatedOrderId == default(int))
            {
                return new HttpNotFoundResult();
            }

            return RedirectToAction("Review", new {id = relatedOrderId});
        }

        [HttpPost]
        [AuthorizeEditOrder]
        public ActionResult Approve(int id /*order*/, string action, string comment, string orderType)
        {
            var order =
                _repositoryFactory.OrderRepository.Queryable.Fetch(x => x.Approvals).Single(x => x.Id == id);
            
            if (!string.IsNullOrWhiteSpace(comment))
            {
                order.AddComment(new OrderComment {Text = comment, User = GetCurrentUser()});
            }

            if (action == "Approve")
            {
                _orderService.Approve(order);   
            }
            else if (action == "Deny")
            {
                if (string.IsNullOrWhiteSpace(comment))
                {
                    ErrorMessage = Resources.CommentRequired_Order;
                    return RedirectToAction("Review", new {id});
                }

                _orderService.Deny(order, comment);
            }
            else if (action == "Complete")
            {
                var isPurchaser = order.StatusCode.Id == OrderStatusCode.Codes.Purchaser;

                Check.Require(isPurchaser);

                var newOrderType = _repositoryFactory.OrderTypeRepository.GetNullableById(orderType);
                Check.Require(newOrderType != null);

                _orderService.Complete(order, newOrderType);
            }

            _repositoryFactory.OrderRepository.EnsurePersistent(order); //Save approval changes

            Message = string.Format(Resources.ApprovalAction_Success, action, CurrentUser.Identity.Name);

            return RedirectToAction("Review", "Order", new {id});
        }

        [HttpPost]
        public ActionResult Cancel(int id, string comment)
        {
            var order = _repositoryFactory.OrderRepository.GetNullableById(id);

            Check.Require(order != null);

            if (order.CreatedBy.Id != CurrentUser.Identity.Name)
            {
                ErrorMessage = Resources.CancelOrder_NoAccess;
                return RedirectToAction("Review", new {id});
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                ErrorMessage = Resources.CancelOrder_CommentRequired;
                return RedirectToAction("Review", new {id});
            }

            order.AddComment(new OrderComment { Text = comment, User = GetCurrentUser() });

            _orderService.Cancel(order);

            _repositoryFactory.OrderRepository.EnsurePersistent(order);

            Message = Resources.OrderCancelled_Success;

            return RedirectToAction("Review", "Order", new { id });
        }

        /// <summary>
        /// Reroute the approval given by Id to the kerb person instead of the currently assigned user(s)
        /// </summary>
        [HttpPost]
        [AuthorizeEditOrder]
        public ActionResult ReRouteApproval(int id, int approvalId, string kerb)
        {
            var approval = _repositoryFactory.ApprovalRepository.GetNullableById(approvalId);

            Check.Require(approval != null);
            Check.Require(!approval.Completed);
            Check.Require(!approval.Order.Workgroup.Accounts.Select(a => a.Account.Id).Contains(approval.Split.Account), Resources.ReRouteApproval_AccountError);

            var user = _securityService.GetUser(kerb);

            _orderService.ReRouteSingleApprovalForExistingOrder(approval, user);

            _repositoryFactory.ApprovalRepository.EnsurePersistent(approval);

            return Json(new {success = true, name = user.FullName});
        }

        [HttpPost]
        [AuthorizeEditOrder]
        public JsonNetResult AddComment(int id, string comment)
        {
            var order = Repository.OfType<Order>().GetNullableById(id);

            var orderComment = new OrderComment() {Text = comment, User = GetCurrentUser()};
            order.AddComment(orderComment);

            _repositoryFactory.OrderRepository.EnsurePersistent(order);

            return
                new JsonNetResult(
                    new {Date = DateTime.Now.ToShortDateString(), Text = comment, User = orderComment.User.FullName});
        }

        /// <summary>
        /// Ajax call to search for any commodity codes, match by name
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public JsonResult SearchCommodityCodes(string searchTerm)
        {
            var results =
                _repositoryFactory.SearchRepository.SearchCommodities(searchTerm).Select(a => new {a.Id, a.Name});

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeReadOrEditOrder]
        public JsonNetResult GetLineItemsAndSplits(int id)
        {
            var inactiveAccounts = GetInactiveAccountsForOrder(id);

            var lineItems = _repositoryFactory.LineItemRepository
                .Queryable
                .Where(x => x.Order.Id == id)
                .Select(
                    x =>
                    new OrderViewModel.LineItem
                    {
                        CatalogNumber = x.CatalogNumber,
                        CommodityCode = x.Commodity.Id,
                        Description = x.Description,
                        Id = x.Id,
                        Notes = x.Notes,
                        Price = x.UnitPrice.ToString(),
                        Quantity = x.Quantity.ToString(),
                        Units = x.Unit,
                        Url = x.Url
                    })
                .ToList();

            var splits = (from s in _repositoryFactory.SplitRepository.Queryable
                          join a in _repositoryFactory.AccountRepository.Queryable on s.Account equals a.Id
                          where s.Order.Id == id
                          select new OrderViewModel.Split
                                     {
                                         Account = inactiveAccounts.Contains(a.Id) ? string.Empty : a.Id,
                                         AccountName = a.Name,
                                         Amount = s.Amount.ToString(CultureInfo.InvariantCulture),
                                         LineItemId = s.LineItem == null ? 0 : s.LineItem.Id,
                                         Project = s.Project,
                                         SubAccount = s.SubAccount
                                     }).ToList();

            OrderViewModel.SplitTypes splitType;

            if (splits.Any(x => x.LineItemId != 0))
            {
                splitType = OrderViewModel.SplitTypes.Line;
            }
            else
            {
                //splits count = 0 if no account specified, 1 if only one account was specified with no splits
                splitType = splits.Count <= 1 ? OrderViewModel.SplitTypes.None : OrderViewModel.SplitTypes.Order;
            }

            return new JsonNetResult(new { id, lineItems, splits, splitType = splitType.ToString() });
        }
        
        [HttpPost]
        [BypassAntiForgeryToken]
        public ActionResult AddVendor(int workgroupId, WorkgroupVendor vendor)
        {
            var workgroup = _repositoryFactory.WorkgroupRepository.GetById(workgroupId);

            Check.Require(_securityService.HasWorkgroupAccess(workgroup));

            workgroup.AddVendor(vendor);

            _repositoryFactory.WorkgroupRepository.EnsurePersistent(workgroup);

            return Json(new { id = vendor.Id });
        }

        [HttpPost]
        [BypassAntiForgeryToken]
        public ActionResult AddAddress(int workgroupId, WorkgroupAddress workgroupAddress)
        {
            var workgroup = _repositoryFactory.WorkgroupRepository.GetById(workgroupId);

            Check.Require(_securityService.HasWorkgroupAccess(workgroup));

            workgroup.AddAddress(workgroupAddress);

            _repositoryFactory.WorkgroupRepository.EnsurePersistent(workgroup);

            return Json(new { id = workgroupAddress.Id });
        }

        [HttpPost]
        [BypassAntiForgeryToken]
        public ActionResult UploadFile(int? orderId)
        {
            var request = ControllerContext.HttpContext.Request;
            var qqFile = request["qqfile"];

            var attachment = new Attachment
            {
                DateCreated = DateTime.Now,
                User = GetCurrentUser(),
                FileName = qqFile,
                ContentType = request.Headers["X-File-Type"]
            };

            if (String.IsNullOrEmpty(qqFile)) // IE
            {
                Check.Require(request.Files.Count > 0, Resources.FileUpload_NoFile);
                var file = request.Files[0];

                attachment.FileName = Path.GetFileNameWithoutExtension(file.FileName) +
                    Path.GetExtension(file.FileName).ToLower();

                attachment.ContentType = file.ContentType;
            }

            using (var binaryReader = new BinaryReader(request.InputStream))
            {
                attachment.Contents = binaryReader.ReadBytes((int)request.InputStream.Length);
            }

            if (orderId.HasValue) //Save directly to order if a value is passed, otherwise it needs to be assoc. by form
            {
                attachment.Order = _repositoryFactory.OrderRepository.GetById(orderId.Value);
            }

            _repositoryFactory.AttachmentRepository.EnsurePersistent(attachment);

            return Json(new { success = true, id = attachment.Id }, "text/html");
        }

        /// <summary>
        /// Allows a user to download any attachment file by providing the file ID
        /// </summary>
        public ActionResult ViewFile(Guid fileId)
        {
            var file = _repositoryFactory.AttachmentRepository.GetNullableById(fileId);

            if (file == null) return HttpNotFound(Resources.ViewFile_NotFound);

            var accessLevel = _securityService.GetAccessLevel(file.Order);

            if (!(OrderAccessLevel.Edit | OrderAccessLevel.Readonly).HasFlag(accessLevel))
            {
                return new HttpUnauthorizedResult(Resources.ViewFile_AccessDenied);
            }

            return File(file.Contents, file.ContentType, file.FileName);
        }

        private List<string> GetInactiveAccountsForOrder(int id)
        {
            var orderAccounts = _repositoryFactory.SplitRepository.Queryable.Where(x => x.Order.Id == id && x.Account != null).Select(x => x.Account).ToArray();

            var inactiveAccounts =
                _repositoryFactory.AccountRepository.Queryable.Where(a => orderAccounts.Contains(a.Id) && !a.IsActive).
                    Select(x => x.Id).ToList();

            return inactiveAccounts;
        } 

        private void BindOrderModel(Order order, OrderViewModel model, bool includeLineItemsAndSplits = false)
        {
            var workgroup = _repositoryFactory.WorkgroupRepository.GetById(model.Workgroup);

            //TODO: automapper?
            order.Vendor = model.Vendor == 0 ? null : _repositoryFactory.WorkgroupVendorRepository.GetById(model.Vendor);
            order.Address = _repositoryFactory.WorkgroupAddressRepository.GetById(model.ShipAddress);
            order.ShippingType = _repositoryFactory.ShippingTypeRepository.GetById(model.ShippingType);
            order.DateNeeded = model.DateNeeded;
            order.AllowBackorder = model.AllowBackorder;
            order.Workgroup = workgroup;
            order.Organization = workgroup.PrimaryOrganization; //TODO: why is this needed?
            order.DeliverTo = model.ShipTo;
            order.DeliverToEmail = model.ShipEmail;
            order.OrderType = order.OrderType ?? _repositoryFactory.OrderTypeRepository.GetById(OrderType.Types.OrderRequest);
            order.CreatedBy = _repositoryFactory.UserRepository.GetById(CurrentUser.Identity.Name);
            order.Justification = model.Justification;

            if (!string.IsNullOrWhiteSpace(model.Comments))
            {
                var comment = new OrderComment
                {
                    DateCreated = DateTime.Now,
                    User = _repositoryFactory.UserRepository.GetById(CurrentUser.Identity.Name),
                    Text = model.Comments
                };

                order.AddOrderComment(comment);
            }

            if (model.FileIds != null)
            {
                foreach (var fileId in model.FileIds)
                {
                    order.AddAttachment(_repositoryFactory.AttachmentRepository.GetById(fileId));
                }
            }

            if (model.CustomFields != null)
            {
                order.CustomFieldAnswers.Clear();

                foreach (var customField in model.CustomFields.Where(x => !string.IsNullOrWhiteSpace(x.Answer)))
                {
                    var answer = new CustomFieldAnswer
                    {
                        Answer = customField.Answer,
                        CustomField = _repositoryFactory.CustomFieldRepository.GetById(customField.Id)
                    };

                    order.AddCustomAnswer(answer);
                }
            }

            if (model.Restricted.IsRestricted)
            {
                var restricted = new ControlledSubstanceInformation
                {
                    AuthorizationNum = model.Restricted.Rua,
                    ClassSchedule = model.Restricted.Class,
                    Custodian = model.Restricted.Custodian,
                    EndUser = model.Restricted.Users,
                    StorageSite = model.Restricted.StorageSite,
                    Use = model.Restricted.Use
                };

                order.SetAuthorizationInfo(restricted);
            }
            else
            {
                order.ClearAuthorizationInfo();
            }

            if (includeLineItemsAndSplits)
            {
                order.EstimatedTax = decimal.Parse(model.Tax.TrimEnd('%'));
                order.ShippingAmount = decimal.Parse(model.Shipping.TrimStart('$'));
                order.FreightAmount = decimal.Parse(model.Freight.TrimStart('$'));

                order.LineItems.Clear(); //replace line items and splits
                order.Splits.Clear();

                //Add in line items
                foreach (var lineItem in model.Items)
                {
                    if (lineItem.IsValid())
                    {
                        var commodity = string.IsNullOrWhiteSpace(lineItem.CommodityCode)
                                            ? null
                                            : _repositoryFactory.CommodityRepository.GetNullableById(lineItem.CommodityCode);

                        //TODO: could use automapper later, but need to do validation
                        var orderLineItem = new LineItem
                        {
                            CatalogNumber = lineItem.CatalogNumber,
                            Commodity = commodity,
                            Description = lineItem.Description,
                            Notes = lineItem.Notes,
                            Quantity = decimal.Parse(lineItem.Quantity),
                            Unit = lineItem.Units,//TODO: shouldn't this link to UOM?
                            UnitPrice = decimal.Parse(lineItem.Price),
                            Url = lineItem.Url
                        };

                        order.AddLineItem(orderLineItem);

                        if (model.SplitType == OrderViewModel.SplitTypes.Line)
                        {
                            var lineItemId = lineItem.Id;

                            //Go through each split created for this line item
                            foreach (var split in model.Splits.Where(x => x.LineItemId == lineItemId))
                            {
                                if (split.IsValid())
                                {
                                    order.AddSplit(new Split
                                    {
                                        Account = split.Account,
                                        Amount = decimal.Parse(split.Amount),
                                        LineItem = orderLineItem,
                                        SubAccount = split.SubAccount,
                                        Project = split.Project
                                    });
                                }
                            }
                        }
                    }
                }

                //TODO: note that I am not checking an order split actually has valid splits, or that they add to the total.
                if (model.SplitType == OrderViewModel.SplitTypes.Order)
                {
                    foreach (var split in model.Splits)
                    {
                        if (split.IsValid())
                        {
                            order.AddSplit(new Split
                            {
                                Account = split.Account,
                                Amount = decimal.Parse(split.Amount),
                                SubAccount = split.SubAccount,
                                Project = split.Project
                            });
                        }
                    }
                }
                else if (model.SplitType == OrderViewModel.SplitTypes.None)
                {
                    order.AddSplit(new Split { Amount = order.Total(), Account = model.Account, SubAccount = model.SubAccount, Project = model.Project }); //Order with "no" splits get one split for the full amount
                }

                order.TotalFromDb = order.Total();
            }
        }

        private OrderModifyModel CreateOrderModifyModel(Workgroup workgroup)
        {
            //TODO: possibly just use SQL or get this from a view, depending on perf
            var model = new OrderModifyModel
            {
                Order = new Order(),
                Workgroup = workgroup,
                Units = _repositoryFactory.UnitOfMeasureRepository.GetAll(), //TODO: caching?
                Accounts = _repositoryFactory.WorkgroupAccountRepository.Queryable.Where(x => x.Workgroup.Id == workgroup.Id).Select(x => x.Account).ToList(),
                Vendors = _repositoryFactory.WorkgroupVendorRepository.Queryable.Where(x => x.Workgroup.Id == workgroup.Id).ToList(),
                Addresses = _repositoryFactory.WorkgroupAddressRepository.Queryable.Where(x => x.Workgroup.Id == workgroup.Id).ToList(),
                ShippingTypes = _repositoryFactory.ShippingTypeRepository.GetAll(), //TODO: caching?
                Approvers = _repositoryFactory.WorkgroupPermissionRepository.Queryable.Where(x => x.Role.Id == Role.Codes.Approver).Select(x => x.User).ToList(),
                AccountManagers = _repositoryFactory.WorkgroupPermissionRepository.Queryable.Where(x => x.Role.Id == Role.Codes.AccountManager).Select(x => x.User).ToList(),
                ConditionalApprovals = workgroup.AllConditionalApprovals,
                CustomFields = _repositoryFactory.CustomFieldRepository.Queryable.Where(x => x.Organization.Id == workgroup.PrimaryOrganization.Id).ToList()
            };

            return model;
        }
    }
}
