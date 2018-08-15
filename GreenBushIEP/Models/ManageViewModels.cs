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

    public class MISProviderViewModel
    {
        public List<tblDistrict> districts { get; set; }
        public List<tblProvider> listOfProviders { get; set; }

        public MISProviderViewModel()
        {
            listOfProviders = new List<tblProvider>();
            districts = new List<tblDistrict>();
        }
    }

    public class ProviderViewModel
    {
        public string Name { get; set; }
        public int ProviderID { get; set; }
        public string ProviderCode { get; set; }
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
        public long? KidsID { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Status { get; set; }
        public string BuildingName { get; set; }
        public string USD { get; set; }
        public int? CreatedBy { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Zip { get; set; }

        public string IEPDate { get; set; }
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
            allDistricts = new List<tblDistrict>();
            selectedDistrict = new List<tblDistrict>();
            buildings = new List<BuildingsViewModel>();
            statusCode = new List<tblStatusCode>();
            placementCode = new List<tblPlacementCode>();
            counties = new List<tblCounty>();
            primaryDisabilities = new List<vw_PrimaryDisabilities>();
            secondaryDisabilities = new List<vw_SecondaryDisabilities>();
            statusCodes = new List<tblStatusCode>();
        }

        public tblUser submitter { get; set; }
        public Student student { get; set; }
        public tblStudentInfo info { get; set; }
        public List<tblStudentRelationship> contacts { get; set; }
        public List<tblDistrict> districts { get; set; }
        public List<tblDistrict> allDistricts { get; set; }
        public List<tblDistrict> selectedDistrict { get; set; }
        public List<BuildingsViewModel> buildings { get; set; }
        public List<tblStatusCode> statusCode { get; set; }
        public List<tblPlacementCode> placementCode { get; set; }
        public List<tblCounty> counties { get; set; }
        public List<vw_PrimaryDisabilities> primaryDisabilities { get; set; }
        public List<vw_SecondaryDisabilities> secondaryDisabilities { get; set; }
        public List<tblStatusCode> statusCodes { get; set; }
    }

    public class StudentProcedureViewModel
    {
        public StudentProcedureViewModel()
        {
            isDoc = false;
            studentAge = 12;
            hasplan = false;
            student = new tblUser();
            studentPlan = new StudentPlan(student.UserID);
            birthDate = new DateTime();
            hasAccommodations = false;
            needsBehaviorPlan = false;
            isCreator = false;
        }

        public bool isDoc { get; set; }
        public int studentAge { get; set; }
        public bool hasplan { get; set; }
        public tblUser student { get; set; }
        public IEP studentIEP { get; set; }
        public StudentPlan studentPlan { get; set; }
        public DateTime birthDate { get; set; }
        public bool hasAccommodations { get; set; }
        public bool needsBehaviorPlan { get; set; }
        public bool isCreator { get; set; }
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
        public string building { get; set; }
    }

    public class IEPFormViewModel
    {
        public IEPFormViewModel()
        {
            IEPForms = new List<System.Web.Mvc.SelectListItem>();
            Archives = new List<tblFormArchive>();
        }

        public List<System.Web.Mvc.SelectListItem> IEPForms { get; set; }
        public List<tblFormArchive> Archives { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
    }

    public class IEPFormFileViewModel
    {
        public int studentId { get; set; }
        public int iepId { get; set; }
        public string fileName { get; set; }
        public string fileDesc { get; set; }
        public StudentLegalView fileModel { get; set; }

        public IEPFormFileViewModel()
        {
            studentId = 0;
            fileName = string.Empty;
            fileModel = new StudentLegalView();
            iepId = 0;
            fileDesc = "";
        }
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
        public string LocationCode { get; set; } //not currently used
        public string Location { get; set; }
        public string Frequency { get; set; }
        public string Duration { get; set; }
        public DateTime? AnticipatedStartDate { get; set; }
        public DateTime? AnticipatedEndDate { get; set; }
        public string Message { get; set; }
        public List<tblAccommodation> AccomList { get; set; }
        public List<System.Web.Mvc.SelectListItem> Locations { get; set; }
        public string DefaultStartDate { get; set; }
        public string DefaultEndDate { get; set; }
    }

    public class BehaviorViewModel
    {
        public BehaviorViewModel()
        {
            Triggers = new List<tblBehaviorTriggerType>();
            HypothesisList = new List<tblBehaviorHypothesisType>();
            Strategies = new List<tblBehaviorStrategyType>();
            SelectedTriggers = new List<int>();
            SelectedHypothesis = new List<int>();
            SelectedStrategies = new List<int>();
            TargetedBehaviors = new List<tblBehaviorBaseline>();
            targetedBehavior1 = new tblBehaviorBaseline();
            targetedBehavior2 = new tblBehaviorBaseline();
            targetedBehavior3 = new tblBehaviorBaseline();
        }

        public int StudentId { get; set; }
        public int BehaviorID { get; set; }
        public int IEPid { get; set; }

        public List<int> SelectedTriggers { get; set; }
        public string TriggerOther { get; set; }

        public List<int> SelectedHypothesis { get; set; }
        public string HypothesisOther { get; set; }

        public List<int> SelectedStrategies { get; set; }
        public string StrategiesOther { get; set; }

        public string StrengthMotivator { get; set; }
        public string BehaviorConcern { get; set; } //not currently used
        public string Crisis_Escalation { get; set; }
        public string Crisis_Description { get; set; }
        public string Crisis_Implementation { get; set; }
        public string Crisis_Other { get; set; }
        public string ReviewedBy { get; set; }
        public string Message { get; set; }
        public List<tblBehaviorTriggerType> Triggers { get; set; }
        public List<tblBehaviorHypothesisType> HypothesisList { get; set; }
        public List<tblBehaviorStrategyType> Strategies { get; set; }
        public List<tblBehaviorBaseline> TargetedBehaviors { get; set; }

        public tblBehaviorBaseline targetedBehavior1 { get; set; }
        public tblBehaviorBaseline targetedBehavior2 { get; set; }
        public tblBehaviorBaseline targetedBehavior3 { get; set; }
    }

    public class StudentServiceViewModel
    {
        public int studentId { get; set; }
        public List<tblService> studentServices { get; set; }
        public List<tblServiceType> serviceTypes { get; set; }
        public List<tblProvider> serviceProviders { get; set; }
        public List<tblLocation> serviceLocations { get; set; }
        public List<tblGoal> studentGoals { get; set; }
        public List<tblCalendar> calendar { get; set; }
        public List<tblCalendar> availableCalendarDays { get; set; }
        public List<tblCalendarReporting> calendarReportings { get; set; }
        public DateTime IEPStartDate { get; set; }
        public DateTime MeetingDate { get; set; }

        public StudentServiceViewModel()
        {
            studentId = 0;
            studentServices = new List<tblService>();
            serviceTypes = new List<tblServiceType>();
            serviceProviders = new List<tblProvider>();
            serviceLocations = new List<tblLocation>();
            studentGoals = new List<tblGoal>();
            calendar = new List<tblCalendar>();
            availableCalendarDays = new List<tblCalendar>();
            calendarReportings = new List<tblCalendarReporting>();
            IEPStartDate = new DateTime();
            MeetingDate = new DateTime();

        }
    }

    public class StudentTransitionViewModel
    {
        public int iepId { get; set; }
        public int studentId { get; set; }
        public tblUser student { get; set; }
        public bool isDOC { get; set; }
        public tblTransition transition { get; set; }
        public List<tblTransitionAssessment> assessments { get; set; }
        public List<tblTransitionGoal> goals { get; set; }
        public List<tblTransitionService> services { get; set; }


        public StudentTransitionViewModel()
        {
            iepId = 0;
            studentId = 0;
            student = new tblUser();
            isDOC = false;
            transition = new tblTransition();
            assessments = new List<tblTransitionAssessment>();
            goals = new List<tblTransitionGoal>();
            services = new List<tblTransitionService>();
        }
    }


    public class StudentDetailsPrintViewModel
    {
        public StudentDetailsPrintViewModel()
        {
            student = new tblStudentInfo();
            teacher = new tblUser();
            contacts = new List<tblStudentRelationship>();
            building = new tblBuilding();
            teacherBuilding = new tblBuilding();
            neighborhoodBuilding = new tblBuilding();
        }

        public tblUser teacher { get; set; }
        public tblStudentInfo student { get; set; }
        public List<tblStudentRelationship> contacts { get; set; }
        public string ethnicity { get; set; }
        public string gender { get; set; }
        public string parentLang { get; set; }
        public string studentLang { get; set; }
        public string studentCounty { get; set; }
        public string primaryDisability { get; set; }
        public string secondaryDisability { get; set; }
        public tblBuilding building { get; set; }
        public tblBuilding neighborhoodBuilding { get; set; }
        public tblBuilding teacherBuilding { get; set; }
        public int studentAgeAtIEP { get; set; }
        public string inititationDate { get; set; }
        public int studentAgeAtAnnualMeeting { get; set; }
        public string assignChildCount { get; set; }

    }

    public class StudentServiceObject
    {
        public int ServiceID { get; set; }
        public int IEPid { get; set; }
        public int SchoolYear { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ServiceCode { get; set; }
        public int Frequency { get; set; }
        public byte DaysPerWeek { get; set; }
        public short Minutes { get; set; }
        public int ProviderID { get; set; }
        public string LocationCode { get; set; }

    }
}

public class IEPStatus
{
    public const string CURRENT = "Current";
    public const string ARCHIVE = "Archive";
    public const string DRAFT = "Draft";
    public const string DELETED = "DELETED";
}