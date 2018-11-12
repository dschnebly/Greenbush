using System;
using System.Collections.Generic;
using System.Linq;

namespace GreenBushIEP.Models
{
    public class IEP
    {
        private IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        public bool hasPlan { get; set; }
        public bool isHealthCompleted { get; set; }
        public bool isMotorCompleted { get; set; }
        public bool isCommunicationCompleted { get; set; }
        public bool isSocialCompleted { get; set; }
        public bool isIntelligenceCompleted { get; set; }
        public bool isAcademicCompleted { get; set; }
        public bool isGoalCompleted { get; set; }
        public bool isServiceCompleted { get; set; }
        public bool isOtherCompleted { get; set; }
        public bool isAccommodationsCompleted { get; set; }
        public bool isBehaviorCompleted { get; set; }
        public bool isTransitionCompleted { get; set; }
        public tblIEP current { get; set; }
        public List<tblIEP> listOfStudentsIEPs { get; set; }

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
            listOfStudentsIEPs = new List<tblIEP>();
            studentGoals = new List<tblGoal>();
            studentGoalBenchmarks = new List<tblGoalBenchmark>();
            studentGoalEvalProcs = new List<tblGoalEvaluationProcedure>();
            studentServices = new List<tblService>();
            locations = new List<tblLocation>();
            serviceTypes = new List<tblServiceType>();
            serviceProviders = new List<tblProvider>();
            accommodations = new List<tblAccommodation>();
            studentDetails = new StudentDetailsPrintViewModel();
            isHealthCompleted = false;
            isMotorCompleted = false;
            isCommunicationCompleted = false;
            isSocialCompleted = false;
            isIntelligenceCompleted = false;
            isOtherCompleted = false;
            isAcademicCompleted = false;
            isBehaviorCompleted = false;
            isTransitionCompleted = false;
        }

