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
                        HealthIEP.HearingImparied = (!String.IsNullOrEmpty(collection["HearingImpaired"])) ? true : false;
                        HealthIEP.HearingDate = Convert.ToDateTime(collection["HearingDate"]);
                        HealthIEP.HearingResult = Convert.ToInt32(collection["HearingResult"]);
                        HealthIEP.VisionImparied = collection["VisionImparied"] == "on" ? true : false;
                        HealthIEP.VisionDate = Convert.ToDateTime(collection["VisionDate"]);
                        HealthIEP.VisionResult = Convert.ToInt32(collection["VisionResult"]);
                        HealthIEP.LevelOfPerformance = collection["LevelOfPerformance"].ToString();
                        HealthIEP.AreaOfNeedDescription = collection["AreaOfNeedDescription"].ToString();
                        HealthIEP.MeetNeedBy = Convert.ToInt32(collection["MeetNeedBy"]);
                        HealthIEP.MeetNeedByOtherDescription = collection["MeetNeedByOtherDescription"].ToString();

                        db.SaveChanges();

                        return RedirectToAction("StudentProcedures", "Home", new { stid = IEP.UserID });
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
                        MotorIEP.LevelOfPerformance = collection["ModuleMotorLevelOfPerformance"].ToString();
                        MotorIEP.AreaOfNeedDescription = collection["ModuleMotorAreaOfNeedDescription"].ToString();
                        MotorIEP.MeetNeedBy = Convert.ToInt32(collection["ModuleMotorMeetNeedBy"]);
                        MotorIEP.MeetNeedByOtherDescription = collection["ModuleMotorMeetNeedByOtherDescription"].ToString();

                        db.SaveChanges();

                        return RedirectToAction("StudentProcedures", "Home", new { stid = IEP.UserID });
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
            var CommunicationId = Convert.ToInt32(collection["IEPCommunicationID"]);

            tblIEPCommunication CommunicationIEP = db.tblIEPCommunications.Where(c => c.IEPCommunicationID == CommunicationId).SingleOrDefault();
            tblIEP IEP = db.tblIEPs.Where(i => i.IEPCommunicationID == CommunicationIEP.IEPCommunicationID).FirstOrDefault();
            if (CommunicationIEP != null)
            {
                if (ValidateRequest)
                {
                    try
                    {
                        CommunicationIEP.NoConcerns = collection["ModuleCommunicationNoConcerns"] == "on" ? true : false;
                        CommunicationIEP.ProgressTowardGenEd = collection["ModuleCommunicationProgressTowardGenEd"] == "on" ? true : false;
                        CommunicationIEP.AreaOfNeed = (!String.IsNullOrEmpty(collection["ModuleCommunicationAreaOfNeed"])) ? true : false;
                        CommunicationIEP.Deaf = collection["ModuleCommunicationDeaf"] == "on" ? true : false;
                        CommunicationIEP.LimitedEnglish = collection["ModuleCommunicationDeaf"] == "on" ? true : false;
                        CommunicationIEP.LevelOfPerformance = collection["ModuleCommunicationLevelOfPerformance"].ToString();
                        CommunicationIEP.AreaOfNeedDescription = collection["ModuleCommunicationAreaOfNeedDescription"].ToString();
                        CommunicationIEP.MeetNeedBy = Convert.ToInt32(collection["ModuleCommunicationMeetNeedBy"]);
                        CommunicationIEP.MeetNeedByOtherDescription = collection["ModuleCommunicationMeetNeedByOtherDescription"].ToString();

                        db.SaveChanges();

                        return RedirectToAction("StudentProcedures", "Home", new { stid = IEP.UserID });
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Unable to save changes to Communication Module: " + e.InnerException);
                    }
                }
            }

            throw new Exception("Unable to log you in.");
        }

        // POST: ModuleSection/Edit/5
        [HttpPost]
        public ActionResult EditSocial(FormCollection collection)
        {
            var socialId = Convert.ToInt32(collection["IEPSocialID"]);

            tblIEPSocial SocialIEP = db.tblIEPSocials.Where(s => s.IEPSocialID == socialId).SingleOrDefault();
            tblIEP IEP = db.tblIEPs.Where(i => i.IEPCommunicationID == SocialIEP.IEPSocialID).FirstOrDefault();
            if (SocialIEP != null)
            {
                if (ValidateRequest)
                {
                    try
                    {
                        SocialIEP.NoConcerns = collection["ModuleSocialNoConcern"] == "on" ? true : false;
                        SocialIEP.ProgressTowardGenEd = collection["ModuleSocialProgressTowardGenEd"] == "on" ? true : false;
                        SocialIEP.AreaOfNeed = collection["ModuleSocialAreaOfNeed"] == "on" ? true : false;
                        SocialIEP.MentalHealthDiagnosis = collection["ModuleSocialMentalHealthDiagnosis"] == "on" ? true : false;
                        SocialIEP.SignificantBehaviors = collection["ModuleSocialSignificantBehaviors"] == "on" ? true : false;
                        SocialIEP.SocialDeficit = collection["ModuleSocialDeficit"] == "on" ? true : false;
                        SocialIEP.BehaviorImepedeLearning = collection["ModuleSocialBehaviorImepedeLearning"] == "on" ? true : false;
                        SocialIEP.BehaviorInterventionPlan = collection["ModuleSocialBehaviorInterventionPlan"] == "on" ? true : false;
                        SocialIEP.LevelOfPerformance = collection["ModuleSocialLevelOfPerformance"].ToString();
                        SocialIEP.AreaOfNeedDescription = collection["ModuleSocialAreaOfNeedDescription"].ToString();
                        SocialIEP.MeetNeedBy = Convert.ToInt32(collection["ModuleSocialMeetNeedBy"]);
                        SocialIEP.MeedNeedByOtherDescription = collection["ModuleSocialMeedNeedByOtherDescription"].ToString();

                        db.SaveChanges();

                        return RedirectToAction("StudentProcedures", "Home", new { stid = IEP.UserID });
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Unable to save changes to Social-Emotion Module: " + e.InnerException);
                    }
                }
            }

            throw new Exception("Unable to log you in.");
        }

        // POST: ModuleSection/Edit/5
        [HttpPost]
        public ActionResult EditIntelligence(FormCollection collection)
        {
            var intelligenceId = Convert.ToInt32(collection["IEPIntelligenceID"]);

            tblIEPIntelligence IntellgienceIEP = db.tblIEPIntelligences.Where(i => i.IEPIntelligenceID == intelligenceId).SingleOrDefault();
            tblIEP IEP = db.tblIEPs.Where(i => i.IEPIntelligenceID == IntellgienceIEP.IEPIntelligenceID).FirstOrDefault();
            if (IntellgienceIEP != null)
            {
                if (ValidateRequest)
                {
                    try
                    {
                        IntellgienceIEP.ProgressTowardGenEd = collection["ModuleIntelligenceProgressTowardGenEd"] == "on" ? true : false;
                        IntellgienceIEP.AreaOfNeed = collection["ModuleIntelligenceAreaOfNeed"] == "on" ? true : false;
                        IntellgienceIEP.LevelOfPerformance = collection["ModuleIntelligenceLevelOfPerformance"].ToString();
                        IntellgienceIEP.AreaOfNeedDescription = collection["ModuleIntelligenceAreaOfNeedDescription"].ToString();
                        IntellgienceIEP.MeetNeedBy = Convert.ToInt32(collection["ModuleIntelligenceMeetNeedBy"]);
                        IntellgienceIEP.MeetNeedByOtherDescription = collection["ModuleIntelligenceMeetNeedByOtherDescription"].ToString();

                        db.SaveChanges();

                        return RedirectToAction("StudentProcedures", "Home", new { stid = IEP.UserID });
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Unable to save changes to Communication Module: " + e.InnerException);
                    }
                }
            }

            throw new Exception("Unable to log you in.");
        }

        // POST: ModuleSection/Edit/5
        [HttpPost]
        public ActionResult EditAcademic(FormCollection collection)
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
                if (ValidateRequest)
                {
                    try
                    {
                        viewModel.Academic.NoConcerns = collection["ModuleAcademicNoConcern"] == "on" ? true : false;
                        viewModel.Academic.ProgressTowardGenEd = collection["ModuleAcademicProgressTowardGenEd"] == "on" ? true : false;
                        viewModel.Academic.AreaOfNeed = collection["ModuleAcademicAreaOfNeed"] == "on" ? true : false;
                        viewModel.Academic.LevelOfPerformance = collection["ModuleAcademicLevelOfPerformance"].ToString();
                        viewModel.Academic.AreaOfNeedDescription = collection["ModuleAcademicAreaOfNeedDescription"].ToString();
                        viewModel.Academic.MeetNeedBy = Convert.ToInt32(collection["ModuleAcademicMeetNeedBy"]);
                        viewModel.Academic.MeetNeedByOtherDescription = collection["ModuleAcademicMeetNeedByOtherDescription"].ToString();

                        viewModel.Reading.NoConcerns = collection["ModuleReadingNoConcern"] == "on" ? true : false;
                        viewModel.Reading.ProgressTowardGenEd = collection["ModuleReadingProgressTowardGenEd"] == "on" ? true : false;
                        viewModel.Reading.AreaOfNeed = collection["ModuleReadingAreaOfNeed"] == "on" ? true : false;
                        viewModel.Reading.LevelOfPerformance = collection["ModuleReadingLevelOfPerformance"].ToString();
                        viewModel.Reading.AreaOfNeedDescription = collection["ModuleReadingAreaOfNeedDescription"].ToString();
                        viewModel.Reading.MeetNeedBy = Convert.ToInt32(collection["ModuleReadingMeetNeedBy"]);
                        viewModel.Reading.MeetNeedByOtherDescription = collection["ModuleReadingMeetNeedByOtherDescription"].ToString();

                        viewModel.Math.NoConcerns = collection["ModuleMathNoConcern"] == "on" ? true : false;
                        viewModel.Math.ProgressTowardGenEd = collection["ModuleMathProgressTowardGenEd"] == "on" ? true : false;
                        viewModel.Math.AreaOfNeed = collection["ModuleMathAreaOfNeed"] == "on" ? true : false;
                        viewModel.Math.LevelOfPerformance = collection["ModuleMathLevelOfPerformance"].ToString();
                        viewModel.Math.AreaOfNeedDescription = collection["ModuleMathAreaOfNeedDescription"].ToString();
                        viewModel.Math.MeetNeedBy = Convert.ToInt32(collection["ModuleMathMeetNeedBy"]);
                        viewModel.Math.MeetNeedByOtherDescription = collection["ModuleMathMeetNeedByOtherDescription"].ToString();

                        viewModel.Written.NoConcerns = collection["ModuleWrittenNoConcern"] == "on" ? true : false;
                        viewModel.Written.ProgressTowardGenEd = collection["ModuleWrittenProgressTowardGenEd"] == "on" ? true : false;
                        viewModel.Written.AreaOfNeed = collection["ModuleWrittenAreaOfNeed"] == "on" ? true : false;
                        viewModel.Written.LevelOfPerformance = collection["ModuleWrittenLevelOfPerformance"].ToString();
                        viewModel.Written.AreaOfNeedDescription = collection["ModuleWrittenAreaOfNeedDescription"].ToString();
                        viewModel.Written.MeetNeedBy = Convert.ToInt32(collection["ModuleWrittenMeetNeedBy"]);
                        viewModel.Written.MeetNeedByOtherDescription = collection["ModuleWrittenMeetNeedByOtherDescription"].ToString();

                        db.SaveChanges();

                        return RedirectToAction("StudentProcedures", "Home", new { stid = IEP.UserID });
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Unable to save changes to Communication Module: " + e.InnerException);
                    }
                }
            }

            throw new Exception("Unable to log you in.");
        }

        // POST: ModuleSection/Edit/5
        [HttpPost]
        public ActionResult UpdateAccom(tblAccommodation model)
        {

            var avm = new AccomodationViewModel();
            avm.AccDescription = model.Description;
            avm.AccommodationID = model.AccommodationID;
            avm.AccomType = model.AccomType;
            avm.AnticipatedEndDate = model.AnticipatedEndDate;
            avm.AnticipatedStartDate = model.AnticipatedStartDate;
            avm.Location = model.Location;
            avm.Duration = model.Duration;
            avm.Frequency = model.Frequency;

            if (ModelState.IsValid)
            {
                tblAccommodation existingIEP = db.tblAccommodations.Where(c => c.AccommodationID == model.AccommodationID).FirstOrDefault();
                if (existingIEP != null)
                {
                    avm.IEPid = existingIEP.IEPid;
                    int newId = EditAccomodation(existingIEP, avm);
                    return Json(new { success = true, id = newId });
                }
                else
                {
                    return Json(new { success = false });
                }

            }
            else
            {
                string errorMessage = "";
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        errorMessage += " " + error.ErrorMessage;
                    }
                }

                return Json(new { success = false, error = errorMessage }, JsonRequestBehavior.AllowGet);
            }
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

                return RedirectToAction("StudentProcedures", "Home", new { stid = studentId });
            }
            else
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
                model.ExtendedSchoolYear_RegressionRisk = collection["ExtendedSchoolYear_RegressionRisk"] == "on" ? true : false;
                model.ExtendedSchoolYear_SeverityRisk = collection["ExtendedSchoolYear_SeverityRisk"] == "on" ? true : false;
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

                }

                var tp = collection["TransporationPlan"];
                switch (tp)
                {
                    case "1":
                        model.Transporation_NotEligible = true;
                        break;
                    case "2":
                        model.Transporation_Required = true;
                        break;
                    case "3":
                        model.Transporation_AttendOtherBuilding = true;
                        break;
                    case "4":
                        model.Transporation_Other_flag = true;
                        break;

                }


                //find existing if updating
                tblOtherConsideration OC = db.tblOtherConsiderations.Where(c => c.OtherConsiderationID == model.OtherConsiderationID).FirstOrDefault();

                if (OC == null)
                {
                    model.Create_Date = DateTime.Now;
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
                    OC.StateAssessment_RequiredCompleted = model.StateAssessment_RequiredCompleted;
                    OC.Transporation_NotEligible = model.Transporation_NotEligible;
                    OC.Transporation_Required = model.Transporation_Required;
                    OC.Transporation_AttendOtherBuilding = model.Transporation_AttendOtherBuilding;
                    OC.Transporation_Other_flag = model.Transporation_Other_flag;
                    OC.Transporation_Other_desc = model.Transporation_Other_desc;
                    OC.RegularEducation_NotParticipate = model.RegularEducation_NotParticipate;
                    OC.ExtendedSchoolYear_RegressionRisk = model.ExtendedSchoolYear_RegressionRisk;
                    OC.ExtendedSchoolYear_SeverityRisk = model.ExtendedSchoolYear_SeverityRisk;
                    OC.ExtendedSchoolYear_Justification = model.ExtendedSchoolYear_Justification;
                    OC.Parental_Concerns_flag = model.Parental_Concerns_flag;
                    OC.Parental_Concerns_Desc = model.Parental_Concerns_Desc;
                    OC.Create_Date = DateTime.Now; ;

                    db.SaveChanges();
                    return RedirectToAction("StudentProcedures", "Home", new { stid = studentId });
                }

            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
            }

            throw new Exception("Unable to save changes to Other Considerations Module");

        }


        [HttpPost]
        public ActionResult EditAccom(AccomodationViewModel model)
        {
            int studentId = model.StudentId;

            if (ModelState.IsValid)
            {
                //find existing if updating
                tblAccommodation AccomodationIEP = db.tblAccommodations.Where(c => c.AccommodationID == model.AccommodationID).FirstOrDefault();
                bool isNew = false;

                if (AccomodationIEP == null)
                {
                    isNew = true;
                    AccomodationIEP = new tblAccommodation();
                }


                if (AccomodationIEP != null)
                {
                    int newId = EditAccomodation(AccomodationIEP, model);
                    if (isNew)
                    {
                        return Json(new { success = true, id = newId, iep = model.IEPid, isNew = true });
                    }
                    else
                    {
                        return Json(new { success = true, id = model.AccommodationID, iep = model.IEPid, isNew = false });
                    }
                }
            }

            string errorMessage = "";
            foreach (ModelState modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errorMessage += " " + error.ErrorMessage;
                }
            }

            model.Message = errorMessage;


            return Json(new { success = false, error = model.Message }, JsonRequestBehavior.AllowGet);

        }

        private int EditAccomodation(tblAccommodation AccomodationIEP, AccomodationViewModel model)
        {
            AccomodationIEP.IEPid = model.IEPid;

            try
            {
                AccomodationIEP.AccomType = model.AccomType;
                AccomodationIEP.Description = model.AccDescription;

                if (model.AnticipatedStartDate.HasValue)
                    AccomodationIEP.AnticipatedStartDate = model.AnticipatedStartDate;

                if (model.AnticipatedEndDate.HasValue)
                    AccomodationIEP.AnticipatedEndDate = model.AnticipatedEndDate;

                AccomodationIEP.Duration = model.Duration;
                AccomodationIEP.Frequency = model.Frequency;
                AccomodationIEP.Location = model.Location;
                AccomodationIEP.IEPid = model.IEPid;

                if (model.AccommodationID == 0)
                {
                    AccomodationIEP.Create_Date = DateTime.Now;
                    AccomodationIEP.Update_Date = DateTime.Now;

                    db.tblAccommodations.Add(AccomodationIEP);

                }

                db.SaveChanges();
                int newId = AccomodationIEP.AccommodationID;

                return newId;


            }
            catch (Exception e)
            {
                throw new Exception("Unable to save changes to Accommodation/Modification Module: " + e.InnerException);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeleteStudentGoal(int studentGoalId)
        {
            tblGoal studentGoalToRemove = db.tblGoals.Where(g => g.goalID == studentGoalId).FirstOrDefault();
            if (studentGoalToRemove != null)
            {
                db.tblGoals.Remove(studentGoalToRemove);
                db.SaveChanges();

                return Json(new { Result = "success", Message = "Student Goal was successfully deleted." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "failure", Message = "The Student Goal Benchmark Id was not found and thus not deleted." }, JsonRequestBehavior.AllowGet);
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
        public ActionResult EditStudentGoals(FormCollection collection)
        {
            if (ValidateRequest)
            {
                try
                {
                    int goalId = 0;

                    string verification = collection[0];
                    int studentId = Convert.ToInt32(collection[1]);
                    int iepId = Convert.ToInt32(collection[2]);

                    int j = 2;
                    DateTime temp;
                    int tempInt;
                    bool keyParse = Int32.TryParse(collection[++j], out goalId);
                    StudentGoal studentGoal = (!keyParse) ? new StudentGoal() : new StudentGoal(goalId); // new goal : exsisting goal

                    studentGoal.goal.IEPid = iepId;
                    studentGoal.goal.Module = collection[++j].ToString();
                    studentGoal.goal.Title = collection[++j].ToString();
                    studentGoal.goal.hasSerivce = collection[++j] == "true" ? true : false;
                    studentGoal.goal.AnnualGoal = collection[++j].ToString();
                    studentGoal.goal.Baseline = collection[++j].ToString();
                    studentGoal.goal.StateStandards = collection[++j].ToString();

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
                                benchmark.Method = collection[++j] != null && collection[j] != "" ? Int32.TryParse(collection[j], out tempInt) ? tempInt : 0: 0;
                                benchmark.ObjectiveBenchmark = collection[++j].ToString();
                                benchmark.TransitionActivity = (collection[++j] == "true") ? true : false;
                                benchmark.ProgressDate_Quarter1 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                                benchmark.ProgressDate_Quarter2 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                                benchmark.ProgressDate_Quarter3 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                                benchmark.ProgressDate_Quarter4 = DateTime.TryParse(collection[++j], out temp) ? temp : DateTime.Now;
                                benchmark.Progress_Quarter1 =   ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                benchmark.Progress_Quarter2 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                benchmark.Progress_Quarter3 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                benchmark.Progress_Quarter4 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                benchmark.ProgressDescription_Quarter1 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() :"";
                                benchmark.ProgressDescription_Quarter2 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                benchmark.ProgressDescription_Quarter3 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";
                                benchmark.ProgressDescription_Quarter4 = ++j < collection.Count && collection[j] != null ? collection[j].ToString() : "";

                                studentGoal.benchmarks.Add(benchmark);
                            }

                            keyName = (++j < collection.Count) ? collection.GetKey(j) : String.Empty;
                        }
                    }

                    studentGoal.SaveGoal();
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to save changes to the Student Goals: " + e.InnerException.ToString());
                }
            }

            return RedirectToAction("StudentProcedures", "Home", new { stid = Convert.ToInt32(collection[1]) });
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
                        assessment.CompletedOn = Convert.ToDateTime(collection[i++]);
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
                    }

                    db.SaveChanges();

                    return Json(new { Result = "success", Message = "The Student Transition Goals were added." }, JsonRequestBehavior.AllowGet);
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
                        transitionService.Update_Date = DateTime.Now;

                        if (transitionServiceID == 0)
                        {
                            transitionService.Create_Date = DateTime.Now;
                            db.tblTransitionServices.Add(transitionService);
                        }

                        db.SaveChanges();
                    }

                    // for catching the Community Participation info at the end of the form.
                    if(i < collection.Count)
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
    }
}
