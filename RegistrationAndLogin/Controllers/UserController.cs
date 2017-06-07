using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RegistrationAndLogin.Models;
using System.Net.Mail;
using System.Web.Services.Description;
using System.Net;
using System.Web.Security;

namespace RegistrationAndLogin.Controllers
{
    public class UserController : Controller
    {
        //Registration
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        //Registration Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")]tblUser user)
        {
            bool Status = false;
            string message = "";
            //
            //Model Validation
            if (ModelState.IsValid)
            {
                #region//Email is already exist
                var isExist = IsEmailExist(user.EmailID);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(user);
                }
                #endregion

                #region //Generate Activation Code
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region //Password Hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword); //
                #endregion

                user.IsEmailVerified = false;
                #region //Save to Database
                using(UserInformationEntities db=new UserInformationEntities())
                {
                    db.tblUsers.Add(user);
                    db.SaveChanges();

                }
                #endregion
                #region //Send Email to user
                SendVerificationLink(user.EmailID, user.ActivationCode.ToString());
                message = @"Registration succesfully done. Account verification link has been sent to 
                            your email id -> " + user.EmailID ;
                Status = true;
                #endregion

            }
            else
            {
                message = "Invalid Request";
            }
            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }

        //Verify Account
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool status = false;
            using (UserInformationEntities db=new UserInformationEntities())
            {
                db.Configuration.ValidateOnSaveEnabled = false; //This line here I have added to avoid confirm password
                                                                //does not match issue on save changes

                var v = db.tblUsers.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                    db.SaveChanges();
                    status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }
            ViewBag.Status = status;
            return View();
        }

        //Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        //Login post 
        [HttpPost]
        public ActionResult Login(UserLogin login, string ReturnUrl="")
        {
            string message = "";

            ViewBag.Message = message;

            using(UserInformationEntities db=new UserInformationEntities())
            {
                var v = db.tblUsers.Where(a => a.EmailID == login.EmailID).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(Crypto.Hash(login.Password),v.Password)==0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20;  // 525600min = 1 year
                        var ticket = new FormsAuthenticationTicket(login.EmailID, login.RememberMe, timeout); ;
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);


                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        message = "Invalid credentials provided";
                    }
                }
                else
                {
                    message = "Invalid credentials provided";
                }
            }

            return View();
        }
        //Logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }

        //Verify Email
        [NonAction]
        public bool IsEmailExist(string emailID)
        {
            using(UserInformationEntities db=new UserInformationEntities())
            {
                var v = db.tblUsers.Where(a => a.EmailID == emailID).FirstOrDefault();
                return v != null;
            }
        }

        //Verify Email Link
        [NonAction]
        public void SendVerificationLink(string email, string code)
        {
            var verifyUrl = "/User/VerifyAccount/" + code;
            var link = Request.Url.AbsoluteUri.ToString() + verifyUrl;

            var fromEmail = new MailAddress("sungwanntoursntravels@gmail.com", "Sung Wann Tours & Travel");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "SWTT0987";

            string subject = "Your account is succesfully created!";

            string body = @"<br/><br/>We're excited to tell you that your account for <strong>SungWann Tours & Travel</strong> is
                            succesfully created. Please verify your account by <br/><br/>
                            <a href='"+link+"'>clicking here!</a>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod=SmtpDeliveryMethod.Network,
            UseDefaultCredentials=false,
            Credentials=new NetworkCredential(fromEmail.Address,fromEmailPassword)
            };
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            smtp.Send(message);

        }
    }
}