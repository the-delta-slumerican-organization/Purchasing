﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Purchasing.Core.Domain;
using Purchasing.Core.Helpers;
using Purchasing.Tests.Core;
using Rhino.Mocks;
using UCDArch.Testing;

namespace Purchasing.Tests.ServiceTests.OrderServiceTests
{
    public partial class OrderServiceTests
    {
        /// <summary>
        /// Test conditional Approval is passed
        /// </summary>
        [TestMethod]
        public void TestCreateApprovalsForNewOrder27()
        {
            #region Arrange
            var order = CreateValidEntities.Order(1);
            order.SetIdTo(99);
            order.Splits = new List<Split>();
            order.Splits.Add(new Split());
            order.Splits[0].Account = null;
            order.Splits[0].Order = order;
            order.Workgroup.SetIdTo(1);
            new FakeWorkgroupAccounts(3, WorkgroupAccountRepository);
            new FakeAccounts(3, AccountRepository);

            new FakeUsers(5, UserRepository);

            var conditionalApprovalIds = new[] {1};
            var conditionalApprovals = new List<ConditionalApproval>();
            conditionalApprovals.Add(CreateValidEntities.ConditionalApproval(1));
            conditionalApprovals[0].PrimaryApprover = CreateValidEntities.User(4);
            conditionalApprovals[0].SecondaryApprover = null;

            new FakeConditionalApprovals(0, ConditionalApprovalRepository, conditionalApprovals);

            #endregion Arrange

            #region Act
            OrderService.CreateApprovalsForNewOrder(order, conditionalApprovalIds, null, null, "2");
            #endregion Act

            #region Assert
            SecurityService.AssertWasNotCalled(a => a.GetUser(Arg<string>.Is.Anything)); // the account was not found in the workgroup or the account table
            EventService.AssertWasCalled(a => a.OrderApprovalAdded(Arg<Order>.Is.Anything, Arg<Approval>.Is.Anything, Arg<bool>.Is.Anything), x => x.Repeat.Times(3));
            EventService.AssertWasNotCalled(a => a.OrderAutoApprovalAdded(Arg<Order>.Is.Anything, Arg<Approval>.Is.Anything));
            EventService.AssertWasCalled(a => a.OrderCreated(order));

            Assert.AreEqual(4, order.Approvals.Count);
            Assert.AreEqual(OrderStatusCode.Codes.Approver, order.Approvals[0].StatusCode.Id);
            Assert.IsNull(order.Approvals[0].User);
            Assert.AreEqual(OrderStatusCode.Codes.AccountManager, order.Approvals[1].StatusCode.Id);
            Assert.AreEqual("LastName2", order.Approvals[1].User.LastName);
            Assert.AreEqual(OrderStatusCode.Codes.Purchaser, order.Approvals[2].StatusCode.Id);
            Assert.AreEqual("LastName4", order.Approvals[3].User.LastName);
            Assert.AreEqual(OrderStatusCode.Codes.ConditionalApprover, order.Approvals[3].StatusCode.Id);
            #endregion Assert
        }

        /// <summary>
        /// Test conditional Approval is passed
        /// </summary>
        [TestMethod]
        public void TestCreateApprovalsForNewOrder28()
        {
            #region Arrange
            var order = CreateValidEntities.Order(1);
            order.SetIdTo(99);
            order.Splits = new List<Split>();
            order.Splits.Add(new Split());
            order.Splits[0].Account = null;
            order.Splits[0].Order = order;
            order.Workgroup.SetIdTo(1);
            new FakeWorkgroupAccounts(3, WorkgroupAccountRepository);
            new FakeAccounts(3, AccountRepository);

            new FakeUsers(5, UserRepository);

            var conditionalApprovalIds = new[] { 1 };
            var conditionalApprovals = new List<ConditionalApproval>();
            conditionalApprovals.Add(CreateValidEntities.ConditionalApproval(1));
            conditionalApprovals[0].PrimaryApprover = CreateValidEntities.User(4);
            conditionalApprovals[0].SecondaryApprover = CreateValidEntities.User(5);

            new FakeConditionalApprovals(0, ConditionalApprovalRepository, conditionalApprovals);
            UserIdentity.Expect(a => a.Current).Return("4"); // Same As conditional approval, but conditional approvals do not auto approve.
            #endregion Arrange

            #region Act
            OrderService.CreateApprovalsForNewOrder(order, conditionalApprovalIds, null, null, "2");
            #endregion Act

            #region Assert
            SecurityService.AssertWasNotCalled(a => a.GetUser(Arg<string>.Is.Anything)); // the account was not found in the workgroup or the account table
            EventService.AssertWasCalled(a => a.OrderApprovalAdded(Arg<Order>.Is.Anything, Arg<Approval>.Is.Anything, Arg<bool>.Is.Anything), x => x.Repeat.Times(3));
            EventService.AssertWasNotCalled(a => a.OrderAutoApprovalAdded(Arg<Order>.Is.Anything, Arg<Approval>.Is.Anything));
            EventService.AssertWasCalled(a => a.OrderCreated(order));

            Assert.AreEqual(4, order.Approvals.Count);
            Assert.AreEqual(OrderStatusCode.Codes.Approver, order.Approvals[0].StatusCode.Id);
            Assert.IsNull(order.Approvals[0].User);
            Assert.AreEqual(OrderStatusCode.Codes.AccountManager, order.Approvals[1].StatusCode.Id);
            Assert.AreEqual("LastName2", order.Approvals[1].User.LastName);
            Assert.AreEqual(OrderStatusCode.Codes.Purchaser, order.Approvals[2].StatusCode.Id);
            Assert.AreEqual("LastName4", order.Approvals[3].User.LastName);
            Assert.AreEqual(OrderStatusCode.Codes.ConditionalApprover, order.Approvals[3].StatusCode.Id);
            Assert.AreEqual("LastName5", order.Approvals[3].SecondaryUser.LastName);

            #endregion Assert
        }

