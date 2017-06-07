using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RegistrationAndLogin.Models;
using System.Net.Mail;
using System.Web.Services.Description;
using System.Net;

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
                            your email id :'" + user.EmailID + "'";
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

        //Login

        //Login post 

        //Logout


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
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("mujahid.islam_peal@outlook.com", "SungWann Tours & Travel");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "rtt09876";

            string subject = "Your account is succesfully created!";

            string body = @"<br/><br/>We're excited to tell you that your account for <strong>SungWann Tours & Travel</strong> is
                            succesfully created. Please verify your account by clicking the link given below-><br/><br/>
                            <a href='"+link+"'>"+link+"</a>";

            var smtp = new SmtpClient
            {
                Host = "smtp-mail.outlook.com",
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