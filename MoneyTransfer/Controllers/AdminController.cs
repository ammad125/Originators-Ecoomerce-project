using MoneyTransfer.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MoneyTransfer.Controllers
{
    public class AdminController : Controller
    {
        MoneyGatewaydbEntities11 dbobj = new MoneyGatewaydbEntities11();
        // GET: Admin
        public ActionResult AdminIndex()
        {
            return View();
        }
        public ActionResult AdminDashboard()
        {
            return View();
        }
        public ActionResult AdminTransaction()
        {
            var list = dbobj.tbl_Transaction.ToList();
            return View(list);
        }
        public ActionResult Admin_Receive_Payments()
        {
            return View();
        }
        public ActionResult Admin_SendPayment_Request()
        {
            return View();
        }
        public ActionResult AdminUsers()
        {
            var UserRecord = dbobj.signup_table.ToList();
            return View(UserRecord);
        }
        public ActionResult Delete(int Id)
        {
            var del = dbobj.signup_table.Where(x => x.Id == Id).FirstOrDefault();
            dbobj.signup_table.Remove(del);
            dbobj.SaveChanges();
            var List = dbobj.signup_table.ToList();
            return RedirectToAction("AdminUsers", List);
        }
        public ActionResult Download(int Id)
        {
            var download = dbobj.tbl_Transaction.Where(x => x.ID == Id).FirstOrDefault();

            var From_Acc = download.From_Acc;
            var To_Acc = download.To_Acc;
            var Amount = download.Amount;
            var Date = download.DateTime;

            ViewBag.FromAcc = From_Acc;
            ViewBag.ToAcc = To_Acc;
            ViewBag.Amount = Amount;
            ViewBag.Date = Date;

            return View();
        }
        public ActionResult DeleteNoti(int id)
        {
            var delete = dbobj.Admin_NotificationTbl.Where(x => x.ID == id).FirstOrDefault();
            dbobj.Admin_NotificationTbl.Remove(delete);
            dbobj.SaveChanges();
            var List = dbobj.Admin_NotificationTbl.ToList();
            return RedirectToAction("AdminNotifications", List);
        }
        public ActionResult Update(int id)
        {
            var obj = dbobj.signup_table.Where(x => x.Id == id).FirstOrDefault();

            var user = new signup_table();
            if (obj != null && obj.Id != 0)
            {
                user = dbobj.signup_table.Find(obj.Id);
                user.password = DecryptCipherTextToPlainText(user.password);
                user.conf_password = DecryptCipherTextToPlainText(user.conf_password);
            }

            return View(obj);
        }
        [HttpPost]
        public ActionResult Update(signup_table model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    signup_table tbl = new signup_table();
                    tbl.Id = model.Id;
                    tbl.FullName = model.FullName;
                    tbl.username = model.username;
                    tbl.email = model.email;

                    var encryptedPass = EncryptPlainTextToCipherText(model.password);
                    var encryptedConf_Pass = EncryptPlainTextToCipherText(model.password);

                    tbl.password = encryptedPass;
                    tbl.conf_password = encryptedConf_Pass;
                    tbl.Account_No = model.Account_No;

                    dbobj.Entry(tbl).State = System.Data.Entity.EntityState.Modified;
                    dbobj.SaveChanges();
                }
                ModelState.Clear();
                return RedirectToAction("AdminUsers");
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return View();
        }
        public ActionResult AdminNotifications()
        {
            try
            {
                var notiRead = dbobj.Admin_NotificationTbl.Where(x => x.IsRead == false).ToList();
                notiRead.ForEach(x => x.IsRead = true);
                dbobj.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            var notisave = dbobj.Admin_NotificationTbl.ToList();
            return View(notisave);
        }

        public int notiCount()
        {
            int notisave = 0;
            notisave = dbobj.Admin_NotificationTbl.Where(x => x.IsRead == false).Count();
            if (notisave > 0)
                return notisave;
            else
                return 0;
        }

        //Manage Orders
        public ActionResult ManageOrders()
        {
            var listOrders = dbobj.cartItems.ToList();
            return View(listOrders);
        }
        public ActionResult stripeTransactions()
        {
            var Stripe_Data = dbobj.tbl_stripeDashboard.ToList();
            return View(Stripe_Data);
        }
        public ActionResult AddressDetails()
        {
            var Address = dbobj.address_details.ToList();
            return View(Address);
        }

        public ActionResult settings()
        {
            return View();
        }
        public ActionResult Reply(int id)
        {
            var reply = dbobj.Admin_NotificationTbl.Where(x => x.ID == id).FirstOrDefault();
            //for insert into table
            TempData["username"] = reply.username;
            var name = reply.Name;
            TempData["name"] = name;
            return View();
        }
        public ActionResult UserReply(User_Notifications model)
        {
            if (model.Admin_reply != null)
            {
                if (ModelState.IsValid)
                {
                    User_Notifications tbl = new User_Notifications();
                    tbl.UserName = model.UserName;
                    tbl.Admin_reply = model.Admin_reply;
                    tbl.IsRead = false;
                    dbobj.User_Notifications.Add(tbl);
                    dbobj.SaveChanges();
                }
            }
            ModelState.Clear();
            return RedirectToAction("AdminNotifications");
        }
        public int notiReply()
        {
            try
            {
                var notisave = 0;
                notisave = dbobj.User_Notifications.Where(x => x.IsRead == false && x.UserName == User.Identity.Name).Count();

                if (notisave > 0)
                    return notisave;
                else
                    return 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Password Encrypt/Decrypt
        private const string SecurityKey = "ComplexKeyHere_12121";
        public static string EncryptPlainTextToCipherText(string PlainText)
        {
            byte[] toEncryptedArray = UTF8Encoding.UTF8.GetBytes(PlainText);

            MD5CryptoServiceProvider objMD5CryptoService = new MD5CryptoServiceProvider();
            byte[] securityKeyArray = objMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(SecurityKey));
            objMD5CryptoService.Clear();

            var objTripleDESCryptoService = new TripleDESCryptoServiceProvider();
            objTripleDESCryptoService.Key = securityKeyArray;
            objTripleDESCryptoService.Mode = CipherMode.ECB;
            objTripleDESCryptoService.Padding = PaddingMode.PKCS7;

            var objCrytpoTransform = objTripleDESCryptoService.CreateEncryptor();
            byte[] resultArray = objCrytpoTransform.TransformFinalBlock(toEncryptedArray, 0, toEncryptedArray.Length);
            objTripleDESCryptoService.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string DecryptCipherTextToPlainText(string CipherText)
        {
            byte[] toEncryptArray = Convert.FromBase64String(CipherText);
            MD5CryptoServiceProvider objMD5CryptoService = new MD5CryptoServiceProvider();

            byte[] securityKeyArray = objMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(SecurityKey));
            objMD5CryptoService.Clear();

            var objTripleDESCryptoService = new TripleDESCryptoServiceProvider();
            objTripleDESCryptoService.Key = securityKeyArray;
            objTripleDESCryptoService.Mode = CipherMode.ECB;
            objTripleDESCryptoService.Padding = PaddingMode.PKCS7;

            var objCrytpoTransform = objTripleDESCryptoService.CreateDecryptor();
            byte[] resultArray = objCrytpoTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            objTripleDESCryptoService.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }
    }
}