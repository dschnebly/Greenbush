using System;
using System.Collections.Generic;
using System.Linq;

namespace GreenBushIEP.Models
{
    public class IEP
    {
        private IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        public tblIEP draft { get; set; }

        public tblIEPHealth studentHealth { get; set; }
        public tblIEPMotor studentMotor { get; set; }
        public tblIEPCommunication studentCommunication { get; set; }
        public tblIEPSocial studentSocial { get; set; }
        public tblIEPIntelligence studentIntelligence { get; set; }
        public tblIEPAcademic studentAcademic { get; set; }
        public tblIEPReading studentReading { get; set; }
        public tblIEPMath studentMath { get; set; }
        public tblIEPWritten studentWritten { get; set; }

        public List<tblGoal> studentGoals { get; set; }
        public List<tblGoalBenchmark> studentGoalBenchmarks { get; set; }
        public List<tblService> studentServices { get; set; }
        public List<tblLocation> locations { get; set; }
        public List<tblServiceType> serviceTypes { get; set; }
        public List<tblProvider> serviceProviders { get; set; }
        public List<tblAccommodation> accommodations { get; set; }
        public BehaviorViewModel studentBehavior { get; set; }
        public tblOtherConsideration studentOtherConsiderations { get; set; }
        public StudentTransitionViewModel studentTransition { get; set; }
        public string studentFirstName { get; set; }
        public string studentLastName { get; set; }
        public int studentAge { get; set; }

        public IEP()
        {
            studentGoals = new List<tblGoal>();
            studentGoalBenchmarks = new List<tblGoalBenchmark>();
            studentServices = new List<tblService>();
            locations = new List<tblLocation>();
            serviceTypes = new List<tblServiceType>();
            serviceProviders = new List<tblProvider>();
            accommodations = new List<tblAccommodation>();
        }


        public IEP(int? stid = null)
        {
            if (stid != null)
            {
                this.draft = db.tblIEPs.SingleOrDefault(i => i.UserID == stid && i.IepStatus == IEPStatus.DRAFT);
            }
        }

