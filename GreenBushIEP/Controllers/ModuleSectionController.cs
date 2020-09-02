using GreenBushIEP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Mvc;

namespace GreenBushIEP.Controllers
{
    public class ModuleSectionController : Controller
    {
        private readonly IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        // POST: ModuleSection/Edit/5    
        [HttpPost]
        public ActionResult EditHealth(FormCollection collection)
        {
            if (ValidateRequest)
            {
                try
                {
                    int HealthId = Convert.ToInt32(collection["IEPHealthID"]);
                    tblIEPHealth HealthIEP = db.tblIEPHealths.Where(h => h.IEPHealthID == HealthId).SingleOrDefault();
                    tblIEP IEP = db.tblIEPs.Where(i => i.IEPHealthID == HealthIEP.IEPHealthID).FirstOrDefault();

                    if (HealthIEP != null)
                    {
                        int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;

                        HealthIEP.NoConcerns = collection["HealthNoConcerns"] == "on" ? true : false;
                        HealthIEP.Concerns = !HealthIEP.NoConcerns;
                        HealthIEP.ProgressTowardGenEd = (collection["HealthProgressTowardGenEd"] == "on");
                        HealthIEP.Diagnosis = collection["HealthDiagnosis"] == "on" ? true : false;
                        HealthIEP.HearingImpaired = (!string.IsNullOrEmpty(collection["HearingImpaired"])) ? true : false;
                        HealthIEP.HearingDate = Convert.ToDateTime(collection["HearingDate"]);
                        HealthIEP.HearingResult = Convert.ToInt32(collection["HearingResult"]);
                        HealthIEP.VisionImpaired = collection["VisionImpaired"] == "on" ? true : false;
                        HealthIEP.HealthCarePlan = collection["ModuleHealthCarePlan"] == "on" ? true : false;
                        HealthIEP.AdditionalHealthInfo = collection["AdditionalHealthInfo"].ToString();
                        HealthIEP.VisionDate = Convert.ToDateTime(collection["VisionDate"]);
                        HealthIEP.VisionResult = Convert.ToInt32(collection["VisionResult"]);
                        HealthIEP.PLAAFP_Strengths = collection["PLAAFP_Strengths"].ToString();
                        HealthIEP.PLAAFP_Concerns = collection["PLAAFP_Concerns"].ToString();
                        HealthIEP.AreaOfNeedDescription = collection["AreaOfNeedDescription"].ToString();
                        HealthIEP.Notes = collection["HealthNotes"].ToString();
                        HealthIEP.NeedMetByGoal = collection["MetByGoal"] == "on" ? true : false;
                        HealthIEP.NeedMetByAccommodation = collection["MetByAccommodation"] == "on" ? true : false;
                        HealthIEP.NeedMetByOther = collection["MetByOther"] == "on" ? true : false;
                        HealthIEP.NeedMetByOtherDescription = collection["MeetNeedByOtherDescription"].ToString();
                        HealthIEP.Completed = Convert.ToBoolean(collection["Completed"]);
                        HealthIEP.ModifiedBy = ModifiedBy;
                        HealthIEP.Update_Date = DateTime.Now;

                        db.SaveChanges();

                        return RedirectToAction("StudentProcedures", "Home", new { stid = IEP.UserID, iepID = HealthIEP.IEPid });
                    }

                    return RedirectToAction("Portal", "Home");
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to the Health Module: " + e.InnerException);
                }
            }

            throw new Exception("Unable to log you in.");
        }

        // POST: ModuleSection/Edit/5
        [HttpPost]
        public ActionResult EditMotor(FormCollection collection)
        {
            if (ValidateRequest)
            {
                try
                {
                    int MotorId = Convert.ToInt32(collection["IEPMotorID"]);

                    tblIEPMotor MotorIEP = db.tblIEPMotors.Where(m => m.IEPMotorID == MotorId).SingleOrDefault();
                    tblIEP IEP = db.tblIEPs.Where(i => i.IEPMotorID == MotorIEP.IEPMotorID).FirstOrDefault();

                    if (MotorIEP != null)
                    {
                        int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;

                        MotorIEP.NoConcerns = collection["ModuleMotorNoConcerns"] == "on" ? true : false;
                        MotorIEP.ProgressTowardGenEd = collection["ModuleMotorProgressTowardGenEd"] == "on" ? true : false;
                        MotorIEP.Needs = collection["ModuleMotorNeeds"] == "on" ? true : false;
                        MotorIEP.Participation = Convert.ToInt32(collection["ModuleMotorParticipation"]);
                        MotorIEP.PLAAFP_Strengths = collection["PLAAFP_Strengths"].ToString();
                        MotorIEP.PLAAFP_Concerns = collection["PLAAFP_Concerns"].ToString();
                        MotorIEP.AreaOfNeedDescription = collection["ModuleMotorAreaOfNeedDescription"].ToString();
                        MotorIEP.Notes = collection["MotorNotes"].ToString();
                        MotorIEP.NeedMetByGoal = collection["MetByGoal"] == "on" ? true : false;
                        MotorIEP.NeedMetByAccommodation = collection["MetByAccommodation"] == "on" ? true : false;
                        MotorIEP.NeedMetByOther = collection["MetByOther"] == "on" ? true : false;
                        MotorIEP.NeedMetByOtherDescription = collection["MeetNeedByOtherDescription"].ToString();
                        MotorIEP.Completed = Convert.ToBoolean(collection["Completed"]);
                        MotorIEP.ModifiedBy = ModifiedBy;

                        db.SaveChanges();

                        return RedirectToAction("StudentProcedures", "Home", new { stid = IEP.UserID, iepID = MotorIEP.IEPid });
                    }

                    return RedirectToAction("Portal", "Home");
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to the Motor Module: " + e.InnerException);
                }
            }

            throw new Exception("Unable to log you in.");
        }

        // POST: ModuleSection/Edit/5
        [HttpPost]
        public ActionResult EditCommunication(FormCollection collection)
        {
            if (ValidateRequest)
            {
                try
                {
                    int CommunicationId = Convert.ToInt32(collection["IEPCommunicationID"]);

                    tblIEPCommunication CommunicationIEP = db.tblIEPCommunications.Where(c => c.IEPCommunicationID == CommunicationId).SingleOrDefault();
                    tblIEP IEP = db.tblIEPs.Where(i => i.IEPCommunicationID == CommunicationIEP.IEPCommunicationID).FirstOrDefault();

                    if (CommunicationIEP != null)
                    {
                        int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;

                        CommunicationIEP.NoConcerns = collection["ModuleCommunicationNoConcerns"] == "on" ? true : false;
                        CommunicationIEP.ProgressTowardGenEd = collection["ModuleCommunicationProgressTowardGenEd"] == "on" ? true : false;
                        CommunicationIEP.AreaOfNeed = (!string.IsNullOrEmpty(collection["ModuleCommunicationAreaOfNeed"])) ? true : false;
                        CommunicationIEP.Deaf = collection["ModuleCommunicationDeaf"] == "on" ? true : false;
                        CommunicationIEP.LimitedEnglish = collection["ModuleCommunicationDeaf"] == "on" ? true : false;
                        CommunicationIEP.PLAAFP_Strengths = collection["PLAAFP_Strengths"].ToString();
                        CommunicationIEP.PLAAFP_Concerns = collection["PLAAFP_Concerns"].ToString();
                        CommunicationIEP.AreaOfNeedDescription = collection["ModuleCommunicationAreaOfNeedDescription"].ToString();
                        CommunicationIEP.Notes = collection["CommunitcationNotes"].ToString();
                        CommunicationIEP.NeedMetByGoal = collection["MetByGoal"] == "on" ? true : false;
                        CommunicationIEP.NeedMetByAccommodation = collection["MetByAccommodation"] == "on" ? true : false;
                        CommunicationIEP.NeedMetByOther = collection["MetByOther"] == "on" ? true : false;
                        CommunicationIEP.NeedMetByOtherDescription = collection["MeetNeedByOtherDescription"].ToString();
                        CommunicationIEP.Completed = Convert.ToBoolean(collection["Completed"]);
                        CommunicationIEP.ModifiedBy = ModifiedBy;

                        db.SaveChanges();

                        return RedirectToAction("StudentProcedures", "Home", new { stid = IEP.UserID, iepID = CommunicationIEP.IEPid });
                    }

                    return RedirectToAction("Portal", "Home");
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to the Motor Module: " + e.InnerException);
                }
            }

            throw new Exception("Unable to log you in.");
        }

        // POST: ModuleSection/Edit/5
        [HttpPost]
        public ActionResult EditSocial(FormCollection collection)
        {
            if (ValidateRequest)
            {
                try
                {
                    int socialId = Convert.ToInt32(collection["IEPSocialID"]);

                    tblIEPSocial SocialIEP = db.tblIEPSocials.Where(s => s.IEPSocialID == socialId).SingleOrDefault();
                    tblIEP IEP = db.tblIEPs.Where(i => i.IEPSocialID == socialId).FirstOrDefault();

                    if (SocialIEP != null)
                    {
                        int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;

                        SocialIEP.NoConcerns = collection["ModuleSocialNoConcern"] == "on" ? true : false;
                        SocialIEP.ProgressTowardGenEd = collection["ModuleSocialProgressTowardGenEd"] == "on" ? true : false;
                        SocialIEP.AreaOfNeed = collection["ModuleSocialAreaOfNeed"] == "on" ? true : false;
                        SocialIEP.MentalHealthDiagnosis = collection["ModuleSocialMentalHealthDiagnosis"] == "on" ? true : false;
                        SocialIEP.SignificantBehaviors = collection["ModuleSocialSignificantBehaviors"] == "on" ? true : false;
                        SocialIEP.SocialDeficit = collection["ModuleSocialDeficit"] == "on" ? true : false;
                        SocialIEP.BehaviorImepedeLearning = collection["ModuleSocialBehaviorImepedeLearning"] == "yes" ? true : false;
                        SocialIEP.BehaviorInterventionPlan = collection["ModuleSocialBehaviorInterventionPlan"] == "yes" ? true : false;
                        SocialIEP.PLAAFP_Strengths = collection["PLAAFP_Strengths"].ToString();
                        SocialIEP.PLAAFP_Concerns = collection["PLAAFP_Concerns"].ToString();
                        SocialIEP.AreaOfNeedDescription = collection["ModuleSocialAreaOfNeedDescription"].ToString();
                        SocialIEP.Notes = collection["SocialNotes"].ToString();
                        SocialIEP.NeedMetByGoal = collection["MetByGoal"] == "on" ? true : false;
                        SocialIEP.NeedMetByAccommodation = collection["MetByAccommodation"] == "on" ? true : false;
                        SocialIEP.NeedMetByOther = collection["MetByOther"] == "on" ? true : false;
                        SocialIEP.NeedMetByOtherDescription = collection["MeetNeedByOtherDescription"].ToString();
                        SocialIEP.Completed = Convert.ToBoolean(collection["Completed"]);
                        SocialIEP.ModifiedBy = ModifiedBy;

                        db.SaveChanges();

                        return RedirectToAction("StudentProcedures", "Home", new { stid = IEP.UserID, iepID = SocialIEP.IEPid });
                    }

                    return RedirectToAction("Portal", "Home");
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to the Social Module: " + e.InnerException);
                }
            }

            throw new Exception("Unable to log you in.");
        }