        /// <summary>
        /// Test conditional Approval is passed
        /// </summary>
        [TestMethod]
        public void TestCreateApprovalsForNewOrder29()
        {
            #region Arrange
            var order = CreateValidEntities.Order(1);
            order.SetIdTo(99);
            order.Splits = new List<Split>();
            order.Splits.Add(new Split());
            order.Splits[0].Account = null;
            order.Splits[0].Order = order;
            order.Workgroup.SetIdTo(1);
            new FakeWorkgroupAccounts(3, WorkgroupAccountRepository);
            new FakeAccounts(3, AccountRepository);

            new FakeUsers(6, UserRepository);

            var conditionalApprovalIds = new[] { 1,2 };
            var conditionalApprovals = new List<ConditionalApproval>();
            conditionalApprovals.Add(CreateValidEntities.ConditionalApproval(1));
            conditionalApprovals[0].PrimaryApprover = CreateValidEntities.User(4);
            conditionalApprovals[0].SecondaryApprover = CreateValidEntities.User(5);

            conditionalApprovals.Add(CreateValidEntities.ConditionalApproval(2));
            conditionalApprovals[1].PrimaryApprover = CreateValidEntities.User(6);
            conditionalApprovals[1].SecondaryApprover = null;

            new FakeConditionalApprovals(0, ConditionalApprovalRepository, conditionalApprovals);
            UserIdentity.Expect(a => a.Current).Return("4"); // Same As conditional approval, but conditional approvals do not auto approve.
            #endregion Arrange

            #region Act
            OrderService.CreateApprovalsForNewOrder(order, conditionalApprovalIds, null, null, "2");
            #endregion Act

            #region Assert
            SecurityService.AssertWasNotCalled(a => a.GetUser(Arg<string>.Is.Anything)); // the account was not found in the workgroup or the account table
            EventService.AssertWasCalled(a => a.OrderApprovalAdded(Arg<Order>.Is.Anything, Arg<Approval>.Is.Anything, Arg<bool>.Is.Anything), x => x.Repeat.Times(3));
            EventService.AssertWasNotCalled(a => a.OrderAutoApprovalAdded(Arg<Order>.Is.Anything, Arg<Approval>.Is.Anything));
            EventService.AssertWasCalled(a => a.OrderCreated(order));

            Assert.AreEqual(5, order.Approvals.Count);
            Assert.AreEqual(OrderStatusCode.Codes.Approver, order.Approvals[0].StatusCode.Id);
            Assert.IsNull(order.Approvals[0].User);
            Assert.AreEqual(OrderStatusCode.Codes.AccountManager, order.Approvals[1].StatusCode.Id);
            Assert.AreEqual("LastName2", order.Approvals[1].User.LastName);
            Assert.AreEqual(OrderStatusCode.Codes.Purchaser, order.Approvals[2].StatusCode.Id);
            Assert.AreEqual("LastName4", order.Approvals[3].User.LastName);
            Assert.AreEqual(OrderStatusCode.Codes.ConditionalApprover, order.Approvals[3].StatusCode.Id);
            Assert.AreEqual("LastName5", order.Approvals[3].SecondaryUser.LastName);
            Assert.AreEqual("LastName6", order.Approvals[4].User.LastName);
            Assert.AreEqual(OrderStatusCode.Codes.ConditionalApprover, order.Approvals[4].StatusCode.Id);
            Assert.IsNull(order.Approvals[4].SecondaryUser);
            #endregion Assert
        }

