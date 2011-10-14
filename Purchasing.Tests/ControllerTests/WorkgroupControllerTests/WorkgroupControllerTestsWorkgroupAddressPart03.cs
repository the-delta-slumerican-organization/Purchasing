﻿using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.MappingModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcContrib.TestHelper;
using Purchasing.Core.Domain;
using Purchasing.Tests.Core;
using Purchasing.Web.Controllers;
using Purchasing.Web.Models;
using Rhino.Mocks;
using UCDArch.Testing;


namespace Purchasing.Tests.ControllerTests.WorkgroupControllerTests
{
    public partial class WorkgroupControllerTests
    {
        #region AddressDetails Tests
        [TestMethod]
        public void TestAddressDetailsRedirectsIfWorkgroupIdNotFound()
        {
            #region Arrange
            SetupDataForAddress();
            #endregion Arrange

            #region Act
            Controller.AddressDetails(4, 6)
                .AssertActionRedirect()
                .ToAction<WorkgroupController>(a => a.Index());
            #endregion Act

            #region Assert
            Assert.AreEqual("Workgroup could not be found.", Controller.ErrorMessage);
            #endregion Assert
        }

        [TestMethod]
        public void TestAddressDetailsRedirectsIfAddressIsNotInWorkgroup()
        {
            #region Arrange
            SetupDataForAddress();
            #endregion Arrange

            #region Act
            var result = Controller.AddressDetails(3, 6)
                .AssertActionRedirect()
                .ToAction<WorkgroupController>(a => a.Addresses(3));
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.RouteValues["id"]);
            Assert.AreEqual("Address not found.", Controller.ErrorMessage);
            #endregion Assert
        }

        [TestMethod]
        public void TestAddressDetailsReturnsView1()
        {
            #region Arrange
            SetupDataForAddress();
            #endregion Arrange

            #region Act
            var result = Controller.AddressDetails(2, 4)
                .AssertViewRendered()
                .WithViewData<WorkgroupAddressViewModel>();
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Name2", result.State.Name);
            Assert.IsNull(result.States);
            Assert.AreEqual("Name4", result.WorkgroupAddress.Name);
            #endregion Assert
        }

        [TestMethod]
        public void TestAddressDetailsReturnsView2()
        {
            #region Arrange
            SetupDataForAddress();
            #endregion Arrange

            #region Act
            var result = Controller.AddressDetails(2, 5)
                .AssertViewRendered()
                .WithViewData<WorkgroupAddressViewModel>();
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Name2", result.State.Name);
            Assert.IsNull(result.States);
            Assert.AreEqual("Name5", result.WorkgroupAddress.Name);
            #endregion Assert
        }

        [TestMethod]
        public void TestAddressDetailsReturnsView3()
        {
            #region Arrange
            SetupDataForAddress();
            #endregion Arrange

            #region Act
            var result = Controller.AddressDetails(2, 6)
                .AssertViewRendered()
                .WithViewData<WorkgroupAddressViewModel>();
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Name2", result.State.Name);
            Assert.IsNull(result.States);
            Assert.AreEqual("Name6", result.WorkgroupAddress.Name);
            #endregion Assert
        }
        #endregion AddressDetails Tests

        #region EditAddress Get Tests

        [TestMethod]
        public void TestEditAddressGetRedirectsIfWorkgroupIdNotFound()
        {
            #region Arrange
            SetupDataForAddress();
            #endregion Arrange

            #region Act
            Controller.EditAddress(4, 6)
                .AssertActionRedirect()
                .ToAction<WorkgroupController>(a => a.Index());
            #endregion Act

            #region Assert
            Assert.AreEqual("Workgroup could not be found.", Controller.ErrorMessage);
            #endregion Assert
        }

