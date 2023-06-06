
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Xunit;

namespace MoneyTransfer.Controllers.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Aboutus_ReturnsViewResult()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Aboutus();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Contacts_ReturnsViewResult()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Contacts();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Contact_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var controller = new HomeController();
            var model = new Admin_NotificationTbl
            {
                Name = "John Doe",
                email = "john.doe@example.com",
                Subject = "Test Subject",
                Message = "Test Message"
            };

            // Act
            var result = controller.Contact(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Contacts", result.ActionName);
        }

        [Fact]
        public void Dashboard_ReturnsViewResult()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Dashboard();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void DownloadUser_ValidId_ReturnsViewResult()
        {
            // Arrange
            var controller = new HomeController();
            var dbobj = new MoneyGatewaydbEntities11();
            var transaction = new tbl_Transaction
            {
                ID = 1,
                From_Acc = "FromAccount",
                To_Acc = "ToAccount",
                Amount = 100,
                DateTime = DateTime.Now
            };
            dbobj.tbl_Transaction.Add(transaction);
            dbobj.SaveChanges();

            // Act
            var result = controller.DownloadUser(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transaction.From_Acc, result.ViewBag.FromAcc);
            Assert.Equal(transaction.To_Acc, result.ViewBag.ToAcc);
            Assert.Equal(transaction.Amount, result.ViewBag.Amount);
            Assert.Equal(transaction.DateTime, result.ViewBag.Date);
        }

        [Fact]
        public void SendMoney_WithSendMoneySession_ReturnsViewResult()
        {
            // Arrange
            var controller = new HomeController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Session = new TestSession()
                }
            };
            controller.HttpContext.Session.SetInt32("sendMoney", 1);

            // Act
            var result = controller.SendMoney(null);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void SendMoney_WithoutSendMoneySession_RedirectsToAction()
        {
            // Arrange
            var controller = new HomeController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Session = new TestSession()
                }
            };
        }
    }
}