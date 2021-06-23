using GreenBushIEP.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GreenBushIEP.Controllers
{
    public class FormController : Controller
    {
        private const string owner = "1"; //level 5
        private const string mis = "2"; //level 4
        private const string admin = "3"; //level 3
        private const string teacher = "4"; //level 2
        private const string student = "5";
        private const string nurse = "6"; //level 1
        private const string principal = "11";
        private const string superintendent = "12";

        private readonly IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        [Authorize]
        [ValidateInput(false)]
        public ActionResult SaveForm(FormCollection collection)
        {

            string StudentHTMLContent = collection["studentText"];
            string HTMLContent = collection["printText"];
            string HTMLContent2 = collection["printText2"];
            string HTMLContent3 = collection["printText3"];
            string studentName = collection["studentName"];
            string studentId = collection["studentId"];
            string isArchive = collection["isArchive"];
            string iepIDStr = collection["iepID"];
            string isIEP = collection["isIEP"];
            string formName = collection["formName"];
            string isSave = collection["isSave"];
            string fileName = collection["fileName"];

            if (isSave == "1")
            {
                try
                {
                    SaveFormValues(HTMLContent, formName, studentId);
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                    if (ex.InnerException != null)
                    {
                        error += " " + ex.InnerException.Message;
                    }

                    TempData["Error"] = error;
                    return RedirectToAction("Index", "Error");
                }

                return RedirectToAction("IEPFormFile", "Home", new { id = int.Parse(studentId), saved = 1, fileName = fileName });
            }
            else
            {
                TempData["Error"] = "Form is missing information to complete the process";
                return RedirectToAction("Index", "Error");
            }
        }

        #region FormPDFDownload
        private void SaveFormValues(string HTMLContent, string formName, string studentId)
        {
            //capture data
            int sid = !string.IsNullOrEmpty(studentId) ? int.Parse(studentId) : 0;

            if (sid == 0)
            {
                return;
            }

            tblUser currentUser = db.tblUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();

            HtmlDocument htmlDocument = new HtmlDocument
            {
                OptionWriteEmptyNodes = true,
                OptionFixNestedTags = true
            };
            htmlDocument.LoadHtml(HTMLContent);

            List<HtmlNode> spans = htmlDocument.DocumentNode.Descendants().Where(o => o.Name.Equals("span") && o.Id != "").ToList();
            List<HtmlNode> checkboxes = htmlDocument.DocumentNode.Descendants().Where(o => o.Name.Equals("img") && o.HasClass("imgCheck")).ToList();
            List<HtmlNode> checkboxesSpans = htmlDocument.DocumentNode.Descendants().Where(o => o.Name.Equals("span") && o.HasClass("imgCheck")).ToList();
            if (checkboxesSpans.Count > 0)
                checkboxes.AddRange(checkboxesSpans);

            string formNameStr = formName.ToUpper();
            switch (formNameStr)
            {
                case "TEAM EVALUATION REPORT":
                    SaveTeamEval(currentUser, sid, spans, checkboxes);
                    break;

                case "SUMMARY OF PERFORMANCE":
                    SaveSOP(currentUser, sid, spans, checkboxes);
                    break;
                case "CONFERENCE SUMMARY":
                    SaveConferenceSummary(currentUser, sid, spans, checkboxes);
                    break;

                case "IEP AMENDMENT FORM":
                    SaveIEPAmendment(currentUser, sid, spans, checkboxes);
                    break;
                case "IEP MEETING-CONSENT TO INVITE REPRESENTATIVE OF NON-EDUCATIONAL AGENCY":
                    SaveIEPMeetingConsent(currentUser, sid, spans, checkboxes);
                    break;
                case "IEP MEETING-EXCUSAL FROM ATTENDANCE FORM":
                    SaveIEPMeetingExcusal(currentUser, sid, spans, checkboxes);
                    break;
                case "IEP TEAM CONSIDERATIONS":
                    SaveIEPTeamConsiderations(currentUser, sid, spans, checkboxes);
                    break;
                case "MANIFESTATION DETERMINATION REVIEW FORM":
                    SaveManifestationDetermination(currentUser, sid, spans, checkboxes);
                    break;
                case "NOTICE OF MEETING":
                    SaveNoticeOfMeeting(currentUser, sid, spans, checkboxes);
                    break;
                case "PARENT CONSENT FOR RELEASE OF INFORMATION AND MEDICAID REIMBURSEMENT":
                    SaveParentConsent(currentUser, sid, spans, checkboxes);
                    break;
                case "PHYSICIAN SCRIPT":
                    SavePhysicianScript(currentUser, sid, spans, checkboxes);
                    break;
                case "PRIOR WRITTEN NOTICE - IDENTIFICATION":
                    SavePWNID(currentUser, sid, spans, checkboxes);
                    break;
                case "PRIOR WRITTEN NOTICE - EVALUATION -ENGLISH":
                    SavePWNEval(currentUser, sid, spans, checkboxes);
                    break;

                case "PRIOR WRITTEN NOTICE - REVOCATION OF ALL SERVICES":
                    SavePWNRevAllServices(currentUser, sid, spans, checkboxes);
                    break;
                case "PRIOR WRITTEN NOTICE - REVOCATION OF PARTICULAR SERVICES":
                    SavePWNRevPartServices(currentUser, sid, spans, checkboxes);
                    break;
                case "REVOCATION OF CONSENT-ALL SERVICES":
                    SaveRevAllServices(currentUser, sid, spans, checkboxes);
                    break;
                case "REVOCATION OF CONSENT - PARTICULAR SERVICES":
                    SaveRevPartServices(currentUser, sid, spans, checkboxes);
                    break;
                case "REQUEST FOR TRANSPORTATION":
                    SaveTransportation(currentUser, sid, spans, checkboxes);
                    break;
                case "INDIVIDUAL CONTINUOUS LEARNING PLAN":
                    SaveILP(currentUser, sid, spans, checkboxes);
                    break;
                case "TRANSITION REFERRAL":
                    SaveTransitionReferral(currentUser, sid, spans, checkboxes);
                    break;
                case "CHILD OUTCOMES SUMMARY":
                    SaveChildOutcomeSummary(currentUser, sid, spans, checkboxes);
                    break;
                case "IDEA & GIFTED FILE REVIEW":
                    SaveFileReview(currentUser, sid, spans, checkboxes);
                    break;
            }
        }

        private void SaveFileReview(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {
            //tblFormFileReview formFileReview = db.tblFormFileReviews.Any(o => o.StudentId == sid) ? db.tblFormFileReviews.FirstOrDefault(o => o.StudentId == sid) : new tblFormFileReview();
            //formFileReview.StudentId = sid;

            tblFormFileReview formFileReview = new tblFormFileReview();

            formFileReview.StudentName = GetInputValue("StudentName", spans);
            
            formFileReview.ParentRightsProvided =  GetRadioInputValue("ParentRightsProvided", spans);
            formFileReview.AssessNotDiscrimatory = GetRadioInputValue("AssessNotDiscrimatory", spans);
            formFileReview.AssessNativeLanguage = GetRadioInputValue("AssessNativeLanguage", spans);
            formFileReview.AssessExceptionality = GetRadioInputValue("AssessExceptionality", spans);
            formFileReview.PublicAgencyPayForDx = GetRadioInputValue("PublicAgencyPayForDx", spans);
            formFileReview.QualifiedProfExceptionality = GetRadioInputValue("QualifiedProfExceptionality", spans);
            formFileReview.GroupContainQualified = GetRadioInputValue("GroupContainQualified", spans);
            formFileReview.NoSingleCriterionUsed = GetRadioInputValue("NoSingleCriterionUsed", spans);
            formFileReview.CarefullyConsider = GetRadioInputValue("CarefullyConsider", spans);
            formFileReview.NotDeterminant = GetRadioInputValue("NotDeterminant", spans);
            formFileReview.AcademicLevelDesc = GetRadioInputValue("AcademicLevelDesc", spans);
            formFileReview.FunctionalLevelDesc = GetRadioInputValue("FunctionalLevelDesc", spans);
            formFileReview.DescribeAffects = GetRadioInputValue("DescribeAffects", spans);
            formFileReview.GoalsDesignedToMeet = GetRadioInputValue("GoalsDesignedToMeet", spans);
            formFileReview.GoalsMeasurable = GetRadioInputValue("GoalsMeasurable", spans);
            formFileReview.GoalsProgress = GetRadioInputValue("GoalsProgress", spans);
            formFileReview.AlternativeAssessment = GetRadioInputValue("AlternativeAssessment", spans);
            formFileReview.IncludeProjectedDate = GetRadioInputValue("IncludeProjectedDate", spans);
            formFileReview.IncludeServices = GetRadioInputValue("IncludeServices", spans);
            formFileReview.EnhanceEducation = GetRadioInputValue("EnhanceEducation", spans);
            formFileReview.PostiveBehavioralIntervention = GetRadioInputValue("PostiveBehavioralIntervention", spans);
            formFileReview.EnglishProficiency = GetRadioInputValue("EnglishProficiency", spans);
            formFileReview.EducationalPlacement = GetRadioInputValue("EducationalPlacement", spans);
            formFileReview.SeverityOfDisability = GetRadioInputValue("SeverityOfDisability", spans);
            formFileReview.Comments = GetInputValue("Comments", spans);

            if (formFileReview.FormFileReviewID == 0)
            {
                formFileReview.CreatedBy = currentUser.UserID;
                formFileReview.Create_Date = DateTime.Now;
                formFileReview.ModifiedBy = currentUser.UserID;
                formFileReview.Update_Date = DateTime.Now;
                db.tblFormFileReviews.Add(formFileReview);
            }
            else
            {
                formFileReview.ModifiedBy = currentUser.UserID;
                formFileReview.Update_Date = DateTime.Now;
            }

           // db.SaveChanges();
        }
        private void SaveTeamEval(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {

            tblFormTeamEval teamEval = db.tblFormTeamEvals.Any(o => o.StudentId == sid) ? db.tblFormTeamEvals.FirstOrDefault(o => o.StudentId == sid) : new tblFormTeamEval();

            teamEval.StudentId = sid;
            teamEval.ReasonReferral = GetInputValue("txtReasonReferral", spans);
            teamEval.MedicalFindings = GetInputValue("txtMedicalFindings", spans);
            teamEval.Hearing = GetInputValue("txtHearing", spans);
            teamEval.Vision = GetInputValue("txtVision", spans);
            teamEval.RelevantBehavior = GetInputValue("txtRelevantBehavior", spans);
            teamEval.InfoReview = GetInputValue("txtInfoReview", spans);
            teamEval.ParentInterview = GetInputValue("txtParentInterview", spans);
            teamEval.TestData = GetInputValue("txtTestData", spans);
            teamEval.IntellectualDevelopment = GetInputValue("txtIntellectualDevelopment", spans);
            teamEval.Peformance = GetInputValue("txtPeformance", spans);
            teamEval.Disadvantage = GetInputValue("txtDisadvantage", spans);
            teamEval.DisadvantageExplain = GetInputValue("txtDisadvantageExplain", spans);
            teamEval.Regulations = GetInputValue("txtRegulations", spans);
            teamEval.SustainedResources = GetInputValue("txtSustainedResources", spans);
            teamEval.Strengths = GetInputValue("txtStrengths", spans);
            teamEval.AreaOfConcern = GetInputValue("txtAreaOfConcern", spans);
            teamEval.GeneralEducationExpectations = GetInputValue("txtGeneralEducationExpectations", spans);
            teamEval.Tried = GetInputValue("txtTried", spans);
            teamEval.NotWorked = GetInputValue("txtNotWorked", spans);
            teamEval.GeneralDirection = GetInputValue("txtGeneralDirection", spans);
            teamEval.MeetEligibility = GetInputValue("txtMeetEligibility", spans);
            teamEval.ResourcesNeeded = GetInputValue("txtResourcesNeeded", spans);
            teamEval.SpecificNeeds = GetInputValue("txtSpecificNeeds", spans);
            teamEval.ConvergentData = GetInputValue("txtConvergentData", spans);
            teamEval.ListSources = GetInputValue("txtListSources", spans);
            teamEval.FormDate = GetInputValueDate("FormDate", spans);

            teamEval.Regulation_flag = GetCheckboxInputValue("Regulation_flag_Yes", "Regulation_flag_No", checkboxes);
            teamEval.SustainedResources_flag = GetCheckboxInputValue("SustainedResources_flag_Yes", "SustainedResources_flag_No", checkboxes);
            teamEval.ConvergentData_flag = GetCheckboxInputValue("ConvergentData_flag_Yes", "ConvergentData_flag_No", checkboxes);

            if (teamEval.FormTeamEvalId == 0)
            {
                teamEval.CreatedBy = currentUser.UserID;
                teamEval.Create_Date = DateTime.Now;
                teamEval.ModifiedBy = currentUser.UserID;
                teamEval.Update_Date = DateTime.Now;
                db.tblFormTeamEvals.Add(teamEval);
            }
            else
            {
                teamEval.ModifiedBy = currentUser.UserID;
                teamEval.Update_Date = DateTime.Now;
            }

            db.SaveChanges();
        }
        private void SaveSOP(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {
            tblFormSummaryPerformance summaryPerf = db.tblFormSummaryPerformances.Any(o => o.StudentId == sid) ? db.tblFormSummaryPerformances.FirstOrDefault(o => o.StudentId == sid) : new tblFormSummaryPerformance();

            summaryPerf.StudentId = sid;
            summaryPerf.DateOfBirth = GetInputValueDate("DateOfBirth", spans);
            summaryPerf.student_phone = GetInputValue("student_phone", spans);
            summaryPerf.student_Name = GetInputValue("student_Name", spans);

            var exitYear = GetInputValue("GraduationExitYear", spans);
            int exitYearVal = 0;
            Int32.TryParse(exitYear, out exitYearVal);
            if (exitYearVal > 0)
                summaryPerf.GraduationExitYear = exitYearVal;

            summaryPerf.CurrentSchool = GetInputValue("CurrentSchool", spans);
            summaryPerf.CurrentCity = GetInputValue("CurrentCity", spans);
            summaryPerf.PrimaryLanguage = GetInputValue("PrimaryLanguage", spans);
            summaryPerf.ContactName = GetInputValue("ContactName", spans);
            summaryPerf.ContactTitle = GetInputValue("ContactTitle", spans);
            summaryPerf.ContactSchool = GetInputValue("ContactSchool", spans);
            summaryPerf.ContactEmail = GetInputValue("ContactEmail", spans);
            summaryPerf.ContactPhone = GetInputValue("ContactPhone", spans);
            summaryPerf.Team_StudentName = GetInputValue("Team_StudentName", spans);
            summaryPerf.Team_ParentName = GetInputValue("Team_ParentName", spans);
            summaryPerf.Team_TeacherName1 = GetInputValue("Team_TeacherName1", spans);
            summaryPerf.Team_TeacherName2 = GetInputValue("Team_TeacherName2", spans);
            summaryPerf.Team_OtherProvider1 = GetInputValue("Team_OtherProvider1", spans);
            summaryPerf.Team_OtherProvider2 = GetInputValue("Team_OtherProvider2", spans);

            summaryPerf.Goal_Learning = GetInputValue("Goal_Learning", spans);
            summaryPerf.Goal_LearningRecommendation = GetInputValue("Goal_LearningRecommendation", spans);
            summaryPerf.Goal_Working = GetInputValue("Goal_Working", spans);
            summaryPerf.Goal_WorkingRecommendation = GetInputValue("Goal_WorkingRecommendation", spans);
            summaryPerf.Goal_Living = GetInputValue("Goal_Living", spans);
            summaryPerf.Goal_LivingRecommendation = GetInputValue("Goal_LivingRecommendation", spans);
            summaryPerf.AC_ReadingPerformance = GetInputValue("AC_ReadingPerformance", spans);
            summaryPerf.AC_ReadingAccommodations = GetInputValue("AC_ReadingAccommodations", spans);
            summaryPerf.AC_MathPerformance = GetInputValue("AC_MathPerformance", spans);
            summaryPerf.AC_MathAccommodations = GetInputValue("AC_MathAccommodations", spans);
            summaryPerf.AC_LanguagePerformance = GetInputValue("AC_LanguagePerformance", spans);
            summaryPerf.AC_LanguageAccommodations = GetInputValue("AC_LanguageAccommodations", spans);
            summaryPerf.AC_LearningPerformance = GetInputValue("AC_LearningPerformance", spans);
            summaryPerf.AC_LearningAccommodations = GetInputValue("AC_LearningAccommodations", spans);
            summaryPerf.AC_OtherPerformance = GetInputValue("AC_OtherPerformance", spans);
            summaryPerf.AC_OtherAccommodations = GetInputValue("AC_OtherAccommodations", spans);
            summaryPerf.Functional_SocialPerformance = GetInputValue("Functional_SocialPerformance", spans);
            summaryPerf.Functional_SocialAccommodations = GetInputValue("Functional_SocialAccommodations", spans);
            summaryPerf.Functional_LivingPerformance = GetInputValue("Functional_LivingPerformance", spans);
            summaryPerf.Functional_LivingAccommodations = GetInputValue("Functional_LivingAccommodations", spans);
            summaryPerf.Functional_MobiilityPerformance = GetInputValue("Functional_MobiilityPerformance", spans);
            summaryPerf.Functional_MobiilityAccommodations = GetInputValue("Functional_MobiilityAccommodations", spans);
            summaryPerf.Functional_AdvocacyPerformance = GetInputValue("Functional_AdvocacyPerformance", spans);
            summaryPerf.Functional_AdvocacyAccommodations = GetInputValue("Functional_AdvocacyAccommodations", spans);
            summaryPerf.Functional_EmploymentPerformance = GetInputValue("Functional_EmploymentPerformance", spans);
            summaryPerf.Functional_EmploymentAccommodations = GetInputValue("Functional_EmploymentAccommodations", spans);
            summaryPerf.Functional_AdditionsPerformance = GetInputValue("Functional_AdditionsPerformance", spans);
            summaryPerf.Functional_AdditionsAccommodations = GetInputValue("Functional_AdditionsAccommodations", spans);
            summaryPerf.DateCompleted = GetInputDateValue("DateCompleted", spans);
            summaryPerf.Documentation_PsychologicalAssementName = GetInputValue("Documentation_PsychologicalAssementName", spans);
            summaryPerf.Documentation_PsychologicalDate = GetInputDateValue("Documentation_PsychologicalDate", spans);
            summaryPerf.Documentation_NeuropsychologicalAssementName = GetInputValue("Documentation_NeuropsychologicalAssementName", spans);
            summaryPerf.Documentation_NeuropsychologicalDate = GetInputDateValue("Documentation_NeuropsychologicalDate", spans);
            summaryPerf.Documentation_MedicalAssementName = GetInputValue("Documentation_MedicalAssementName", spans);
            summaryPerf.Documentation_MedicalDate = GetInputDateValue("Documentation_MedicalDate", spans);
            summaryPerf.Documentation_CommunicationAssementName = GetInputValue("Documentation_CommunicationAssementName", spans);
            summaryPerf.Documentation_CommunicationDate = GetInputDateValue("Documentation_CommunicationDate", spans);
            summaryPerf.Documentation_AdaptiveBehaviorAssementName = GetInputValue("Documentation_AdaptiveBehaviorAssementName", spans);
            summaryPerf.Documentation_AdaptiveBehaviorDate = GetInputDateValue("Documentation_AdaptiveBehaviorDate", spans);
            summaryPerf.Documentation_InterpersonalAssementName = GetInputValue("Documentation_InterpersonalAssementName", spans);
            summaryPerf.Documentation_InterpersonalDate = GetInputDateValue("Documentation_InterpersonalDate", spans);
            summaryPerf.Documentation_SpeechAssementName = GetInputValue("Documentation_SpeechAssementName", spans);
            summaryPerf.Documentation_SpeechDate = GetInputDateValue("Documentation_SpeechDate", spans);
            summaryPerf.Documentation_MTSSAssementName = GetInputValue("Documentation_MTSSAssementName", spans);
            summaryPerf.Documentation_MTSSDate = GetInputDateValue("Documentation_MTSSDate", spans);
            summaryPerf.Documentation_CareerAssementName = GetInputValue("Documentation_CareerAssementName", spans);
            summaryPerf.Documentation_CareerDate = GetInputDateValue("Documentation_CareerDate", spans);
            summaryPerf.Documentation_CommunityAssementName = GetInputValue("Documentation_CommunityAssementName", spans);
            summaryPerf.Documentation_CommunityDate = GetInputDateValue("Documentation_CommunityDate", spans);
            summaryPerf.Documentation_SelfDeterminationAssementName = GetInputValue("Documentation_SelfDeterminationAssementName", spans);
            summaryPerf.Documentation_SelfDeterminationDate = GetInputDateValue("Documentation_SelfDeterminationDate", spans);
            summaryPerf.Documentation_AssistiveTechAssementName = GetInputValue("Documentation_AssistiveTechAssementName", spans);
            summaryPerf.Documentation_AssistiveTechDate = GetInputDateValue("Documentation_AssistiveTechDate", spans);
            summaryPerf.Documentation_ClassroomAssementName = GetInputValue("Documentation_ClassroomAssementName", spans);
            summaryPerf.Documentation_ClassroomDate = GetInputDateValue("Documentation_ClassroomDate", spans);
            summaryPerf.Documentation_OtherAssementName = GetInputValue("Documentation_OtherAssementName", spans);
            summaryPerf.Documentation_OtherDate = GetInputDateValue("Documentation_OtherDate", spans);
            summaryPerf.AdditionalInformation = GetInputValue("AdditionalInformation", spans);


            if (summaryPerf.FormSummaryPerformanceId == 0)
            {
                summaryPerf.CreatedBy = currentUser.UserID;
                summaryPerf.Create_Date = DateTime.Now;
                summaryPerf.ModifiedBy = currentUser.UserID;
                summaryPerf.Update_Date = DateTime.Now;
                db.tblFormSummaryPerformances.Add(summaryPerf);
            }
            else
            {
                summaryPerf.ModifiedBy = currentUser.UserID;
                summaryPerf.Update_Date = DateTime.Now;
            }

            db.SaveChanges();
        }
        private void SaveConferenceSummary(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {

            tblFormConferenceSummary conf = db.tblFormConferenceSummaries.Any(o => o.StudentId == sid) ? db.tblFormConferenceSummaries.FirstOrDefault(o => o.StudentId == sid) : new tblFormConferenceSummary();

            conf.StudentId = sid;
            conf.BuildingAdministrator = GetInputValue("txtBuildingAdministrator", spans);
            conf.RequestedBy = GetInputValue("txtRequestedBy", spans);
            conf.ReasonForConfrence = GetInputValue("txtReasonForConfrence", spans);
            conf.Conclusions = GetInputValue("txtConclusions", spans);
            conf.PlacementCode = GetInputValue("PlacementCode", spans);
            conf.TeacherName = GetInputValue("TeacherName", spans);

            if (conf.FormConferenceSummaryId == 0)
            {
                conf.CreatedBy = currentUser.UserID;
                conf.Create_Date = DateTime.Now;
                conf.ModifiedBy = currentUser.UserID;
                conf.Update_Date = DateTime.Now;
                db.tblFormConferenceSummaries.Add(conf);
            }
            else
            {
                conf.ModifiedBy = currentUser.UserID;
                conf.Update_Date = DateTime.Now;
            }

            db.SaveChanges();
        }
        private void SaveIEPAmendment(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {
            tblFormIEPAmendment formAmend = db.tblFormIEPAmendments.Any(o => o.StudentId == sid) ? db.tblFormIEPAmendments.FirstOrDefault(o => o.StudentId == sid) : new tblFormIEPAmendment();

            formAmend.StudentId = sid;
            formAmend.AgreeToAmmend = GetCheckboxSingleInputValue("AgreeToAmmend", checkboxes);
            formAmend.DisagreeToAmmend = GetCheckboxSingleInputValue("DisagreeToAmmend", checkboxes);
            formAmend.ConveneMeeting = GetCheckboxSingleInputValue("ConveneMeeting", checkboxes);
            formAmend.DoNotConveneMeeting = GetCheckboxSingleInputValue("DoNotConveneMeeting", checkboxes);
            formAmend.Description = GetInputValue("Description", spans);

            if (formAmend.FormIEPAmendmentId == 0)
            {
                formAmend.CreatedBy = currentUser.UserID;
                formAmend.Create_Date = DateTime.Now;
                formAmend.ModifiedBy = currentUser.UserID;
                formAmend.Update_Date = DateTime.Now;
                db.tblFormIEPAmendments.Add(formAmend);
            }
            else
            {
                formAmend.ModifiedBy = currentUser.UserID;
                formAmend.Update_Date = DateTime.Now;
            }

            db.SaveChanges();
        }
        private void SaveIEPMeetingConsent(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {
            tblFormIEPMeetingConsentToInvite formMeetConsent = db.tblFormIEPMeetingConsentToInvites.Any(o => o.StudentId == sid) ? db.tblFormIEPMeetingConsentToInvites.FirstOrDefault(o => o.StudentId == sid) : new tblFormIEPMeetingConsentToInvite();

            formMeetConsent.StudentId = sid;
            formMeetConsent.FurtherInformed = GetCheckboxSingleInputValue("FurtherInformed", checkboxes);
            formMeetConsent.ProvideTransitionService = GetCheckboxSingleInputValue("ProvideTransitionService", checkboxes);
            formMeetConsent.ParticipatingAgency = GetInputValue("ParticipatingAgency", spans);
            formMeetConsent.MeetingDate = GetInputValueDate("MeetingDate", spans);

            if (formMeetConsent.FormIEPMeetingConsentToInviteId == 0)
            {
                formMeetConsent.CreatedBy = currentUser.UserID;
                formMeetConsent.Create_Date = DateTime.Now;
                formMeetConsent.ModifiedBy = currentUser.UserID;
                formMeetConsent.Update_Date = DateTime.Now;
                db.tblFormIEPMeetingConsentToInvites.Add(formMeetConsent);
            }
            else
            {
                formMeetConsent.ModifiedBy = currentUser.UserID;
                formMeetConsent.Update_Date = DateTime.Now;
            }

            db.SaveChanges();
        }
        private void SaveIEPMeetingExcusal(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {
            tblFormIEPMeetingExcusal formExcusal = db.tblFormIEPMeetingExcusals.Any(o => o.StudentId == sid) ? db.tblFormIEPMeetingExcusals.FirstOrDefault(o => o.StudentId == sid) : new tblFormIEPMeetingExcusal();

            formExcusal.StudentId = sid;
            formExcusal.ParentName = GetInputValue("ParentName", spans);
            formExcusal.SchoolRepresentative = GetInputValue("SchoolRepresentative", spans);
            formExcusal.PositionOfRepresentative = GetInputValue("PositionOfRepresentative", spans);
            formExcusal.PositionOfMemberNotAttending = GetInputValue("PositionOfMemberNotAttending", spans);

            formExcusal.Services_MayBe_ModOrDisc_NonAttend = GetCheckboxSingleInputValue("Services_MayBe_ModOrDisc_NonAttend", checkboxes);
            formExcusal.Services_MayBe_ModOrDisc_PartialAttend = GetCheckboxSingleInputValue("Services_MayBe_ModOrDisc_PartialAttend", checkboxes);

            formExcusal.Services_MayBe_ModOrDisc_IssueDiscussed = GetInputValue("Services_MayBe_ModOrDisc_IssueDiscussed", spans);
            formExcusal.Services_Not_ModOrDisc_IssueDiscussed = GetInputValue("Services_Not_ModOrDisc_IssueDiscussed", spans);

            formExcusal.Services_Not_ModOrDisc_Agree = GetCheckboxSingleInputValue("Services_Not_ModOrDisc_Agree", checkboxes);
            formExcusal.Services_Not_ModOrDisc_Disagree = GetCheckboxSingleInputValue("Services_Not_ModOrDisc_Disagree", checkboxes);

            formExcusal.Services_Not_ModOrDisc_NonAttend = GetCheckboxSingleInputValue("Services_Not_ModOrDisc_NonAttend", checkboxes);
            formExcusal.Services_Not_ModOrDisc_PartialAttend = GetCheckboxSingleInputValue("Services_Not_ModOrDisc_PartialAttend", checkboxes);

            formExcusal.Services_MayBe_ModOrDisc_Agree = GetCheckboxSingleInputValue("Services_MayBe_ModOrDisc_Agree", checkboxes);
            formExcusal.Services_MayBe_ModOrDisc_Disagree = GetCheckboxSingleInputValue("Services_MayBe_ModOrDisc_Disagree", checkboxes);

            formExcusal.FormDate = GetInputValueDate("FormDate", spans);
            formExcusal.IEPDate = GetInputValueDate("IEPDate", spans);

            if (formExcusal.FormIEPMeetingExcusalId == 0)
            {
                formExcusal.CreatedBy = currentUser.UserID;
                formExcusal.Create_Date = DateTime.Now;
                formExcusal.ModifiedBy = currentUser.UserID;
                formExcusal.Update_Date = DateTime.Now;
                db.tblFormIEPMeetingExcusals.Add(formExcusal);
            }
            else
            {
                formExcusal.ModifiedBy = currentUser.UserID;
                formExcusal.Update_Date = DateTime.Now;
            }

            db.SaveChanges();
        }
        private void SaveIEPTeamConsiderations(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {
            tblFormIEPTeamConsideration formTeam = db.tblFormIEPTeamConsiderations.Any(o => o.StudentId == sid) ? db.tblFormIEPTeamConsiderations.FirstOrDefault(o => o.StudentId == sid) : new tblFormIEPTeamConsideration();

            formTeam.StudentId = sid;
            formTeam.ChildsStrength = GetInputValue("ChildsStrength", spans);
            formTeam.UtilizeStrength = GetInputValue("UtilizeStrength", spans);
            formTeam.ConcernsForEnhancing = GetInputValue("ConcernsForEnhancing", spans);
            formTeam.ConcernsAddressesd = GetInputValue("ConcernsAddressesd", spans);
            formTeam.HarmfulEffects = GetInputValue("HarmfulEffects", spans);
            formTeam.PhysicalEducatoin = GetInputValue("PhysicalEducatoin", spans);
            formTeam.ExtendedSchoolYear = GetInputValue("ExtendedSchoolYear", spans);
            formTeam.NeedsBeenConsidered_Yes = GetCheckboxSingleInputValue("NeedsBeenConsidered_Yes", checkboxes);
            formTeam.NeedsBeenConsidered_No = GetCheckboxSingleInputValue("NeedsBeenConsidered_No", checkboxes);
            formTeam.NeedsBeenConsidered_Desc = GetInputValue("NeedsBeenConsidered_Desc", spans);
            formTeam.BehaviorImpede_Yes = GetCheckboxSingleInputValue("BehaviorImpede_Yes", checkboxes);
            formTeam.BehaviorImpede_No = GetCheckboxSingleInputValue("BehaviorImpede_No", checkboxes);
            formTeam.BehaviorImpede_Desc = GetInputValue("BehaviorImpede_Desc", spans);
            formTeam.InstructionInBraille_Yes = GetCheckboxSingleInputValue("InstructionInBraille_Yes", checkboxes);
            formTeam.InstructionInBraille_No = GetCheckboxSingleInputValue("InstructionInBraille_No", checkboxes);
            formTeam.InstructionInBraille_Desc = GetInputValue("InstructionInBraille_Desc", spans);
            formTeam.LimitedEnglish_Yes = GetCheckboxSingleInputValue("LimitedEnglish_Yes", checkboxes);
            formTeam.LimitedEnglish_No = GetCheckboxSingleInputValue("LimitedEnglish_No", checkboxes);
            formTeam.LimitedEnglish_Desc = GetInputValue("LimitedEnglish_Desc", spans);
            formTeam.CommunicationNeeds_Yes = GetCheckboxSingleInputValue("CommunicationNeeds_Yes", checkboxes);
            formTeam.CommunicationNeeds_No = GetCheckboxSingleInputValue("CommunicationNeeds_No", checkboxes);
            formTeam.CommunicationNeeds_Desc = GetInputValue("CommunicationNeeds_Desc", spans);
            formTeam.HearingCommunicationNeeds_Yes = GetCheckboxSingleInputValue("HearingCommunicationNeeds_Yes", checkboxes);
            formTeam.HearingCommunicationNeeds_No = GetCheckboxSingleInputValue("HearingCommunicationNeeds_No", checkboxes);
            formTeam.HearingCommunicationNeeds_Desc = GetInputValue("HearingCommunicationNeeds_Desc", spans);
            formTeam.AssistiveTech_Yes = GetCheckboxSingleInputValue("AssistiveTech_Yes", checkboxes);
            formTeam.AssistiveTech_No = GetCheckboxSingleInputValue("AssistiveTech_No", checkboxes);
            formTeam.AssistiveTech_Desc = GetInputValue("AssistiveTech_Desc", spans);
            formTeam.IEPMeetingDate = GetInputValueDate("IEPMeetingDate", spans);

            if (formTeam.FormIEPTeamConsiderationsId == 0)
            {
                formTeam.CreatedBy = currentUser.UserID;
                formTeam.Create_Date = DateTime.Now;
                formTeam.ModifiedBy = currentUser.UserID;
                formTeam.Update_Date = DateTime.Now;
                db.tblFormIEPTeamConsiderations.Add(formTeam);
            }
            else
            {
                formTeam.ModifiedBy = currentUser.UserID;
                formTeam.Update_Date = DateTime.Now;
            }

            db.SaveChanges();
        }
        private void SaveManifestationDetermination(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {

            tblFormManifestationDeterminiation formMani = db.tblFormManifestationDeterminiations.Any(o => o.StudentId == sid) ? db.tblFormManifestationDeterminiations.FirstOrDefault(o => o.StudentId == sid) : new tblFormManifestationDeterminiation();

            formMani.StudentId = sid;
            formMani.FormDate = GetInputValueDate("FormDate", spans);
            formMani.StudentBehavior = GetInputValue("StudentBehavior", spans);
            formMani.StudentIEP = GetInputValue("StudentIEP", spans);
            formMani.TeacherObservation = GetInputValue("TeacherObservation", spans);
            formMani.ParentInformation = GetInputValue("ParentInformation", spans);
            formMani.OtherInformation = GetInputValue("OtherInformation", spans);
            formMani.IsManifestationOfDisability = GetCheckboxSingleInputValue("IsManifestationOfDisability", checkboxes);
            formMani.StudentWillReturn = GetCheckboxSingleInputValue("StudentWillReturn", checkboxes);
            formMani.BehaviorPlan_IsManifest_Develop = GetCheckboxSingleInputValue("BehaviorPlan_IsManifest_Develop", checkboxes);
            formMani.ReviewBehaviorPlan = GetCheckboxSingleInputValue("ReviewBehaviorPlan", checkboxes);
            formMani.IsNotManifestationOfDisability = GetCheckboxSingleInputValue("IsNotManifestationOfDisability", checkboxes);
            formMani.DisciplinaryRemovalMayOccur = GetCheckboxSingleInputValue("DisciplinaryRemovalMayOccur", checkboxes);
            formMani.BehaviorPlan_NotManifest_Develop = GetCheckboxSingleInputValue("BehaviorPlan_NotManifest_Develop", checkboxes);
            formMani.Attachments = GetCheckboxInputValue("Attachments_Yes", "Attachments_No", checkboxes);
            formMani.ConductCausedByDisability_Yes = GetCheckboxSingleInputValue("ConductCausedByDisability_No", checkboxes);
            formMani.ConductCausedByDisability_No = GetCheckboxSingleInputValue("ConductCausedByDisability_No", checkboxes);
            formMani.ConductCausedByFailure_Yes = GetCheckboxSingleInputValue("ConductCausedByFailure_Yes", checkboxes);
            formMani.ConductCausedByFailure_No = GetCheckboxSingleInputValue("ConductCausedByFailure_No", checkboxes);

            if (formMani.FormManifestationDeterminiationId == 0)
            {
                formMani.CreatedBy = currentUser.UserID;
                formMani.Create_Date = DateTime.Now;
                formMani.ModifiedBy = currentUser.UserID;
                formMani.Update_Date = DateTime.Now;
                db.tblFormManifestationDeterminiations.Add(formMani);
            }
            else
            {
                formMani.ModifiedBy = currentUser.UserID;
                formMani.Update_Date = DateTime.Now;
            }

            db.SaveChanges();

            //get member info

            //delete all
            foreach (tblFormManifestDeterm_TeamMembers existingPD in db.tblFormManifestDeterm_TeamMembers.Where(o => o.FormManifestationDeterminiationId == formMani.FormManifestationDeterminiationId))
            {
                db.tblFormManifestDeterm_TeamMembers.Remove(existingPD);
            }

            db.SaveChanges();

            //add back
            List<HtmlNode> memberPresentSpans = spans.Where(o => o.HasClass("memberPresent")).ToList();
            foreach (HtmlNode mp in memberPresentSpans.Where(o => o.HasClass("memberName")))
            {
                string elementId = mp.Id;  // = memberPresentTitle_0"
                string[] elementSplit = elementId.Split('_');
                if (elementSplit.Length > 1)
                {
                    bool isDissent = mp.HasClass("dissent");
                    HtmlNode elementTitle = null;
                    if (isDissent)
                    {
                        elementTitle = memberPresentSpans.Where(o => o.Id == "memberPresentTitle_" + elementSplit[1] && o.HasClass("dissent")).FirstOrDefault();
                    }
                    else
                    {
                        elementTitle = memberPresentSpans.Where(o => o.Id == "memberPresentTitle_" + elementSplit[1] && !o.HasClass("dissent")).FirstOrDefault();
                    }

                    tblFormManifestDeterm_TeamMembers teamMember = new tblFormManifestDeterm_TeamMembers()
                    {
                        Name = mp.InnerHtml,
                        Title = elementTitle != null ? elementTitle.InnerHtml : "",
                        Dissenting = isDissent,
                        StudentId = sid,
                        FormManifestationDeterminiationId = formMani.FormManifestationDeterminiationId,
                        CreatedBy = currentUser.UserID,

                    };

                    if (!string.IsNullOrEmpty(teamMember.Name))
                    {
                        db.tblFormManifestDeterm_TeamMembers.Add(teamMember);
                    }
                }
            }

            db.SaveChanges();

        }

        private void SaveNoticeOfMeeting(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {
            tblFormNoticeOfMeeting formNotice = db.tblFormNoticeOfMeetings.Any(o => o.StudentId == sid) ? db.tblFormNoticeOfMeetings.FirstOrDefault(o => o.StudentId == sid) : new tblFormNoticeOfMeeting();

            formNotice.StudentId = sid;

            formNotice.Parentname = GetInputValue("ParentName", spans);
            formNotice.Address = GetInputValue("Address", spans);
            formNotice.CityStateZip = GetInputValue("CityStateZip", spans);

            formNotice.ProposedMeetingInfo = GetInputValue("ProposedMeetingInfo", spans);
            formNotice.MeetingToReviewEvaluation = GetCheckboxSingleInputValue("MeetingToReviewEvaluation", checkboxes);
            formNotice.DevelopIEP = GetCheckboxSingleInputValue("DevelopIEP", checkboxes);
            formNotice.DiscussIEPChanges = GetCheckboxSingleInputValue("DiscussIEPChanges", checkboxes);
            formNotice.AnnualIEPReview = GetCheckboxSingleInputValue("AnnualIEPReview", checkboxes);
            formNotice.TransitionAssesment = GetCheckboxSingleInputValue("TransitionAssesment", checkboxes);
            formNotice.Other = GetCheckboxSingleInputValue("Other", checkboxes);
            formNotice.SpecialExpertise1 = GetInputValue("SpecialExpertise1", spans);
            formNotice.SpecialExpertise2 = GetInputValue("SpecialExpertise2", spans);
            formNotice.SpecialExpertise3 = GetInputValue("SpecialExpertise3", spans);
            formNotice.SpecialExpertise4 = GetInputValue("SpecialExpertise4", spans);
            formNotice.SpecialExpertise5 = GetInputValue("SpecialExpertise5", spans);
            formNotice.SpecialExpertise6 = GetInputValue("SpecialExpertise6", spans);
            formNotice.AgencyStaff = GetInputValue("AgencyStaff", spans);
            formNotice.SchoolContactName = GetInputValue("SchoolContactName", spans);
            formNotice.SchoolContactPhone = GetInputValue("SchoolContactPhone", spans);
            formNotice.DeliveriedByWho = GetInputValue("DeliveriedByWho", spans);
            formNotice.DeliveriedTo = GetInputValue("DeliveriedTo", spans);
            formNotice.DelieveredByHand = GetCheckboxSingleInputValue("DelieveredByHand", checkboxes);
            formNotice.DelieveredByMail = GetCheckboxSingleInputValue("DelieveredByMail", checkboxes);
            formNotice.DelieveredByOther = GetCheckboxSingleInputValue("DelieveredByOther", checkboxes);
            formNotice.DelieveredByOtherDesc = GetInputValue("DelieveredByOtherDesc", spans);
            formNotice.PlanToAttend = GetCheckboxSingleInputValue("PlanToAttend", checkboxes);
            formNotice.RescheduleMeeting = GetCheckboxSingleInputValue("RescheduleMeeting", checkboxes);
            formNotice.AvaliableToAttend_flag = GetCheckboxSingleInputValue("AvaliableToAttend_flag", checkboxes);
            formNotice.AvailableToAttend_desc = GetInputValue("AvailableToAttend_desc", spans);
            formNotice.WaiveRightToNotice = GetCheckboxSingleInputValue("WaiveRightToNotice", checkboxes);
            formNotice.DelieveredDate = GetInputValueDate("DelieveredDate", spans);
            formNotice.FormDate = GetInputValueDate("FormDate", spans);

            if (formNotice.FormNoticeOfMeetingId == 0)
            {
                formNotice.CreatedBy = currentUser.UserID;
                formNotice.Create_Date = DateTime.Now;
                formNotice.ModifiedBy = currentUser.UserID;
                formNotice.Update_Date = DateTime.Now;
                db.tblFormNoticeOfMeetings.Add(formNotice);
            }
            else
            {
                formNotice.ModifiedBy = currentUser.UserID;
                formNotice.Update_Date = DateTime.Now;
            }

            db.SaveChanges();
        }

        private void SaveParentConsent(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {

            tblFormParentConsent formParentConsent = db.tblFormParentConsents.Any(o => o.StudentId == sid) ? db.tblFormParentConsents.FirstOrDefault(o => o.StudentId == sid) : new tblFormParentConsent();

            formParentConsent.StudentId = sid;
            formParentConsent.School = GetInputValue("School", spans);
            formParentConsent.GiveConsent = GetCheckboxSingleInputValue("GiveConsent", checkboxes);
            formParentConsent.DoNotGiveConsent = GetCheckboxSingleInputValue("DoNotGiveConsent", checkboxes);
            formParentConsent.BeginDate = GetInputValueDate("BeginDate", spans);

            if (formParentConsent.FormParentConsentId == 0)
            {
                formParentConsent.CreatedBy = currentUser.UserID;
                formParentConsent.Create_Date = DateTime.Now;
                formParentConsent.ModifiedBy = currentUser.UserID;
                formParentConsent.Update_Date = DateTime.Now;
                db.tblFormParentConsents.Add(formParentConsent);
            }
            else
            {
                formParentConsent.ModifiedBy = currentUser.UserID;
                formParentConsent.Update_Date = DateTime.Now;
            }

            db.SaveChanges();

        }

        private void SavePhysicianScript(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {
            tblFormPhysicianScript formPhysicianScript = db.tblFormPhysicianScripts.Any(o => o.StudentId == sid) ? db.tblFormPhysicianScripts.FirstOrDefault(o => o.StudentId == sid) : new tblFormPhysicianScript();

            formPhysicianScript.StudentId = sid;
            formPhysicianScript.PhysicianName = GetInputValue("PhysicianName", spans);
            formPhysicianScript.FormDate = GetInputValueDate("FormDate", spans);

            if (formPhysicianScript.FormPhysicianScriptId == 0)
            {
                formPhysicianScript.CreatedBy = currentUser.UserID;
                formPhysicianScript.Create_Date = DateTime.Now;
                formPhysicianScript.ModifiedBy = currentUser.UserID;
                formPhysicianScript.Update_Date = DateTime.Now;
                db.tblFormPhysicianScripts.Add(formPhysicianScript);
            }
            else
            {
                formPhysicianScript.ModifiedBy = currentUser.UserID;
                formPhysicianScript.Update_Date = DateTime.Now;
            }

            db.SaveChanges();

        }

        private void SavePWNID(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {

            tblFormPriorWritten_Ident formPWN = db.tblFormPriorWritten_Ident.Any(o => o.StudentId == sid) ? db.tblFormPriorWritten_Ident.FirstOrDefault(o => o.StudentId == sid) : new tblFormPriorWritten_Ident();

            formPWN.StudentId = sid;
            formPWN.ParentName = GetInputValue("ParentName", spans);
            formPWN.FormDate = GetInputValueDate("FormDate", spans);
            formPWN.MeetingDate = GetInputValueDate("MeetingDate", spans);
            formPWN.ChildSPEDEligible = GetCheckboxSingleInputValue("ChildSPEDEligible", checkboxes);
            formPWN.SPEDNeeded = GetCheckboxSingleInputValue("SPEDNeeded", checkboxes);
            formPWN.SPEDPlacement = GetCheckboxSingleInputValue("SPEDPlacement", checkboxes);
            formPWN.ModificationsThatEnableSPED = GetCheckboxSingleInputValue("ModificationsThatEnableSPED", checkboxes);
            formPWN.Identification_Section = GetCheckboxSingleInputValue("Identification_Section", checkboxes);
            formPWN.ChildIsEligible = GetCheckboxSingleInputValue("ChildIsEligible", checkboxes);
            formPWN.ChildMeetsCriteria = GetCheckboxSingleInputValue("ChildMeetsCriteria", checkboxes);
            formPWN.SPEDNecessary = GetCheckboxSingleInputValue("SPEDNecessary", checkboxes);
            formPWN.ChildNotElgible = GetCheckboxSingleInputValue("ChildNotElgible", checkboxes);
            formPWN.ChildDoesNotMeetCriteria = GetCheckboxSingleInputValue("ChildDoesNotMeetCriteria", checkboxes);
            formPWN.SPEDNotNecessary = GetCheckboxSingleInputValue("SPEDNotNecessary", checkboxes);
            formPWN.InitialServices_Section = GetCheckboxSingleInputValue("InitialServices_Section", checkboxes);
            formPWN.ChangesInService_Section = GetCheckboxSingleInputValue("ChangesInService_Section", checkboxes);
            formPWN.ChangeInService = GetCheckboxSingleInputValue("ChangeInService", checkboxes);
            formPWN.MaterialChangeInService = GetCheckboxSingleInputValue("MaterialChangeInService", checkboxes);
            formPWN.ChangeInPlacement_Section = GetCheckboxSingleInputValue("ChangeInPlacement_Section", checkboxes);
            formPWN.ChangeInPlacements = GetCheckboxSingleInputValue("ChangeInPlacements", checkboxes);
            formPWN.SubstantialChangeInPlacement = GetCheckboxSingleInputValue("SubstantialChangeInPlacement", checkboxes);
            formPWN.OtherChanges = GetCheckboxSingleInputValue("OtherChanges", checkboxes);
            formPWN.LEARefusesToChangeIdentification = GetCheckboxSingleInputValue("LEARefusesToChangeIdentification", checkboxes);
            formPWN.DescriptionOfAction = GetInputValue("DescriptionOfAction", spans);
            formPWN.ExplaninationWhy = GetInputValue("ExplaninationWhy", spans);
            formPWN.OptionsConsidered = GetInputValue("OptionsConsidered", spans);
            formPWN.DescriptionOfData = GetInputValue("DescriptionOfData", spans);
            formPWN.OtherFactors = GetInputValue("OtherFactors", spans);
            formPWN.DeliveriedByWho = GetInputValue("DeliveriedByWho", spans);
            formPWN.DelieveredByHand = GetCheckboxSingleInputValue("DelieveredByHand", checkboxes);
            formPWN.DelieveredByMail = GetCheckboxSingleInputValue("DelieveredByMail", checkboxes);
            formPWN.DelieveredByOther = GetCheckboxSingleInputValue("DelieveredByOther", checkboxes);
            formPWN.DelieveredByOtherDesc = GetInputValue("DelieveredByOtherDesc", spans);
            formPWN.DeliveriedTo = GetInputValue("DeliveriedTo", spans);
            formPWN.DelieveredDate = GetInputValueDate("DelieveredDate", spans);
            formPWN.SchoolContact = GetInputValue("SchoolContact", spans);
            formPWN.SchoolContactAddress = GetInputValue("SchoolContactAddress", spans);
            formPWN.SchoolContactPhone = GetInputValue("SchoolContactPhone", spans);
            formPWN.GivenConsent = GetCheckboxSingleInputValue("GivenConsent", checkboxes);
            formPWN.RefuseConsent = GetCheckboxSingleInputValue("RefuseConsent", checkboxes);


            if (formPWN.FormPriorWritten_IdentId == 0)
            {
                formPWN.CreatedBy = currentUser.UserID;
                formPWN.Create_Date = DateTime.Now;
                formPWN.ModifiedBy = currentUser.UserID;
                formPWN.Update_Date = DateTime.Now;
                db.tblFormPriorWritten_Ident.Add(formPWN);
            }
            else
            {
                formPWN.ModifiedBy = currentUser.UserID;
                formPWN.Update_Date = DateTime.Now;
            }

            db.SaveChanges();

        }

        private void SavePWNEval(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {

            tblFormPriorWritten_Eval formPWNEval = db.tblFormPriorWritten_Eval.Any(o => o.StudentId == sid) ? db.tblFormPriorWritten_Eval.FirstOrDefault(o => o.StudentId == sid) : new tblFormPriorWritten_Eval();

            formPWNEval.StudentId = sid;
            formPWNEval.ParentName = GetInputValue("ParentName", spans);
            formPWNEval.FormDate = GetInputValueDate("FormDate", spans);

            formPWNEval.InitialEval = GetCheckboxSingleInputValue("InitialEval", checkboxes);
            formPWNEval.InitialEval_NoAdditional = GetCheckboxSingleInputValue("InitialEval_NoAdditional", checkboxes);
            formPWNEval.Reeval = GetCheckboxSingleInputValue("Reeval", checkboxes);
            formPWNEval.Reeval_NoAdditional = GetCheckboxSingleInputValue("Reeval_NoAdditional", checkboxes);
            formPWNEval.Refuse_InitialEval = GetCheckboxSingleInputValue("Refuse_InitialEval", checkboxes);
            formPWNEval.Refuse_Reeval = GetCheckboxSingleInputValue("Refuse_Reeval", checkboxes);
            formPWNEval.SchoolContact = GetInputValue("SchoolContact", spans);
            formPWNEval.SchoolContactPhone = GetInputValue("SchoolContactPhone", spans);
            formPWNEval.NewData_HealthMotor = GetCheckboxSingleInputValue("NewData_HealthMotor", checkboxes);
            formPWNEval.ExistingData_HealthMotor = GetCheckboxSingleInputValue("ExistingData_HealthMotor", checkboxes);
            formPWNEval.NewData_Vision = GetCheckboxSingleInputValue("NewData_Vision", checkboxes);
            formPWNEval.ExistingData_Vision = GetCheckboxSingleInputValue("ExistingData_Vision", checkboxes);
            formPWNEval.NewData_Hearing = GetCheckboxSingleInputValue("NewData_Hearing", checkboxes);
            formPWNEval.ExistingData_Hearing = GetCheckboxSingleInputValue("ExistingData_Hearing", checkboxes);
            formPWNEval.NewData_SEBStatus = GetCheckboxSingleInputValue("NewData_SEBStatus", checkboxes);
            formPWNEval.ExistingData_SEBStatus = GetCheckboxSingleInputValue("ExistingData_SEBStatus", checkboxes);
            formPWNEval.NewData_GenIntelligence = GetCheckboxSingleInputValue("NewData_GenIntelligence", checkboxes);
            formPWNEval.ExistingData_GenIntelligence = GetCheckboxSingleInputValue("ExistingData_GenIntelligence", checkboxes);
            formPWNEval.NewData_Academic = GetCheckboxSingleInputValue("NewData_Academic", checkboxes);
            formPWNEval.ExistingData_Academic = GetCheckboxSingleInputValue("ExistingData_Academic", checkboxes);
            formPWNEval.NewData_Communicative = GetCheckboxSingleInputValue("NewData_Communicative", checkboxes);
            formPWNEval.ExistingData_Communicative = GetCheckboxSingleInputValue("ExistingData_Communicative", checkboxes);
            formPWNEval.NewData_Transistion = GetCheckboxSingleInputValue("NewData_Transistion", checkboxes);
            formPWNEval.ExistingData_Transistion = GetCheckboxSingleInputValue("ExistingData_Transistion", checkboxes);
            formPWNEval.NewData_OtherData = GetCheckboxSingleInputValue("NewData_OtherData", checkboxes);
            formPWNEval.ExistingData_OtherData = GetCheckboxSingleInputValue("ExistingData_OtherData", checkboxes);
            formPWNEval.OtherData_Desc = GetInputValue("OtherData_Desc", spans);


            formPWNEval.ExplaninationWhy = GetInputValue("ExplaninationWhy", spans);
            formPWNEval.OptionsConsidered = GetInputValue("OptionsConsidered", spans);
            formPWNEval.DescriptionOfData = GetInputValue("DescriptionOfData", spans);
            formPWNEval.OtherFactors = GetInputValue("OtherFactors", spans);
            formPWNEval.DeliveriedByWho = GetInputValue("DeliveriedByWho", spans);
            formPWNEval.DelieveredByHand = GetCheckboxSingleInputValue("DelieveredByHand", checkboxes);
            formPWNEval.DelieveredByMail = GetCheckboxSingleInputValue("DelieveredByMail", checkboxes);
            formPWNEval.DelieveredByOther = GetCheckboxSingleInputValue("DelieveredByOther", checkboxes);
            formPWNEval.DelieveredByOtherDesc = GetInputValue("DelieveredByOtherDesc", spans);
            formPWNEval.DeliveriedTo = GetInputValue("DeliveriedTo", spans);
            formPWNEval.DelieveredDate = GetInputValueDate("DelieveredDate", spans);

            formPWNEval.GivenConsent = GetCheckboxSingleInputValue("GivenConsent", checkboxes);
            formPWNEval.RefuseConsent = GetCheckboxSingleInputValue("RefuseConsent", checkboxes);

            if (formPWNEval.FormPriorWritten_EvalId == 0)
            {
                formPWNEval.CreatedBy = currentUser.UserID;
                formPWNEval.Create_Date = DateTime.Now;
                formPWNEval.ModifiedBy = currentUser.UserID;
                formPWNEval.Update_Date = DateTime.Now;
                db.tblFormPriorWritten_Eval.Add(formPWNEval);
            }
            else
            {
                formPWNEval.ModifiedBy = currentUser.UserID;
                formPWNEval.Update_Date = DateTime.Now;
            }

            db.SaveChanges();
        }

        private void SavePWNRevAllServices(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {

            tblFormPriorWritten_ReokeAll formPWNRevAll = db.tblFormPriorWritten_ReokeAll.Any(o => o.StudentId == sid) ? db.tblFormPriorWritten_ReokeAll.FirstOrDefault(o => o.StudentId == sid) : new tblFormPriorWritten_ReokeAll();

            formPWNRevAll.StudentId = sid;
            formPWNRevAll.ParentName = GetInputValue("ParentName", spans);
            formPWNRevAll.FormDate = GetInputValueDate("FormDate", spans);
            formPWNRevAll.MeetingDate = GetInputValueDate("MeetingDate", spans);
            formPWNRevAll.EndDate = GetInputValueDate("EndDate", spans);
            formPWNRevAll.DeliveriedByWho = GetInputValue("DeliveriedByWho", spans);
            formPWNRevAll.DelieveredByHand = GetCheckboxSingleInputValue("DelieveredByHand", checkboxes);
            formPWNRevAll.DelieveredByMail = GetCheckboxSingleInputValue("DelieveredByMail", checkboxes);
            formPWNRevAll.DelieveredByOther = GetCheckboxSingleInputValue("DelieveredByOther", checkboxes);
            formPWNRevAll.DelieveredByOtherDesc = GetInputValue("DelieveredByOtherDesc", spans);
            formPWNRevAll.DeliveriedTo = GetInputValue("DeliveriedTo", spans);
            formPWNRevAll.DelieveredDate = GetInputValueDate("DelieveredDate", spans);

            if (formPWNRevAll.FormPriorWritten_ReokeAllId == 0)
            {
                formPWNRevAll.CreatedBy = currentUser.UserID;
                formPWNRevAll.Create_Date = DateTime.Now;
                formPWNRevAll.ModifiedBy = currentUser.UserID;
                formPWNRevAll.Update_Date = DateTime.Now;
                db.tblFormPriorWritten_ReokeAll.Add(formPWNRevAll);
            }
            else
            {
                formPWNRevAll.ModifiedBy = currentUser.UserID;
                formPWNRevAll.Update_Date = DateTime.Now;
            }
            db.SaveChanges();
        }

        private void SavePWNRevPartServices(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {


            tblFormPriorWritten_ReokePart formPWNRevPart = db.tblFormPriorWritten_ReokePart.Any(o => o.StudentId == sid) ? db.tblFormPriorWritten_ReokePart.FirstOrDefault(o => o.StudentId == sid) : new tblFormPriorWritten_ReokePart();

            formPWNRevPart.StudentId = sid;
            formPWNRevPart.ParentName = GetInputValue("ParentName", spans);
            formPWNRevPart.FormDate = GetInputValueDate("FormDate", spans);
            formPWNRevPart.SubmitDate = GetInputValueDate("SubmitDate", spans);
            formPWNRevPart.ActionTakenEndDate = GetInputValueDate("ActionTakenEndDate", spans);
            formPWNRevPart.ServicesRevoked = GetInputValue("ServicesRevoked", spans);

            formPWNRevPart.ActionTaken = GetCheckboxSingleInputValue("ActionTaken", checkboxes);
            formPWNRevPart.ActionTakenDescription = GetInputValue("ActionTakenDescription", spans);
            formPWNRevPart.ActionRefused = GetCheckboxSingleInputValue("ActionRefused", checkboxes);
            formPWNRevPart.ActionRefusedDescription = GetInputValue("ActionRefusedDescription", spans);
            formPWNRevPart.OptionsConsidered = GetInputValue("OptionsConsidered", spans);
            formPWNRevPart.DataUsed = GetInputValue("DataUsed", spans);
            formPWNRevPart.OtherFactors = GetInputValue("OtherFactors", spans);

            formPWNRevPart.DeliveriedByWho = GetInputValue("DeliveriedByWho", spans);
            formPWNRevPart.DelieveredByHand = GetCheckboxSingleInputValue("DelieveredByHand", checkboxes);
            formPWNRevPart.DelieveredByMail = GetCheckboxSingleInputValue("DelieveredByMail", checkboxes);
            formPWNRevPart.DelieveredByOther = GetCheckboxSingleInputValue("DelieveredByOther", checkboxes);
            formPWNRevPart.DelieveredByOtherDesc = GetInputValue("DelieveredByOtherDesc", spans);
            formPWNRevPart.DeliveriedTo = GetInputValue("DeliveriedTo", spans);
            formPWNRevPart.DelieveredDate = GetInputValueDate("DelieveredDate", spans);

            if (formPWNRevPart.FormPriorWritten_ReokePartId == 0)
            {
                formPWNRevPart.CreatedBy = currentUser.UserID;
                formPWNRevPart.Create_Date = DateTime.Now;
                formPWNRevPart.ModifiedBy = currentUser.UserID;
                formPWNRevPart.Update_Date = DateTime.Now;
                db.tblFormPriorWritten_ReokePart.Add(formPWNRevPart);
            }
            else
            {
                formPWNRevPart.ModifiedBy = currentUser.UserID;
                formPWNRevPart.Update_Date = DateTime.Now;
            }
            db.SaveChanges();
        }

        private void SaveRevAllServices(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {

            tblFormRevokeConsentAll formRevAll = db.tblFormRevokeConsentAlls.Any(o => o.StudentId == sid) ? db.tblFormRevokeConsentAlls.FirstOrDefault(o => o.StudentId == sid) : new tblFormRevokeConsentAll();

            formRevAll.StudentId = sid;
            formRevAll.AuthorityName = GetInputValue("AuthorityName", spans);
            formRevAll.FormDate = GetInputValueDate("FormDate", spans);
            formRevAll.RevokeConsentDate = GetInputValueDate("RevokeConsentDate", spans);
            formRevAll.OnBehalfOfStudent = GetCheckboxSingleInputValue("OnBehalfOfStudent", checkboxes);
            formRevAll.OnMyOwnBehalf = GetCheckboxSingleInputValue("OnMyOwnBehalf", checkboxes);

            if (formRevAll.FormRevokeConsentAllId == 0)
            {
                formRevAll.CreatedBy = currentUser.UserID;
                formRevAll.Create_Date = DateTime.Now;
                formRevAll.ModifiedBy = currentUser.UserID;
                formRevAll.Update_Date = DateTime.Now;
                db.tblFormRevokeConsentAlls.Add(formRevAll);
            }
            else
            {
                formRevAll.ModifiedBy = currentUser.UserID;
                formRevAll.Update_Date = DateTime.Now;
            }
            db.SaveChanges();
        }

        private void SaveRevPartServices(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {

            tblFormRevokeConsentPart formRevPart = db.tblFormRevokeConsentParts.Any(o => o.StudentId == sid) ? db.tblFormRevokeConsentParts.FirstOrDefault(o => o.StudentId == sid) : new tblFormRevokeConsentPart();

            formRevPart.StudentId = sid;
            formRevPart.AuthorityName = GetInputValue("AuthorityName", spans);
            formRevPart.RepresenativeName = GetInputValue("RepresenativeName", spans);
            formRevPart.RevokedServices = GetInputValue("RevokedServices", spans);
            formRevPart.EffectiveDate = GetInputValueDate("EffectiveDate", spans);
            formRevPart.StudentMeets = GetCheckboxSingleInputValue("StudentMeets", checkboxes);
            formRevPart.StudentDoesNotMeet = GetCheckboxSingleInputValue("StudentDoesNotMeet", checkboxes);
            formRevPart.OnBehalfOfStudent = GetCheckboxSingleInputValue("OnBehalfOfStudent", checkboxes);
            formRevPart.OnMyOwnBehalf = GetCheckboxSingleInputValue("OnMyOwnBehalf", checkboxes);

            if (formRevPart.FormRevokeConsentPartId == 0)
            {
                formRevPart.CreatedBy = currentUser.UserID;
                formRevPart.Create_Date = DateTime.Now;
                formRevPart.ModifiedBy = currentUser.UserID;
                formRevPart.Update_Date = DateTime.Now;
                db.tblFormRevokeConsentParts.Add(formRevPart);
            }
            else
            {
                formRevPart.ModifiedBy = currentUser.UserID;
                formRevPart.Update_Date = DateTime.Now;
            }
            db.SaveChanges();
        }

        private void SaveTransportation(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {

            tblFormTransportationRequest formTransRequest = db.tblFormTransportationRequests.Any(o => o.StudentId == sid) ? db.tblFormTransportationRequests.FirstOrDefault(o => o.StudentId == sid) : new tblFormTransportationRequest();

            formTransRequest.StudentId = sid;
            formTransRequest.FormDate = GetInputValueDate("FormDate", spans);
            formTransRequest.BeginDate = GetInputValueDate("BeginDate", spans);
            formTransRequest.EndDate = GetInputValueDate("EndDate", spans);

            formTransRequest.StudentName = GetInputValue("StudentName", spans);

            formTransRequest.USD = GetInputValue("USD", spans);
            formTransRequest.School = GetInputValue("School", spans);
            formTransRequest.Grade = GetInputValue("Grade", spans);
            formTransRequest.DateOfBirth = GetInputValueDate("DateOfBirth", spans);
            formTransRequest.ReceivingUSD = GetInputValue("ReceivingUSD", spans);
            formTransRequest.TransportationDirector = GetInputValue("TransportationDirector", spans);
            formTransRequest.TransportationDirectorPhone = GetInputValue("TransportationDirectorPhone", spans);
            formTransRequest.ReceivingTeacherAndProgram = GetInputValue("ReceivingTeacherAndProgram", spans);

            string hours = GetInputValue("Hours", spans);
            if (!string.IsNullOrEmpty(hours))
            {
                decimal.TryParse(hours, out decimal hourVal);
                formTransRequest.Hours = hourVal;
            }

            formTransRequest.Contact_1_Name = GetInputValue("Contact_1_Name", spans);
            formTransRequest.Contact_1_HomePhone = GetInputValue("Contact_1_HomePhone", spans);
            formTransRequest.Contact_1_WorkPhone = GetInputValue("Contact_1_WorkPhone", spans);
            formTransRequest.Contact_2_Name = GetInputValue("Contact_2_Name", spans);
            formTransRequest.Contact_2_HomePhone = GetInputValue("Contact_2_HomePhone", spans);
            formTransRequest.Contact_2_WorkPhone = GetInputValue("Contact_2_WorkPhone", spans);
            formTransRequest.BabySitterDaycareNameAndPhone = GetInputValue("BabySitterDaycareNameAndPhone", spans);
            formTransRequest.HomeAddress = GetInputValue("HomeAddress", spans);
            formTransRequest.FamilyPhysicianAndHosptial = GetInputValue("FamilyPhysicianAndHosptial", spans);
            formTransRequest.PickupLocation = GetInputValue("PickupLocation", spans);
            formTransRequest.ReturnLocation = GetInputValue("ReturnLocation", spans);
            formTransRequest.WheelChair = GetCheckboxSingleInputValue("WheelChair", checkboxes);
            formTransRequest.CarSeat = GetCheckboxSingleInputValue("CarSeat", checkboxes);
            formTransRequest.SeatBelt = GetCheckboxSingleInputValue("SeatBelt", checkboxes);
            formTransRequest.ChestHarness = GetCheckboxSingleInputValue("ChestHarness", checkboxes);
            formTransRequest.BusLift = GetCheckboxSingleInputValue("BusLift", checkboxes);
            formTransRequest.BoosterSeat = GetCheckboxSingleInputValue("BoosterSeat", checkboxes);
            formTransRequest.Tray = GetCheckboxSingleInputValue("Tray", checkboxes);
            formTransRequest.PersonalCareAttendant = GetCheckboxSingleInputValue("PersonalCareAttendant", checkboxes);
            formTransRequest.AdductorInPlace = GetCheckboxSingleInputValue("AdductorInPlace", checkboxes);
            formTransRequest.Communication = GetCheckboxSingleInputValue("Communication", checkboxes);
            formTransRequest.Other = GetCheckboxSingleInputValue("Other", checkboxes);
            formTransRequest.Other_Desc = GetInputValue("Other_Desc", spans);
            formTransRequest.Documentation = GetInputValue("Documentation", spans);
            formTransRequest.PositioningAndHandling = GetInputValue("PositioningAndHandling", spans);
            formTransRequest.MeidicationAndSideEffects = GetInputValue("MeidicationAndSideEffects", spans);
            formTransRequest.Equipment = GetInputValue("Equipment", spans);


            if (formTransRequest.FormTransportationRequestId == 0)
            {
                formTransRequest.CreatedBy = currentUser.UserID;
                formTransRequest.Create_Date = DateTime.Now;
                formTransRequest.ModifiedBy = currentUser.UserID;
                formTransRequest.Update_Date = DateTime.Now;
                db.tblFormTransportationRequests.Add(formTransRequest);
            }
            else
            {
                formTransRequest.ModifiedBy = currentUser.UserID;
                formTransRequest.Update_Date = DateTime.Now;
            }
            db.SaveChanges();

        }

        private void SaveILP(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {

            tblFormContinuousLearningPlan formICLP = db.tblFormContinuousLearningPlans.Any(o => o.StudentId == sid) ? db.tblFormContinuousLearningPlans.FirstOrDefault(o => o.StudentId == sid) : new tblFormContinuousLearningPlan();
            formICLP.StudentId = sid;
            formICLP.ICLPDate = GetInputValueDate("ICLPDate", spans);
            formICLP.EffectiveDate = GetInputValueDate("EffectiveDate", spans);
            formICLP.EndingDate = GetInputValueDate("EndingDate", spans);
            formICLP.StudentName = GetInputValue("StudentName", spans);
            formICLP.ResponsibleBuilding = GetInputValue("ResponsibleBuilding", spans);
            formICLP.Grade = GetInputValue("Grade", spans);
            formICLP.PrimaryDisability = GetInputValue("PrimaryDisability", spans);
            formICLP.Provider = GetInputValue("Provider", spans);
            formICLP.AttendingBuilding = GetInputValue("AttendingBuilding", spans);
            formICLP.District = GetInputValue("District", spans);
            formICLP.DateOfBirth = GetInputValueDate("DateOfBirth", spans);
            formICLP.EvaluationCompletion = GetInputValueDate("EvaluationCompletion", spans);
            formICLP.IEPDate = GetInputValueDate("IEPDate", spans);
            formICLP.AccessToInternetBasedActivities_Yes = GetCheckboxSingleInputValue("AccessToInternetBasedActivities_Yes", checkboxes);
            formICLP.AccessToInternetBasedActivities_No = GetCheckboxSingleInputValue("AccessToInternetBasedActivities_No", checkboxes);
            formICLP.AccessToInternetBasedActivities_Home = GetCheckboxSingleInputValue("AccessToInternetBasedActivities_Home", checkboxes);
            formICLP.AccessToInternetBasedActivities_HotSpot = GetCheckboxSingleInputValue("AccessToInternetBasedActivities_HotSpot", checkboxes);
            formICLP.AccessToServiceDelivery_Yes = GetCheckboxSingleInputValue("AccessToServiceDelivery_Yes", checkboxes);
            formICLP.AccessToServiceDelivery_No = GetCheckboxSingleInputValue("AccessToServiceDelivery_No", checkboxes);
            formICLP.AccessToEmailCommunication_Yes = GetCheckboxSingleInputValue("AccessToEmailCommunication_Yes", checkboxes);
            formICLP.AccessToEmailCommunication_No = GetCheckboxSingleInputValue("AccessToEmailCommunication_No", checkboxes);
            formICLP.AccessToWorkPacket_Yes = GetCheckboxSingleInputValue("AccessToWorkPacket_Yes", checkboxes);
            formICLP.AccessToWorkPacket_No = GetCheckboxSingleInputValue("AccessToWorkPacket_No", checkboxes);
            formICLP.AccessToWorkPacket_DateProvided = GetInputValue("AccessToWorkPacket_DateProvided", spans);
            formICLP.AccessToWorkPacket_Method = GetInputValue("AccessToWorkPacket_Method", spans);
            formICLP.ServicesOffered = GetCheckboxSingleInputValue("ServicesOffered", checkboxes);
            formICLP.ServicesAccepted = GetCheckboxSingleInputValue("ServicesAccepted", checkboxes);
            formICLP.ServicesDeclineded = GetCheckboxSingleInputValue("ServicesDeclineded", checkboxes);
            formICLP.Accommodations_HasNoCurrent = GetCheckboxSingleInputValue("Accommodations_HasNoCurrent", checkboxes);
            formICLP.Accommodations_OfferedAndDeclined = GetCheckboxSingleInputValue("Accommodations_OfferedAndDeclined", checkboxes);
            formICLP.Accommodation_Description1 = GetInputValue("Accommodation_Description1", spans);
            formICLP.Accommodation_Implementation1 = GetInputValue("Accommodation_Implementation1", spans);
            formICLP.Accommodation_Frequency1 = GetInputValue("Accommodation_Frequency1", spans);
            formICLP.Accommodation_Description2 = GetInputValue("Accommodation_Description2", spans);
            formICLP.Accommodation_Implementation2 = GetInputValue("Accommodation_Implementation2", spans);
            formICLP.Accommodation_Frequency2 = GetInputValue("Accommodation_Frequency2", spans);
            formICLP.Accommodation_Description3 = GetInputValue("Accommodation_Description3", spans);
            formICLP.Accommodation_Implementation3 = GetInputValue("Accommodation_Implementation3", spans);
            formICLP.Accommodation_Frequency3 = GetInputValue("Accommodation_Frequency3", spans);
            formICLP.Accommodation_Description4 = GetInputValue("Accommodation_Description4", spans);
            formICLP.Accommodation_Implementation4 = GetInputValue("Accommodation_Implementation4", spans);
            formICLP.Accommodation_Frequency4 = GetInputValue("Accommodation_Frequency4", spans);
            formICLP.Services_OfferedAndDeclined = GetCheckboxSingleInputValue("Services_OfferedAndDeclined", checkboxes);
            formICLP.Services_ServiceProvided1 = GetInputValue("Services_ServiceProvided1", spans);
            formICLP.Services_Setting1 = GetInputValue("Services_Setting1", spans);
            formICLP.Services_Subject1 = GetInputValue("Services_Subject1", spans);
            formICLP.Services_Minutes1 = GetInputValue("Services_Minutes1", spans);
            formICLP.Services_Frequency1 = GetInputValue("Services_Frequency1", spans);
            formICLP.Services_StartDate1 = GetInputValueDate("Services_StartDate1", spans);
            formICLP.Services_EndDate1 = GetInputValueDate("Services_EndDate1", spans);
            formICLP.Services_ServiceProvided2 = GetInputValue("Services_ServiceProvided2", spans);
            formICLP.Services_Setting2 = GetInputValue("Services_Setting2", spans);
            formICLP.Services_Subject2 = GetInputValue("Services_Subject2", spans);
            formICLP.Services_Minutes2 = GetInputValue("Services_Minutes2", spans);
            formICLP.Services_Frequency2 = GetInputValue("Services_Frequency2", spans);
            formICLP.Services_StartDate2 = GetInputValueDate("Services_StartDate2", spans);
            formICLP.Services_EndDate2 = GetInputValueDate("Services_EndDate2", spans);
            formICLP.Provider_WillContactPhone = GetCheckboxSingleInputValue("Provider_WillContactPhone", checkboxes);
            formICLP.Provider_WillContactPhone_Weekly = GetCheckboxSingleInputValue("Provider_WillContactPhone_Weekly", checkboxes);
            formICLP.Provider_WillContactPhone_Biweekly = GetCheckboxSingleInputValue("Provider_WillContactPhone_Biweekly", checkboxes);
            formICLP.Provider_WillContactEmail = GetCheckboxSingleInputValue("Provider_WillContactEmail", checkboxes);
            formICLP.Provider_WillContactEmail_OnceWeek = GetCheckboxSingleInputValue("Provider_WillContactEmail_OnceWeek", checkboxes);
            formICLP.Provider_WillContactEmail_TwiceWeek = GetCheckboxSingleInputValue("Provider_WillContactEmail_TwiceWeek", checkboxes);
            formICLP.Provider_WillContactEmail_Biweekly = GetCheckboxSingleInputValue("Provider_WillContactEmail_Biweekly", checkboxes);
            formICLP.Provider_ParentsContact = GetCheckboxSingleInputValue("Provider_ParentsContact", checkboxes);
            formICLP.Goals_OfferedAndDeclined = GetCheckboxSingleInputValue("Goals_OfferedAndDeclined", checkboxes);
            formICLP.Goals_Number1 = GetInputValue("Goals_Number1", spans);
            formICLP.Goal_TrackProgress_Engagement1 = GetCheckboxSingleInputValue("Goal_TrackProgress_Engagement1", checkboxes);
            formICLP.Goal_TrackProgress_Feedback1 = GetCheckboxSingleInputValue("Goal_TrackProgress_Feedback1", checkboxes);
            formICLP.Goal_TrackProgress_Other1 = GetCheckboxSingleInputValue("Goal_TrackProgress_Other1", checkboxes);
            formICLP.Goals_Number2 = GetInputValue("Goals_Number2", spans);
            formICLP.Goal_TrackProgress_Engagement2 = GetCheckboxSingleInputValue("Goal_TrackProgress_Engagement2", checkboxes);
            formICLP.Goal_TrackProgress_Feedback2 = GetCheckboxSingleInputValue("Goal_TrackProgress_Feedback2", checkboxes);
            formICLP.Goal_TrackProgress_Other2 = GetCheckboxSingleInputValue("Goal_TrackProgress_Other2", checkboxes);
            formICLP.Goals_Number3 = GetInputValue("Goals_Number3", spans);
            formICLP.Goal_TrackProgress_Engagement3 = GetCheckboxSingleInputValue("Goal_TrackProgress_Engagement3", checkboxes);
            formICLP.Goal_TrackProgress_Feedback3 = GetCheckboxSingleInputValue("Goal_TrackProgress_Feedback3", checkboxes);
            formICLP.Goal_TrackProgress_Other3 = GetCheckboxSingleInputValue("Goal_TrackProgress_Other3", checkboxes);
            formICLP.Goals_Number4 = GetInputValue("Goals_Number4", spans);
            formICLP.Goal_TrackProgress_Engagement4 = GetCheckboxSingleInputValue("Goal_TrackProgress_Engagement4", checkboxes);
            formICLP.Goal_TrackProgress_Feedback4 = GetCheckboxSingleInputValue("Goal_TrackProgress_Feedback4", checkboxes);
            formICLP.Goal_TrackProgress_Other4 = GetCheckboxSingleInputValue("Goal_TrackProgress_Other4", checkboxes);
            formICLP.ActivitiesOfferedToEnableAccess = GetInputValue("ActivitiesOfferedToEnableAccess", spans);
            formICLP.ProviderName = GetInputValue("ProviderName", spans);

            formICLP.Goals_Statement1 = GetInputValue("Goals_Statement1", spans);
            formICLP.Goals_Statement2 = GetInputValue("Goals_Statement2", spans);
            formICLP.Goals_Statement3 = GetInputValue("Goals_Statement3", spans);
            formICLP.Goals_Statement4 = GetInputValue("Goals_Statement4", spans);

            if (formICLP.FormContinuousLearningPlanId == 0)
            {
                formICLP.CreatedBy = currentUser.UserID;
                formICLP.Create_Date = DateTime.Now;
                formICLP.ModifiedBy = currentUser.UserID;
                formICLP.Update_Date = DateTime.Now;
                db.tblFormContinuousLearningPlans.Add(formICLP);
            }
            else
            {
                formICLP.ModifiedBy = currentUser.UserID;
                formICLP.Update_Date = DateTime.Now;
            }
            db.SaveChanges();

        }

        private void SaveTransitionReferral(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {
            tblFormTransitionReferral formTransRefer = db.tblFormTransitionReferrals.Any(o => o.StudentId == sid) ? db.tblFormTransitionReferrals.FirstOrDefault(o => o.StudentId == sid) : new tblFormTransitionReferral();
            formTransRefer.StudentId = sid;
            formTransRefer.FormDate = GetInputValueDate("FormDate", spans);
            formTransRefer.From_School = GetInputValue("From_School", spans);
            formTransRefer.From_Address = GetInputValue("From_Address", spans);
            formTransRefer.From_Phone = GetInputValue("From_Phone", spans);
            formTransRefer.From_ResposibleLEAStaff = GetInputValue("From_ResposibleLEAStaff", spans);
            formTransRefer.To_LocalRegabilitationOffice = GetInputValue("To_LocalRegabilitationOffice", spans);
            formTransRefer.To_Address = GetInputValue("To_Address", spans);
            formTransRefer.To_Phone = GetInputValue("To_Phone", spans);
            formTransRefer.To_ATTN = GetInputValue("To_ATTN", spans);
            formTransRefer.Student_Name = GetInputValue("Student_Name", spans);
            formTransRefer.Student_Address = GetInputValue("Student_Address", spans);
            formTransRefer.Student_Phone = GetInputValue("Student_Phone", spans);
            formTransRefer.Student_DOB = GetInputValue("Student_DOB", spans);
            formTransRefer.Student_ExitDate = GetInputValue("Student_ExitDate", spans);

            if (formTransRefer.FormTransitionReferralID == 0)
            {
                formTransRefer.CreatedBy = currentUser.UserID;
                formTransRefer.Create_Date = DateTime.Now;
                formTransRefer.ModifiedBy = currentUser.UserID;
                formTransRefer.Update_Date = DateTime.Now;
                db.tblFormTransitionReferrals.Add(formTransRefer);
            }
            else
            {
                formTransRefer.ModifiedBy = currentUser.UserID;
                formTransRefer.Update_Date = DateTime.Now;
            }
            db.SaveChanges();

        }

        private void SaveChildOutcomeSummary(tblUser currentUser, int sid, List<HtmlNode> spans, List<HtmlNode> checkboxes)
        {
            tblFormChildOutcome formChild = db.tblFormChildOutcomes.Any(o => o.StudentId == sid) ? db.tblFormChildOutcomes.FirstOrDefault(o => o.StudentId == sid) : new tblFormChildOutcome();
            formChild.StudentId = sid;
            formChild.RatingDate = GetInputValueDate("RatingDate", spans);
            formChild.DateOfBirth = GetInputValueDate("DateOfBirth", spans);
            formChild.ServiceDate = GetInputValueDate("ServiceDate", spans);
            formChild.ServiceEndDate = GetInputValueDate("ServiceEndDate", spans);
            formChild.FirstName = GetInputValue("FirstName", spans);
            formChild.MiddleName = GetInputValue("MiddleName", spans);
            formChild.LastName = GetInputValue("LastName", spans);
            long kidsId = 0;
            var kidIdStr = GetInputValue("KIDSID", spans);
            long.TryParse(kidIdStr, out kidsId);
            formChild.KIDSID = kidsId;

            formChild.FamilyInfo_ReceivedInTeamMeeting = GetCheckboxSingleInputValue("FamilyInfo_ReceivedInTeamMeeting", checkboxes);
            formChild.FamilyInfo_CollectedSeperately = GetCheckboxSingleInputValue("FamilyInfo_CollectedSeperately", checkboxes);
            formChild.FamilyInfo_IncoporatedIntoAssessment = GetCheckboxSingleInputValue("FamilyInfo_IncoporatedIntoAssessment", checkboxes);
            formChild.FamilyInfo_NotIncluded = GetCheckboxSingleInputValue("FamilyInfo_NotIncluded", checkboxes);
            formChild.SocialEmotional_ShownNewBehaviors_Yes = GetCheckboxSingleInputValue("SocialEmotional_ShownNewBehaviors_Yes", checkboxes);
            formChild.SocialEmotional_ShownNewBehaviors_No = GetCheckboxSingleInputValue("SocialEmotional_ShownNewBehaviors_No", checkboxes);
            formChild.SocialEmotional_ShownNewBehaviors_YesDescription = GetInputValue("SocialEmotional_ShownNewBehaviors_YesDescription", spans);
            formChild.AquireUsing_ShownNewBehaviors_Yes = GetCheckboxSingleInputValue("AquireUsing_ShownNewBehaviors_Yes", checkboxes);
            formChild.AquireUsing_ShownNewBehaviors_No = GetCheckboxSingleInputValue("AquireUsing_ShownNewBehaviors_No", checkboxes);
            formChild.AquireUsing_ShownNewBehaviors_YesDescription = GetInputValue("AquireUsing_ShownNewBehaviors_YesDescription", spans);
            formChild.AppropriateAction_ShownNewBehaviors_Yes = GetCheckboxSingleInputValue("AppropriateAction_ShownNewBehaviors_Yes", checkboxes);
            formChild.AppropriateAction_ShownNewBehaviors_No = GetCheckboxSingleInputValue("AppropriateAction_ShownNewBehaviors_No", checkboxes);
            formChild.AppropriateAction_ShownNewBehaviors_YesDescription = GetInputValue("AppropriateAction_ShownNewBehaviors_YesDescription", spans);


            var ageAppropriate1 = GetCheckboxSingleInputValue("SocialEmotional_ShowAgeAppropriateBehavior_1", checkboxes);
            var ageAppropriate2 = GetCheckboxSingleInputValue("SocialEmotional_ShowAgeAppropriateBehavior_2", checkboxes);
            var ageAppropriate3 = GetCheckboxSingleInputValue("SocialEmotional_ShowAgeAppropriateBehavior_3", checkboxes);
            var ageAppropriate4 = GetCheckboxSingleInputValue("SocialEmotional_ShowAgeAppropriateBehavior_4", checkboxes);
            var ageAppropriate5 = GetCheckboxSingleInputValue("SocialEmotional_ShowAgeAppropriateBehavior_5", checkboxes);
            var ageAppropriate6 = GetCheckboxSingleInputValue("SocialEmotional_ShowAgeAppropriateBehavior_6", checkboxes);
            var ageAppropriate7 = GetCheckboxSingleInputValue("SocialEmotional_ShowAgeAppropriateBehavior_7", checkboxes);

            if (ageAppropriate1)
                formChild.SocialEmotional_ShowAgeAppropriateBehavior = 1;

            if (ageAppropriate2)
                formChild.SocialEmotional_ShowAgeAppropriateBehavior = 2;

            if (ageAppropriate3)
                formChild.SocialEmotional_ShowAgeAppropriateBehavior = 3;

            if (ageAppropriate4)
                formChild.SocialEmotional_ShowAgeAppropriateBehavior = 4;

            if (ageAppropriate5)
                formChild.SocialEmotional_ShowAgeAppropriateBehavior = 5;

            if (ageAppropriate6)
                formChild.SocialEmotional_ShowAgeAppropriateBehavior = 6;

            if (ageAppropriate7)
                formChild.SocialEmotional_ShowAgeAppropriateBehavior = 7;

            ageAppropriate1 = GetCheckboxSingleInputValue("AquireUsing_ShowAgeAppropriateBehavior_1", checkboxes);
            ageAppropriate2 = GetCheckboxSingleInputValue("AquireUsing_ShowAgeAppropriateBehavior_2", checkboxes);
            ageAppropriate3 = GetCheckboxSingleInputValue("AquireUsing_ShowAgeAppropriateBehavior_3", checkboxes);
            ageAppropriate4 = GetCheckboxSingleInputValue("AquireUsing_ShowAgeAppropriateBehavior_4", checkboxes);
            ageAppropriate5 = GetCheckboxSingleInputValue("AquireUsing_ShowAgeAppropriateBehavior_5", checkboxes);
            ageAppropriate6 = GetCheckboxSingleInputValue("AquireUsing_ShowAgeAppropriateBehavior_6", checkboxes);
            ageAppropriate7 = GetCheckboxSingleInputValue("AquireUsing_ShowAgeAppropriateBehavior_7", checkboxes);

            if (ageAppropriate1)
                formChild.AquireUsing_ShowAgeAppropriateBehavior = 1;

            if (ageAppropriate2)
                formChild.AquireUsing_ShowAgeAppropriateBehavior = 2;

            if (ageAppropriate3)
                formChild.AquireUsing_ShowAgeAppropriateBehavior = 3;

            if (ageAppropriate4)
                formChild.AquireUsing_ShowAgeAppropriateBehavior = 4;

            if (ageAppropriate5)
                formChild.AquireUsing_ShowAgeAppropriateBehavior = 5;

            if (ageAppropriate6)
                formChild.AquireUsing_ShowAgeAppropriateBehavior = 6;

            if (ageAppropriate7)
                formChild.AquireUsing_ShowAgeAppropriateBehavior = 7;

            ageAppropriate1 = GetCheckboxSingleInputValue("AppropriateAction_ShowAgeAppropriateBehavior_1", checkboxes);
            ageAppropriate2 = GetCheckboxSingleInputValue("AppropriateAction_ShowAgeAppropriateBehavior_2", checkboxes);
            ageAppropriate3 = GetCheckboxSingleInputValue("AppropriateAction_ShowAgeAppropriateBehavior_3", checkboxes);
            ageAppropriate4 = GetCheckboxSingleInputValue("AppropriateAction_ShowAgeAppropriateBehavior_4", checkboxes);
            ageAppropriate5 = GetCheckboxSingleInputValue("AppropriateAction_ShowAgeAppropriateBehavior_5", checkboxes);
            ageAppropriate6 = GetCheckboxSingleInputValue("AppropriateAction_ShowAgeAppropriateBehavior_6", checkboxes);
            ageAppropriate7 = GetCheckboxSingleInputValue("AppropriateAction_ShowAgeAppropriateBehavior_7", checkboxes);

            if (ageAppropriate1)
                formChild.AppropriateAction_ShowAgeAppropriateBehavior = 1;

            if (ageAppropriate2)
                formChild.AppropriateAction_ShowAgeAppropriateBehavior = 2;

            if (ageAppropriate3)
                formChild.AppropriateAction_ShowAgeAppropriateBehavior = 3;

            if (ageAppropriate4)
                formChild.AppropriateAction_ShowAgeAppropriateBehavior = 4;

            if (ageAppropriate5)
                formChild.AppropriateAction_ShowAgeAppropriateBehavior = 5;

            if (ageAppropriate6)
                formChild.AppropriateAction_ShowAgeAppropriateBehavior = 6;

            if (ageAppropriate7)
                formChild.AppropriateAction_ShowAgeAppropriateBehavior = 7;

            if (formChild.FormChildOutcomeID == 0)
            {
                formChild.CreatedBy = currentUser.UserID;
                formChild.Create_Date = DateTime.Now;
                formChild.ModifiedBy = currentUser.UserID;
                formChild.Update_Date = DateTime.Now;
                db.tblFormChildOutcomes.Add(formChild);
            }
            else
            {
                formChild.ModifiedBy = currentUser.UserID;
                formChild.Update_Date = DateTime.Now;
            }
            db.SaveChanges();

            foreach (tblFormChildOutcomes_PersonsInvolved existingPD in db.tblFormChildOutcomes_PersonsInvolved.Where(o => o.FormChildOutcomeID == formChild.FormChildOutcomeID))
            {
                db.tblFormChildOutcomes_PersonsInvolved.Remove(existingPD);
            }

            db.SaveChanges();

            //add back
            List<HtmlNode> personSpans = spans.Where(o => o.HasClass("person")).ToList();
            foreach (HtmlNode mp in personSpans.Where(o => o.HasClass("personName")))
            {
                string elementId = mp.Id;  // = memberPresentTitle_0"
                string[] elementSplit = elementId.Split('_');
                if (elementSplit.Length > 1)
                {

                    HtmlNode elementTitle = null;
                    elementTitle = personSpans.Where(o => o.Id == "personRole_" + elementSplit[1]).FirstOrDefault();

                    tblFormChildOutcomes_PersonsInvolved personsInvolved = new tblFormChildOutcomes_PersonsInvolved()
                    {
                        Name = mp.InnerHtml,
                        Role = elementTitle != null ? elementTitle.InnerHtml : "",
                        FormChildOutcomeID = formChild.FormChildOutcomeID,
                        CreatedBy = currentUser.UserID,
                        Create_Date = DateTime.Now,
                        Update_Date = DateTime.Now

                    };

                    if (!string.IsNullOrEmpty(personsInvolved.Name))
                    {
                        db.tblFormChildOutcomes_PersonsInvolved.Add(personsInvolved);
                    }
                }
            }

            foreach (tblFormChildOutcomes_SupportingEvidence existingSE in db.tblFormChildOutcomes_SupportingEvidence.Where(o => o.FormChildOutcomeID == formChild.FormChildOutcomeID))
            {
                db.tblFormChildOutcomes_SupportingEvidence.Remove(existingSE);
            }

            db.SaveChanges();

            //add back
            List<HtmlNode> evidenceSpans = spans.Where(o => o.HasClass("evidence")).ToList();
            foreach (HtmlNode es in evidenceSpans.Where(o => o.HasClass("evidenceName")))
            {
                string elementId = es.Id;
                string[] elementSplit = elementId.Split('_');
                if (elementSplit.Length > 1)
                {
                    string questionType = es.HasClass("3A") ? "3A_" : es.HasClass("1A") ? "1A_" : "2A_";
                    HtmlNode elementDate = evidenceSpans.Where(o => o.Id == "evidenceDate_" + elementSplit[1]).FirstOrDefault();
                    HtmlNode elementResult = evidenceSpans.Where(o => o.Id == "evidenceResult_" + elementSplit[1]).FirstOrDefault();

                    tblFormChildOutcomes_SupportingEvidence evidenceObj = new tblFormChildOutcomes_SupportingEvidence()
                    {
                        Source = es.InnerHtml,
                        FormChildOutcomeID = formChild.FormChildOutcomeID,
                        CreatedBy = currentUser.UserID,
                        Question = questionType,
                        Date = elementDate != null && !string.IsNullOrEmpty(elementDate.InnerHtml) ? (DateTime?)Convert.ToDateTime(elementDate.InnerHtml) : null,
                        SummaryOfResults = elementResult != null ? elementResult.InnerHtml : "",
                        Create_Date = DateTime.Now,
                        Update_Date = DateTime.Now
                    };

                    db.tblFormChildOutcomes_SupportingEvidence.Add(evidenceObj);

                }
            }

            db.SaveChanges();

        }

        private DateTime? GetInputValueDate(string inputName, List<HtmlNode> inputs)
        {
            string dateStr = GetInputValue(inputName, inputs);

            if (!string.IsNullOrEmpty(dateStr))
            {
                DateTime dt = DateTime.MinValue;
                DateTime.TryParse(dateStr, out dt);
                if (dt != DateTime.MinValue)
                {
                    return dt;
                }
            }

            return null;
        }

        private string GetInputValue(string inputName, List<HtmlNode> inputs)
        {
            HtmlNode input = inputs.Where(o => o.Id == inputName).FirstOrDefault();
            if (input != null)
            {
                return input.InnerHtml != null ? input.InnerHtml.Replace("&nbsp;", "") : "";
            }
            else
            {
                return "";
            }
        }

        private int? GetRadioInputValue(string inputName, List<HtmlNode> inputs)
        {
            List<HtmlNode> radioButtons = inputs.Where(o => o.Id == inputName).ToList();
            int? selectedVal = null;

            foreach (var input in radioButtons)
            {
                if (input != null && input.Attributes["data-val"] != null && input.InnerHtml.Contains("radio-yes"))
                {
                    int val = 0;
                    Int32.TryParse(input.Attributes["data-val"].Value, out val);
                    selectedVal = val;
                }
            }

            return selectedVal;
        }

        private DateTime? GetInputDateValue(string inputName, List<HtmlNode> inputs)
        {
            HtmlNode input = inputs.Where(o => o.Id == inputName).FirstOrDefault();
            if (input != null)
            {
                if (DateTime.TryParse(input.InnerHtml, out DateTime dtVal))
                {
                    return dtVal;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private bool? GetCheckboxInputValue(string inputName, string inputName2, List<HtmlNode> checkboxes)
        {
            bool? returnValue = null;
            string valYes = "";
            string valNo = "";

            HtmlNode input = checkboxes.Where(o => o.Id == inputName).FirstOrDefault();
            if (input != null)
            {
                if (input.Name == "span")
                {
                    valYes = input.InnerHtml != null && input.InnerHtml.Contains("[X]") ? "Y" : "";
                }
                else
                {
                    valYes = input.OuterHtml != null && input.OuterHtml.Contains("check_yes") ? "Y" : "";
                }
            }

            HtmlNode input2 = checkboxes.Where(o => o.Id == inputName2).FirstOrDefault();
            if (input2 != null)
            {
                if (input2.Name == "span")
                {
                    valNo = input2.InnerHtml != null && input2.InnerHtml.Contains("[X]") ? "Y" : "";
                }
                else
                {
                    valNo = input2.OuterHtml != null && input2.OuterHtml.Contains("check_yes") ? "Y" : "";
                }
            }

            if (valYes == "Y")
            {
                returnValue = true;
            }
            else if (valNo == "Y")
            {
                returnValue = false;
            }

            return returnValue;

        }

        private bool GetCheckboxSingleInputValue(string inputName, List<HtmlNode> checkboxes)
        {

            string valYes = "";


            HtmlNode input = checkboxes.Where(o => o.Id == inputName).FirstOrDefault();
            if (input != null)
            {

                if (input.Name == "span")
                {
                    valYes = input.InnerHtml != null && input.InnerHtml.Contains("[X]") ? "Y" : "";
                }
                else
                {
                    valYes = input.OuterHtml != null && input.OuterHtml.Contains("check_yes") ? "Y" : "";
                }
            }

            if (valYes == "Y")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

    }
}