        // POST: ModuleSection/Edit/5
        [HttpPost]
        public ActionResult EditIntelligence(FormCollection collection)
        {
            if (ValidateRequest)
            {
                try
                {
                    int intelligenceId = Convert.ToInt32(collection["IEPIntelligenceID"]);

                    tblIEPIntelligence IntellgienceIEP = db.tblIEPIntelligences.Where(i => i.IEPIntelligenceID == intelligenceId).SingleOrDefault();
                    tblIEP IEP = db.tblIEPs.Where(i => i.IEPIntelligenceID == intelligenceId).FirstOrDefault();

                    if (IntellgienceIEP != null)
                    {
                        int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;

                        IntellgienceIEP.Concerns = !(collection["ModuleIntelligenceNoConcerns"] == "on" ? true : false);
                        IntellgienceIEP.ProgressTowardGenEd = collection["ModuleIntelligenceProgressTowardGenEd"] == "on" ? true : false;
                        IntellgienceIEP.AreaOfNeed = collection["ModuleIntelligenceAreaOfNeed"] == "on" ? true : false;
                        IntellgienceIEP.AreaOfNeedDescription = collection["ModuleIntelligenceAreaOfNeedDescription"].ToString();
                        IntellgienceIEP.Notes = collection["IntelligenceNotes"].ToString();
                        IntellgienceIEP.PLAAFP_Strengths = collection["PLAAFP_Strengths"].ToString();
                        IntellgienceIEP.PLAAFP_Concerns = collection["PLAAFP_Concerns"].ToString();
                        IntellgienceIEP.Completed = Convert.ToBoolean(collection["Completed"]);
                        IntellgienceIEP.ModifiedBy = ModifiedBy;

                        db.SaveChanges();

                        return RedirectToAction("StudentProcedures", "Home", new { stid = IEP.UserID, iepID = IntellgienceIEP.IEPid });
                    }

                    return RedirectToAction("Portal", "Home");
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to the Intelligence Module: " + e.InnerException);
                }
            }

            throw new Exception("Unable to log you in.");
        }

        // POST: ModuleSection/Edit/5
        [HttpPost]
        public ActionResult EditAcademic(FormCollection collection)
        {
            if (ValidateRequest)
            {
                try
                {
                    int academicID = Convert.ToInt32(collection["Academic.IEPAcademicID"]);
                    int readingID = Convert.ToInt32(collection["Reading.IEPReadingID"]);
                    int mathID = Convert.ToInt32(collection["Math.IEPMathID"]);
                    int writtenID = Convert.ToInt32(collection["Written.IEPWrittenID"]);

                    ModuleAcademicViewModel viewModel = new ModuleAcademicViewModel
                    {
                        Academic = db.tblIEPAcademics.Where(a => a.IEPAcademicID == academicID).SingleOrDefault(),
                        Reading = db.tblIEPReadings.Where(r => r.IEPReadingID == readingID).SingleOrDefault(),
                        Math = db.tblIEPMaths.Where(m => m.IEPMathID == mathID).SingleOrDefault(),
                        Written = db.tblIEPWrittens.Where(w => w.IEPWrittenID == writtenID).SingleOrDefault()
                    };

                    int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;

                    tblIEP IEP = db.tblIEPs.Where(i => i.IEPAcademicID == academicID).FirstOrDefault();
                    if (IEP != null)
                    {
                        viewModel.Academic.NoConcerns = collection["ModuleAcademicNoConcern"] == "on" ? true : false;
                        viewModel.Academic.ProgressTowardGenEd = collection["ModuleAcademicProgressTowardGenEd"] == "on" ? true : false;
                        viewModel.Academic.AreaOfNeed = collection["ModuleAcademicAreaOfNeed"] == "on" ? true : false;
                        viewModel.Academic.PLAAFP_Strengths = collection["ModuleAcademic_PLAAFP_Strengths"];
                        viewModel.Academic.PLAAFP_Concerns = collection["ModuleAcademic_PLAAFP_Concerns"];
                        viewModel.Academic.AreaOfNeedDescription = collection["ModuleAcademicAreaOfNeedDescription"].ToString();
                        viewModel.Academic.Notes = collection["AcademicNotes"].ToString();
                        viewModel.Academic.NeedMetByGoal = collection["ModuleAcademicMetByGoal"] == "on" ? true : false;
                        viewModel.Academic.NeedMetByAccommodation = collection["ModuleAcademicMetByAccommodation"] == "on" ? true : false;
                        viewModel.Academic.NeedMetByOther = collection["ModuleAcademicMetByOther"] == "on" ? true : false;
                        viewModel.Academic.NeedMetByOtherDescription = collection["ModuleAcademicMeetNeedByOtherDescription"].ToString();
                        viewModel.Academic.Completed = Convert.ToBoolean(collection["Academic.Completed"]);
                        viewModel.Academic.ModifiedBy = ModifiedBy;

                        viewModel.Reading.NoConcerns = collection["ModuleReadingNoConcern"] == "on" ? true : false;
                        viewModel.Reading.ProgressTowardGenEd = collection["ModuleReadingProgressTowardGenEd"] == "on" ? true : false;
                        viewModel.Reading.AreaOfNeed = collection["ModuleReadingAreaOfNeed"] == "on" ? true : false;
                        viewModel.Reading.PLAAFP_Strengths = collection["ModuleReading_PLAAFP_Strengths"];
                        viewModel.Reading.PLAAFP_Concerns = collection["ModuleReading_PLAAFP_Concerns"];
                        viewModel.Reading.AreaOfNeedDescription = collection["ModuleReadingAreaOfNeedDescription"].ToString();
                        viewModel.Reading.Notes = collection["ReadingNotes"].ToString();
                        viewModel.Reading.NeedMetByGoal = collection["ModuleReadingMetByGoal"] == "on" ? true : false;
                        viewModel.Reading.NeedMetByAccommodation = collection["ModuleReadingMetByAccommodation"] == "on" ? true : false;
                        viewModel.Reading.NeedMetByOther = collection["ModuleReadingMetByOther"] == "on" ? true : false;
                        viewModel.Reading.NeedMetByOtherDescription = collection["ModuleReadingMeetNeedByOtherDescription"].ToString();
                        viewModel.Reading.ModifiedBy = ModifiedBy;

                        viewModel.Math.NoConcerns = collection["ModuleMathNoConcern"] == "on" ? true : false;
                        viewModel.Math.ProgressTowardGenEd = collection["ModuleMathProgressTowardGenEd"] == "on" ? true : false;
                        viewModel.Math.AreaOfNeed = collection["ModuleMathAreaOfNeed"] == "on" ? true : false;
                        viewModel.Math.PLAAFP_Strengths = collection["ModuleMath_PLAAFP_Strengths"];
                        viewModel.Math.PLAAFP_Concerns = collection["ModuleMath_PLAAFP_Concerns"];
                        viewModel.Math.AreaOfNeedDescription = collection["ModuleMathAreaOfNeedDescription"].ToString();
                        viewModel.Math.Notes = collection["MathNotes"].ToString();
                        viewModel.Math.NeedMetByGoal = collection["ModuleMathMetByGoal"] == "on" ? true : false;
                        viewModel.Math.NeedMetByAccommodation = collection["ModuleMathMetByAccommodation"] == "on" ? true : false;
                        viewModel.Math.NeedMetByOther = collection["ModuleMathMetByOther"] == "on" ? true : false;
                        viewModel.Math.NeedMetByOtherDescription = collection["ModuleMathMeetNeedByOtherDescription"].ToString();
                        viewModel.Math.ModifiedBy = ModifiedBy;

                        viewModel.Written.NoConcerns = collection["ModuleWrittenNoConcern"] == "on" ? true : false;
                        viewModel.Written.ProgressTowardGenEd = collection["ModuleWrittenProgressTowardGenEd"] == "on" ? true : false;
                        viewModel.Written.AreaOfNeed = collection["ModuleWrittenAreaOfNeed"] == "on" ? true : false;
                        viewModel.Written.PLAAFP_Strengths = collection["ModuleWritten_PLAAFP_Strengths"];
                        viewModel.Written.PLAAFP_Concerns = collection["ModuleWritten_PLAAFP_Concerns"];
                        viewModel.Written.AreaOfNeedDescription = collection["ModuleWrittenAreaOfNeedDescription"].ToString();
                        viewModel.Written.Notes = collection["WrittenNotes"].ToString();
                        viewModel.Written.NeedMetByGoal = collection["ModuleWrittenMetByGoal"] == "on" ? true : false;
                        viewModel.Written.NeedMetByAccommodation = collection["ModuleWrittenMetByAccommodation"] == "on" ? true : false;
                        viewModel.Written.NeedMetByOther = collection["ModuleWrittenMetByOther"] == "on" ? true : false;
                        viewModel.Written.NeedMetByOtherDescription = collection["ModuleWrittenMeetNeedByOtherDescription"].ToString();
                        viewModel.Written.ModifiedBy = ModifiedBy;

                        db.SaveChanges();

                        return Json(new { Result = "success", Message = "Academic Module Saved.", Completed = viewModel.Academic.Completed }, JsonRequestBehavior.AllowGet);
                    }

                    return RedirectToAction("Portal", "Home");
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "Unknown Error. " + e.Message, Completed = false }, JsonRequestBehavior.AllowGet);
                }
            }

