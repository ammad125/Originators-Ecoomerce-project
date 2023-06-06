using MoneyTransfer.Database;
using MoneyTransfer.Model;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MoneyTransfer.Controllers
{
    
    public class HomeController : Controller
    {
        MoneyGatewaydbEntities11 dbobj = new MoneyGatewaydbEntities11();

        string resetCode;
        
        // GET: HomeDefault
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Aboutus()
        {
            return View();
        }
        public ActionResult Contacts()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Contact(Admin_NotificationTbl model)
        {
            if (ModelState.IsValid)
            {
                Admin_NotificationTbl tbl = new Admin_NotificationTbl();
                var username = User.Identity.Name;
                tbl.username = username;
                tbl.Name = model.Name;
                tbl.email = model.email;
                tbl.Subject = model.Subject;
                tbl.Message = model.Message;
                tbl.IsRead = false;
                dbobj.Admin_NotificationTbl.Add(tbl);
                dbobj.SaveChanges();
                TempData["AlertMessage"] = "Message has been Sent";
            }
            ModelState.Clear();
            return RedirectToAction("Contacts");
        }
        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult DownloadUser(int Id)
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
        public ActionResult SendMoney(string Message)
        {
            if (Convert.ToInt32(Session["sendMoney"]) == 1)
            {
                try
                {
                    var totalPrice = Session["total"];

                    var user = dbobj.signup_table.Where(BalanceTbl => BalanceTbl.username == User.Identity.Name).SingleOrDefault();
                    var data = dbobj.BalanceTbls.Where(BalanceTbl => BalanceTbl.Acc_Num == user.Account_No).SingleOrDefault();
                    
                    totalPrice = Convert.ToInt32(totalPrice);

                    ViewBag.Dollar = data.ActiveDollars;
                    ViewBag.Account = data.Acc_Num;
                    if (totalPrice == null)
                    {
                        ViewBag.totalAmount = 0;
                    }
                    else
                    {
                        ViewBag.totalAmount = totalPrice;
                    }

                    ViewBag.Message = Message;
                }
                catch (Exception ex)
                {
                    ex.Message.ToString();
                }
                return View();
            }
            else
            {
                return RedirectToAction("OTPForPayment");
            }
        }

        public ActionResult OTPForPayment()
        {
            var email = dbobj.signup_table.Where(x => x.username == User.Identity.Name).Select(u => u.email).SingleOrDefault();

            Random num = new Random();
            var key = "0123456789";
            var random = "";
            for (int i = 0; i < 6; i++)
            {
                int a = num.Next(10);
                random = random + key.ElementAt(a);
            }

            //List<StoreSignUp> storeSessions = (List<StoreSignUp>)Session["signupDATA"];

            resetExtra RE = new resetExtra();

            var list = dbobj.resetExtras.ToList();
            foreach (var item in list)
            {
                dbobj.resetExtras.Remove(item);
            }

            RE.username = User.Identity.Name;
            RE.code = random;

            dbobj.resetExtras.Add(RE);
            dbobj.SaveChanges();

            var subject = "Security Code";
            var body = "Hi " + User.Identity.Name + ", <br/> Your Payment Security code is. " +

                 " <br/><br/>" + random + "</a> <br/><br/>" +
                 "If you did not request a password reset, please ignore this email or reply to let us know.<br/><br/> Thank you";

            SendEmail(email, body, subject);

            TempData["Match"] = "Please enter security code we have sent you on -> " + email;

            return RedirectToAction("ActivationCodePayment");
        }
        public ActionResult ActivationCodePayment()
        {
            return View();
        }
        public ActionResult Security_code(string Act_code)
        {
            var yesORno = dbobj.ForgotPasswords.Where(x => x.guid == Act_code).ToList();
            var yes = dbobj.resetExtras.Where(x => x.code == Act_code).ToList();

            if (yesORno.Count == 0 && yes.Count != 0)
            {
                ForgotPassword FP = new ForgotPassword();
                FP.username = User.Identity.Name;
                FP.guid = yes[0].code;
                FP.createdOn = DateTime.Now;
                dbobj.ForgotPasswords.Add(FP);

                foreach (var item in yes)
                {
                    dbobj.resetExtras.Remove(item);
                }

                dbobj.SaveChanges();
                Session["sendMoney"] = "1";
                return RedirectToAction("SendMoney");
            }

            TempData["WrongCode"] = "Please enter correct code!!";
            return RedirectToAction("ActivationCodePayment");
        }

        [HttpPost]
        public ActionResult Charge(string stripeToken, string stripeEmail, int totalAmount)
        {
            StripeConfiguration.ApiKey = "sk_test_51KabhVGJmCzbucdgteD5ItiniCGXCjOJ4xeFQOmv53cIxqyLZOQk8jWzb6chbRVKg2Fje93HqnTLCLJ54R2K2nGx00Kwqktmqd";
            try
            {
                var myCharge = new Stripe.ChargeCreateOptions();

                // always set these properties
                myCharge.Amount = totalAmount * (100);
                myCharge.Currency = "PKR";
                myCharge.ReceiptEmail = stripeEmail;
                myCharge.Description = "Live Transaction";
                myCharge.Source = stripeToken;
                myCharge.Capture = true;
                var chargeService = new Stripe.ChargeService();

                Random num = new Random();
                var key = "0123456789";
                var random = "";
                for (int i = 0; i < 5; i++)
                {
                    int a = num.Next(10);
                    random = random + key.ElementAt(a);
                }

                var Transaction_id = random;
                var Amount = totalAmount;
                var Currency = myCharge.Currency;
                var Description = myCharge.Description;
                var Email = myCharge.ReceiptEmail;
                var Date = DateTime.Now;

                Charge stripeCharge = chargeService.Create(myCharge);

                var Balance_ID = stripeCharge.Id;
                var Risk = stripeCharge.Outcome.RiskScore;
                var Risk_Level = stripeCharge.Outcome.RiskLevel;
                var brand = stripeCharge.PaymentMethodDetails.Card.Brand;
                var Status = stripeCharge.Status;
                var Fingerprint = stripeCharge.PaymentMethodDetails.Card.Fingerprint;
                var Last4 = stripeCharge.PaymentMethodDetails.Card.Last4;
                var Exp_Month = stripeCharge.PaymentMethodDetails.Card.ExpMonth;
                var Exp_Year = stripeCharge.PaymentMethodDetails.Card.ExpYear;

                tbl_stripeDashboard tbl_stripe = new tbl_stripeDashboard()
                {
                    trans_id = Convert.ToInt32(Transaction_id),
                    Balnc_ID = Balance_ID,
                    amount = Amount,
                    currency = Currency,
                    description = Description,
                    email = Email,
                    date = Date,
                    username = User.Identity.Name,
                    risk = Risk.ToString(),
                    risk_level = Risk_Level,
                    Brand = brand,
                    status = Status,
                    fingerprint = Fingerprint,
                    last4 = Convert.ToInt32(Last4),
                    ExpMonth = Exp_Month.ToString(),
                    ExpYear = Exp_Year.ToString()
                };
                dbobj.tbl_stripeDashboard.Add(tbl_stripe);  //----- Insert Data -----

                List<Cart> cart = (List<Cart>)Session["cart"];    //-----------
                cartItem items = new cartItem();
                int ii = cart.Count();
                var set = Session["total"];

                foreach (var item in cart)
                {
                    items.trans_id = Transaction_id;
                    items.price = Convert.ToInt32(item.Price);
                    items.name = item.Name;
                    items.product = item._Product;
                    items.quantity = item.Quantity.ToString();
                    items.cart = item.cart.ToString();
                    items.total = set.ToString();
                    items.username = User.Identity.Name;

                    dbobj.cartItems.Add(items);  //----- Insert Data -----
                    dbobj.SaveChanges();
                }


                List<Payment_Details> p_details = (List<Payment_Details>)Session["address_details"];    //-----------

                address_details add_Details = new address_details();
                foreach (var item in p_details)
                {
                    add_Details.trans_id = Transaction_id;
                    add_Details.Full_Name = item.Full_Name;
                    add_Details.phone_num = item.phone_number;
                    add_Details.email = item.email;
                    add_Details.city = item.city;
                    add_Details.address_type = item.address_type;
                    add_Details.username = User.Identity.Name;
                }
                dbobj.address_details.Add(add_Details);  //----- Insert Data -----
                dbobj.SaveChanges();

                if (Status == "succeeded")
                {
                    Session["cart"] = null;
                    Session["address_details"] = null;
                    Session["total"] = null;
                    ViewBag.totalAmount = 0;
                }

                return RedirectToAction("SendMoney", new { Message = "Your Payment has been " + stripeCharge.Status + "" });
            }
            catch (Exception ex)
            {
                return RedirectToAction("SendMoney", new { Message = ex.Message });
            }
        }

        //Graphical Data
        public ActionResult Graph()
        {
            return View();
        }
        public ActionResult LoadData()
        {
            var single = dbobj.tbl_stripeDashboard.Where(x => x.username == User.Identity.Name).ToList();

            var amountGraph = single.Select(s => s.amount).ToList();
            var Trans_ID = single.Select(s => s.trans_id).ToList();

            //var amountGraph = dbobj.tbl_stripeDashboard.Select(u => u.amount).ToList();
            //var Trans_ID = dbobj.tbl_stripeDashboard.Select(u => u.trans_id).ToList();

            List<Data_Class> dc = new List<Data_Class>();
            Data_Class ac;

            for (int i = 0; i < Trans_ID.Count; i++)
            {
                ac = new Data_Class(1, amountGraph[i].ToString(), Convert.ToInt32(Trans_ID[i]));
                dc.Add(ac);
            }

            return Json(dc, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Modal(BalanceTbl model, string stripeToken, string stripeEmail)
        {
            var dollar = dbobj.BalanceTbls.Where(BalanceTbl => BalanceTbl.Name == User.Identity.Name).SingleOrDefault();
            var acc_valid = dbobj.signup_table.Where(x => x.Account_No == model.Acc_Num).FirstOrDefault();

            if (model.ActiveDollars <= dollar.ActiveDollars && model.ActiveDollars > 0 && acc_valid != null)
            {
                if (ModelState.IsValid)
                {
                    var check = dbobj.signup_table.Where(x => x.Account_No == model.Acc_Num).FirstOrDefault();
                    var txtActiveDollar = model.ActiveDollars;
                    if (check != null)
                    {
                        BalanceTbl tbl = new BalanceTbl();
                        tbl_Transaction trans = new tbl_Transaction();
                        var current = dbobj.BalanceTbls.Where(x => x.Acc_Num == model.Acc_Num).FirstOrDefault();
                        current.ActiveDollars = current.ActiveDollars + model.ActiveDollars;
                        dollar.ActiveDollars = dollar.ActiveDollars - model.ActiveDollars;

                        trans.From_Acc = dollar.Acc_Num;
                        trans.To_Acc = model.Acc_Num;
                        trans.Amount = model.ActiveDollars;
                        trans.DateTime = DateTime.Now.ToString();
                        dbobj.tbl_Transaction.Add(trans);
                        trans.IsRead_Sent = false;
                        trans.IsRead_Received = false;
                        dbobj.SaveChanges();

                        return RedirectToAction("success");
                    }
                }
            }
            else
            {
                TempData["load"] = "Your Transaction Failed due to some error";
                TempData.Keep(key: "load");
            }

            ModelState.Clear();
            return RedirectToAction("SendMoney");
        }
        public ActionResult success()
        {
            return View();
        }
        public ActionResult ReceivedMoney()
        {
            try
            {
                var model = dbobj.BalanceTbls.Where(x => x.Name == User.Identity.Name).FirstOrDefault();
                var match = dbobj.tbl_Transaction.Where(x => x.To_Acc == model.Acc_Num).FirstOrDefault();
                if (match != null)
                {
                    var notiRead = dbobj.tbl_Transaction.Where(x => x.IsRead_Received == false && x.To_Acc == match.To_Acc).ToList();
                    notiRead.ForEach(x => x.IsRead_Received = true);
                    dbobj.SaveChanges();
                }

                var rec_money = dbobj.BalanceTbls.Where(x => x.Name == User.Identity.Name).FirstOrDefault();
                var check_money = dbobj.tbl_Transaction.Where(x => x.To_Acc == rec_money.Acc_Num).ToList();
                return View(check_money);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult SentMoney()
        {
            try
            {
                var model = dbobj.BalanceTbls.Where(x => x.Name == User.Identity.Name).FirstOrDefault();
                var match = dbobj.tbl_Transaction.Where(x => x.From_Acc == model.Acc_Num).FirstOrDefault();
                if (match != null)
                {
                    var notiRead = dbobj.tbl_Transaction.Where(x => x.IsRead_Sent == false && x.From_Acc == match.From_Acc).ToList();
                    notiRead.ForEach(x => x.IsRead_Sent = true);
                    dbobj.SaveChanges();
                }
                var rec_money = dbobj.BalanceTbls.Where(x => x.Name == User.Identity.Name).FirstOrDefault();
                var check_money = dbobj.tbl_Transaction.Where(x => x.From_Acc == rec_money.Acc_Num).ToList();
                return View(check_money);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult RequestMoney()
        {
            return View();
        }
        public ActionResult WithdrawMoney()
        {
            return View();
        }
        public ActionResult DepositMoney()
        {
            return View();
        }
        public ActionResult Notifications()
        {
            //For True
            var notificationAdmin = dbobj.User_Notifications.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
            if (notificationAdmin != null)
            {
                var notiReadTrue = dbobj.User_Notifications.Where(x => x.IsRead == false && x.UserName == notificationAdmin.UserName).ToList();
                notiReadTrue.ForEach(x => x.IsRead = true);
                dbobj.SaveChanges();
            }

            //For List
            var notificationAdminList = dbobj.User_Notifications.Where(x => x.UserName == User.Identity.Name).ToList();
            return View(notificationAdminList);
        }
        public ActionResult DeleteNoti(int id)
        {
            var del = dbobj.User_Notifications.Where(x => x.ID == id).FirstOrDefault();
            dbobj.User_Notifications.Remove(del);
            dbobj.SaveChanges();
            return RedirectToAction("Notifications");
        }

        public ActionResult ReplyAdmin()
        {
            return View();
        }
        public int notiCountSent()
        {
            try
            {
                var model = dbobj.BalanceTbls.Where(x => x.Name == User.Identity.Name).FirstOrDefault();
                var match = dbobj.tbl_Transaction.Where(x => x.From_Acc == model.Acc_Num).FirstOrDefault();
                var notisave = 0;

                if (match != null)
                {
                    notisave = dbobj.tbl_Transaction.Where(x => x.IsRead_Sent == false && x.From_Acc == match.From_Acc).Count();
                }
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
        public int notiCountReceived()
        {
            try
            {
                var model = dbobj.BalanceTbls.Where(x => x.Name == User.Identity.Name).FirstOrDefault();
                var match = dbobj.tbl_Transaction.Where(x => x.To_Acc == model.Acc_Num).FirstOrDefault();
                int notisave = 0;
                if (match != null)
                {
                    notisave = dbobj.tbl_Transaction.Where(x => x.IsRead_Received == false && x.To_Acc == match.To_Acc).Count();
                }
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

        //Stripe Dashboard
        public ActionResult Stripe_Dashboard()
        {
            var Stripe_Data = dbobj.tbl_stripeDashboard.Where(x => x.username == User.Identity.Name).ToList();
            return View(Stripe_Data);
        }
        public ActionResult MoreInfo(int trans_id)
        {
            if (trans_id != 0)
            {
                var Stripe_Data = dbobj.tbl_stripeDashboard.Where(x => x.trans_id == trans_id).ToList();
                return View(Stripe_Data);
            }

            return RedirectToAction("Stripe_Dashboard");
        }

        public ActionResult Payment(string name_name, string number_name, string email_name, string city_name, string adress_name)
        {
            return RedirectToAction("SendMoney");
        }

        public ActionResult User_Profile()
        {
            var userName = User.Identity.Name;
            if (userName != null)
            {
                var details = dbobj.signup_table.Where(x => x.username == userName).ToList();

                ViewBag.username = details[0].username;
                ViewBag.email = details[0].email;
                ViewBag.password = DecryptCipherTextToPlainText(details[0].password);
                ViewBag.account = details[0].Account_No;
            }
            return View();
        }
        //change password
        public ActionResult Settings()
        {
            return View();
        }

        //sending password reset link
        public ActionResult setting()
        {
            var email = dbobj.signup_table.Where(x => x.username == User.Identity.Name).Select(u => u.email).SingleOrDefault();
            resetCode = Guid.NewGuid().ToString();

            resetExtra RE = new resetExtra();

            var list = dbobj.resetExtras.ToList();
            foreach (var item in list)
            {
                dbobj.resetExtras.Remove(item);
            }

            RE.username = User.Identity.Name;
            RE.code = resetCode;

            dbobj.resetExtras.Add(RE);
            dbobj.SaveChanges();

            var username = (string)Session["newSign"];
            if (username == "success")
            {
                var verifyUrl = "/Home/setting2/" + resetCode;
                var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

                var subject = "Password Reset Request";
                var body = "Hi " + User.Identity.Name + ", <br/> You recently requested to reset your password for your account. Click the link below to reset it. " +

                     " <br/><br/><a href='" + link + "'>" + link + "</a> <br/><br/>" +
                     "If you did not request a password reset, please ignore this email or reply to let us know.<br/><br/> Thank you";

                SendEmail("hassaanpafkiet@gmail.com", body, subject);

                TempData["Match"] = "A password reset link has been sent to you at -> " + email;
            }

            else
            {
                TempData["NoMatch"] = "Please Enter Correct Password";
            }

            return RedirectToAction("Settings");
        }

        public ActionResult setting2(string id)
        {
            var yesORno = dbobj.ForgotPasswords.Where(x => x.guid == id).ToList();

            if (yesORno.Count == 0)
            {
                return View();
            }
            else
            {
                TempData["linkExpire"] = "This link has been expired!!";
                return RedirectToAction("Settings");
            }
        }
        public ActionResult Update_Password(string New_name, string Confirm_name)
        {
            if (New_name == Confirm_name)
            {
                var email = dbobj.signup_table.Where(x => x.username == User.Identity.Name).SingleOrDefault();

                signup_table tbl = new signup_table();
                tbl.Id = email.Id;
                tbl.FullName = email.FullName;
                tbl.username = email.username;
                tbl.email = email.email;
                tbl.password = EncryptPlainTextToCipherText(New_name);
                tbl.conf_password = EncryptPlainTextToCipherText(Confirm_name);
                tbl.Account_No = email.Account_No;

                dbobj.Entry(email).CurrentValues.SetValues(tbl);

                ForgotPassword FP = new ForgotPassword();
                resetExtra RE = new resetExtra();

                int c = 0;
                var resetRecycle = dbobj.resetExtras.Where(x => x.username == User.Identity.Name).ToList();
                c = resetRecycle.Count();
                var code = resetRecycle[c - 1].code;

                FP.username = User.Identity.Name;
                FP.guid = code;
                FP.createdOn = DateTime.Now;
                dbobj.ForgotPasswords.Add(FP);

                foreach (var item in resetRecycle)
                {
                    var delete = dbobj.resetExtras.Remove(item);
                }

                dbobj.SaveChanges();

                TempData["UpdatePass"] = "Password Updated Successfully";
                return RedirectToAction("Settings");
            }
            else
            {
                TempData["MatchPwd"] = "Password and confirm Password must be same";
                return RedirectToAction("Setting2");
            }
        }

        //change email
        public ActionResult setting_Email()
        {
            return View();
        }
        public ActionResult settings_Email()
        {
            var email = dbobj.signup_table.Where(x => x.username == User.Identity.Name).Select(u => u.email).SingleOrDefault();
            var username = (string)Session["newSign_email"];
            if (username == "success")
            {
                TempData["Match_email"] = "A email reset link has been sent to you at -> " + email;
                return RedirectToAction("setting_Email");
            }
            else
            {
                TempData["NoMatch_email"] = "Please Enter Correct Email";
                return RedirectToAction("setting_Email");
            }
        }
        public ActionResult MyOrders()
        {
            var cartItems = dbobj.cartItems.Where(x => x.username == User.Identity.Name).ToList();
            return View(cartItems);
        }

        private void SendEmail(string emailAddress, string body, string subject)
        {
            using (MailMessage mm = new MailMessage(emailAddress, emailAddress))
            {
                mm.Subject = subject;
                mm.Body = body;

                mm.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                NetworkCredential NetworkCred = new NetworkCredential("hassaanpafkiet@gmail.com", "vrcvlxgslkplafjd");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = 587;
                smtp.Send(mm);
            }
        }

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