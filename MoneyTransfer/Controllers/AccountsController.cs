using MoneyTransfer.Database;
using MoneyTransfer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MoneyTransfer.Controllers
{
    public class AccountsController : Controller
    {

        MoneyGatewaydbEntities11 dbobj = new MoneyGatewaydbEntities11();
        // GET: Accounts
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(signup_table model)
        {
            try
            {
                var EncryptPass = EncryptPlainTextToCipherText(model.password);
                var user = dbobj.signup_table.Where(x => x.username == model.username && x.password == EncryptPass).FirstOrDefault();

                if (model.username == "Admin" && model.password == "admin123")
                {
                    FormsAuthentication.SetAuthCookie(model.username, false);
                    return RedirectToAction("AdminIndex", "Admin");
                }
                else if (user != null)
                {
                    FormsAuthentication.SetAuthCookie(model.username, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Error = "Incorrect Username OR Password";
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
                ViewBag.Error = "Incorrect Username OR Password";
            }
            finally
            {
                ModelState.Clear();
            }
            return View();
        }
        public ActionResult SignupIndex()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Signup(signup_table model)
        {
            var sameEmail = dbobj.signup_table.Where(x => x.email == model.email).FirstOrDefault();
            var username = dbobj.signup_table.Where(x => x.username == model.username).FirstOrDefault();

            var EncryptPass = EncryptPlainTextToCipherText(model.password);

            if (sameEmail == null)
            {
                if (username == null)
                {
                    if (model.password == model.conf_password)
                    {
                        if (ModelState.IsValid)
                        {
                            Random num = new Random();
                            var key = "0123456789";
                            var random = "";
                            random += "00";
                            for (int i = 0; i < 12; i++)
                            {
                                int a = num.Next(10);
                                random = random + key.ElementAt(a);
                            }

                            StoreSignUp sp = new StoreSignUp();
                            sp.id = 1;
                            sp.fullname = model.FullName;
                            sp.username = model.username;
                            sp.email = model.email;
                            sp.password = EncryptPass;
                            sp.conf_password = EncryptPass;
                            sp.account_no = random;

                            List<StoreSignUp> storeSession = new List<StoreSignUp>();
                            storeSession.Add(sp);

                            Session["signupDATA"] = storeSession;
                            List<StoreSignUp> storeSessions = (List<StoreSignUp>)Session["signupDATA"];

                            return RedirectToAction("EmailSending");
                        }
                    }
                    else
                    {
                        ViewBag.same = "Password & Conf_Password Must Be Same";
                    }
                }
                else
                {
                    ViewBag.sameEmail = "This username already exists";
                }
            }
            else
            {
                ViewBag.sameEmail = "This email address already exists";
            }

            ModelState.Clear();
            return View("SignupIndex");
        }
        public ActionResult EmailSending()
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

            List<StoreSignUp> storeSessions = (List<StoreSignUp>)Session["signupDATA"];

            resetExtra RE = new resetExtra();

            var list = dbobj.resetExtras.ToList();
            foreach (var item in list)
            {
                dbobj.resetExtras.Remove(item);
            }

            RE.username = storeSessions[0].username;
            RE.code = random;

            dbobj.resetExtras.Add(RE);
            dbobj.SaveChanges();

            var subject = "Activation Code";
            var body = "Hi " + User.Identity.Name + ", <br/> Your Account acctivation code is. " +

                 " <br/><br/>" + random + "</a> <br/><br/>" +
                 "If you did not request a password reset, please ignore this email or reply to let us know.<br/><br/> Thank you";

            SendEmail(storeSessions[0].email, body, subject);

            TempData["Match"] = "An account activation code has been sent to you at -> " + email;

            return RedirectToAction("ActivationCode");
        }
        public ActionResult ActivationCode()
        {
            return View();
        }
        public ActionResult Confirm_Code(string Act_code)
        {
            var yesORno = dbobj.ForgotPasswords.Where(x => x.guid == Act_code).ToList();
            var yes = dbobj.resetExtras.Where(x => x.code == Act_code).ToList();

            if (yesORno.Count == 0 && yes.Count != 0)
            {
                List<StoreSignUp> storeSessions = (List<StoreSignUp>)Session["signupDATA"];
                if (storeSessions != null)
                {
                    signup_table obj = new signup_table();
                    BalanceTbl tbl = new BalanceTbl();

                    tbl.Name = storeSessions[0].username;
                    tbl.ActiveDollars = 10;

                    obj.FullName = storeSessions[0].fullname;
                    obj.username = storeSessions[0].username;
                    obj.email = storeSessions[0].email;
                    obj.password = storeSessions[0].password;
                    obj.conf_password = storeSessions[0].conf_password;
                    obj.Account_No = storeSessions[0].account_no;

                    tbl.Acc_Num = storeSessions[0].account_no;

                    dbobj.signup_table.Add(obj);
                    dbobj.BalanceTbls.Add(tbl);

                    ForgotPassword FP = new ForgotPassword();
                    FP.username = storeSessions[0].username;
                    FP.guid = yes[0].code;
                    FP.createdOn = DateTime.Now;
                    dbobj.ForgotPasswords.Add(FP);

                    foreach (var item in yes)
                    {
                        var delete = dbobj.resetExtras.Remove(item);
                    }

                    dbobj.SaveChanges();

                    TempData["AlertMessage"] = "You are registered successfully";
                    return RedirectToAction("Login");
                }
                else
                {
                    return RedirectToAction("SignupIndex");
                }
            }
            else
            {
                TempData["linkExpire"] = "This is not a valid code!!";
                return RedirectToAction("ActivationCode");
            }
        }
        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        public ActionResult change(string Password_name)
        {
            var sign = "";
            var userName = User.Identity.Name;
            var dec = EncryptPlainTextToCipherText(Password_name);
            var data = dbobj.signup_table.Where(x => x.username == userName && x.password == dec).Count();
            //var data = dbobj.signup_table.Where( x => x.password == dec).Count();
            if (data != 0)
            {
                sign = "success";
            }
            else
            {
                sign = "error";
            }
            Session["newSign"] = sign;
            return RedirectToAction("setting", "Home");
        }
        public ActionResult change_Email(string Email_name)
        {
            var sign = "";
            var userName = User.Identity.Name;
            var data = dbobj.signup_table.Where(x => x.email == Email_name).Count();
            if (data != 0)
            {
                sign = "success";
            }
            else
            {
                sign = "error";
            }
            Session["newSign_email"] = sign;
            return RedirectToAction("setting_Email", "Home");
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