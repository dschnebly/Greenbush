using GreenBushIEP.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace GreenBushIEP.Controllers
{
    public class ModuleSectionController : Controller
    {
        private IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        // POST: ModuleSection/Edit/5
        [HttpPost]
        public ActionResult EditHealth(FormCollection collection)
        {
            if (ValidateRequest)
            {
                try
                {
                    var HealthId = Convert.ToInt32(collection["IEPHealthID"]);

                    tblIEPHealth HealthIEP = db.tblIEPHealths.Where(h => h.IEPHealthID == HealthId).SingleOrDefault();
                    tblIEP IEP = db.tblIEPs.Where(i => i.IEPHealthID == HealthIEP.IEPHealthID).FirstOrDefault();
                    if (HealthIEP != null)
                    {
                        HealthIEP.NoConcerns = collection["HealthNoConcerns"] == "on" ? true : false;
                        HealthIEP.Concerns = !HealthIEP.NoConcerns;
                        HealthIEP.ProgressTowardGenEd = collection["HealthProgressTowardGenEd"] == "on" ? true : false;
                        HealthIEP.Diagnosis = collection["HealthDiagnosis"] == "on" ? true : false;
                        HealthIEP.HearingImpaired = (!String.IsNullOrEmpty(collection["HearingImpaired"])) ? true : false;
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
                        HealthIEP.NeedMetByGoal = collection["MetByGoal"] == "on" ? true : false;
                        HealthIEP.NeedMetByAccommodation = collection["MetByAccommodation"] == "on" ? true : false;
                        HealthIEP.NeedMetByOther = collection["MetByOther"] == "on" ? true : false;
                        HealthIEP.NeedMetByOtherDescription = collection["MeetNeedByOtherDescription"].ToString();
                        HealthIEP.Completed = Convert.ToBoolean(collection["Completed"]);

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
                    var MotorId = Convert.ToInt32(collection["IEPMotorID"]);

                    tblIEPMotor MotorIEP = db.tblIEPMotors.Where(m => m.IEPMotorID == MotorId).SingleOrDefault();
                    tblIEP IEP = db.tblIEPs.Where(i => i.IEPMotorID == MotorIEP.IEPMotorID).FirstOrDefault();
                    if (MotorIEP != null)
                    {
                        MotorIEP.NoConcerns = collection["ModuleMotorNoConcerns"] == "on" ? true : false;
                        MotorIEP.ProgressTowardGenEd = collection["ModuleMotorProgressTowardGenEd"] == "on" ? true : false;
                        MotorIEP.Needs = collection["ModuleMotorNeeds"] == "on" ? true : false;
                        MotorIEP.Participation = Convert.ToInt32(collection["ModuleMotorParticipation"]);
                        MotorIEP.PLAAFP_Strengths = collection["PLAAFP_Strengths"].ToString();
                        MotorIEP.PLAAFP_Concerns = collection["PLAAFP_Concerns"].ToString();
                        MotorIEP.AreaOfNeedDescription = collection["ModuleMotorAreaOfNeedDescription"].ToString();
                        MotorIEP.NeedMetByGoal = collection["MetByGoal"] == "on" ? true : false;
                        MotorIEP.NeedMetByAccommodation = collection["MetByAccommodation"] == "on" ? true : false;
                        MotorIEP.NeedMetByOther = collection["MetByOther"] == "on" ? true : false;
                        MotorIEP.NeedMetByOtherDescription = collection["MeetNeedByOtherDescription"].ToString();
                        MotorIEP.Completed = Convert.ToBoolean(collection["Completed"]);

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
                    var CommunicationId = Convert.ToInt32(collection["IEPCommunicationID"]);

                    tblIEPCommunication CommunicationIEP = db.tblIEPCommunications.Where(c => c.IEPCommunicationID == CommunicationId).SingleOrDefault();
                    tblIEP IEP = db.tblIEPs.Where(i => i.IEPCommunicationID == CommunicationIEP.IEPCommunicationID).FirstOrDefault();
                    if (CommunicationIEP != null)
                    {

                        CommunicationIEP.NoConcerns = collection["ModuleCommunicationNoConcerns"] == "on" ? true : false;
                        CommunicationIEP.ProgressTowardGenEd = collection["ModuleCommunicationProgressTowardGenEd"] == "on" ? true : false;
                        CommunicationIEP.AreaOfNeed = (!String.IsNullOrEmpty(collection["ModuleCommunicationAreaOfNeed"])) ? true : false;
                        CommunicationIEP.Deaf = collection["ModuleCommunicationDeaf"] == "on" ? true : false;
                        CommunicationIEP.LimitedEnglish = collection["ModuleCommunicationDeaf"] == "on" ? true : false;
                        CommunicationIEP.PLAAFP_Strengths = collection["PLAAFP_Strengths"].ToString();
                        CommunicationIEP.PLAAFP_Concerns = collection["PLAAFP_Concerns"].ToString();
                        CommunicationIEP.AreaOfNeedDescription = collection["ModuleCommunicationAreaOfNeedDescription"].ToString();
                        CommunicationIEP.NeedMetByGoal = collection["MetByGoal"] == "on" ? true : false;
                        CommunicationIEP.NeedMetByAccommodation = collection["MetByAccommodation"] == "on" ? true : false;
                        CommunicationIEP.NeedMetByOther = collection["MetByOther"] == "on" ? true : false;
                        CommunicationIEP.NeedMetByOtherDescription = collection["MeetNeedByOtherDescription"].ToString();
                        CommunicationIEP.Completed = Convert.ToBoolean(collection["Completed"]);

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
                    var socialId = Convert.ToInt32(collection["IEPSocialID"]);

                    tblIEPSocial SocialIEP = db.tblIEPSocials.Where(s => s.IEPSocialID == socialId).SingleOrDefault();
                    tblIEP IEP = db.tblIEPs.Where(i => i.IEPSocialID == socialId).FirstOrDefault();
                    if (SocialIEP != null)
                    {
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
                        SocialIEP.NeedMetByGoal = collection["MetByGoal"] == "on" ? true : false;
                        SocialIEP.NeedMetByAccommodation = collection["MetByAccommodation"] == "on" ? true : false;
                        SocialIEP.NeedMetByOther = collection["MetByOther"] == "on" ? true : false;
                        SocialIEP.NeedMetByOtherDescription = collection["MeetNeedByOtherDescription"].ToString();
                        SocialIEP.Completed = Convert.ToBoolean(collection["Completed"]);

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
                    var intelligenceId = Convert.ToInt32(collection["IEPIntelligenceID"]);

                    tblIEPIntelligence IntellgienceIEP = db.tblIEPIntelligences.Where(i => i.IEPIntelligenceID == intelligenceId).SingleOrDefault();
                    tblIEP IEP = db.tblIEPs.Where(i => i.IEPIntelligenceID == intelligenceId).FirstOrDefault();
                    if (IntellgienceIEP != null)
                    {
                        IntellgienceIEP.Concerns = !(collection["ModuleIntelligenceNoConcerns"] == "on" ? true : false);
                        IntellgienceIEP.ProgressTowardGenEd = collection["ModuleIntelligenceProgressTowardGenEd"] == "on" ? true : false;
                        IntellgienceIEP.AreaOfNeed = collection["ModuleIntelligenceAreaOfNeed"] == "on" ? true : false;
                        IntellgienceIEP.AreaOfNeedDescription = collection["ModuleIntelligenceAreaOfNeedDescription"].ToString();
                        IntellgienceIEP.PLAAFP_Strengths = collection["PLAAFP_Strengths"].ToString();
                        IntellgienceIEP.PLAAFP_Concerns = collection["PLAAFP_Concerns"].ToString();
                        IntellgienceIEP.Completed = Convert.ToBoolean(collection["Completed"]);

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
                    var academicID = Convert.ToInt32(collection["Academic.IEPAcademicID"]);
                    var readingID = Convert.ToInt32(collection["Reading.IEPReadingID"]);
                    var mathID = Convert.ToInt32(collection["Math.IEPMathID"]);
                    var writtenID = Convert.ToInt32(collection["Written.IEPWrittenID"]);

                    ModuleAcademicViewModel viewModel = new ModuleAcademicViewModel();

                    viewModel.Academic = db.tblIEPAcademics.Where(a => a.IEPAcademicID == academicID).SingleOrDefault();
                    viewModel.Reading = db.tblIEPReadings.Where(r => r.IEPReadingID == readingID).SingleOrDefault();
                    viewModel.Math = db.tblIEPMaths.Where(m => m.IEPMathID == mathID).SingleOrDefault();
                    viewModel.Written = db.tblIEPWrittens.Where(w => w.IEPWrittenID == writtenID).SingleOrDefault();

                    tblIEP IEP = db.tblIEPs.Where(i => i.IEPAcademicID == academicID).FirstOrDefault();
                    if (IEP != null)
                    {
                        viewModel.Academic.NoConcerns = collection["ModuleAcademicNoConcern"] == "on" ? true : false;
                        viewModel.Academic.ProgressTowardGenEd = collection["ModuleAcademicProgressTowardGenEd"] == "on" ? true : false;
                        viewModel.Academic.AreaOfNeed = collection["ModuleAcademicAreaOfNeed"] == "on" ? true : false;
                        viewModel.Academic.PLAAFP_Strengths = collection["ModuleAcademic_PLAAFP_Strengths"];
                        viewModel.Academic.PLAAFP_Concerns = collection["ModuleAcademic_PLAAFP_Concerns"];
                        viewModel.Academic.AreaOfNeedDescription = collection["ModuleAcademicAreaOfNeedDescription"].ToString();
                        viewModel.Academic.NeedMetByGoal = collection["ModuleAcademicMetByGoal"] == "on" ? true : false;
                        viewModel.Academic.NeedMetByAccommodation = collection["ModuleAcademicMetByAccommodation"] == "on" ? true : false;
                        viewModel.Academic.NeedMetByOther = collection["ModuleAcademicMetByOther"] == "on" ? true : false;
                        viewModel.Academic.NeedMetByOtherDescription = collection["ModuleAcademicMeetNeedByOtherDescription"].ToString();
                        viewModel.Academic.Completed = Convert.ToBoolean(collection["Academic.Completed"]);

                        viewModel.Reading.NoConcerns = collection["ModuleReadingNoConcern"] == "on" ? true : false;
                        viewModel.Reading.ProgressTowardGenEd = collection["ModuleReadingProgressTowardGenEd"] == "on" ? true : false;
                        viewModel.Reading.AreaOfNeed = collection["ModuleReadingAreaOfNeed"] == "on" ? true : false;
                        viewModel.Reading.PLAAFP_Strengths = collection["ModuleReading_PLAAFP_Strengths"];
                        viewModel.Reading.PLAAFP_Concerns = collection["ModuleReading_PLAAFP_Concerns"];
                        viewModel.Reading.AreaOfNeedDescription = collection["ModuleReadingAreaOfNeedDescription"].ToString();
                        viewModel.Reading.NeedMetByGoal = collection["ModuleReadingMetByGoal"] == "on" ? true : false;
                        viewModel.Reading.NeedMetByAccommodation = collection["ModuleReadingMetByAccommodation"] == "on" ? true : false;
                        viewModel.Reading.NeedMetByOther = collection["ModuleReadingMetByOther"] == "on" ? true : false;
                        viewModel.Reading.NeedMetByOtherDescription = collection["ModuleReadingMeetNeedByOtherDescription"].ToString();

                        viewModel.Math.NoConcerns = collection["ModuleMathNoConcern"] == "on" ? true : false;
                        viewModel.Math.ProgressTowardGenEd = collection["ModuleMathProgressTowardGenEd"] == "on" ? true : false;
                        viewModel.Math.AreaOfNeed = collection["ModuleMathAreaOfNeed"] == "on" ? true : false;
                        viewModel.Math.PLAAFP_Strengths = collection["ModuleMath_PLAAFP_Strengths"];
                        viewModel.Math.PLAAFP_Concerns = collection["ModuleMath_PLAAFP_Concerns"];
                        viewModel.Math.AreaOfNeedDescription = collection["ModuleMathAreaOfNeedDescription"].ToString();
                        viewModel.Math.NeedMetByGoal = collection["ModuleMathMetByGoal"] == "on" ? true : false;
                        viewModel.Math.NeedMetByAccommodation = collection["ModuleMathMetByAccommodation"] == "on" ? true : false;
                        viewModel.Math.NeedMetByOther = collection["ModuleMathMetByOther"] == "on" ? true : false;
                        viewModel.Math.NeedMetByOtherDescription = collection["ModuleMathMeetNeedByOtherDescription"].ToString();

                        viewModel.Written.NoConcerns = collection["ModuleWrittenNoConcern"] == "on" ? true : false;
                        viewModel.Written.ProgressTowardGenEd = collection["ModuleWrittenProgressTowardGenEd"] == "on" ? true : false;
                        viewModel.Written.AreaOfNeed = collection["ModuleWrittenAreaOfNeed"] == "on" ? true : false;
                        viewModel.Written.PLAAFP_Strengths = collection["ModuleWritten_PLAAFP_Strengths"];
                        viewModel.Written.PLAAFP_Concerns = collection["ModuleWritten_PLAAFP_Concerns"];
                        viewModel.Written.AreaOfNeedDescription = collection["ModuleWrittenAreaOfNeedDescription"].ToString();
                        viewModel.Written.NeedMetByGoal = collection["ModuleWrittenMetByGoal"] == "on" ? true : false;
                        viewModel.Written.NeedMetByAccommodation = collection["ModuleWrittenMetByAccommodation"] == "on" ? true : false;
                        viewModel.Written.NeedMetByOther = collection["ModuleWrittenMetByOther"] == "on" ? true : false;
                        viewModel.Written.NeedMetByOtherDescription = collection["ModuleWrittenMeetNeedByOtherDescription"].ToString();

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
            if (ModelState.IsValid)
            {
                tblBehavior BehaviorIEP = db.tblBehaviors.Where(c => c.BehaviorID == model.BehaviorID).FirstOrDefault();
                int studentId = model.StudentId;
                int iepId = model.IEPid;
                int behaviorId = model.BehaviorID;

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

                if (behaviorId == 0)
                {
                    BehaviorIEP.Create_Date = DateTime.Now;
                    db.tblBehaviors.Add(BehaviorIEP);
                }
                db.SaveChanges();
                behaviorId = BehaviorIEP.BehaviorID;

                //triggers
                var existingTriggers = db.tblBehaviorTriggers.Where(o => o.BehaviorID == behaviorId);
                foreach (var existingTrigger in existingTriggers)
                {
                    db.tblBehaviorTriggers.Remove(existingTrigger);
                }

                db.SaveChanges();

                foreach (var trigger in model.SelectedTriggers)
                {
                    var tType = db.tblBehaviorTriggerTypes.Where(o => o.BehaviorTriggerTypeID == trigger).FirstOrDefault();
                    string otherDesc = "";
                    if (tType != null && tType.BehaviorTriggerType.ToUpper() == "OTHER")
                    {
                        otherDesc = model.TriggerOther;
                    }
                    db.tblBehaviorTriggers.Add(new tblBehaviorTrigger { IEPid = iepId, BehaviorID = behaviorId, BehaviorTriggerTypeID = trigger, Create_Date = DateTime.Now, OtherDescription = otherDesc });
                }
                db.SaveChanges();

                //hypotheses 
                var existingHypotheses = db.tblBehaviorHypothesis.Where(o => o.BehaviorID == behaviorId);

                foreach (var existingHypothesis in existingHypotheses)
                {
                    db.tblBehaviorHypothesis.Remove(existingHypothesis);
                }
                db.SaveChanges();

                foreach (var hypothesis in model.SelectedHypothesis)
                {
                    var tType = db.tblBehaviorHypothesisTypes.Where(o => o.BehaviorHypothesisTypeID == hypothesis).FirstOrDefault();
                    string otherDesc = "";
                    if (tType != null && tType.BehaviorHypothesisType.ToUpper() == "OTHER")
                    {
                        otherDesc = model.HypothesisOther;
                    }
                    db.tblBehaviorHypothesis.Add(new tblBehaviorHypothesi { IEPid = iepId, BehaviorID = behaviorId, BehaviorHypothesisTypeID = hypothesis, Create_Date = DateTime.Now, OtherDescription = otherDesc });
                }
                db.SaveChanges();

                //Strategies  
                var existingStrategies = db.tblBehaviorStrategies.Where(o => o.BehaviorID == behaviorId);
                foreach (var existingStrategy in existingStrategies)
                {
                    db.tblBehaviorStrategies.Remove(existingStrategy);
                }
                db.SaveChanges();

                foreach (var strategy in model.SelectedStrategies)
                {
                    var tType = db.tblBehaviorStrategyTypes.Where(o => o.BehaviorStrategyTypeID == strategy).FirstOrDefault();
                    string otherDesc = "";
                    if (tType != null && tType.BehaviorStrategyType.ToUpper() == "OTHER")
                    {
                        otherDesc = model.StrategiesOther;
                    }
                    db.tblBehaviorStrategies.Add(new tblBehaviorStrategy { IEPid = iepId, BehaviorID = behaviorId, BehaviorStrategyTypeID = strategy, Create_Date = DateTime.Now, OtherDescription = otherDesc });
                }
                db.SaveChanges();

                //targeted behaviors

                //1
                var tbid = collection["targetId1"].ToString();
                var tbBehavior = collection["tbBehavior1"].ToString();
                var tbBaseline = collection["tbBaseline1"].ToString();
                if (tbid == "0")
                {
                    //new tbl
                    db.tblBehaviorBaselines.Add(new tblBehaviorBaseline { IEPid = iepId, BehaviorID = behaviorId, Behavior = tbBehavior, Baseline = tbBaseline, Create_Date = DateTime.Now });
                }
                else
                {
                    int behaviorBaselineID = 0;
                    Int32.TryParse(tbid, out behaviorBaselineID);
                    var existingtb1 = db.tblBehaviorBaselines.Where(o => o.BehaviorID == behaviorId && o.BehaviorBaselineID == behaviorBaselineID).FirstOrDefault();
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
                    db.tblBehaviorBaselines.Add(new tblBehaviorBaseline { IEPid = iepId, BehaviorID = behaviorId, Behavior = tbBehavior, Baseline = tbBaseline, Create_Date = DateTime.Now });
                }
                else
                {
                    int behaviorBaselineID = 0;
                    Int32.TryParse(tbid, out behaviorBaselineID);
                    var existingtb2 = db.tblBehaviorBaselines.Where(o => o.BehaviorID == behaviorId && o.BehaviorBaselineID == behaviorBaselineID).FirstOrDefault();
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
                    db.tblBehaviorBaselines.Add(new tblBehaviorBaseline { IEPid = iepId, BehaviorID = behaviorId, Behavior = tbBehavior, Baseline = tbBaseline, Create_Date = DateTime.Now });
                }
                else
                {
                    int behaviorBaselineID = 0;
                    Int32.TryParse(tbid, out behaviorBaselineID);
                    var existingtb3 = db.tblBehaviorBaselines.Where(o => o.BehaviorID == behaviorId && o.BehaviorBaselineID == behaviorBaselineID).FirstOrDefault();
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
                Int32.TryParse(collection["StudentId"].ToString(), out studentId);

                model.AssistiveTechnology_Require = collection["AssistiveTechnology_Require"] == "on" ? true : false;
                model.Parental_Concerns_flag = collection["Parental_Concerns_flag"] == "on" ? true : false;
                model.Parental_CopyIEP_flag = collection["Parental_CopyIEP_flag"] == "on" ? true : false;
                model.Parental_RightsBook_flag = collection["Parental_Rightsbook_flag"] == "on" ? true : false;
                model.ExtendedSchoolYear_RegressionRisk = collection["ExtendedSchoolYear_RegressionRisk"] == "on" ? true : false;
                model.ExtendedSchoolYear_SeverityRisk = collection["ExtendedSchoolYear_SeverityRisk"] == "on" ? true : false;
                model.Completed = Convert.ToBoolean(collection["Completed"]);

                var dwa = collection["DistrictWideAssessments"];
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

                var swa = collection["StateWideAssessments"];
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

                var tp = collection["TransporationPlan"];
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

                var vehicleTypeValue = collection["inputVehicleType"];
                var beginDate = collection["inputBegin"];
                var endDate = collection["inputEnd"];
                var minutesValue = collection["inputMinutes"];
                var vehicleType = "";
                var minutes = "25";
                if (!string.IsNullOrEmpty(vehicleTypeValue))
                {
                    if (vehicleTypeValue == "1")
                        vehicleType = "special education";
                    else if (vehicleTypeValue == "2")
                        vehicleType = "general education";
                    else
                        vehicleType = "";
                }

                minutes = string.IsNullOrEmpty(minutesValue) ? "25" : minutesValue;

                otherDesc = string.Format(@"The student will receive transportation each day that school is in session, on a {0} vehicle, from the time the student boards the vehicle from the departure point until arrival at the destination and from the time the student boards the vehicle until arrival at the returning destination. ({1} minutes estimated normal commute) beginning on {2} and ending on {3} following the school calendar.", vehicleType, minutes, beginDate, endDate);

                //find existing if updating
                tblOtherConsideration OC = db.tblOtherConsiderations.Where(c => c.OtherConsiderationID == model.OtherConsiderationID).FirstOrDefault();

                if (OC == null)
                {
                    model.Create_Date = DateTime.Now;
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
                    OC.ExtendedSchoolYear_RegressionRisk = model.ExtendedSchoolYear_RegressionRisk;
                    OC.ExtendedSchoolYear_SeverityRisk = model.ExtendedSchoolYear_SeverityRisk;
                    OC.ExtendedSchoolYear_Justification = model.ExtendedSchoolYear_Justification;
                    OC.Parental_Concerns_flag = model.Parental_Concerns_flag;
                    OC.Parental_Concerns_Desc = model.Parental_Concerns_Desc;
                    OC.Parental_CopyIEP_flag = model.Parental_CopyIEP_flag;
                    OC.Parental_RightsBook_flag = model.Parental_RightsBook_flag;
                    OC.Completed = model.Completed;
                    OC.Create_Date = DateTime.Now;

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
                tblAccommodation accommodation = db.tblAccommodations.Where(a => a.AccommodationID == model.AccommodationID).FirstOrDefault();
                if (accommodation != null)
                {
                    accommodation.AccomType = model.AccomType;
                    accommodation.Completed = model.Completed;
                    accommodation.Description = model.Description;
                    accommodation.Duration = model.Duration;
                    accommodation.Frequency = model.Frequency;
                    accommodation.Location = model.Location;
                    accommodation.IEPid = model.IEPid;

                    if (model.AnticipatedStartDate.HasValue)
                        accommodation.AnticipatedStartDate = model.AnticipatedStartDate;

                    if (model.AnticipatedEndDate.HasValue)
                        accommodation.AnticipatedEndDate = model.AnticipatedEndDate;

                    accommodation.Update_Date = DateTime.Now;
                    db.SaveChanges();

                    return Json(new { success = true, id = accommodation.AccommodationID, iep = accommodation.IEPid, isNew = false });
                }
                else
                {
                    tblAccommodation newAccomodation = new tblAccommodation();
                    newAccomodation.AccomType = model.AccomType;
                    newAccomodation.Completed = model.Completed;
                    newAccomodation.Description = model.Description;
                    newAccomodation.Duration = model.Duration;
                    newAccomodation.Frequency = model.Frequency;
                    newAccomodation.Location = model.Location;
                    newAccomodation.IEPid = model.IEPid;

                    if (model.AnticipatedStartDate.HasValue)
                        newAccomodation.AnticipatedStartDate = model.AnticipatedStartDate;

                    if (model.AnticipatedEndDate.HasValue)
                        newAccomodation.AnticipatedEndDate = model.AnticipatedEndDate;

                    newAccomodation.Update_Date = DateTime.Now;
                    newAccomodation.Create_Date = DateTime.Now;

                    db.tblAccommodations.Add(newAccomodation);
                    db.SaveChanges();

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

                var evalsToDelete = db.tblGoalEvaluationProcedures.Where(g => g.goalID == studentGoalId).ToList();
                foreach (tblGoalEvaluationProcedure objEP in evalsToDelete)
                {
                    db.tblGoalEvaluationProcedures.Remove(objEP);
                    db.SaveChanges();
                }

                var benchmarksToDelete = db.tblGoalBenchmarks.Where(g => g.goalID == studentGoalId).ToList();
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
                    var healthGoal = db.tblIEPHealths.Where(g => g.IEPid == iepID && g.Concerns).FirstOrDefault();
                    if (healthGoal != null)
                        baselineText = healthGoal.PLAAFP_Concerns;
                    break;
                case 2:
                    var motorGoal = db.tblIEPMotors.Where(g => g.IEPid == iepID && !g.NoConcerns).FirstOrDefault();
                    if (motorGoal != null)
                        baselineText = motorGoal.PLAAFP_Concerns;
                    break;
                case 3:
                    var commGoal = db.tblIEPCommunications.Where(g => g.IEPid == iepID && !g.NoConcerns).FirstOrDefault();
                    if (commGoal != null)
                        baselineText = commGoal.PLAAFP_Concerns;
                    break;
                case 4:
                    var socialGoal = db.tblIEPSocials.Where(g => g.IEPid == iepID && !g.NoConcerns).FirstOrDefault();
                    if (socialGoal != null)
                        baselineText = socialGoal.PLAAFP_Concerns;
                    break;
                case 5:
                    var giGoal = db.tblIEPIntelligences.Where(g => g.IEPid == iepID).FirstOrDefault();
                    if (giGoal != null)
                        baselineText = giGoal.PLAAFP_Concerns;
                    break;
                case 6:
                    var academicGoal = db.tblIEPAcademics.Where(g => g.IEPid == iepID && !g.NoConcerns).FirstOrDefault();
                    if (academicGoal != null)
                        baselineText = academicGoal.PLAAFP_Concerns;
                    break;
                case 7:
                    var readGoal = db.tblIEPReadings.Where(g => g.IEPid == iepID && !g.NoConcerns).FirstOrDefault();
                    if (readGoal != null)
                        baselineText = readGoal.PLAAFP_Concerns;
                    break;
                case 8:
                    var mathGoal = db.tblIEPMaths.Where(g => g.IEPid == iepID && !g.NoConcerns).FirstOrDefault();
                    if (mathGoal != null)
                        baselineText = mathGoal.PLAAFP_Concerns;
                    break;
                case 9:
                    var writtenGoal = db.tblIEPWrittens.Where(g => g.IEPid == iepID && !g.NoConcerns).FirstOrDefault();
                    if (writtenGoal != null)
                        baselineText = writtenGoal.PLAAFP_Concerns;
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

                    int j = 3;
                    //DateTime temp;
                    bool keyParse = Int32.TryParse(collection[++j], out goalId);
                    StudentGoal studentGoal = (!keyParse) ? new StudentGoal() : new StudentGoal(goalId); // new goal : exsisting goal

                    studentGoal.goal.IEPid = iepId;
                    studentGoal.goal.Module = collection[++j].ToString();
                    studentGoal.goal.Title = collection[++j].ToString();
                    studentGoal.goal.hasSerivce = collection[++j] == "true" ? true : false;
                    studentGoal.goal.AnnualGoal = collection[++j].ToString();
                    studentGoal.goal.Baseline = collection[++j].ToString();
                    studentGoal.goal.StateStandards = collection[++j].ToString();
                    studentGoal.goal.Completed = Convert.ToBoolean(collection["completed"]);

                    var evalProcedures = "";
                    if (collection.AllKeys.Where(o => o.Contains("StudentGoalBenchmarkMethods")).Any())
                    {
                        var evalProceduresStr = collection.AllKeys.Where(o => o.Contains("StudentGoalBenchmarkMethods")).FirstOrDefault();
                        evalProcedures = collection[evalProceduresStr];
                        if (evalProcedures != null)
                        {
                            j++; //only increment when values are submitted otherwise it throws the count off for the rest
                        }
                    }

                    int tempInt;
                    studentGoal.benchmarks.Clear();

                    int keyNum = ++j;
                    string keyName = (collection.Keys.Count - 1) > keyNum ? collection.GetKey(keyNum) : "";
                    if (!string.IsNullOrEmpty(keyName))
                    {
                        while (keyName.Substring(0, Math.Min(keyName.Length, 20)) == "StudentGoalBenchmark")
                        {
                            int BenchmarkID;
                            bool isBenchmarkID = Int32.TryParse(collection[j], out BenchmarkID);
                            tblGoalBenchmark benchmark = (!isBenchmarkID) ? new tblGoalBenchmark() : db.tblGoalBenchmarks.Where(b => b.goalBenchmarkID == BenchmarkID).FirstOrDefault();
                            if (benchmark != null)
                            {
                                benchmark.goalID = studentGoal.goal.goalID;
                                benchmark.Method = collection[++j] != null && collection[j] != "" ? Int32.TryParse(collection[j], out tempInt) ? tempInt : 0 : 0;
                                benchmark.ObjectiveBenchmark = collection[++j].ToString();
                                benchmark.TransitionActivity = (collection[++j].ToLower() == "true") ? true : false;

                                studentGoal.benchmarks.Add(benchmark);
                            }

                            keyName = (++j < collection.Count) ? collection.GetKey(j) : String.Empty;
                        }
                    }

                    studentGoal.SaveGoal(evalProcedures);
                    goalId = studentGoal.goal.goalID;

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
        [Authorize]
        public ActionResult EditProgressReport(FormCollection collection)
        {
            if (ValidateRequest)
            {
                try
                {
                    DateTime temp;
                    string verification = collection[0];
                    int goalId = Convert.ToInt32(collection["progressGoalId"]);
                    int studentId = Convert.ToInt32(collection["studentId"]);
                    int iepId = Convert.ToInt32(collection["iepId"]);

                    int j = 4;
                    StudentGoal studentGoal = new StudentGoal(goalId);
                    studentGoal.goal.IEPid = iepId;
                    studentGoal.goal.ProgressDate_Quarter1 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
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
                    studentGoal.benchmarks.Clear();

                    int keyNum = ++j;
                    string keyName = (collection.Keys.Count - 1) > keyNum ? collection.GetKey(keyNum) : "";
                    if (!string.IsNullOrEmpty(keyName))
                    {
                        while (keyName.Substring(0, Math.Min(keyName.Length, 20)) == "StudentGoalBenchmark")
                        {
                            int BenchmarkID;
                            bool isBenchmarkID = Int32.TryParse(collection[j], out BenchmarkID);
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

                            keyName = (++j < collection.Count) ? collection.GetKey(j) : String.Empty;
                        }
                    }

                    studentGoal.SaveGoal(String.Empty);
                    return Json(new { Result = "success", Message = "The Student Goal was added." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to the Student Goals: " + e.InnerException.ToString());
                }
            }

            return Json(new { Result = "error", Message = "The Student Goal was was not added."}, JsonRequestBehavior.AllowGet);
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

                    int j = 11;
                    //DateTime temp;
                    bool keyParse = Int32.TryParse(collection[3], out goalId);
                    StudentGoal studentGoal = (!keyParse) ? new StudentGoal() : new StudentGoal(goalId); // new goal : exsisting goal

                    studentGoal.goal.IEPid = iepId;
                    studentGoal.goal.Module = collection[++j].ToString();
                    studentGoal.goal.Title = collection[++j].ToString();
                    studentGoal.goal.hasSerivce = collection[++j] == "true" ? true : false;
                    studentGoal.goal.AnnualGoal = collection[++j].ToString();
                    studentGoal.goal.Baseline = collection[++j].ToString();
                    studentGoal.goal.StateStandards = collection[++j].ToString();
                    studentGoal.goal.Completed = Convert.ToBoolean(collection["completed"]);

                    //var evalProcedures = "";
                    //if (collection.AllKeys.Where(o => o.Contains("StudentGoalBenchmarkMethods")).Any())
                    //{
                    //    var evalProceduresStr = collection.AllKeys.Where(o => o.Contains("StudentGoalBenchmarkMethods")).FirstOrDefault();
                    //    evalProcedures = collection[evalProceduresStr];
                    //    if (evalProcedures != null)
                    //    {
                    //        j++; //only increment when values are submitted otherwise it throws the count off for the rest
                    //    }
                    //}

                    //studentGoal.goal.ProgressDate_Quarter1 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                    //studentGoal.goal.ProgressDate_Quarter2 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                    //studentGoal.goal.ProgressDate_Quarter3 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                    //studentGoal.goal.ProgressDate_Quarter4 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                    //studentGoal.goal.Progress_Quarter1 = collection[++j].ToString();
                    //studentGoal.goal.Progress_Quarter2 = collection[++j].ToString();
                    //studentGoal.goal.Progress_Quarter3 = collection[++j].ToString();
                    //studentGoal.goal.Progress_Quarter4 = collection[++j].ToString();
                    //studentGoal.goal.ProgressDescription_Quarter1 = collection[++j].ToString();
                    //studentGoal.goal.ProgressDescription_Quarter2 = collection[++j].ToString();
                    //studentGoal.goal.ProgressDescription_Quarter3 = collection[++j].ToString();
                    //studentGoal.goal.ProgressDescription_Quarter4 = collection[++j].ToString();

                    int tempInt;
                    studentGoal.benchmarks.Clear();

                    int keyNum = ++j;
                    string keyName = (collection.Keys.Count - 1) > keyNum ? collection.GetKey(keyNum) : "";
                    if (!string.IsNullOrEmpty(keyName))
                    {
                        while (keyName.Substring(0, Math.Min(keyName.Length, 20)) == "StudentGoalBenchmark")
                        {
                            int BenchmarkID;
                            bool isBenchmarkID = Int32.TryParse(collection[j], out BenchmarkID);
                            tblGoalBenchmark benchmark = (!isBenchmarkID) ? new tblGoalBenchmark() : db.tblGoalBenchmarks.Where(b => b.goalBenchmarkID == BenchmarkID).FirstOrDefault();

                            if (benchmark != null)
                            {
                                benchmark.goalID = studentGoal.goal.goalID;
                                benchmark.Method = collection[++j] != null && collection[j] != "" ? Int32.TryParse(collection[j], out tempInt) ? tempInt : 0 : 0;
                                benchmark.ObjectiveBenchmark = collection[++j].ToString();
                                benchmark.TransitionActivity = (collection[++j].ToLower() == "true") ? true : false;
                                //benchmark.ProgressDate_Quarter1 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                                //benchmark.ProgressDate_Quarter2 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                                //benchmark.ProgressDate_Quarter3 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                                //benchmark.ProgressDate_Quarter4 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                                //benchmark.Progress_Quarter1 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                //benchmark.Progress_Quarter2 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                //benchmark.Progress_Quarter3 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                //benchmark.Progress_Quarter4 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                //benchmark.ProgressDescription_Quarter1 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                //benchmark.ProgressDescription_Quarter2 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                //benchmark.ProgressDescription_Quarter3 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                //benchmark.ProgressDescription_Quarter4 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                benchmark.Update_Date = DateTime.Now;
                                benchmark.Create_Date = DateTime.Now;

                                if (benchmark.goalBenchmarkID == 0)
                                {
                                    db.tblGoalBenchmarks.Add(benchmark);
                                }

                                db.SaveChanges();
                            }

                            keyName = (++j < collection.Count) ? collection.GetKey(j) : String.Empty;
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
                try
                {
                    int studentId = Convert.ToInt32(collection["studentId"]);
                    int iedId = Convert.ToInt32(collection["IEPid"]);
                    int i = 2;

                    tblTransition transition = db.tblTransitions.Where(t => t.IEPid == iedId).FirstOrDefault() ?? new tblTransition();
                    transition.IEPid = iedId;
                    transition.Assessment_Needs = collection["transitionNeeds"].ToString();
                    transition.Assessment_Strengths = collection["transitionStrengths"].ToString();
                    transition.Assessment_Prefrences = collection["transitionPreferences"].ToString();
                    transition.Assessment_Interest = collection["transitionInterest"].ToString();
                    transition.Update_Date = DateTime.Now;

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
                        assessment.Narrative = collection[i++].ToString();
                        var completedOn = collection[i++];
                        if (completedOn != null && completedOn != "")
                            assessment.CompletedOn = Convert.ToDateTime(completedOn);
                        assessment.Performance = collection[i++].ToString();
                        assessment.IEPid = transition.IEPid;
                        assessment.Update_Date = DateTime.Now;

                        if (assessment.TransitionAssementID == 0)
                        {
                            assessment.Create_Date = DateTime.Now;
                            db.tblTransitionAssessments.Add(assessment);
                        }

                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to Transition Assessments: " + e.InnerException.ToString());
                }

                return Json(new { Result = "success", Message = "The Student Transition Assessment was updated." }, JsonRequestBehavior.AllowGet);
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
                    int i = 2;

                    tblTransition transition = db.tblTransitions.Where(t => t.IEPid == iedId).FirstOrDefault();
                    bool hasEmploymentGoal = false;
                    bool hasEducationalGoal = false;
                    bool canComplete = false;
                    while (i < collection.Count - 2)
                    {
                        var transitionGoalID = Convert.ToInt32(collection[i++]);
                        tblTransitionGoal transitionGoal = db.tblTransitionGoals.Where(g => g.IEPid == iedId && g.TransitionGoalID == transitionGoalID).FirstOrDefault() ?? new tblTransitionGoal();

                        transitionGoal.IEPid = iedId;
                        transitionGoal.TransitionID = transition.TransitionID;
                        transitionGoal.GoalType = collection[i++].ToString();
                        transitionGoal.CompletetionType = collection[i++].ToString();
                        transitionGoal.Behavior = collection[i++].ToString();
                        transitionGoal.WhereAndHow = collection[i++].ToString();
                        transitionGoal.Update_Date = DateTime.Now;

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

                    }

                    db.SaveChanges();
                    if (hasEmploymentGoal && hasEducationalGoal)
                        canComplete = true;

                    return Json(new { Result = "success", Message = "The Student Transition Goals were added.", CanComplete = canComplete }, JsonRequestBehavior.AllowGet);
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
                try
                {
                    int studentId = Convert.ToInt32(collection["studentId"]);
                    int iedId = Convert.ToInt32(collection["IEPid"]);
                    int serviceCount = collection.Count - 4;
                    int i = 2;

                    tblTransition transition = db.tblTransitions.Where(t => t.IEPid == iedId).FirstOrDefault();
                    while (i < serviceCount)
                    {
                        var transitionServiceID = Convert.ToInt32(collection[i++]);
                        tblTransitionService transitionService = db.tblTransitionServices.Where(s => s.IEPid == iedId && s.TransitionServiceID == transitionServiceID).FirstOrDefault() ?? new tblTransitionService();

                        transitionService.IEPid = iedId;
                        transitionService.TransitionID = transition.TransitionID;
                        transitionService.ServiceType = collection[i++].ToString();
                        transitionService.ServiceDescription = collection[i++].ToString();
                        transitionService.Frequency = collection[i++].ToString();
                        transitionService.Duration = collection[i++].ToString();
                        transitionService.Location = collection[i++].ToString();

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
                    }

                    // for catching the Community Participation info at the end of the form.
                    if (i < collection.Count)
                    {
                        transition.CommunityParticipation = collection["isCommunityParticipation"] == "on" ? true : false;
                        transition.CommunityParticipation_Description = collection["communityParticipationDesc"].ToString();

                        db.SaveChanges();
                    }

                    return Json(new { Result = "success", Message = "The Student Transition Services were added." }, JsonRequestBehavior.AllowGet);
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

                    tblTransition transition = db.tblTransitions.Where(t => t.IEPid == iedId).FirstOrDefault();

                    transition.Planning_Facilitate = collection["isFocusFunctionalAchievement"] == "on" ? true : false;
                    transition.Planning_Align = collection["isAlignStudentPostGoals"] == "on" ? true : false;
                    DateTime graduationDate = (!string.IsNullOrEmpty(collection["graduationYear"])) ? Convert.ToDateTime(collection["graduationYear"]) : DateTime.Now;
                    transition.Planning_GraduationMonth = graduationDate.Month;
                    transition.Planning_GraduationYear = graduationDate.Year;
                    transition.Planning_Completion = (collection["planningCompletion"] != null) ? collection["planningCompletion"].ToString() : String.Empty;
                    transition.Planning_Credits = (!string.IsNullOrEmpty(collection["totalcredits"])) ? Convert.ToInt32(collection["totalcredits"]) : 0;
                    transition.Planning_BenefitKRS = collection["isVocationalRehabiltiation"] == "on" ? true : false;
                    transition.Planning_ConsentPrior = collection["isConfidentailReleaseObtained"] == "on" ? true : false;
                    transition.Planning_Occupation = (collection["occupationText"] != null) ? collection["occupationText"].ToString() : String.Empty;
                    transition.CareerPathID = (collection["CareerPathID"] != null && collection["CareerPathID"] != "") ? Convert.ToInt32(collection["CareerPathID"]) : 0;
                    transition.Planning_BenefitKRS_OtherAgencies = collection["otherAgencies"];
                    transition.isReleaseBefore21 = collection["isReleaseBefore21"] == "1" ? true : false;
                    db.SaveChanges();

                    return Json(new { Result = "success", Message = "The Student Transition Study was added." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to Transition Study: " + e.InnerException.ToString());
                }
            }

            return Json(new { Result = "failure", Message = "The Student Course of Study was not added." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult ModuleCompleted(int stdIEPId, string module)
        {
            switch (module)
            {
                case "Health":
                    db.tblIEPHealths.Where(h => h.IEPid == stdIEPId).FirstOrDefault().Completed = false;
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Health Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Motor":
                    db.tblIEPMotors.Where(m => m.IEPid == stdIEPId).FirstOrDefault().Completed = false;
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Motore Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Communication":
                    db.tblIEPCommunications.Where(c => c.IEPid == stdIEPId).FirstOrDefault().Completed = false;
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Communication Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Social":
                    db.tblIEPSocials.Where(s => s.IEPid == stdIEPId).FirstOrDefault().Completed = false;
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Social Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Intelligence":
                    db.tblIEPIntelligences.Where(i => i.IEPid == stdIEPId).FirstOrDefault().Completed = false;
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The General Intelligence Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Academic":
                    db.tblIEPAcademics.Where(a => a.IEPid == stdIEPId).FirstOrDefault().Completed = false;
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Academic Performance Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Other":
                    db.tblOtherConsiderations.Where(o => o.IEPid == stdIEPId).FirstOrDefault().Completed = false;
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Other Considerations Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Goals":
                    db.tblGoals.Where(g => g.IEPid == stdIEPId).ToList().ForEach(g => g.Completed = false);
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Goals Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Services":
                    db.tblServices.Where(s => s.IEPid == stdIEPId).ToList().ForEach(s => s.Completed = false);
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Service Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Accommodation":
                    db.tblAccommodations.Where(a => a.IEPid == stdIEPId).ToList().ForEach(a => a.Completed = false);
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Accommodations Module was updated." }, JsonRequestBehavior.AllowGet);
                case "Behavior":
                    db.tblBehaviors.Where(b => b.IEPid == stdIEPId).FirstOrDefault().Completed = false;
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "The Behavior Module was updated." }, JsonRequestBehavior.AllowGet);
                default:

                    return Json(new { Result = "error", Message = "Unable to find the module you requested to update." }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
