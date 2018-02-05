using GreenBushIEP.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace GreenBushIEP.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }

    public class UserOrganizationViewModel
    {
        public tblUser user { get; set; }
        public List<tblUser> staff { get; set; }
    }

    public class MISCalendarViewModel
    {
        public List<tblDistrict> districts { get; set; }
        public List<tblBuilding> buildings { get; set; }
        public List<tblCalendar> calendarDays { get; set; }
    }

    public class Student
    {
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string RoleID { get; set; }
        public string BuildingID { get; set; }
        public string NeighborhoodBuildingID { get; set; }
        public string ImageURL { get; set; }
        public Nullable<bool> Agreement { get; set; }
        public Nullable<bool> Archive { get; set; }
        public int? KidsID { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Status { get; set; }
        public string BuildingName { get; set; }
        public string USD { get; set; }
    }

    public class StudentViewModel
    {
        public tblUser Teacher { get; set; }
        public ICollection<Student> Students { get; set; }
    }

    public class BuildingsViewModel
    {
        public string BuildingID { get; set; }
        public string BuildingName { get; set; }
        public string BuildingUSD { get; set; }
    }

    public class UserDetailsViewModel
    {
        public UserDetailsViewModel()
        {
            user = new tblUser();
            submitter = new tblUser();
            districts = new List<tblDistrict>();
            selectedDistrict = new List<tblDistrict>();
            buildings = new List<BuildingsViewModel>();
        }

        public tblUser submitter { get; set; }
        public tblUser user { get; set; }
        public List<tblDistrict> districts { get; set; }
        public List<tblDistrict> selectedDistrict { get; set; }
        public List<BuildingsViewModel> buildings { get; set; }
    }

    public class StudentDetailsViewModel
    {
        public StudentDetailsViewModel()
        {
            submitter = new tblUser();
            student = new Student();
            info = new tblStudentInfo();
            contacts = new List<tblStudentRelationship>();
            districts = new List<tblDistrict>();
            selectedDistrict = new List<tblDistrict>();
            buildings = new List<BuildingsViewModel>();
        }

        public tblUser submitter { get; set; }
        public Student student { get; set; }
        public tblStudentInfo info { get; set; }
        public List<tblStudentRelationship> contacts { get; set; }
        public List<tblDistrict> districts { get; set; }
        public List<tblDistrict> selectedDistrict { get; set; }
        public List<BuildingsViewModel> buildings { get; set; }
    }

    public class StudentProcedureViewModel
    {
        public StudentProcedureViewModel()
        {
            hasplan = false;
            student = new tblUser();
            studentPlan = new StudentPlan(student.UserID);
        }

        public bool hasplan { get; set; }
        public tblUser student { get; set; }
        public IEP studentIEP { get; set; }
        public StudentPlan studentPlan { get; set; }
    }

    public class StudentLegalView
    {
        public StudentLegalView()
        {
            student = new tblUser();
            studentInfo = new tblStudentInfo();
            contacts = new List<tblStudentRelationship>();
        }

        public tblUser student { get; set; }
        public tblStudentInfo studentInfo { get; set; }
        public tblUser teacher { get; set; }
        public List<tblStudentRelationship> contacts { get; set; }
    }

    public class ModuleAcademicViewModel
    {
        public ModuleAcademicViewModel()
        {
            Academic = new tblIEPAcademic();
            Reading = new tblIEPReading();
            Math = new tblIEPMath();
            Written = new tblIEPWritten();
        }

        public tblIEPAcademic Academic { get; set; }
        public tblIEPReading Reading { get; set; }
        public tblIEPMath Math { get; set; }
        public tblIEPWritten Written { get; set; }
    }

    public class StudentGoalsViewModel
    {
        public int iepId { get; set; }
        public int studentId { get; set; }
        public List<StudentGoal> studentGoals { get; set; } = new List<StudentGoal>();

        public StudentGoalsViewModel()
        {
            iepId = 0;
            studentId = 0;
            studentGoals = new List<StudentGoal>();
        }
    }

    public class AccomodationViewModel
    {
        public AccomodationViewModel()
        {
            AccomList = new List<tblAccommodation>();
            Locations = new List<System.Web.Mvc.SelectListItem>();
        
        }

        public int StudentId { get; set; }

        [Required(ErrorMessage = "Please select a Type: Accommodation or Modification.")]
        public int AccomType { get; set; }
        public int AccommodationID { get; set; }
        public int IEPid { get; set; }
        public string AccDescription { get; set; }
        public string LocationCode { get; set; }
        public int? Frequency { get; set; }
        public int Duration { get; set; }
        public DateTime? AnticipatedStartDate { get; set; }
        public DateTime? AnticipatedEndDate { get; set; }
        public string Message { get; set; }
        public List<tblAccommodation> AccomList { get; set; }
        public List<System.Web.Mvc.SelectListItem> Locations { get; set; }

    }

    public class StudentServiceViewModel
    {
        public int studentId { get; set; }
        public List<tblService> studentService { get; set; }
        public List<tblServiceType> serviceTypes { get; set; }
        public List<tblProvider> serviceProviders { get; set; }
        public List<tblLocation> serviceLocations { get; set; }
        public List<tblGoal> studentGoals { get; set; }
        public List<tblCalendar> calendar { get; set; }

        public StudentServiceViewModel()
        {
            studentId = 0;
            studentService = new List<tblService>();
            serviceTypes = new List<tblServiceType>();
            serviceProviders = new List<tblProvider>();
            serviceLocations = new List<tblLocation>();
            calendar = new List<tblCalendar>();
        }
    }
}

public class IEPStatus
{
    public const string CURRENT = "Current";
    public const string ARCHIVE = "Archive";
    public const string DRAFT = "Draft";
    public const string DELETED = "DELETED";
}