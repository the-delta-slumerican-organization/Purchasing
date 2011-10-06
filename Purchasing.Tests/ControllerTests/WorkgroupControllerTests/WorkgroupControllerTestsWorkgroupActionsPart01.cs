﻿using System;
using System.Collections.Generic;
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
using UCDArch.Testing.Fakes;


namespace Purchasing.Tests.ControllerTests.WorkgroupControllerTests
{
    public partial class WorkgroupControllerTests
    {
        #region Index Tests

        [TestMethod]
        public void TestIndexReturnsView1()
        {
            #region Arrange
            Controller.ControllerContext.HttpContext = new MockHttpContext(0, new[] { "" }, "2");
            SetupDataForWorkgroupActions1();
            #endregion Arrange

            #region Act
            var result = Controller.Index()
                .AssertViewRendered()
                .WithViewData<IEnumerable<Workgroup>>().ToList();
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());

            Assert.AreEqual("Name4", result[0].Name);
            Assert.AreEqual("Name1", result[0].Organizations.ElementAt(0).Name);
            Assert.AreEqual("Name4", result[0].Organizations.ElementAt(1).Name);

            Assert.AreEqual("Name5", result[1].Name);
            Assert.AreEqual("Name1", result[1].Organizations.ElementAt(0).Name);
            Assert.AreEqual("Name5", result[1].Organizations.ElementAt(1).Name);

            Assert.AreEqual("Name6", result[2].Name);
            Assert.AreEqual("Name1", result[2].Organizations.ElementAt(0).Name);
            Assert.AreEqual("Name6", result[2].Organizations.ElementAt(1).Name);
            #endregion Assert		
        }
        
        [TestMethod]
        public void TestIndexReturnsView2()
        {
            #region Arrange
            Controller.ControllerContext.HttpContext = new MockHttpContext(0, new[] { "" }, "1");
            SetupDataForWorkgroupActions1();
            #endregion Arrange

            #region Act
            var result = Controller.Index()
                .AssertViewRendered()
                .WithViewData<IEnumerable<Workgroup>>();
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(9, result.Count());
            #endregion Assert
        }

        #endregion Index Tests

        #region Create Get Tests

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestCreateGetThrowsExceptionIfNoCurrentUser()
        {
            var thisFar = false;
            try
            {
                #region Arrange
                Controller.ControllerContext.HttpContext = new MockHttpContext(0, new[] { "" }, "NoMatch");
                SetupDataForWorkgroupActions1();
                thisFar = true;
                #endregion Arrange

                #region Act
                Controller.Create();
                #endregion Act
            }
            catch (Exception ex)
            {
                Assert.IsTrue(thisFar);
                Assert.IsNotNull(ex);
                Assert.AreEqual("Sequence contains no elements", ex.Message);
                throw;
            }	
        }


        [TestMethod]
        public void TestCreateGetReturnsView1()
        {
            #region Arrange
            Controller.ControllerContext.HttpContext = new MockHttpContext(0, new[] { "" }, "2");
            SetupDataForWorkgroupActions1();
            #endregion Arrange

            #region Act
            var result = Controller.Create()
                .AssertViewRendered()
                .WithViewData<WorkgroupModifyModel>();
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Organizations.Count());
            Assert.AreEqual("Name4", result.Organizations[0].ToString());
            Assert.AreEqual("Name5", result.Organizations[1].ToString());
            Assert.AreEqual("Name6", result.Organizations[2].ToString());
            Assert.IsNotNull(result.Workgroup);
            #endregion Assert		
        }

        [TestMethod]
        public void TestCreateGetReturnsView2()
        {
            #region Arrange
            Controller.ControllerContext.HttpContext = new MockHttpContext(0, new[] { "" }, "1");
            SetupDataForWorkgroupActions1();
            #endregion Arrange

            #region Act
            var result = Controller.Create()
                .AssertViewRendered()
                .WithViewData<WorkgroupModifyModel>();
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Organizations.Count());
            Assert.AreEqual("Name1", result.Organizations[0].ToString());
            Assert.AreEqual("Name2", result.Organizations[1].ToString());
            Assert.AreEqual("Name3", result.Organizations[2].ToString());
            Assert.IsNotNull(result.Workgroup);
            #endregion Assert
        }

        [TestMethod]
        public void TestCreateGetReturnsView3()
        {
            #region Arrange
            Controller.ControllerContext.HttpContext = new MockHttpContext(0, new[] { "" }, "3");
            SetupDataForWorkgroupActions1();
            #endregion Arrange

            #region Act
            var result = Controller.Create()
                .AssertViewRendered()
                .WithViewData<WorkgroupModifyModel>();
            #endregion Act

            #region Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Organizations.Count());
            Assert.AreEqual("Name7", result.Organizations[0].ToString());
            Assert.AreEqual("Name8", result.Organizations[1].ToString());
            Assert.AreEqual("Name9", result.Organizations[2].ToString());
            Assert.IsNotNull(result.Workgroup);
            #endregion Assert
        }

        #endregion Create Get Tests
    }
}