        public IEP(int? stid = null, int? iepId = null)
        {
            if (stid != null)
            {
                listOfStudentsIEPs = db.tblIEPs.Where(i => i.UserID == stid).OrderBy(i => i.IepStatus).ThenBy(i => i.Amendment).ToList();
                current = (iepId != null) ? listOfStudentsIEPs.Where(i => i.IEPid == iepId).FirstOrDefault() : listOfStudentsIEPs.FirstOrDefault();
                hasPlan = current != null;

                if (current != null)
                {
                    isHealthCompleted = db.tblIEPHealths.Where(h => h.IEPHealthID == current.IEPHealthID).First().Completed;
                    isMotorCompleted = db.tblIEPMotors.Where(m => m.IEPMotorID == current.IEPMotorID).First().Completed;
                    isCommunicationCompleted = db.tblIEPCommunications.Where(c => c.IEPCommunicationID == current.IEPCommunicationID).First().Completed;
                    isSocialCompleted = db.tblIEPSocials.Where(s => s.IEPSocialID == current.IEPSocialID).First().Completed;
                    isIntelligenceCompleted = db.tblIEPIntelligences.Where(i => i.IEPIntelligenceID == current.IEPIntelligenceID).First().Completed;
                    isAcademicCompleted = db.tblIEPAcademics.Where(a => a.IEPAcademicID == current.IEPAcademicID).First().Completed;
                    isOtherCompleted = db.tblOtherConsiderations.Where(o => o.IEPid == current.IEPid).FirstOrDefault() != null ? db.tblOtherConsiderations.Where(o => o.IEPid == current.IEPid).FirstOrDefault().Completed : false ;
                    isGoalCompleted = db.tblGoals.Where(g => g.IEPid == current.IEPid).FirstOrDefault() != null ? db.tblGoals.Where(g => g.IEPid == current.IEPid).ToList().All(g => g.Completed) : false ;
                    isServiceCompleted = db.tblServices.Where(s => s.IEPid == current.IEPid).FirstOrDefault() != null ? db.tblServices.Where(s => s.IEPid == current.IEPid).ToList().All(s => s.Completed) : false ;
                    isBehaviorCompleted = db.tblBehaviors.Where(b => b.IEPid == current.IEPid).FirstOrDefault() != null ? db.tblBehaviors.Where(b => b.IEPid == current.IEPid).FirstOrDefault().Completed : false;
                    isAccommodationsCompleted = db.tblAccommodations.Where(a => a.IEPid == current.IEPid).FirstOrDefault() != null ? db.tblAccommodations.Where(a => a.IEPid == current.IEPid).FirstOrDefault().Completed : false ;
                }
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
            if (current != null)
            {
                throw new System.ArgumentException("There is already a draft IEP for this user");
            }

            current = new tblIEP();
            current.UserID = stid;
            current.IepStatus = IEPStatus.DRAFT;
            current.Create_Date = DateTime.Now;
            current.end_Date = null;
            current.Update_Date = DateTime.Now;
            current.begin_date = null;
            current.Amendment = false;
            current.StateAssessment = string.Empty;

            try
            {
                db.tblIEPs.Add(current);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                throw new System.ArgumentException("Failed to create the IEP table -" + e.InnerException.ToString());
            }

            // Adding Health Table
            studentHealth = new tblIEPHealth();
            studentHealth.IEPid = current.IEPid;
            studentHealth.NoConcerns = true;
            studentHealth.Concerns = false;
            studentHealth.Diagnosis = false;
            studentHealth.HearingDate = DateTime.Now;
            studentHealth.HearingResult = -1;
            studentHealth.VisionDate = DateTime.Now;
            studentHealth.VisionResult = -1;
            studentHealth.VisionImparied = false;
            studentHealth.Completed = false;

            try
            {
                db.tblIEPHealths.Add(studentHealth);
                db.SaveChanges();

                HealthID = studentHealth.IEPHealthID;
            }
            catch
            {
                current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Health table");
            }

            // Adding Motor Table
            studentMotor = new tblIEPMotor();
            studentMotor.IEPid = current.IEPid;
            studentMotor.NoConcerns = true;
            studentMotor.ProgressTowardGenEd = false;
            studentMotor.Needs = false;
            studentMotor.Participation = -1;
            studentMotor.Completed = false;

            try
            {
                db.tblIEPMotors.Add(studentMotor);
                db.SaveChanges();

                MotorID = studentMotor.IEPMotorID;
            }
            catch
            {
                current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Motor table");
            }

            // Add Communication Table
            studentCommunication = new tblIEPCommunication();
            studentCommunication.IEPid = current.IEPid;
            studentCommunication.NoConcerns = true;
            studentCommunication.ProgressTowardGenEd = false;
            studentCommunication.SpeechImpactPerformance = false;
            studentCommunication.Deaf = false;
            studentCommunication.LimitedEnglish = false;
            studentCommunication.Completed = false;

            try
            {
                db.tblIEPCommunications.Add(studentCommunication);
                db.SaveChanges();

                CommunicationID = studentCommunication.IEPCommunicationID;
            }
            catch
            {
                current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Communication table");
            }

            // Add Social Table
            studentSocial = new tblIEPSocial();
            studentSocial.IEPid = current.IEPid;
            studentSocial.NoConcerns = true;
            studentSocial.ProgressTowardGenEd = false;
            studentSocial.AreaOfNeed = false;
            studentSocial.MentalHealthDiagnosis = false;
            studentSocial.SignificantBehaviors = false;
            studentSocial.BehaviorImepedeLearning = false;
            studentSocial.BehaviorInterventionPlan = false;
            studentSocial.Completed = false;

            try
            {
                db.tblIEPSocials.Add(studentSocial);
                db.SaveChanges();

                SocialID = studentSocial.IEPSocialID;
            }
            catch
            {
                current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Social table");
            }

            // Add Intelligence Table
            studentIntelligence = new tblIEPIntelligence();
            studentIntelligence.IEPid = current.IEPid;
            studentIntelligence.Concerns = false;
            studentIntelligence.Completed = false;

            try
            {
                db.tblIEPIntelligences.Add(studentIntelligence);
                db.SaveChanges();

                IntelligenceID = studentIntelligence.IEPIntelligenceID;
            }
            catch (Exception e)
            {
                current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Intelligence table: " + e.InnerException.Message.ToString());
            }

            // Add Academic Table
            studentAcademic = new tblIEPAcademic();
            studentAcademic.IEPid = current.IEPid;
            studentAcademic.NoConcerns = true;
            studentAcademic.AreaOfNeed = false;
            studentAcademic.Completed = false;

            try
            {
                db.tblIEPAcademics.Add(studentAcademic);
                db.SaveChanges();

                AcademicID = studentAcademic.IEPAcademicID;
            }
            catch
            {
                current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Academic table");
            }

            // Add Reading Table
            studentReading = new tblIEPReading();
            studentReading.IEPid = current.IEPid;
            studentReading.NoConcerns = true;
            studentReading.ProgressTowardGenEd = false;
            studentReading.InstructionalTier1 = false;
            studentReading.InstructionalTier2 = false;
            studentReading.InstructionalTier3 = false;
            studentReading.AreaOfNeed = false;

            try
            {
                db.tblIEPReadings.Add(studentReading);
                db.SaveChanges();

                ReadingID = studentReading.IEPReadingID;
            }
            catch
            {
                current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Reading table");
            }

            // Add Math Table
            studentMath = new tblIEPMath();
            studentMath.IEPid = current.IEPid;
            studentMath.NoConcerns = true;
            studentMath.ProgressTowardGenEd = false;
            studentMath.InstructionalTier1 = false;
            studentMath.InstructionalTier2 = false;
            studentMath.InstructionalTier3 = false;
            studentMath.AreaOfNeed = false;

            try
            {
                db.tblIEPMaths.Add(studentMath);
                db.SaveChanges();

                MathID = studentMath.IEPMathID;
            }
            catch
            {
                current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Math table");
            }

            // Add Written Table
            studentWritten = new tblIEPWritten();
            studentWritten.IEPid = current.IEPid;
            studentWritten.NoConcerns = true;
            studentWritten.ProgressTowardGenEd = false;
            studentWritten.InstructionalTier1 = false;
            studentWritten.InstructionalTier2 = false;
            studentWritten.InstructionalTier3 = false;
            studentWritten.AreaOfNeed = false;

            try
            {
                db.tblIEPWrittens.Add(studentWritten);
                db.SaveChanges();

                WrittenID = studentWritten.IEPWrittenID;
            }
            catch
            {
                current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Written table");
            }

            // Add references to all the new created tables.
            current.IEPAcademicID = AcademicID;
            current.IEPCommunicationID = CommunicationID;
            current.IEPHealthID = HealthID;
            current.IEPIntelligenceID = IntelligenceID;
            current.IEPMathID = MathID;
            current.IEPMotorID = MotorID;
            current.IEPReadingID = ReadingID;
            current.IEPSocialID = SocialID;
            current.IEPWrittenID = WrittenID;

            db.SaveChanges();

            hasPlan = true;

            return this;
        }
    }
}