            throw new Exception("Unable to log you in.");
        }

        [HttpPost]
        public ActionResult EditBehvior(BehaviorViewModel model, FormCollection collection)
        {
            tblUser submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            if (ModelState.IsValid)
            {
                tblBehavior BehaviorIEP = db.tblBehaviors.Where(c => c.BehaviorID == model.BehaviorID).FirstOrDefault();
                int studentId = model.StudentId;
                int iepId = model.IEPid;
                int behaviorId = model.BehaviorID;
                int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;

                if (BehaviorIEP == null)
                {
                    BehaviorIEP = new tblBehavior();
                }
                else
                {
                    BehaviorIEP.BehaviorID = behaviorId;
                }

                BehaviorIEP.IEPid = iepId;
                BehaviorIEP.Completed = model.Completed;
                BehaviorIEP.BehaviorConcern = model.BehaviorConcern;
                BehaviorIEP.Crisis_Description = model.Crisis_Description;
                BehaviorIEP.Crisis_Escalation = model.Crisis_Escalation;
                BehaviorIEP.Crisis_Implementation = model.Crisis_Implementation;
                BehaviorIEP.Crisis_Other = model.Crisis_Other;
                BehaviorIEP.ReviewedBy = model.ReviewedBy;
                BehaviorIEP.StrengthMotivator = model.StrengthMotivator;
                BehaviorIEP.Update_Date = DateTime.Now;
                BehaviorIEP.ModifiedBy = ModifiedBy;

                if (behaviorId == 0)
                {
                    BehaviorIEP.Create_Date = DateTime.Now;
                    db.tblBehaviors.Add(BehaviorIEP);
                }
                db.SaveChanges();
                behaviorId = BehaviorIEP.BehaviorID;

                //triggers
                IQueryable<tblBehaviorTrigger> existingTriggers = db.tblBehaviorTriggers.Where(o => o.BehaviorID == behaviorId);
                foreach (tblBehaviorTrigger existingTrigger in existingTriggers)
                {
                    db.tblBehaviorTriggers.Remove(existingTrigger);
                }

                db.SaveChanges();

                foreach (int trigger in model.SelectedTriggers)
                {
                    tblBehaviorTriggerType tType = db.tblBehaviorTriggerTypes.Where(o => o.BehaviorTriggerTypeID == trigger).FirstOrDefault();
                    string otherDesc = "";
                    if (tType != null && tType.BehaviorTriggerType.ToUpper() == "OTHER")
                    {
                        otherDesc = model.TriggerOther;
                    }
                    db.tblBehaviorTriggers.Add(new tblBehaviorTrigger { IEPid = iepId, BehaviorID = behaviorId, BehaviorTriggerTypeID = trigger, Create_Date = DateTime.Now, OtherDescription = otherDesc, Update_Date = DateTime.Now, CreatedBy = submitter.UserID, ModifiedBy = submitter.UserID });
                }
                db.SaveChanges();

                //hypotheses 
                IQueryable<tblBehaviorHypothesi> existingHypotheses = db.tblBehaviorHypothesis.Where(o => o.BehaviorID == behaviorId);

                foreach (tblBehaviorHypothesi existingHypothesis in existingHypotheses)
                {
                    db.tblBehaviorHypothesis.Remove(existingHypothesis);
                }
                db.SaveChanges();

                foreach (int hypothesis in model.SelectedHypothesis)
                {
                    tblBehaviorHypothesisType tType = db.tblBehaviorHypothesisTypes.Where(o => o.BehaviorHypothesisTypeID == hypothesis).FirstOrDefault();
                    string otherDesc = "";
                    if (tType != null && tType.BehaviorHypothesisType.ToUpper() == "OTHER")
                    {
                        otherDesc = model.HypothesisOther;
                    }
                    db.tblBehaviorHypothesis.Add(new tblBehaviorHypothesi { IEPid = iepId, BehaviorID = behaviorId, BehaviorHypothesisTypeID = hypothesis, Create_Date = DateTime.Now, OtherDescription = otherDesc, Update_Date = DateTime.Now, CreatedBy = submitter.UserID, ModifiedBy = submitter.UserID });
                }
                db.SaveChanges();

                //Strategies  
                IQueryable<tblBehaviorStrategy> existingStrategies = db.tblBehaviorStrategies.Where(o => o.BehaviorID == behaviorId);
                foreach (tblBehaviorStrategy existingStrategy in existingStrategies)
                {
                    db.tblBehaviorStrategies.Remove(existingStrategy);
                }
                db.SaveChanges();

                foreach (int strategy in model.SelectedStrategies)
                {
                    tblBehaviorStrategyType tType = db.tblBehaviorStrategyTypes.Where(o => o.BehaviorStrategyTypeID == strategy).FirstOrDefault();
                    string otherDesc = "";
                    if (tType != null && tType.BehaviorStrategyType.ToUpper() == "OTHER")
                    {
                        otherDesc = model.StrategiesOther;
                    }
                    db.tblBehaviorStrategies.Add(new tblBehaviorStrategy { IEPid = iepId, BehaviorID = behaviorId, BehaviorStrategyTypeID = strategy, Create_Date = DateTime.Now, OtherDescription = otherDesc, Update_Date = DateTime.Now, CreatedBy = submitter.UserID, ModifiedBy = submitter.UserID });
                }
                db.SaveChanges();

                //targeted behaviors

                //1
                string tbid = collection["targetId1"].ToString();
                string tbBehavior = collection["tbBehavior1"].ToString();
                string tbBaseline = collection["tbBaseline1"].ToString();
                if (tbid == "0")
                {
                    //new tbl
                    db.tblBehaviorBaselines.Add(new tblBehaviorBaseline { IEPid = iepId, BehaviorID = behaviorId, Behavior = tbBehavior, Baseline = tbBaseline, Create_Date = DateTime.Now, Update_Date = DateTime.Now, CreatedBy = submitter.UserID, ModifiedBy = submitter.UserID });
                }
                else
                {
                    int.TryParse(tbid, out int behaviorBaselineID);
                    tblBehaviorBaseline existingtb1 = db.tblBehaviorBaselines.Where(o => o.BehaviorID == behaviorId && o.BehaviorBaselineID == behaviorBaselineID).FirstOrDefault();
                    if (existingtb1 != null)
                    {
                        existingtb1.Baseline = tbBaseline;
                        existingtb1.Behavior = tbBehavior;
                    }
                }

                //2
                tbid = collection["targetId2"].ToString();
                tbBehavior = collection["tbBehavior2"].ToString();
                tbBaseline = collection["tbBaseline2"].ToString();
                if (tbid == "0")
                {
                    //new tbl
                    db.tblBehaviorBaselines.Add(new tblBehaviorBaseline { IEPid = iepId, BehaviorID = behaviorId, Behavior = tbBehavior, Baseline = tbBaseline, Create_Date = DateTime.Now, Update_Date = DateTime.Now, CreatedBy = submitter.UserID, ModifiedBy = submitter.UserID });
                }
                else
                {
                    int.TryParse(tbid, out int behaviorBaselineID);
                    tblBehaviorBaseline existingtb2 = db.tblBehaviorBaselines.Where(o => o.BehaviorID == behaviorId && o.BehaviorBaselineID == behaviorBaselineID).FirstOrDefault();
                    if (existingtb2 != null)
                    {
                        existingtb2.Baseline = tbBaseline;
                        existingtb2.Behavior = tbBehavior;
                    }
                }

                //3
                tbid = collection["targetId3"].ToString();
                tbBehavior = collection["tbBehavior3"].ToString();
                tbBaseline = collection["tbBaseline3"].ToString();
                if (tbid == "0")
                {
                    //new tbl
                    db.tblBehaviorBaselines.Add(new tblBehaviorBaseline { IEPid = iepId, BehaviorID = behaviorId, Behavior = tbBehavior, Baseline = tbBaseline, Create_Date = DateTime.Now, Update_Date = DateTime.Now, ModifiedBy = submitter.UserID, CreatedBy = submitter.UserID });
                }
                else
                {
                    int.TryParse(tbid, out int behaviorBaselineID);
                    tblBehaviorBaseline existingtb3 = db.tblBehaviorBaselines.Where(o => o.BehaviorID == behaviorId && o.BehaviorBaselineID == behaviorBaselineID).FirstOrDefault();
                    if (existingtb3 != null)
                    {
                        existingtb3.Baseline = tbBaseline;
                        existingtb3.Behavior = tbBehavior;
                    }
                }

                db.SaveChanges();

                return RedirectToAction("StudentProcedures", "Home", new { stid = studentId, iepID = BehaviorIEP.IEPid });
            }

            return RedirectToAction("StudentProcedures", "Home", new { stid = model.StudentId });
        }

        [HttpPost]
        public ActionResult EditOtherConsiderations(tblOtherConsideration model, FormCollection collection)
        {
            int studentId = 0;

            try
            {
                int.TryParse(collection["StudentId"].ToString(), out studentId);

                model.AssistiveTechnology_Require = collection["AssistiveTechnology_Require"] == "on" ? true : false;
                model.Parental_Concerns_flag = collection["Parental_Concerns_flag"] == "on" ? true : false;
                model.Parental_CopyIEP_flag = collection["Parental_CopyIEP_flag"] == "on" ? true : false;
                model.Parental_RightsBook_flag = collection["Parental_Rightsbook_flag"] == "on" ? true : false;
                model.ExtendedSchoolYear_RegressionRisk = collection["ExtendedSchoolYear_RegressionRisk"] == "on" ? true : false;
                model.ExtendedSchoolYear_SeverityRisk = collection["ExtendedSchoolYear_SeverityRisk"] == "on" ? true : false;
                model.Potential_HarmfulEffects_flag = collection["Potential_Harmful_Effects_flag"] == "on" ? true : false;
                model.Potential_HarmfulEffects_desc = collection["Potential_Harmful_Effects_Desc"];
                model.Completed = Convert.ToBoolean(collection["Completed"]);
                model.ExtendedSchoolYear_Necessary = collection["ExtendYear"];

                string dwa = collection["DistrictWideAssessments"];
                switch (dwa)
                {
                    case "1":
                        model.DistrictAssessment_NoAccommodations_flag = true;
                        break;
                    case "2":
                        model.DistrictAssessment_WithAccommodations_flag = true;
                        break;
                    case "3":
                        model.DistrictAssessment_Alternative_flag = true;
                        break;
                    case "4":
                        model.DistrictAssessment_GradeNotAssessed = true;
                        break;
                }

                string swa = collection["StateWideAssessments"];
                switch (swa)
                {
                    case "1":
                        model.StateAssessment_NoAccommodations_flag = true;
                        break;
                    case "2":
                        model.StateAssessment_WithAccommodations_flag = true;
                        break;
                    case "3":
                        model.StateAssessment_RequiredCompleted = true;
                        break;
                    case "4":
                        model.StateAssesment_Alternative_flag = true;
                        break;
                }

                string tp = collection["TransporationPlan"];
                switch (tp)
                {
                    case "1":
                        model.Transporation_NotEligible = true;
                        break;
                    case "2":
                        model.Transporation_Disability_flag = true;
                        break;
                    case "3":
                        model.Transporation_AttendOtherBuilding = true;
                        break;
                    case "4":
                        model.Transporation_Other_flag = true;
                        break;
                }

                string otherDesc = "";

                string vehicleTypeValue = collection["inputVehicleType"];
                string beginDate = collection["inputBegin"];
                string endDate = collection["inputEnd"];
                string minutesValue = collection["inputMinutes"];
                string vehicleType = "";
                string minutes = "25";
                if (!string.IsNullOrEmpty(vehicleTypeValue))
                {
                    if (vehicleTypeValue == "1")
                    {
                        vehicleType = "special education";
                    }
                    else if (vehicleTypeValue == "2")
                    {
                        vehicleType = "general education";
                    }
                    else
                    {
                        vehicleType = "";
                    }
                }

                minutes = string.IsNullOrEmpty(minutesValue) ? "25" : minutesValue;

                otherDesc = string.Format(@"The student will receive transportation each day that school is in session, on a {0} vehicle, from the time the student boards the vehicle from the departure point until arrival at the destination and from the time the student boards the vehicle until arrival at the returning destination. ({1} minutes estimated normal commute) beginning on {2} and ending on {3} following the school calendar.", vehicleType, minutes, beginDate, endDate);
                int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;

                //find existing if updating
                tblOtherConsideration OC = db.tblOtherConsiderations.Where(c => c.OtherConsiderationID == model.OtherConsiderationID).FirstOrDefault();

                if (OC == null)
                {
                    model.Create_Date = DateTime.Now;
                    model.ModifiedBy = ModifiedBy;
                    model.Transporation_Other_desc = model.Transporation_NotEligible.HasValue && model.Transporation_NotEligible.Value ? "" : otherDesc;
                    db.tblOtherConsiderations.Add(model);
                    db.SaveChanges();
                    return RedirectToAction("StudentProcedures", "Home", new { stid = studentId });
                }
                else
                {
                    OC.AssistiveTechnology_Description = model.AssistiveTechnology_Description;
                    OC.AssistiveTechnology_Require = model.AssistiveTechnology_Require;
                    OC.DistrictAssessment_NoAccommodations_flag = model.DistrictAssessment_NoAccommodations_flag;
                    OC.DistrictAssessment_NoAccommodations_desc = model.DistrictAssessment_NoAccommodations_desc;
                    OC.DistrictAssessment_WithAccommodations_flag = model.DistrictAssessment_WithAccommodations_flag;
                    OC.DistrictAssessment_WithAccommodations_desc = model.DistrictAssessment_WithAccommodations_desc;
                    OC.DistrictAssessment_Alternative_flag = model.DistrictAssessment_Alternative_flag;
                    OC.DistrictAssessment_Alternative_desc = model.DistrictAssessment_Alternative_desc;
                    OC.DistrictAssessment_GradeNotAssessed = model.DistrictAssessment_GradeNotAssessed;
                    OC.StateAssessment_NoAccommodations_flag = model.StateAssessment_NoAccommodations_flag;
                    OC.StateAssessment_NoAccommodations_desc = model.StateAssessment_NoAccommodations_desc;
                    OC.StateAssessment_WithAccommodations_flag = model.StateAssessment_WithAccommodations_flag;
                    OC.StateAssessment_WithAccommodations_desc = model.StateAssessment_WithAccommodations_desc;
                    OC.StateAssesment_Alternative_flag = model.StateAssesment_Alternative_flag;
                    OC.StateAssesment_Alternative_Desc = model.StateAssesment_Alternative_Desc;
                    OC.StateAssessment_RequiredCompleted = model.StateAssessment_RequiredCompleted;
                    OC.Transporation_NotEligible = model.Transporation_NotEligible;
                    OC.Transporation_Required = model.Transporation_Required;
                    OC.Transporation_Disability_flag = model.Transporation_Disability_flag;
                    OC.Transporation_AttendOtherBuilding = model.Transporation_AttendOtherBuilding;
                    OC.Transporation_Other_flag = model.Transporation_Other_flag;
                    OC.Transporation_Other_desc = model.Transporation_NotEligible.HasValue && model.Transporation_NotEligible.Value ? "" : otherDesc;
                    OC.RegularEducation_NotParticipate = model.RegularEducation_NotParticipate;
                    OC.ExtendedSchoolYear_Necessary = model.ExtendedSchoolYear_Necessary;
                    OC.ExtendedSchoolYear_RegressionRisk = model.ExtendedSchoolYear_RegressionRisk;
                    OC.ExtendedSchoolYear_SeverityRisk = model.ExtendedSchoolYear_SeverityRisk;
                    OC.ExtendedSchoolYear_Justification = model.ExtendedSchoolYear_Justification;
                    OC.Parental_Concerns_flag = model.Parental_Concerns_flag;
                    OC.Parental_Concerns_Desc = model.Parental_Concerns_Desc;
                    OC.Parental_CopyIEP_flag = model.Parental_CopyIEP_flag;
                    OC.Parental_RightsBook_flag = model.Parental_RightsBook_flag;
                    OC.Potential_HarmfulEffects_flag = model.Potential_HarmfulEffects_flag;
                    OC.Potential_HarmfulEffects_desc = model.Potential_HarmfulEffects_desc;
                    OC.Completed = model.Completed;
                    OC.Create_Date = DateTime.Now;
                    OC.ModifiedBy = ModifiedBy;

                    db.SaveChanges();
                    return RedirectToAction("StudentProcedures", "Home", new { stid = studentId, iepID = OC.IEPid });
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
            }

            throw new Exception("Unable to save changes to Other Considerations Module");
        }

        [HttpPost]
        public ActionResult EditAccomodation(AccomodationViewModel model)
        {
            int studentId = model.StudentId;
            int IEPid = model.IEPid;

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Title))
                {
                    if (model.AccomType == 1)
                    {
                        model.Title = "Accommodation ";
                    }
                    else if (model.AccomType == 2)
                    {
                        model.Title = "Modification ";
                    }
                    else if (model.AccomType == 3)
                    {
                        model.Title = "Supplemental Aids and Services ";
                    }
                    else if (model.AccomType == 4)
                    {
                        model.Title = "Support for School Personnel ";
                    }
                    else if (model.AccomType == 5)
                    {
                        model.Title = "Transportation ";
                    }
                }

                tblAccommodation accommodation = db.tblAccommodations.Where(a => a.AccommodationID == model.AccommodationID).FirstOrDefault();
                int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;

                if (accommodation != null)
                {
                    accommodation.Title = model.Title;
                    accommodation.AccomType = model.AccomType;
                    accommodation.Completed = model.Completed;
                    accommodation.Description = model.Description;
                    accommodation.Duration = model.Duration;
                    accommodation.Frequency = model.Frequency;
                    accommodation.Location = model.Location;
                    accommodation.IEPid = model.IEPid;
                    accommodation.Module = model.Module;
                    accommodation.ModifiedBy = ModifiedBy;

                    if (model.AnticipatedStartDate.HasValue)
                    {
                        accommodation.AnticipatedStartDate = model.AnticipatedStartDate;
                    }

                    if (model.AnticipatedEndDate.HasValue)
                    {
                        accommodation.AnticipatedEndDate = model.AnticipatedEndDate;
                    }

                    accommodation.Update_Date = DateTime.Now;

                    //delete 
                    if (model.SelectedModules != null)
                    {
                        List<tblAccommodationModule> existingModules = db.tblAccommodationModules.Where(o => o.AccommodationID == accommodation.AccommodationID && !model.SelectedModules.Contains(o.ModuleID)).ToList();
                        foreach (tblAccommodationModule em in existingModules)
                        {
                            db.tblAccommodationModules.Remove(em);
                        }

                        //add 
                        foreach (int accomModuleId in model.SelectedModules)
                        {
                            if (!db.tblAccommodationModules.Any(o => o.AccommodationID == accommodation.AccommodationID && o.ModuleID == accomModuleId))
                            {
                                db.tblAccommodationModules.Add(new tblAccommodationModule() { AccommodationID = accommodation.AccommodationID, ModuleID = accomModuleId, CreatedBy = 0, Create_Date = DateTime.Now });
                            }
                        }
                    }

                    db.SaveChanges();

                    return Json(new { success = true, id = accommodation.AccommodationID, iep = accommodation.IEPid, isNew = false });
                }
                else
                {
                    tblAccommodation newAccomodation = new tblAccommodation
                    {
                        AccomType = model.AccomType,
                        Title = model.Title,
                        Completed = model.Completed,
                        Description = model.Description,
                        Duration = model.Duration,
                        Frequency = model.Frequency,
                        Location = model.Location,
                        IEPid = model.IEPid,
                        Module = model.Module,
                        ModifiedBy = ModifiedBy
                    };

                    if (model.AnticipatedStartDate.HasValue)
                    {
                        newAccomodation.AnticipatedStartDate = model.AnticipatedStartDate;
                    }

                    if (model.AnticipatedEndDate.HasValue)
                    {
                        newAccomodation.AnticipatedEndDate = model.AnticipatedEndDate;
                    }

                    newAccomodation.Update_Date = DateTime.Now;
                    newAccomodation.Create_Date = DateTime.Now;

                    db.tblAccommodations.Add(newAccomodation);

                    db.SaveChanges();

                    //add modules
                    if (model.SelectedModules != null)
                    {
                        foreach (int accomModuleId in model.SelectedModules)
                        {
                            db.tblAccommodationModules.Add(new tblAccommodationModule() { AccommodationID = newAccomodation.AccommodationID, ModuleID = accomModuleId, CreatedBy = 0, Create_Date = DateTime.Now });
                        }

                        db.SaveChanges();
                    }

                    return Json(new { success = true, id = newAccomodation.AccommodationID, iep = newAccomodation.IEPid, isNew = true });
                }
            }

            return Json(new { success = false, error = "Unable to edit your accomodation" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeleteStudentGoal(int studentGoalId)
        {
            tblGoal studentGoalToRemove = db.tblGoals.Where(g => g.goalID == studentGoalId).FirstOrDefault();
            if (studentGoalToRemove != null)
            {
                if (studentGoalToRemove.tblServices.Any())
                {
                    return Json(new { Result = "failure", Message = "You must first remove the goal from all the services." }, JsonRequestBehavior.AllowGet);
                }

                List<tblGoalEvaluationProcedure> evalsToDelete = db.tblGoalEvaluationProcedures.Where(g => g.goalID == studentGoalId).ToList();
                foreach (tblGoalEvaluationProcedure objEP in evalsToDelete)
                {
                    db.tblGoalEvaluationProcedures.Remove(objEP);
                    db.SaveChanges();
                }

                List<tblGoalBenchmark> benchmarksToDelete = db.tblGoalBenchmarks.Where(g => g.goalID == studentGoalId).ToList();
                foreach (tblGoalBenchmark obj in benchmarksToDelete)
                {
                    db.tblGoalBenchmarks.Remove(obj);
                    db.SaveChanges();
                }

                db.tblGoals.Remove(studentGoalToRemove);
                db.SaveChanges();

                return Json(new { Result = "success", Message = "Student Goal was successfully deleted." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "failure", Message = "The Student Goal Benchmark Id was not found and thus not deleted." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult GetGoalBaseline(int goalTypeId, int iepID)
        {
            string baselineText = "";
            switch (goalTypeId)
            {
                case 1:
                    tblIEPHealth healthGoal = db.tblIEPHealths.Where(g => g.IEPid == iepID && g.Concerns).FirstOrDefault();
                    if (healthGoal != null)
                    {
                        baselineText = healthGoal.PLAAFP_Concerns;
                    }

                    break;
                case 2:
                    tblIEPMotor motorGoal = db.tblIEPMotors.Where(g => g.IEPid == iepID && !g.NoConcerns).FirstOrDefault();
                    if (motorGoal != null)
                    {
                        baselineText = motorGoal.PLAAFP_Concerns;
                    }

                    break;
                case 3:
                    tblIEPCommunication commGoal = db.tblIEPCommunications.Where(g => g.IEPid == iepID && !g.NoConcerns).FirstOrDefault();
                    if (commGoal != null)
                    {
                        baselineText = commGoal.PLAAFP_Concerns;
                    }

                    break;
                case 4:
                    tblIEPSocial socialGoal = db.tblIEPSocials.Where(g => g.IEPid == iepID && !g.NoConcerns).FirstOrDefault();
                    if (socialGoal != null)
                    {
                        baselineText = socialGoal.PLAAFP_Concerns;
                    }

                    break;
                case 5:
                    tblIEPIntelligence giGoal = db.tblIEPIntelligences.Where(g => g.IEPid == iepID).FirstOrDefault();
                    if (giGoal != null)
                    {
                        baselineText = giGoal.PLAAFP_Concerns;
                    }

                    break;
                case 6:
                    tblIEPAcademic academicGoal = db.tblIEPAcademics.Where(g => g.IEPid == iepID && !g.NoConcerns).FirstOrDefault();
                    if (academicGoal != null)
                    {
                        baselineText = academicGoal.PLAAFP_Concerns;
                    }

                    break;
                case 7:
                    tblIEPReading readGoal = db.tblIEPReadings.Where(g => g.IEPid == iepID && !g.NoConcerns).FirstOrDefault();
                    if (readGoal != null)
                    {
                        baselineText = readGoal.PLAAFP_Concerns;
                    }

                    break;
                case 8:
                    tblIEPMath mathGoal = db.tblIEPMaths.Where(g => g.IEPid == iepID && !g.NoConcerns).FirstOrDefault();
                    if (mathGoal != null)
                    {
                        baselineText = mathGoal.PLAAFP_Concerns;
                    }

                    break;
                case 9:
                    tblIEPWritten writtenGoal = db.tblIEPWrittens.Where(g => g.IEPid == iepID && !g.NoConcerns).FirstOrDefault();
                    if (writtenGoal != null)
                    {
                        baselineText = writtenGoal.PLAAFP_Concerns;
                    }

                    break;
            }

            return Json(new { Result = true, Message = baselineText }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [Authorize]
        public ActionResult DeleteStudentBenchmark(int studentBenchmarkId)
        {
            tblGoalBenchmark benchmarkToRemove = db.tblGoalBenchmarks.Where(b => b.goalBenchmarkID == studentBenchmarkId).FirstOrDefault();
            if (benchmarkToRemove != null)
            {
                db.tblGoalBenchmarks.Remove(benchmarkToRemove);
                db.SaveChanges();

                return Json(new { Result = "success", Message = "Student Benchmark was successfully deleted." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "failure", Message = "The Student Benchmark was not deleted." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult EditStudentGoals(FormCollection collection)
        {
            int goalId = 0;
            if (ValidateRequest)
            {
                try
                {
                    string verification = collection[0];
                    int studentId = Convert.ToInt32(collection["studentId"]);
                    int iepId = Convert.ToInt32(collection["IEPid"]);
                    int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;

                    int j = 3;
                    bool keyParse = int.TryParse(collection[++j], out goalId);
                    StudentGoal studentGoal = (!keyParse) ? new StudentGoal() : new StudentGoal(goalId); // new goal : exsisting goal

                    studentGoal.goal.IEPid = iepId;
                    studentGoal.goal.Module = collection[++j].ToString();
                    studentGoal.goal.Title = collection[++j].ToString();
                    studentGoal.goal.hasSerivce = collection[++j] == "true" ? true : false;
                    studentGoal.goal.AnnualGoal = collection[++j].ToString();
                    studentGoal.goal.Baseline = collection[++j].ToString();
                    studentGoal.goal.StateStandards = collection[++j].ToString();
                    studentGoal.goal.Completed = Convert.ToBoolean(collection["completed"]);
                    studentGoal.goal.Update_Date = DateTime.Now;
                    studentGoal.goal.ModifiedBy = ModifiedBy;

                    string evalProcedures = "";
                    string evalProcOtherIndex = collection.AllKeys.Where(o => o.Contains("StudentGoalBenchmarkOther")).FirstOrDefault();
                    string evalProcOtherStr = collection[evalProcOtherIndex];
                    if (collection.AllKeys.Where(o => o.Contains("StudentGoalBenchmarkMethods")).Any())
                    {
                        string evalProceduresStr = collection.AllKeys.Where(o => o.Contains("StudentGoalBenchmarkMethods")).FirstOrDefault();
                        evalProcedures = collection[evalProceduresStr];
                        if (evalProcedures != null)
                        {
                            j++; //only increment when values are submitted otherwise it throws the count off for the rest
                        }
                    }


                    studentGoal.benchmarks.Clear();
                    List<int> existingBenchmarks = new List<int>();

                    foreach (string key in collection.AllKeys)
                    {
                        int benchmarkIDVal = 0;
                        int tempId = 0;
                        bool isTemp = false;
                        if (key.Contains("StudentGoalBenchmarkId"))
                        {
                            string value = collection[key];
                            int.TryParse(value, out benchmarkIDVal);
                            tblGoalBenchmark benchmark = benchmarkIDVal == 0 ? new tblGoalBenchmark() : db.tblGoalBenchmarks.Where(b => b.goalBenchmarkID == benchmarkIDVal).FirstOrDefault();
                            int.TryParse(key.Substring(22), out tempId);

                            if (benchmarkIDVal == 0)
                            {
                                //get temp value								
                                benchmarkIDVal = tempId;
                                isTemp = true;
                            }
                            else
                            {
                                existingBenchmarks.Add(benchmarkIDVal);
                            }

                            if (benchmark != null)
                            {
                                //string transitionActivity = collection[string.Format("StudentGoalBenchmarkHasTransition{0}", benchmarkIDVal)];


                                benchmark.goalID = studentGoal.goal.goalID;
                                benchmark.ObjectiveBenchmark = collection[string.Format("StudentGoalBenchmarkTitle{0}", benchmarkIDVal)];

                                //allow multipel methods
                                string methodsStr = collection[string.Format("StudentGoalShorttermBenchmarkMethods{0}", benchmarkIDVal)];
                                string methodsOtherStr = collection[string.Format("StudentGoalShorttermBenchmarkOther{0}", benchmarkIDVal)];

                                List<tblGoalBenchmarkMethod> listTempShortTerms = new List<tblGoalBenchmarkMethod>();

                                if (!string.IsNullOrEmpty(methodsStr))
                                {
                                    string[] methodsArray = methodsStr.Split(',');

                                    foreach (string methodItem in methodsArray)
                                    {
                                        int.TryParse(methodItem, out int methodItemVal);
                                        tblGoalBenchmarkMethod tempShortTerm = new tblGoalBenchmarkMethod();

                                        if (methodItemVal > 0)
                                        {
                                            if (benchmark.goalBenchmarkID > 0)
                                            {
                                                tempShortTerm.goalBenchmarkID = benchmark.goalBenchmarkID;
                                            }

                                            tempShortTerm.EvaluationProcedureID = methodItemVal;
                                            tempShortTerm.OtherDescription = methodsOtherStr;
                                            tempShortTerm.Create_Date = DateTime.Now;
                                            tempShortTerm.Update_Date = DateTime.Now;
                                            tempShortTerm.CreatedBy = ModifiedBy;
                                            tempShortTerm.ModifiedBy = ModifiedBy;
                                            listTempShortTerms.Add(tempShortTerm);
                                        }
                                    }
                                }

                                if (isTemp)
                                {

                                    //need to save new benchmark to allow for saving mulitple short term benchmarks with it
                                    benchmark.ProgressDate_Quarter1 = DateTime.Now;
                                    benchmark.ProgressDate_Quarter2 = DateTime.Now;
                                    benchmark.ProgressDate_Quarter3 = DateTime.Now;
                                    benchmark.ProgressDate_Quarter4 = DateTime.Now;
                                    benchmark.TransitionActivity = benchmark.TransitionActivity;
                                    benchmark.Create_Date = DateTime.Now;
                                    benchmark.Update_Date = DateTime.Now;
                                    benchmark.CreatedBy = ModifiedBy;
                                    benchmark.ModifiedBy = ModifiedBy;

                                    db.tblGoalBenchmarks.Add(benchmark);

                                    db.SaveChanges();

                                    int id = benchmark.goalBenchmarkID;

                                    foreach (tblGoalBenchmarkMethod stb in listTempShortTerms)
                                    {
                                        stb.goalBenchmarkID = id;
                                        db.tblGoalBenchmarkMethods.Add(stb);
                                    }

                                    db.SaveChanges();
                                }
                                else
                                {
                                    studentGoal.benchmarks.Add(benchmark);
                                    if (listTempShortTerms.Count > 0)
                                    {
                                        studentGoal.shortTermBenchmarkMethods.AddRange(listTempShortTerms);
                                    }
                                }


                            }
                        }
                    }
                    studentGoal.SaveGoal(evalProcedures, evalProcOtherStr);
                    goalId = studentGoal.goal.goalID;


                    List<tblGoalBenchmark> newGoalBenchmarks = db.tblGoalBenchmarks.Where(b => b.goalID == goalId && !(existingBenchmarks.Contains(b.goalBenchmarkID))).ToList();

                    List<tblGoalBenchmarkMethod> newBenchmarkMethods = new List<tblGoalBenchmarkMethod>();

                    foreach (tblGoalBenchmark newGoalBenchark in newGoalBenchmarks)
                    {
                        List<tblGoalBenchmarkMethod> newMethod = db.tblGoalBenchmarkMethods.Where(o => o.goalBenchmarkID == newGoalBenchark.goalBenchmarkID).ToList();

                        newBenchmarkMethods.AddRange(newMethod);
                    }

                    return Json(new { Result = "success", Message = "The Student Goal was added.", GoalId = goalId, GoalBenchmarks = newGoalBenchmarks, BenchmarkMethods = newBenchmarkMethods }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to the Student Goals: " + e.InnerException.ToString());
                }
            }

            return Json(new { Result = "error", Message = "The Student Goal was was not added.", GoalId = goalId }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult RemoveStudentTransition(int studentId, int iepId)
        {
            tblIEP theIEP = db.tblIEPs.Where(i => i.IEPid == iepId && i.UserID == studentId).FirstOrDefault();
            if (theIEP != null)
            {
                tblTransition transition = db.tblTransitions.Where(t => t.IEPid == iepId).FirstOrDefault();
                transition.isReleaseBefore21 = false;
                db.SaveChanges();

                return Json(new { Result = "success", Message = "The transition was updated." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "There was an error while trying to access the transition." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult EditProgressReport(FormCollection collection)
        {
            if (ValidateRequest)
            {
                try
                {
                    string verification = collection[0];
                    int goalId = Convert.ToInt32(collection["progressGoalId"]);
                    int studentId = Convert.ToInt32(collection["studentId"]);
                    int iepId = Convert.ToInt32(collection["iepId"]);
                    int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;

                    int j = 4;
                    StudentGoal studentGoal = new StudentGoal(goalId);
                    studentGoal.goal.IEPid = iepId;
                    string printGoal = collection[++j].ToString();
                    studentGoal.goal.ProgressDate_Quarter1 = DateTime.TryParse(collection[++j], out DateTime temp) ? temp : DateTime.Now;
                    studentGoal.goal.ProgressDate_Quarter2 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                    studentGoal.goal.ProgressDate_Quarter3 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                    studentGoal.goal.ProgressDate_Quarter4 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                    studentGoal.goal.Progress_Quarter1 = collection[++j].ToString();
                    studentGoal.goal.Progress_Quarter2 = collection[++j].ToString();
                    studentGoal.goal.Progress_Quarter3 = collection[++j].ToString();
                    studentGoal.goal.Progress_Quarter4 = collection[++j].ToString();
                    studentGoal.goal.ProgressDescription_Quarter1 = collection[++j].ToString();
                    studentGoal.goal.ProgressDescription_Quarter2 = collection[++j].ToString();
                    studentGoal.goal.ProgressDescription_Quarter3 = collection[++j].ToString();
                    studentGoal.goal.ProgressDescription_Quarter4 = collection[++j].ToString();
                    studentGoal.goal.ModifiedBy = ModifiedBy;
                    studentGoal.benchmarks.Clear();

                    int keyNum = ++j;
                    string keyName = (collection.Keys.Count - 1) > keyNum ? collection.GetKey(keyNum) : "";
                    if (!string.IsNullOrEmpty(keyName))
                    {
                        while (keyName.Substring(0, Math.Min(keyName.Length, 20)) == "StudentGoalBenchmark")
                        {
                            bool isBenchmarkID = int.TryParse(collection[j], out int BenchmarkID);
                            tblGoalBenchmark benchmark = (!isBenchmarkID) ? new tblGoalBenchmark() : db.tblGoalBenchmarks.Where(b => b.goalBenchmarkID == BenchmarkID).FirstOrDefault();
                            if (benchmark != null)
                            {
                                benchmark.goalID = studentGoal.goal.goalID;
                                benchmark.TransitionActivity = (collection[++j].ToLower() == "true") ? true : false;
                                benchmark.ProgressDate_Quarter1 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                                benchmark.ProgressDate_Quarter2 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                                benchmark.ProgressDate_Quarter3 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                                benchmark.ProgressDate_Quarter4 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                                benchmark.Progress_Quarter1 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                benchmark.Progress_Quarter2 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                benchmark.Progress_Quarter3 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                benchmark.Progress_Quarter4 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                benchmark.ProgressDescription_Quarter1 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                benchmark.ProgressDescription_Quarter2 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                benchmark.ProgressDescription_Quarter3 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                benchmark.ProgressDescription_Quarter4 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";

                                studentGoal.benchmarks.Add(benchmark);
                            }

                            keyName = (++j < collection.Count) ? collection.GetKey(j) : string.Empty;
                        }
                    }

                    studentGoal.SaveGoal(string.Empty, string.Empty);
                    return Json(new { Result = "success", Message = "The Student Goal was added." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to the Student Goals: " + e.InnerException.ToString());
                }
            }

            return Json(new { Result = "error", Message = "The Student Goal was was not added." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult EditStudentGoalsReadOnly(FormCollection collection)
        {
            int goalId = 0;
            if (ValidateRequest)
            {
                try
                {
                    string verification = collection[0];
                    int studentId = Convert.ToInt32(collection["studentId"]);
                    int iepId = Convert.ToInt32(collection["IEPid"]);
                    int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;

                    int j = 11;
                    //DateTime temp;
                    bool keyParse = int.TryParse(collection[3], out goalId);
                    StudentGoal studentGoal = (!keyParse) ? new StudentGoal() : new StudentGoal(goalId); // new goal : exsisting goal

                    studentGoal.goal.IEPid = iepId;
                    studentGoal.goal.Module = collection[++j].ToString();
                    studentGoal.goal.Title = collection[++j].ToString();
                    studentGoal.goal.hasSerivce = collection[++j] == "true" ? true : false;
                    studentGoal.goal.AnnualGoal = collection[++j].ToString();
                    studentGoal.goal.Baseline = collection[++j].ToString();
                    studentGoal.goal.StateStandards = collection[++j].ToString();
                    studentGoal.goal.Completed = Convert.ToBoolean(collection["completed"]);
                    studentGoal.goal.ModifiedBy = ModifiedBy;

                    studentGoal.benchmarks.Clear();

                    int keyNum = ++j;
                    string keyName = (collection.Keys.Count - 1) > keyNum ? collection.GetKey(keyNum) : "";
                    if (!string.IsNullOrEmpty(keyName))
                    {
                        while (keyName.Substring(0, Math.Min(keyName.Length, 20)) == "StudentGoalBenchmark")
                        {
                            bool isBenchmarkID = int.TryParse(collection[j], out int BenchmarkID);
                            tblGoalBenchmark benchmark = (!isBenchmarkID) ? new tblGoalBenchmark() : db.tblGoalBenchmarks.Where(b => b.goalBenchmarkID == BenchmarkID).FirstOrDefault();

                            if (benchmark != null)
                            {
                                benchmark.goalID = studentGoal.goal.goalID;
                                benchmark.Method = collection[++j] != null && collection[j] != "" ? int.TryParse(collection[j], out int tempInt) ? tempInt : 0 : 0;
                                benchmark.ObjectiveBenchmark = collection[++j].ToString();
                                benchmark.TransitionActivity = (collection[++j].ToLower() == "true") ? true : false;
                                benchmark.Update_Date = DateTime.Now;
                                benchmark.Create_Date = DateTime.Now;

                                if (benchmark.goalBenchmarkID == 0)
                                {
                                    db.tblGoalBenchmarks.Add(benchmark);
                                }

                                db.SaveChanges();
                            }

                            keyName = (++j < collection.Count) ? collection.GetKey(j) : string.Empty;
                        }
                    }

                    db.SaveChanges();

                    return Json(new { Result = "success", Message = "The Student Goal was added.", GoalId = goalId }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to the Student Goals: " + e.InnerException.ToString());
                }
            }

            return Json(new { Result = "error", Message = "The Student Goal was was not added.", GoalId = goalId }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditTransitionAssessements(FormCollection collection)
        {
            if (ValidateRequest)
            {
                bool assessmentFound = false;

                try
                {
                    int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;
                    int studentId = Convert.ToInt32(collection["studentId"]);
                    int iedId = Convert.ToInt32(collection["IEPid"]);
                    int i = 2;


                    tblTransition transition = db.tblTransitions.Where(t => t.IEPid == iedId).FirstOrDefault() ?? new tblTransition();
                    transition.IEPid = iedId;
                    transition.isReleaseBefore21 = true;
                    transition.Assessment_Needs = collection["transitionNeeds"].ToString();
                    transition.Assessment_Strengths = collection["transitionStrengths"].ToString();
                    transition.Assessment_Prefrences = collection["transitionPreferences"].ToString();
                    transition.Assessment_Interest = collection["transitionInterest"].ToString();
                    transition.Update_Date = DateTime.Now;
                    transition.ModifiedBy = ModifiedBy;

                    if (transition.TransitionID == 0)
                    {
                        transition.Create_Date = DateTime.Now;
                        db.tblTransitions.Add(transition);
                    }
                    db.SaveChanges();

                    while (i < collection.Count - 4)
                    {
                        int asmtId = Convert.ToInt32(collection[i++]);

                        tblTransitionAssessment assessment = db.tblTransitionAssessments.Where(a => a.TransitionAssementID == asmtId).FirstOrDefault() ?? new tblTransitionAssessment();
                        assessment.TransitionID = transition.TransitionID;
                        string completedOn = collection[i++];
                        if (completedOn != null && completedOn != "")
                        {
                            assessment.CompletedOn = Convert.ToDateTime(completedOn);
                        }

                        assessment.Performance = collection[i++].ToString();
                        assessment.Narrative = collection[i++].ToString();
                        assessment.IEPid = transition.IEPid;
                        assessment.Update_Date = DateTime.Now;

                        if (assessment.TransitionAssementID == 0)
                        {
                            assessment.Create_Date = DateTime.Now;
                            db.tblTransitionAssessments.Add(assessment);
                        }

                        db.SaveChanges();

                        assessmentFound = true;
                    }

                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to Transition Assessments: " + e.InnerException.ToString());
                }

                if (assessmentFound)
                {
                    return Json(new { Result = "success", Message = "The Student Transition Assessment was updated." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Result = "failure", Message = "At least one Transition Assessment is required." }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { Result = "failure", Message = "The Student Transition Assessment was not added." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteTransitionAssessments(int assessmentId)
        {
            tblTransitionAssessment assessmentToRemove = db.tblTransitionAssessments.Where(a => a.TransitionAssementID == assessmentId).FirstOrDefault();

            if (assessmentToRemove != null)
            {
                db.tblTransitionAssessments.Remove(assessmentToRemove);
                db.SaveChanges();

                return Json(new { Result = "success", Message = "Student Transition Assessment was successfully deleted." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "failure", Message = "The Student Transition Assessment was not deleted." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditTransitionGoals(FormCollection collection)
        {
            if (ValidateRequest)
            {
                try
                {
                    int studentId = Convert.ToInt32(collection["studentId"]);
                    int iedId = Convert.ToInt32(collection["IEPid"]);
                    int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;
                    int i = 2;

                    tblTransition transition = db.tblTransitions.Where(t => t.IEPid == iedId).FirstOrDefault();
                    bool hasEmploymentGoal = false;
                    bool hasEducationalGoal = false;
                    bool canComplete = false;
                    List<TempTransitionItemViewModel> draftGoals = new List<TempTransitionItemViewModel>();

                    while (i < collection.Count - 2)
                    {
                        string tempElementName = collection.AllKeys[i].ToString();
                        int transitionGoalID = Convert.ToInt32(collection[i++]);

                        tblTransitionGoal transitionGoal = db.tblTransitionGoals.Where(g => g.IEPid == iedId && g.TransitionGoalID == transitionGoalID).FirstOrDefault() ?? new tblTransitionGoal();

                        transitionGoal.IEPid = iedId;
                        transitionGoal.TransitionID = transition.TransitionID;
                        transitionGoal.GoalType = collection[i++].ToString();
                        transitionGoal.CompletetionType = collection[i++].ToString();
                        transitionGoal.Behavior = collection[i++].ToString();
                        transitionGoal.WhereAndHow = collection[i++].ToString();
                        transitionGoal.Update_Date = DateTime.Now;
                        transitionGoal.ModifiedBy = ModifiedBy;

                        if (transitionGoalID == 0)
                        {
                            transitionGoal.Create_Date = DateTime.Now;
                            db.tblTransitionGoals.Add(transitionGoal);
                        }

                        if (transitionGoal.GoalType == "education")
                        {
                            hasEducationalGoal = true;
                        }

                        if (transitionGoal.GoalType == "employment")
                        {
                            hasEmploymentGoal = true;
                        }

                        db.SaveChanges();

                        if (transitionGoalID == 0)
                        {
                            draftGoals.Add(new TempTransitionItemViewModel() { TransitionItemID = transitionGoal.TransitionGoalID, ElementName = tempElementName });
                        }
                    }

                    if (hasEmploymentGoal && hasEducationalGoal)
                    {
                        canComplete = true;
                    }
                    else
                    {
                        canComplete = false;
                        return Json(new { Result = "failure", DraftGoals = draftGoals, Message = "At least one Education/Training Goal and one Employment Goal is required." }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { Result = "success", DraftGoals = draftGoals, Message = "The Student Transition Goals were added.", CanComplete = canComplete }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to Transition Assessments: " + e.InnerException.ToString());
                }
            }

            return Json(new { Result = "failure", Message = "The Student Transition Goals were not added." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteTransitionGoal(int goalId)
        {

            tblTransitionGoal goalToRemove = db.tblTransitionGoals.Where(g => g.TransitionGoalID == goalId).FirstOrDefault();

            if (goalToRemove != null)
            {
                db.tblTransitionGoals.Remove(goalToRemove);
                db.SaveChanges();

                return Json(new { Result = "success", Message = "Student Transition Goal was successfully deleted." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "failure", Message = "The Student Transition Goal was not deleted." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditTransitionServices(FormCollection collection)
        {
            if (ValidateRequest)
            {
                bool serviceFound = false;

                try
                {
                    int studentId = Convert.ToInt32(collection["studentId"]);
                    int iedId = Convert.ToInt32(collection["IEPid"]);
                    int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;
                    int serviceCount = collection.Count - 4;
                    int i = 2;
                    List<TempTransitionItemViewModel> draftServices = new List<TempTransitionItemViewModel>();


                    tblTransition transition = db.tblTransitions.Where(t => t.IEPid == iedId).FirstOrDefault();
                    while (i < serviceCount)
                    {
                        string tempElementName = collection.AllKeys[i].ToString();
                        int transitionServiceID = Convert.ToInt32(collection[i++]);
                        tblTransitionService transitionService = db.tblTransitionServices.Where(s => s.IEPid == iedId && s.TransitionServiceID == transitionServiceID).FirstOrDefault() ?? new tblTransitionService();

                        transitionService.IEPid = iedId;
                        transitionService.TransitionID = transition.TransitionID;
                        transitionService.ServiceType = collection[i++].ToString();
                        transitionService.ServiceDescription = collection[i++].ToString();
                        transitionService.Frequency = collection[i++].ToString();
                        transitionService.Location = collection[i++].ToString();
                        transitionService.Duration = collection[i++].ToString();

                        transitionService.ModifiedBy = ModifiedBy;

                        string startDateStr = collection[i++].ToString();
                        string endDateStr = collection[i++].ToString();
                        if (!string.IsNullOrEmpty(startDateStr))
                        {
                            transitionService.StartDate = Convert.ToDateTime(startDateStr);
                        }
                        if (!string.IsNullOrEmpty(endDateStr))
                        {
                            transitionService.EndDate = Convert.ToDateTime(endDateStr);
                        }

                        transitionService.Update_Date = DateTime.Now;
                        if (transitionServiceID == 0)
                        {
                            transitionService.Create_Date = DateTime.Now;
                            db.tblTransitionServices.Add(transitionService);
                        }

                        db.SaveChanges();
                        serviceFound = true;

                        if (transitionServiceID == 0)
                        {
                            draftServices.Add(new TempTransitionItemViewModel() { TransitionItemID = transitionService.TransitionServiceID, ElementName = tempElementName });
                        }
                    }

                    // for catching the Community Participation info at the end of the form.
                    if (i < collection.Count)
                    {
                        transition.CommunityParticipation = collection["isCommunityParticipation"] == "on" ? true : false;
                        transition.CommunityParticipation_Description = collection["communityParticipationDesc"].ToString();

                        db.SaveChanges();
                    }

                    if (serviceFound)
                    {
                        return Json(new { Result = "success", DraftServices = draftServices, Message = "The Student Transition Services were added." }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { Result = "failure", DraftServices = draftServices, Message = "At least one Service must be added." }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to Transition Services: " + e.InnerException.ToString());
                }
            }

            return Json(new { Result = "failure", Message = "The Student Transition Services were not added." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteTransitionService(int serviceId)
        {

            tblTransitionService serviceToRemove = db.tblTransitionServices.Where(s => s.TransitionServiceID == serviceId).FirstOrDefault();

            if (serviceToRemove != null)
            {
                db.tblTransitionServices.Remove(serviceToRemove);
                db.SaveChanges();

                return Json(new { Result = "success", Message = "Student Transition Service was successfully deleted." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "failure", Message = "The Student Transition Service was not deleted." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditTransitionStudy(FormCollection collection)
        {
            if (ValidateRequest)
            {
                try
                {
                    int studentId = Convert.ToInt32(collection["studentId"]);
                    int iedId = Convert.ToInt32(collection["IEPid"]);
                    int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID; ;

                    tblTransition transition = db.tblTransitions.Where(t => t.IEPid == iedId).FirstOrDefault();

                    if (transition == null)
                    {
                        transition = new tblTransition();
                    }

                    transition.Planning_Facilitate = collection["isFocusFunctionalAchievement"] == null ? false : collection["isFocusFunctionalAchievement"] == "on" ? true : false;
                    transition.Planning_Align = collection["isAlignStudentPostGoals"] == null ? false : collection["isAlignStudentPostGoals"] == "on" ? true : false;
                    DateTime graduationDate = collection["graduationYear"] == null || collection["graduationYear"] == "0-00" ? DateTime.Now : (!string.IsNullOrEmpty(collection["graduationYear"])) ? Convert.ToDateTime(collection["graduationYear"]) : DateTime.Now;
                    transition.Planning_GraduationMonth = graduationDate.Month;
                    transition.Planning_GraduationYear = graduationDate.Year;
                    transition.Planning_Completion = (collection["planningCompletion"] != null) ? collection["planningCompletion"].ToString() : string.Empty;
                    transition.Planning_Credits = collection["totalcredits"] == null ? 0 : (!string.IsNullOrEmpty(collection["totalcredits"])) ? Convert.ToDecimal(collection["totalcredits"]) : 0;
                    transition.Planning_BenefitKRS = collection["isVocationalRehabiltiation"] == null ? false : collection["isVocationalRehabiltiation"] == "on" ? true : false;
                    transition.Planning_ConsentPrior = collection["isConfidentailReleaseObtained"] == null ? false : collection["isConfidentailReleaseObtained"] == "on" ? true : false;
                    transition.Planning_Occupation = (collection["occupationText"] != null) ? collection["occupationText"].ToString() : string.Empty;
                    transition.CareerPathID = (collection["CareerPathID"] != null && collection["CareerPathID"] != "") ? Convert.ToInt32(collection["CareerPathID"]) : 0;
                    transition.Planning_BenefitKRS_OtherAgencies = collection["otherAgencies"];
                    transition.isReleaseBefore21 = collection["isReleaseBefore21"] == "1" || collection["isReleaseBefore21"] == "True";
                    transition.ModifiedBy = ModifiedBy;

                    transition.Completed = (collection["isComplete"] != null);
                    db.SaveChanges();

                    return Json(new { Result = "success", Message = "The Student Transition Study was added.", IsComplete = transition.Completed }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to Transition Study: " + e.InnerException.ToString());
                }
            }

            return Json(new { Result = "failure", Message = "The Student Course of Study was not added." }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult EditContingency(FormCollection collection)
        {
            int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;

            if (ValidateRequest)
            {
                Int32.TryParse(collection["studentId"], out int studentId);
                Int32.TryParse(collection["iepId"], out int iepId);

                try
                {
                    bool isAdding = false;
                    var whatever = collection["remoteDistrict"];
                    Boolean.TryParse(collection["isCompleted"], out bool IsCompleted);
                    Boolean.TryParse(collection["noRemote"], out bool NoContingencyPlan);
                    Boolean.TryParse(collection["remoteDistrict"], out bool RemoteLearning_DistrictResponse);
                    Boolean.TryParse(collection["remoteParent"], out bool RemoteLearning_ParentRequest);

                    tblContingencyPlan plan = db.tblContingencyPlans.Where(p => p.IEPid == iepId).FirstOrDefault();
                    if(plan == null)
                    {
                        plan = new tblContingencyPlan() { IEPid = iepId, NoContingencyPlan = true, RemoteLearning_DistrictResponse = false, RemoteLearning_ParentRequest = false, Completed = false };
                        isAdding = true;
                    }

                    plan.Completed = IsCompleted;
                    plan.NoContingencyPlan = NoContingencyPlan;
                    plan.RemoteLearning_DistrictResponse = RemoteLearning_DistrictResponse;
                    plan.RemoteLearning_ParentRequest = RemoteLearning_ParentRequest;

                    if (plan.RemoteLearning_ParentRequest)
                    {
                        plan.Services = collection["remoteParentServices"].ToString();
                        plan.Accommodations = collection["remoteParentAccomodations"].ToString();
                        plan.Goals = collection["remoteParentGoals"].ToString();
                        plan.OtherConsiderations = collection["remoteParentOther"].ToString();
                    }
                    else if (plan.RemoteLearning_DistrictResponse)
                    {
                        plan.Services = collection["remoteDistrictServices"].ToString();
                        plan.Accommodations = collection["remoteDistrictAccommodations"].ToString();
                        plan.Goals = collection["remoteDistrictGoals"].ToString();
                        plan.OtherConsiderations = collection["remoteDistrictOther"].ToString();
                    }
                    else
                    {
                        plan.Services = string.Empty;
                        plan.Accommodations = string.Empty;
                        plan.Goals = string.Empty;
                        plan.OtherConsiderations = string.Empty;
                    }

                    if (isAdding)
                    {
                        db.tblContingencyPlans.Add(plan);
                        tblAuditLog log = new tblAuditLog() { IEPid = plan.IEPid, Create_Date = DateTime.Now, Update_Date = DateTime.Now, TableName = "ContingencyPlan", ModifiedBy = ModifiedBy, UserID = studentId, Value = "Continency Plan Created." };
                    }
                    else
                    {
                        tblAuditLog log = new tblAuditLog() { IEPid = plan.IEPid, Update_Date = DateTime.Now, TableName = "ContingencyPlan", ModifiedBy = ModifiedBy, UserID = studentId, Value = "Continency Plan Edited." };
                    }

                    db.SaveChanges();

                    return RedirectToAction("StudentProcedures", "Home", new { stid = studentId, iepId = plan.IEPid });
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to Student Contingency Plan: " + e.Message.ToString());
                }
            }

            return Json(new { Result = "failure", Message = "The contingency plan was not saved." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult ModuleCompleted(int stdIEPId, string module)
        {
            int ModifiedBy = db.tblUsers.Where(u => u.Email == User.Identity.Name).SingleOrDefault().UserID;
            switch (module)
            {
                case "Health":
                    db.tblIEPHealths.Where(h => h.IEPid == stdIEPId).ToList().ForEach(h => { h.Completed = false; h.ModifiedBy = ModifiedBy; });
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Health Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Motor":
                    db.tblIEPMotors.Where(m => m.IEPid == stdIEPId).ToList().ForEach(m => { m.Completed = false; m.ModifiedBy = ModifiedBy; });
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Motore Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Communication":
                    db.tblIEPCommunications.Where(c => c.IEPid == stdIEPId).ToList().ForEach(c => { c.Completed = false; c.ModifiedBy = ModifiedBy; });
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Communication Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Social":
                    db.tblIEPSocials.Where(s => s.IEPid == stdIEPId).ToList().ForEach(s => { s.Completed = false; s.ModifiedBy = ModifiedBy; });
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Social Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Intelligence":
                    db.tblIEPIntelligences.Where(i => i.IEPid == stdIEPId).ToList().ForEach(i => { i.Completed = false; i.ModifiedBy = ModifiedBy; });
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The General Intelligence Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Academic":
                    db.tblIEPAcademics.Where(a => a.IEPid == stdIEPId).ToList().ForEach(a => { a.Completed = false; a.ModifiedBy = ModifiedBy; });
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Academic Performance Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Other":
                    db.tblOtherConsiderations.Where(o => o.IEPid == stdIEPId).ToList().ForEach(o => { o.Completed = false; o.ModifiedBy = ModifiedBy; });
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Other Considerations Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Goals":
                    db.tblGoals.Where(g => g.IEPid == stdIEPId).ToList().ForEach(g => { g.Completed = false; g.ModifiedBy = ModifiedBy; });
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Goals Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Services":
                    db.tblServices.Where(s => s.IEPid == stdIEPId).ToList().ForEach(s => { s.Completed = false; s.ModifiedBy = ModifiedBy; });
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Service Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Transition":
                    db.tblTransitions.Where(s => s.IEPid == stdIEPId).ToList().ForEach(s => { s.Completed = false; s.ModifiedBy = ModifiedBy; });
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Student Transition was updated." }, JsonRequestBehavior.AllowGet);
                case "Accommodation":
                    List<tblAccommodation> list = db.tblAccommodations.Where(a => a.IEPid == stdIEPId).ToList();
                    foreach (tblAccommodation accomodation in list)
                    {
                        accomodation.Completed = false;
                        accomodation.ModifiedBy = ModifiedBy;
                        db.SaveChanges();
                    }
                    return Json(new { Result = "success", Message = "The Accommodations Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Behavior":
                    db.tblBehaviors.Where(b => b.IEPid == stdIEPId).ToList().ForEach(b => { b.Completed = false; b.ModifiedBy = ModifiedBy; });
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Behavior Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Contingency":
                    db.tblContingencyPlans.Where(p => p.IEPid == stdIEPId).ToList().ForEach(i => { i.Completed = false; });
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Contingency Module was updated." }, JsonRequestBehavior.AllowGet);
                default:
                    return Json(new { Result = "error", Message = "Unable to find the module you requested to update." }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
