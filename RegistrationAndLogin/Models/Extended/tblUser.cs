using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RegistrationAndLogin.Models
{
    [MetadataType(typeof(UserMetaData))]
    public partial class tblUser
    {
        public string ConfirmPassword { get; set; }
    }

    public class UserMetaData
    {
        [Display(Name ="First Name")]
        [Required(AllowEmptyStrings =false, ErrorMessage ="First name required")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name required")]
        public string LastName { get; set; }

        [Display(Name = "E-mail ID")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "E-mail ID required")]
        [DataType(DataType.EmailAddress)]
        public string EmailID { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode =true,DataFormatString ="{0:MM/dd/yyyy}")]
        public string DateOfBirth { get; set; }


        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(8,ErrorMessage ="Minimum 8 charaters required")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage ="Password didn't match")]
        public string ConfirmPassword { get; set; }


    }
}