        [TestMethod]
        public void TestEditAddressGetRedirectsIfAddressIsNotInWorkgroup()
        {
            #region Arrange
            SetupDataForAddress();
            #endregion Arrange

            #region Act
            var result = Controller.EditAddress(3, 6)
                .AssertActionRedirect()
                .ToAction<WorkgroupController>(a => a.Addresses(3));
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.RouteValues["id"]);
            Assert.AreEqual("Address not found.", Controller.ErrorMessage);
            #endregion Assert
        }

        [TestMethod]
        public void TestEditAddressGetReturnsView1()
        {
            #region Arrange
            SetupDataForAddress();
            #endregion Arrange

            #region Act
            var result = Controller.EditAddress(2, 4)
                .AssertViewRendered()
                .WithViewData<WorkgroupAddressViewModel>();
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.States.Count());
            Assert.AreEqual("Name4", result.WorkgroupAddress.Name);
            #endregion Assert
        }

        [TestMethod]
        public void TestEditAddressGetReturnsView2()
        {
            #region Arrange
            SetupDataForAddress();
            #endregion Arrange

            #region Act
            var result = Controller.EditAddress(2, 5)
                .AssertViewRendered()
                .WithViewData<WorkgroupAddressViewModel>();
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.States.Count());
            Assert.AreEqual("Name5", result.WorkgroupAddress.Name);
            #endregion Assert
        }

        [TestMethod]
        public void TestEditAddressGetReturnsView3()
        {
            #region Arrange
            SetupDataForAddress();
            #endregion Arrange

            #region Act
            var result = Controller.EditAddress(2, 6)
                .AssertViewRendered()
                .WithViewData<WorkgroupAddressViewModel>();
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.States.Count());
            Assert.AreEqual("Name6", result.WorkgroupAddress.Name);
            #endregion Assert
        }
        #endregion EditAddress Get Tests

        #region Edit Address Post Tests

        [TestMethod]
        public void TestEditAddressPostRedirectsIfWorkgroupIdNotFound()
        {
            #region Arrange
            SetupDataForAddress();
            #endregion Arrange

            #region Act
            Controller.EditAddress(4, 6, new WorkgroupAddress())
                .AssertActionRedirect()
                .ToAction<WorkgroupController>(a => a.Index());
            #endregion Act

            #region Assert
            Assert.AreEqual("Workgroup could not be found.", Controller.ErrorMessage);
            #endregion Assert
        }

        [TestMethod]
        public void TestEditAddressPostRedirectsIfAddressIsNotInWorkgroup()
        {
            #region Arrange
            SetupDataForAddress();
            #endregion Arrange

            #region Act
            var result = Controller.EditAddress(3, 6, new WorkgroupAddress())
                .AssertActionRedirect()
                .ToAction<WorkgroupController>(a => a.Addresses(3));
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.RouteValues["id"]);
            Assert.AreEqual("Address not found.", Controller.ErrorMessage);
            #endregion Assert
        }


        [TestMethod]
        public void TestEditAddressPostWithInvalidAddressReturnsView()
        {
            #region Arrange
            SetupDataForAddress();
            var address = CreateValidEntities.WorkgroupAddress(4);
            address.Name = string.Empty;
            #endregion Arrange

            #region Act
            var result = Controller.EditAddress(2, 4, address)
                .AssertViewRendered()
                .WithViewData<WorkgroupAddressViewModel>();
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.States.Count());
            Assert.AreEqual(string.Empty, result.WorkgroupAddress.Name);
            Assert.AreEqual("Unable to save due to errors.", Controller.ErrorMessage);
            Controller.ModelState.AssertErrorsAre("The Name field is required.");
            #endregion Assert		
        }


        [TestMethod]
        public void TestEditAddressPostWhenAddressIsNotChanged()
        {
            #region Arrange
            SetupDataForAddress();
            var address = CreateValidEntities.WorkgroupAddress(4);
            address.SetIdTo(4);
            WorkgroupAddressService.Expect(a => a.CompareAddress(address, WorkgroupRepository.GetNullableById(2).Addresses[0])).Return(4);
            #endregion Arrange

            #region Act
            var result = Controller.EditAddress(2, 4, address)
                .AssertViewRendered()
                .WithViewData<WorkgroupAddressViewModel>();
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.States.Count());
            Assert.AreEqual("Name4", result.WorkgroupAddress.Name);
            Assert.AreEqual("No changes made", Controller.Message);
            #endregion Assert		
        }

        [TestMethod]
        public void TestEditAddressPostWhenAddressIsChangedToAnActiveAddress()
        {
            #region Arrange
            SetupDataForAddress();
            var address = CreateValidEntities.WorkgroupAddress(4);
            address.SetIdTo(4);
            WorkgroupAddressService.Expect(a => a.CompareAddress(address, WorkgroupRepository.GetNullableById(2).Addresses[0])).Return(0);
            WorkgroupAddressService.Expect(a => a.CompareAddress(address, WorkgroupRepository.GetNullableById(2).Addresses[2])).Return(6);
            #endregion Arrange

            #region Act
            var result = Controller.EditAddress(2, 4, address)
                .AssertViewRendered()
                .WithViewData<WorkgroupAddressViewModel>();
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.States.Count());
            Assert.AreEqual("Name4", result.WorkgroupAddress.Name);
            Assert.AreEqual("The address you are changing this to already exists. Unable to save.", Controller.ErrorMessage);
            #endregion Assert
        }

        [TestMethod]
        public void TestEditAddressPostWhenAddressIsChangedToAnInactiveAddress()
        {
            #region Arrange
            SetupDataForAddress();
            var address = CreateValidEntities.WorkgroupAddress(4);
            address.Name = "Fake";
            address.SetIdTo(4);
            WorkgroupAddressService.Expect(a => a.CompareAddress(address, WorkgroupRepository.GetNullableById(2).Addresses[0])).Return(0);
            WorkgroupAddressService.Expect(a => a.CompareAddress(address, WorkgroupRepository.GetNullableById(2).Addresses[1])).Return(5);
            WorkgroupAddressService.Expect(a => a.CompareAddress(address, WorkgroupRepository.GetNullableById(2).Addresses[2])).Return(0);
            #endregion Arrange

            #region Act
            var result = Controller.EditAddress(2, 4, address)
                .AssertActionRedirect()
                .ToAction<WorkgroupController>(a => a.Addresses(2));
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.RouteValues["id"]);
            Assert.AreEqual("Address updated", Controller.Message);
            WorkgroupRepository.AssertWasCalled(a => a.EnsurePersistent(Arg<Workgroup>.Is.Anything));
            var args = (Workgroup) WorkgroupRepository.GetArgumentsForCallsMadeOn(a => a.EnsurePersistent(Arg<Workgroup>.Is.Anything))[0][0]; 
            Assert.IsNotNull(args);
            Assert.AreEqual(3, args.Addresses.Count());
            Assert.AreEqual("Name4", args.Addresses[0].Name);
            Assert.IsFalse(args.Addresses[0].IsActive); //Disabled one we are editing

            Assert.AreEqual("Name5", args.Addresses[1].Name);
            Assert.IsTrue(args.Addresses[1].IsActive); //Enabled one that matches in workgroup

            Assert.AreEqual("Name6", args.Addresses[2].Name); //Not Touched
            Assert.IsTrue(args.Addresses[2].IsActive);
            #endregion Assert
        }

        [TestMethod]
        public void TestEditAddressPostWhenAddressIsChangedToAnNewAddress()
        {
            #region Arrange
            SetupDataForAddress();
            var address = CreateValidEntities.WorkgroupAddress(4);
            address.Name = "Fake";
            address.SetIdTo(4);
            WorkgroupAddressService.Expect(a => a.CompareAddress(address, WorkgroupRepository.GetNullableById(2).Addresses[0])).Return(0);
            WorkgroupAddressService.Expect(a => a.CompareAddress(address, WorkgroupRepository.GetNullableById(2).Addresses[1])).Return(0);
            WorkgroupAddressService.Expect(a => a.CompareAddress(address, WorkgroupRepository.GetNullableById(2).Addresses[2])).Return(0);
            #endregion Arrange

            #region Act
            var result = Controller.EditAddress(2, 4, address)
                .AssertActionRedirect()
                .ToAction<WorkgroupController>(a => a.Addresses(2));
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.RouteValues["id"]);
            Assert.AreEqual("Address updated.", Controller.Message);
            WorkgroupRepository.AssertWasCalled(a => a.EnsurePersistent(Arg<Workgroup>.Is.Anything));
            var args = (Workgroup)WorkgroupRepository.GetArgumentsForCallsMadeOn(a => a.EnsurePersistent(Arg<Workgroup>.Is.Anything))[0][0];
            Assert.IsNotNull(args);
            Assert.AreEqual(4, args.Addresses.Count());
            Assert.AreEqual("Name4", args.Addresses[0].Name);
            Assert.IsFalse(args.Addresses[0].IsActive); //Disabled one we are editing

            Assert.AreEqual("Name5", args.Addresses[1].Name);
            Assert.IsFalse(args.Addresses[1].IsActive); 

            Assert.AreEqual("Name6", args.Addresses[2].Name); //Not Touched
            Assert.IsTrue(args.Addresses[2].IsActive);

            Assert.AreEqual("Fake", args.Addresses[3].Name); //New one
            Assert.IsTrue(args.Addresses[3].IsActive);
            #endregion Assert
        }
        #endregion Edit Address Post Tests
    }
}