        public IEP CreateNewIEP(int stid)
        {
            int IEPID;
            int HealthID;
            int MotorID;
            int CommunicationID;
            int SocialID;
            int IntelligenceID;
            int AcademicID;
            int ReadingID;
            int MathID;
            int WrittenID;

            // Check that we don't already have a draft copy being used by this user. 
            // If we do we need to return an error.
            if (this.draft != null)
            {
                throw new System.ArgumentException("There is already a draft IEP for this user");
            }

            this.draft = new tblIEP();
            this.draft.UserID = stid;
            this.draft.IepStatus = IEPStatus.DRAFT;
            this.draft.Create_Date = DateTime.Now;
            this.draft.Amendment = false;
            this.draft.StateAssessment = String.Empty;

            try
            {
                db.tblIEPs.Add(this.draft);
                IEPID = db.SaveChanges();
            }
            catch
            {
                throw new System.ArgumentException("Failed to create the IEP table");
            }

            // Adding Health Table
            this.studentHealth = new tblIEPHealth();
            this.studentHealth.IEPid = IEPID;
            this.studentHealth.NoConcerns = true;
            this.studentHealth.Concerns = false;
            this.studentHealth.Diagnosis = false;
            this.studentHealth.HearingDate = DateTime.Now;
            this.studentHealth.HearingResult = -1;
            this.studentHealth.VisionDate = DateTime.Now;
            this.studentHealth.VisionResult = -1;
            this.studentHealth.VisionImparied = false;

            try
            {
                db.tblIEPHealths.Add(this.studentHealth);
                HealthID = db.SaveChanges();
            }
            catch
            {
                this.draft.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Health table");
            }

            // Adding Motor Table
            this.studentMotor = new tblIEPMotor();
            this.studentMotor.IEPid = IEPID;
            this.studentMotor.NoConcerns = true;
            this.studentMotor.ProgressTowardGenEd = false;
            this.studentMotor.Needs = false;
            this.studentMotor.Participation = -1;

            try
            {
                db.tblIEPMotors.Add(this.studentMotor);
                MotorID = db.SaveChanges();
            }
            catch
            {
                this.draft.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Motor table");
            }

            // Add Communication Table
            this.studentCommunication = new tblIEPCommunication();
            this.studentCommunication.IEPid = IEPID;
            this.studentCommunication.NoConcerns = true;
            this.studentCommunication.ProgressTowardGenEd = false;
            this.studentCommunication.SpeechImpactPerformance = false;
            this.studentCommunication.Deaf = false;
            this.studentCommunication.LimitedEnglish = false;

            try
            {
                db.tblIEPCommunications.Add(this.studentCommunication);
                CommunicationID = db.SaveChanges();
            }
            catch
            {
                this.draft.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Communication table");
            }

            // Add Social Table
            this.studentSocial = new tblIEPSocial();
            this.studentSocial.IEPid = IEPID;
            this.studentSocial.NoConcerns = true;
            this.studentSocial.ProgressTowardGenEd = false;
            this.studentSocial.AreaOfNeed = false;
            this.studentSocial.MentalHealthDiagnosis = false;
            this.studentSocial.SignificantBehaviors = false;
            this.studentSocial.BehaviorImepedeLearning = false;
            this.studentSocial.BehaviorInterventionPlan = false;

            try
            {
                db.tblIEPSocials.Add(this.studentSocial);
                SocialID = db.SaveChanges();
            }
            catch
            {
                this.draft.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Social table");
            }

            // Add Intelligence Table
            this.studentIntelligence = new tblIEPIntelligence();
            this.studentIntelligence.IEPIntelligenceID = IEPID;
            this.studentIntelligence.Concerns = false;

            try
            {
                db.tblIEPIntelligences.Add(this.studentIntelligence);
                IntelligenceID = db.SaveChanges();
            }
            catch(Exception e)
            {
                this.draft.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Itellegince table");
            }

            // Add Academic Table
            this.studentAcademic = new tblIEPAcademic();
            this.studentAcademic.IEPid = IEPID;
            this.studentAcademic.NoConcerns = true;
            this.studentAcademic.AreaOfNeed = false;

            try
            {
                db.tblIEPAcademics.Add(this.studentAcademic);
                AcademicID = db.SaveChanges();
            }
            catch
            {
                this.draft.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Academic table");
            }

            // Add Reading Table
            this.studentReading = new tblIEPReading();
            this.studentReading.IEPid = IEPID;
            this.studentReading.NoConcerns = true;
            this.studentReading.ProgressTowardGenEd = false;
            this.studentReading.InstructionalTier1 = false;
            this.studentReading.InstructionalTier2 = false;
            this.studentReading.InstructionalTier3 = false;
            this.studentReading.AreaOfNeed = false;

            try
            {
                db.tblIEPReadings.Add(this.studentReading);
                ReadingID = db.SaveChanges();
            }
            catch
            {
                this.draft.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Reading table");
            }

            // Add Math Table
            this.studentMath = new tblIEPMath();
            this.studentMath.IEPid = IEPID;
            this.studentMath.NoConcerns = true;
            this.studentMath.ProgressTowardGenEd = false;
            this.studentMath.InstructionalTier1 = false;
            this.studentMath.InstructionalTier2 = false;
            this.studentMath.InstructionalTier3 = false;
            this.studentMath.AreaOfNeed = false;

            try
            {
                db.tblIEPMaths.Add(this.studentMath);
                MathID = db.SaveChanges();
            }
            catch
            {
                this.draft.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Math table");
            }

            // Add Written Table
            this.studentWritten = new tblIEPWritten();
            this.studentWritten.IEPid = IEPID;
            this.studentWritten.NoConcerns = true;
            this.studentWritten.ProgressTowardGenEd = false;
            this.studentWritten.InstructionalTier1 = false;
            this.studentWritten.InstructionalTier2 = false;
            this.studentWritten.InstructionalTier3 = false;
            this.studentWritten.AreaOfNeed = false;

            try
            {
                db.tblIEPWrittens.Add(this.studentWritten);
                WrittenID = db.SaveChanges();
            }
            catch
            {
                this.draft.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Written table");
            }

            // Add references to all the new created tables.
            this.draft.IEPAcademicID = AcademicID;
            this.draft.IEPCommunicationID = CommunicationID;
            this.draft.IEPHealthID = HealthID;
            this.draft.IEPIntelligenceID = IntelligenceID;
            this.draft.IEPMathID = MathID;
            this.draft.IEPMotorID = MotorID;
            this.draft.IEPReadingID = ReadingID;
            this.draft.IEPSocialID = SocialID;
            this.draft.IEPWrittenID = WrittenID;

            db.SaveChanges();

            return this;
        }
    }
}