        /// <summary>
        /// Same as test 22 except conditional approval added
        /// 2 external account, 1 with workgroup
        /// 1 auto Approval because of amount
        /// </summary>
        [TestMethod]
        public void TestCreateApprovalsForNewOrder30()
        {
            #region Arrange
            var order = CreateValidEntities.Order(1);
            order.SetIdTo(99);
            order.Splits = new List<Split>();
            order.Splits.Add(new Split());
            order.Splits[0].Account = "12345";
            order.Splits[0].Order = order;

            order.Splits.Add(new Split());
            order.Splits[1].Account = "23456";
            order.Splits[1].Order = order;

            order.Splits.Add(new Split());
            order.Splits[2].Account = "777";
            order.Splits[2].Order = order;

            order.Workgroup.SetIdTo(1);
            var workgroupAccounts = new List<WorkgroupAccount>();
            workgroupAccounts.Add(CreateValidEntities.WorkgroupAccount(1));
            workgroupAccounts[0].Workgroup = order.Workgroup;
            workgroupAccounts[0].Account = CreateValidEntities.Account(9);
            workgroupAccounts[0].Account.SetIdTo("777");
            workgroupAccounts[0].Approver = CreateValidEntities.User(11);
            workgroupAccounts[0].Approver.SetIdTo("11");
            workgroupAccounts[0].AccountManager = CreateValidEntities.User(22);
            workgroupAccounts[0].Purchaser = CreateValidEntities.User(33);

            new FakeWorkgroupAccounts(0, WorkgroupAccountRepository, workgroupAccounts);

            var accounts = new List<Account>();
            accounts.Add(CreateValidEntities.Account(1));
            accounts[0].SetIdTo("23456");
            accounts[0].AccountManagerId = "TestUser";
            accounts.Add(CreateValidEntities.Account(2));
            accounts[1].SetIdTo("12345");
            accounts[1].AccountManagerId = null;
            new FakeAccounts(0, AccountRepository, accounts, true);
            SecurityService.Expect(a => a.GetUser("TestUser")).Return(CreateValidEntities.User(55));
            SecurityService.Expect(a => a.GetUser(null)).Return(null);

            var autoApprovals = new List<AutoApproval>();
            autoApprovals.Add(CreateValidEntities.AutoApproval(1));
            autoApprovals[0].User = CreateValidEntities.User(11);
            autoApprovals[0].IsActive = true;
            autoApprovals[0].Expiration = DateTime.UtcNow.ToPacificTime().AddDays(1);
            autoApprovals[0].TargetUser = null;
            autoApprovals[0].Account = workgroupAccounts[0].Account;
            new FakeAutoApprovals(0, AutoAprovalRepository, autoApprovals);

            //UserIdentity.Expect(a => a.Current).Return("11");

            new FakeUsers(5, UserRepository);

            var conditionalApprovalIds = new[] { 1 };
            var conditionalApprovals = new List<ConditionalApproval>();
            conditionalApprovals.Add(CreateValidEntities.ConditionalApproval(1));
            conditionalApprovals[0].PrimaryApprover = CreateValidEntities.User(4);
            conditionalApprovals[0].SecondaryApprover = CreateValidEntities.User(5);

            new FakeConditionalApprovals(0, ConditionalApprovalRepository, conditionalApprovals);
            #endregion Arrange

            #region Act
            OrderService.CreateApprovalsForNewOrder(order, conditionalApprovalIds, null, null, null);
            #endregion Act

            #region Assert
            SecurityService.AssertWasCalled(a => a.GetUser("TestUser"));
            SecurityService.AssertWasCalled(a => a.GetUser(null));
            EventService.AssertWasCalled(a => a.OrderApprovalAdded(Arg<Order>.Is.Anything, Arg<Approval>.Is.Anything, Arg<bool>.Is.Anything), x => x.Repeat.Times(4));
            EventService.AssertWasCalled(a => a.OrderAutoApprovalAdded(Arg<Order>.Is.Anything, Arg<Approval>.Is.Anything));
            EventService.AssertWasCalled(a => a.OrderCreated(order));

            Assert.AreEqual(6, order.Approvals.Count);
            Assert.AreEqual(OrderStatusCode.Codes.AccountManager, order.Approvals[0].StatusCode.Id);
            Assert.AreEqual(OrderStatusCode.Codes.Purchaser, order.Approvals[1].StatusCode.Id);
            var purchaserCount = 0;
            var approverCount = 0;
            var acctManagerCount = 0;
            var conditionalApproverCount = 0;
            foreach(var approval in order.Approvals)
            {
                switch(approval.StatusCode.Id)
                {
                    case OrderStatusCode.Codes.Purchaser:
                        purchaserCount++;
                        break;
                    case OrderStatusCode.Codes.AccountManager:
                        acctManagerCount++;
                        break;
                    case OrderStatusCode.Codes.Approver:
                        approverCount++;
                        break;
                    case OrderStatusCode.Codes.ConditionalApprover:
                        conditionalApproverCount++;
                        break;
                    default:
                        throw new Exception("Should not be here!");
                }

                //Assert.IsNull(approval.User);
            }
            Assert.AreEqual(1, purchaserCount);
            Assert.AreEqual(3, acctManagerCount);
            Assert.AreEqual(1, approverCount);
            Assert.AreEqual(1, conditionalApproverCount);
            //Assert.AreEqual("LastName66", order.Approvals[0].User.LastName);
            Assert.AreEqual("LastName55", order.Approvals[2].User.LastName);
            Assert.AreEqual("12345", order.Splits[0].Account);
            Assert.AreEqual("23456", order.Splits[1].Account);
            Assert.IsTrue(order.Approvals[3].Completed);
            #endregion Assert
        }
    }
}
