﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace GreenBushIEP.Models
{
    public class IEP
    {
        private IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        public bool hasPlan { get; set; }
        public tblIEP current { get; set; }

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
        public List<tblGoalEvaluationProcedure> studentGoalEvalProcs { get; set; }
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

        //for printing
        public StudentDetailsPrintViewModel studentDetails { get; set; }
        
        public IEP()
        {
            hasPlan = false;
            studentGoals = new List<tblGoal>();
            studentGoalBenchmarks = new List<tblGoalBenchmark>();
            studentGoalEvalProcs = new List<tblGoalEvaluationProcedure>();
            studentServices = new List<tblService>();
            locations = new List<tblLocation>();
            serviceTypes = new List<tblServiceType>();
            serviceProviders = new List<tblProvider>();
            accommodations = new List<tblAccommodation>();
            studentDetails = new StudentDetailsPrintViewModel();
        }

        public IEP(int? stid = null)
        {
            if (stid != null)
            {
                this.current = db.tblIEPs.SingleOrDefault(i => i.UserID == stid);
                this.hasPlan = this.current != null;
            }
        }

        public IEP CreateNewIEP(int stid)
        {
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
            if (this.current != null)
            {
                throw new System.ArgumentException("There is already a draft IEP for this user");
            }

            this.current = new tblIEP();
            this.current.UserID = stid;
            this.current.IepStatus = IEPStatus.DRAFT;
            this.current.Create_Date = DateTime.Now;
            this.current.Amendment = false;
            this.current.StateAssessment = String.Empty;

            try
            {
                db.tblIEPs.Add(this.current);
                db.SaveChanges();
            }
            catch
            {
                throw new System.ArgumentException("Failed to create the IEP table");
            }

            // Adding Health Table
            this.studentHealth = new tblIEPHealth();
            this.studentHealth.IEPid = this.current.IEPid;
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
                db.SaveChanges();

                HealthID = this.studentHealth.IEPHealthID;
            }
            catch
            {
                this.current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Health table");
            }

            // Adding Motor Table
            this.studentMotor = new tblIEPMotor();
            this.studentMotor.IEPid = this.current.IEPid;
            this.studentMotor.NoConcerns = true;
            this.studentMotor.ProgressTowardGenEd = false;
            this.studentMotor.Needs = false;
            this.studentMotor.Participation = -1;

            try
            {
                db.tblIEPMotors.Add(this.studentMotor);
                db.SaveChanges();

                MotorID = this.studentMotor.IEPMotorID;
            }
            catch
            {
                this.current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Motor table");
            }

            // Add Communication Table
            this.studentCommunication = new tblIEPCommunication();
            this.studentCommunication.IEPid = this.current.IEPid;
            this.studentCommunication.NoConcerns = true;
            this.studentCommunication.ProgressTowardGenEd = false;
            this.studentCommunication.SpeechImpactPerformance = false;
            this.studentCommunication.Deaf = false;
            this.studentCommunication.LimitedEnglish = false;

            try
            {
                db.tblIEPCommunications.Add(this.studentCommunication);
                db.SaveChanges();

                CommunicationID = this.studentCommunication.IEPCommunicationID;
            }
            catch
            {
                this.current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Communication table");
            }

            // Add Social Table
            this.studentSocial = new tblIEPSocial();
            this.studentSocial.IEPid = this.current.IEPid;
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
                db.SaveChanges();

                SocialID = this.studentSocial.IEPSocialID;
            }
            catch
            {
                this.current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Social table");
            }

            // Add Intelligence Table
            this.studentIntelligence = new tblIEPIntelligence();
            this.studentIntelligence.IEPid = this.current.IEPid;
            this.studentIntelligence.Concerns = false;

            try
            {
                db.tblIEPIntelligences.Add(this.studentIntelligence);
                db.SaveChanges();

                IntelligenceID = this.studentIntelligence.IEPIntelligenceID;
            }
            catch(Exception e)
            {
                this.current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Intelligence table: " + e.InnerException.Message.ToString());
            }

            // Add Academic Table
            this.studentAcademic = new tblIEPAcademic();
            this.studentAcademic.IEPid = this.current.IEPid;
            this.studentAcademic.NoConcerns = true;
            this.studentAcademic.AreaOfNeed = false;

            try
            {
                db.tblIEPAcademics.Add(this.studentAcademic);
                db.SaveChanges();

                AcademicID = this.studentAcademic.IEPAcademicID;
            }
            catch
            {
                this.current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Academic table");
            }

            // Add Reading Table
            this.studentReading = new tblIEPReading();
            this.studentReading.IEPid = this.current.IEPid;
            this.studentReading.NoConcerns = true;
            this.studentReading.ProgressTowardGenEd = false;
            this.studentReading.InstructionalTier1 = false;
            this.studentReading.InstructionalTier2 = false;
            this.studentReading.InstructionalTier3 = false;
            this.studentReading.AreaOfNeed = false;

            try
            {
                db.tblIEPReadings.Add(this.studentReading);
                db.SaveChanges();

                ReadingID = this.studentReading.IEPReadingID;
            }
            catch
            {
                this.current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Reading table");
            }

            // Add Math Table
            this.studentMath = new tblIEPMath();
            this.studentMath.IEPid = this.current.IEPid;
            this.studentMath.NoConcerns = true;
            this.studentMath.ProgressTowardGenEd = false;
            this.studentMath.InstructionalTier1 = false;
            this.studentMath.InstructionalTier2 = false;
            this.studentMath.InstructionalTier3 = false;
            this.studentMath.AreaOfNeed = false;

            try
            {
                db.tblIEPMaths.Add(this.studentMath);
                db.SaveChanges();

                MathID = this.studentMath.IEPMathID;
            }
            catch
            {
                this.current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Math table");
            }

            // Add Written Table
            this.studentWritten = new tblIEPWritten();
            this.studentWritten.IEPid = this.current.IEPid;
            this.studentWritten.NoConcerns = true;
            this.studentWritten.ProgressTowardGenEd = false;
            this.studentWritten.InstructionalTier1 = false;
            this.studentWritten.InstructionalTier2 = false;
            this.studentWritten.InstructionalTier3 = false;
            this.studentWritten.AreaOfNeed = false;

            try
            {
                db.tblIEPWrittens.Add(this.studentWritten);
                db.SaveChanges();

                WrittenID = this.studentWritten.IEPWrittenID;
            }
            catch
            {
                this.current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Written table");
            }

            // Add references to all the new created tables.
            this.current.IEPAcademicID = AcademicID;
            this.current.IEPCommunicationID = CommunicationID;
            this.current.IEPHealthID = HealthID;
            this.current.IEPIntelligenceID = IntelligenceID;
            this.current.IEPMathID = MathID;
            this.current.IEPMotorID = MotorID;
            this.current.IEPReadingID = ReadingID;
            this.current.IEPSocialID = SocialID;
            this.current.IEPWrittenID = WrittenID;

            db.SaveChanges();

            hasPlan = true;

            return this;
        }
    }
}