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

    public class PortalViewModel
    {
        public tblUser user { get; set; }
        public List<tblDistrict> districts { get; set; }
        public List<tblBuilding> buildings { get; set; }
        public List<StudentIEPViewModel> members { get; set; }

        public PortalViewModel()
        {
            user = new tblUser();
            districts = new List<tblDistrict>();
            buildings = new List<tblBuilding>();
            members = new List<StudentIEPViewModel>();
        }
    }

    public class StudentIEPViewModel
    {
        public int UserID;
        public string FirstName;
        public string MiddleName;
        public string LastName;
        public string RoleID;
        public bool hasIEP;
        public string KidsID;
        public bool isAssigned;
    }

    public class UserOrganizationViewModel
    {
        public tblUser user { get; set; }
        public List<tblUser> staff { get; set; }
        public List<tblDistrict> districts { get; set; }
        public List<tblBuilding> buildings { get; set; }
    }

    public class MISCalendarViewModel
    {
        public List<tblDistrict> districts { get; set; }
        public List<tblBuilding> buildings { get; set; }
        public List<tblCalendar> calendarDays { get; set; }
    }

    public class MISNotesUI
    {
        public int CommentId { get; set; }
        public string Note { get; set; }
        public int StudentID { get; set; }
        public int CreatedBy { get; set; }
        public bool isArchive { get; set; }
        public DateTime Create_Date { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime Update_Date { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class MISNotesViewModel
    {
        public List<MISNotesUI> notes { get; set; }
        public tblUser student { get; set; }
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

    public class MISDistricContactViewModel
    {
        public List<tblDistrict> myDistricts { get; set; }
        public tblDistrict currentDistrict { get; set; }
        public tblContact districtContact { get; set; }

        public MISDistricContactViewModel()
        {
            myDistricts = new List<tblDistrict>();
            currentDistrict = new tblDistrict();
            districtContact = new tblContact();
        }
    }

    public class ProviderViewModel
    {
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
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
        public string County { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string RoleID { get; set; }
        public string BuildingID { get; set; }
        public string NeighborhoodBuildingID { get; set; }
        public string ImageURL { get; set; }
        public Nullable<bool> Agreement { get; set; }
        public Nullable<bool> Archive { get; set; }
        [DisplayFormat(DataFormatString = "{0:0000000000}", ApplyFormatInEditMode = true)]
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
        public bool hasIEP { get; set; }

        public string FormattedName
        {
            get
            {
                if (UserID == -1)
                    return string.Format("{0}", FirstName);
                else
                    return string.Format("{0}, {1} {2}", LastName, FirstName, MiddleName);
            }
        }


    }

    public class StudentViewModel
    {
        public tblUser Teacher { get; set; }
        public ICollection<Student> Students { get; set; }
        public List<tblDistrict> districts { get; set; }
        public List<tblBuilding> buildings { get; set; }

        public StudentViewModel()
        {
            districts = new List<tblDistrict>();
            buildings = new List<tblBuilding>();
        }
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
            grades = new List<tblGrade>();
            contacts = new List<tblStudentRelationship>();
            districts = new List<tblDistrict>();
            races = new List<tblRace>();
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
        public List<tblGrade> grades { get; set; }
        public List<tblStudentRelationship> contacts { get; set; }
        public List<tblRace> races { get; set; }
        public tblRace selectedRace { get; set; }
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

    public class ReferralDetailsViewModel
    {
        public ReferralDetailsViewModel()
        {
            submitter = new tblUser();
            student = new tblReferralInfo();
            info = new tblStudentInfo();
            grades = new List<tblGrade>();
            contacts = new List<tblStudentRelationship>();
            districts = new List<tblDistrict>();
            races = new List<tblRace>();
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

        public int referralId { get; set; }
        public tblUser submitter { get; set; }
        public tblReferralInfo student { get; set; }
        public tblStudentInfo info { get; set; }
        public List<tblGrade> grades { get; set; }
        public List<tblStudentRelationship> contacts { get; set; }
        public List<tblRace> races { get; set; }
        public tblRace selectedRace { get; set; }
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
            isGiftedOnly = false;
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
        public bool isGiftedOnly { get; set; }

        public string GetIEPStatus(tblIEP thisIEP)
        {
            if (thisIEP != null)
            {
                if (thisIEP.Amendment && thisIEP.IsActive && thisIEP.IepStatus.ToUpper() == IEPStatus.DRAFT)
                    return IEPStatus.AMENDMENT;
                if (!thisIEP.IsActive)
                    return IEPStatus.ARCHIVE;
                if (thisIEP.AmendingIEPid != null && thisIEP.IsActive && thisIEP.IepStatus.ToUpper() == IEPStatus.ANNUAL)
                    return IEPStatus.ANNUAL;

                return thisIEP.IepStatus.ToUpper();
            }

            return string.Empty;
        }
    }

    public class StudentLegalView
    {
        public StudentLegalView()
        {
            student = new tblUser();
            studentInfo = new tblStudentInfo();
            contacts = new List<tblStudentRelationship>();
            districtContact = new tblContact();
            studentTransition = new tblTransition();
            transitionGoals = new List<tblTransitionGoal>();
            academicGoals = new tblIEPAcademic();
            socialGoals = new tblIEPSocial();
            reading = new tblIEPReading();
            math = new tblIEPMath();
            written = new tblIEPWritten();
        }

        public tblUser student { get; set; }
        public tblStudentInfo studentInfo { get; set; }
        public tblUser teacher { get; set; }
        public List<tblStudentRelationship> contacts { get; set; }
        public tblContact districtContact { get; set; }
        public tblTransition studentTransition { get; set; }
        public List<tblTransitionGoal> transitionGoals { get; set; }
        public tblIEPAcademic academicGoals { get; set; }
        public tblIEPSocial socialGoals { get; set; }
        public tblIEPReading reading { get; set; }
        public tblIEPMath math { get; set; }
        public tblIEPWritten written { get; set; }
        public string building { get; set; }
		public string buildingAddress { get; set; }
		public string buildingCityStZip { get; set; }
		public string buildingPhone { get; set; }
		public string buildingNeigborhood { get; set; }		
		public string districtName { get; set; }
        public string lastReEvalDate { get; set; }
        public string studentLanguage { get; set; }
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
		public bool CanDelete { get; set; }

	}

    public class IEPFormFileViewModel
    {
        public int studentId { get; set; }
        public int iepId { get; set; }
        public int id { get; set; }
        public string fileName { get; set; }
        public string fileDesc { get; set; }
        public string fileDate { get; set; }
        public StudentLegalView fileModel { get; set; }
        public tblFormTeamEval teamEval { get; set; }
        public tblFormSummaryPerformance summaryPerformance { get; set; }
		public tblFormConferenceSummary conferenceSummary { get; set; }
		public tblFormIEPAmendment formAmend { get; set; }
		public tblFormIEPMeetingConsentToInvite formMtgConsent { get; set; }
		public tblFormIEPMeetingExcusal formMtgExcusal { get; set; }
		public tblFormIEPTeamConsideration formIEPTeamConsider { get; set; }
		public tblFormManifestationDeterminiation formMani { get; set; }
		public tblFormNoticeOfMeeting formNotice { get; set; }
		public tblFormParentConsent formConsentMedicaid { get; set; }
		public tblFormPhysicianScript formPhysician { get; set; }
		public tblFormPriorWritten_Ident formPWN { get; set; }
		public tblFormPriorWritten_Eval formPWNEval { get; set; }
		public tblFormPriorWritten_ReokeAll formPWNRevAll { get; set; }
		public tblFormPriorWritten_ReokePart formPWNRevPart { get; set; }
		public tblFormRevokeConsentAll formRevAll { get; set; }
		public tblFormRevokeConsentPart formRevPart { get; set; }
		public tblFormTransportationRequest formTransRequest { get; set; }
		public tblFormContinuousLearningPlan continuousLearningPlan { get; set; }

		public IEPFormFileViewModel()
        {
            studentId = 0;
            fileName = string.Empty;
            fileModel = new StudentLegalView();
            iepId = 0;
            fileDesc = "";
            teamEval = null;
            summaryPerformance = null;
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
        public bool isReadOnly { get; set; }
        public bool canAddProgress { get; set; }
        public string modulesNeedingGoals { get; set; }

        public StudentGoalsViewModel()
        {
            iepId = 0;
            studentId = 0;
            studentGoals = new List<StudentGoal>();
            isReadOnly = false;
            canAddProgress = false;
            modulesNeedingGoals = string.Empty;
        }
    }

    public class AccomodationViewModel
    {
        public AccomodationViewModel()
        {
            AccomList = new List<tblAccommodation>();
            Locations = new List<System.Web.Mvc.SelectListItem>();
            modulesNeedingAccommodations = string.Empty;
            ModuleList = new List<tblModule>();
			AccommModules = new List<tblAccommodationModule>();
        }

        public int StudentId { get; set; }

        [Required(ErrorMessage = "Please select a Type: Accommodation or Modification.")]
        public int AccomType { get; set; }
        public int AccommodationID { get; set; }
        public int IEPid { get; set; }
        public string LocationCode { get; set; } //not currently used
        public string Description { get; set; }
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
        public bool Completed { get; set; }
        public string modulesNeedingAccommodations { get; set; }
        public string Module { get; set; }
        public List<tblModule> ModuleList { get; set; }
		public List<tblAccommodationModule> AccommModules { get; set; }
		public int[] SelectedModules { get; set; }


	}

	public class AccomodationPrintViewModel
	{
		public AccomodationPrintViewModel()
		{
			
		}

		public int StudentId { get; set; }		
		public string AccomType { get; set; }
		public int AccommodationID { get; set; }
		public int IEPid { get; set; }		
		public string Description { get; set; }
		public string Location { get; set; }
		public string Frequency { get; set; }
		public string Duration { get; set; }
		public DateTime? AnticipatedStartDate { get; set; }
		public DateTime? AnticipatedEndDate { get; set; }
		public string Module { get; set; }
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
            isBehaviorPlanInSocialModuleChecked = false;
            Completed = false;
        }

        public int StudentId { get; set; }
        public int BehaviorID { get; set; }
        public int IEPid { get; set; }
        public bool Completed { get; set; }
        public bool isBehaviorPlanInSocialModuleChecked { get; set; }

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
        public List<vw_BuildingsForAttendance> attendanceBuildings { get; set; }
        public List<int> fiscalYears { get; set; }
        public DateTime IEPStartDate { get; set; }
        public DateTime MeetingDate { get; set; }
        public string providedfor { get; set; }
        public string IEPStatus { get; set; }
        public bool isOriginalIEPService { get; set; }

        public StudentServiceViewModel()
        {
            studentId = 0;
            studentServices = new List<tblService>();
            serviceTypes = new List<tblServiceType>();
            serviceProviders = new List<tblProvider>();
            serviceLocations = new List<tblLocation>();
            studentGoals = new List<tblGoal>();
            attendanceBuildings = new List<vw_BuildingsForAttendance>();
            fiscalYears = new List<int>();
            IEPStartDate = new DateTime();
            MeetingDate = new DateTime();
            providedfor = "";
            IEPStatus = "Draft";
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
        public List<tblCareerPath> careers { get; set; }
        public bool isRequired { get; set; }
        public bool canComplete { get; set; }
        public bool isGiftedOnly { get; set; }
        public string gender { get; set; }


        public StudentTransitionViewModel()
        {
            iepId = 0;
            studentId = 0;
            student = new tblUser();
            isDOC = false;
            isGiftedOnly = false;
            transition = new tblTransition();
            assessments = new List<tblTransitionAssessment>();
            goals = new List<tblTransitionGoal>();
            services = new List<tblTransitionService>();
            careers = new List<tblCareerPath>();
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
			reevalDates = new List<tblArchiveEvaluationDate>();
			printProgressGoals = new List<int>();
			printStudentInfo = false;
			printIEPDetails = false;
			printHealth = false;
			printMotor = false;
			printComm = false;
			printSocial = false;
			printGeneral = false;
			printAcademic = false;
			printAcc = false;
			printBehavior = false;
			printTrans = false;
			printOther = false;
			printGoals = false;
			printServices = false;
			printNotice = false;
			printProgressReport = false;
			isArchive = false;
			history = new List<IEPHistoryViewModel>();
			accommodationList = new List<AccomodationPrintViewModel>();

		}

		public tblUser teacher { get; set; }
		public tblStudentInfo student { get; set; }
		public List<tblStudentRelationship> contacts { get; set; }
		public List<tblArchiveEvaluationDate> reevalDates { get; set; }
		public List<int> printProgressGoals { get; set; }
		public List<IEPHistoryViewModel> history { get; set; }
		public List<AccomodationPrintViewModel> accommodationList { get; set; }

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
        public string inititationDateNext { get; set; }
        public int studentAgeAtAnnualMeeting { get; set; }
        public string assignChildCount { get; set; }
        public string placementCodeDesc { get; set; }
        public string edStatusCodeDesc { get; set; }
        public bool isDOC { get; set; }
		public bool isArchive{ get; set; }

		public bool printStudentInfo { get; set; }
        public bool printIEPDetails { get; set; }
        public bool printHealth { get; set; }
        public bool printMotor { get; set; }
        public bool printComm { get; set; }
        public bool printSocial { get; set; }
        public bool printGeneral { get; set; }
        public bool printAcademic { get; set; }
        public bool printAcc { get; set; }
        public bool printBehavior { get; set; }
        public bool printTrans { get; set; }
        public bool printOther { get; set; }
        public bool printGoals { get; set; }
        public bool printServices { get; set; }
        public bool printNotice { get; set; }
        public bool printProgressReport { get; set; }
    }

    public class StudentServiceObject
    {
        public int ServiceID { get; set; }
        public int IEPid { get; set; }
        public int SchoolYear { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ServiceCode { get; set; }
        public string selectedAttendingBuilding { get; set; }
        public int Frequency { get; set; }
        public byte DaysPerWeek { get; set; }
        public short Minutes { get; set; }
        public int ProviderID { get; set; }
        public string LocationCode { get; set; }
        public string Goals { get; set; }
        public string ProvidedFor { get; set; }

    }

    public class TeacherView
    {
        public int UserID { get; set; }
        public string Name { get; set; }
    }

    public class UserView
    {
        public int UserID;
        public string FirstName;
        public string MiddleName;
        public string LastName;
        public string RoleID;
        public bool hasIEP;
        public string KidsID;
        public bool isAssigned;
    }

    public class ExportErrorView
    {
        public string UserID { get; set; }
        public string Description { get; set; }

    }

    public class ReferralViewModel
    {
        public ReferralViewModel()
        {
        }

        public string kidsId { get; set; }
        public string lastName { get; set; }
        public string firstName { get; set; }
        public string middleInitial { get; set; }
        public string grade { get; set; }
        public string studentCounty { get; set; }
        public string assignChildCount { get; set; }
        public string submitDate { get; set; }
        public string notes { get; set; }
        public bool isComplete { get; set; }
        public int referralId { get; set; }
    }

    public class IEPHistoryViewModel
    {
        public IEPHistoryViewModel()
        {
        }

        //public int iepId { get; set; }
        public string edStatus { get; set; }
        public string iepType { get; set; }
        public string iepDate { get; set; }
    }

	public class TempTransitionItemViewModel
	{
		public TempTransitionItemViewModel()
		{
		}

		public int TransitionItemID { get; set; }
		public string ElementName { get; set; }		
	}
}
public class IEPStatus
{
    public const string PLAN = "PLAN";
    public const string ACTIVE = "ACTIVE";
    public const string ARCHIVE = "ARCHIVE";
    public const string DRAFT = "DRAFT";
    public const string DELETED = "DELETED";
    public const string AMENDMENT = "AMENDMENT";
    public const string ANNUAL = "ANNUAL";
}