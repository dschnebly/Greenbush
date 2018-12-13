using System;
using System.Linq;
using System.Reflection;

namespace GreenBushIEP.Models
{
    public class StudentPlan
    {
        // connection to our database.
        private IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        // Private Properties
        public DateTime HealthVisionDate { get; set; }
        public DateTime HealthHearingDate { get; set; }
        public bool HealthNoConcern { get; set; }
        public bool HealthDiagnosis { get; set; }
        public bool HealthMedications { get; set; }
        public int HealthHearingResult { get; set; }
        public int HealthVisionResult { get; set; }
        public bool HealthVisionImpaired { get; set; }
        public bool HealthProgressTowardGenEd { get; set; }
        public bool HealthCarePlan { get; set; }
        public bool MotorNoConcern { get; set; }
        public bool MotorProgress { get; set; }
        public bool MotorNeeds { get; set; }
        public int MotorParticipation { get; set; }
        public bool CommunicationNoConcern { get; set; }
        public bool CommunicationAreaOfNeed { get; set; }
        public bool CommunicationDeaf { get; set; }
        public bool CommunicationEnglish { get; set; }
        public bool CommunicationProgressTowardGenEd { get; set; }
        public bool SocialNoConcern { get; set; }
        public bool SocialProgress { get; set; }
        public bool SocialAreaOfNeed { get; set; }
        public bool SocialMental { get; set; }
        public bool SocialBehaviorSignificant { get; set; }
        public bool SocialBehaviorImpede { get; set; }
        public bool SocialInterventionPlan { get; set; }
        public bool SocialProgressTowardGenEd { get; set; }
        public bool SocialSkillsDeficit { get; set; }
        public bool IntelligenceNoConcern { get; set; }
        public bool IntelligenceProgressTowardGenEd { get; set; }
        public bool IntelligenceAreaOfNeed { get; set; }
        public bool AcademicNoConcern { get; set; }
        public bool AcademicNeeds { get; set; }
        public bool AcademicProgressTowardGenEd { get; set; }
        public bool ReadingNoConcern { get; set; }
        public bool ReadingProgress { get; set; }
        public bool ReadingNeed { get; set; }
        public bool MathNoConcern { get; set; }
        public bool MathProgress { get; set; }
        public bool MathNeed { get; set; }
        public bool WrittenNoConcern { get; set; }
        public bool WrittenProgress { get; set; }
        public bool WrittenNeed { get; set; }
        public bool isDOC { get; set; }
        public int ExtendYear { get; set; }
        public bool RequireAssistiveTechnology { get; set; }

        // Create the student's plan
        public StudentPlan()
        {
            this.HealthHearingDate = DateTime.Now;
            this.HealthVisionDate = DateTime.Now;
            this.HealthNoConcern = true;
            this.HealthDiagnosis = false;
            this.HealthMedications = false;
            this.HealthHearingResult = -1;
            this.HealthVisionResult = -1;
            this.HealthVisionImpaired = false;
            this.HealthProgressTowardGenEd = false;
            this.HealthCarePlan = false;
            this.MotorNoConcern = true;
            this.MotorProgress = false;
            this.MotorNeeds = false;
            this.MotorParticipation = -1;
            this.CommunicationNoConcern = true;
            this.CommunicationAreaOfNeed = false;
            this.CommunicationDeaf = false;
            this.CommunicationEnglish = false;
            this.CommunicationProgressTowardGenEd = false;
            this.SocialNoConcern = true;
            this.SocialProgress = false;
            this.SocialAreaOfNeed = false;
            this.SocialMental = false;
            this.SocialBehaviorSignificant = false;
            this.SocialBehaviorImpede = false;
            this.SocialInterventionPlan = false;
            this.SocialProgressTowardGenEd = false;
            this.SocialSkillsDeficit = false;
            this.IntelligenceNoConcern = false;
            this.IntelligenceProgressTowardGenEd = false;
            this.IntelligenceAreaOfNeed = false;
            this.AcademicNoConcern = true;
            this.AcademicNeeds = false;
            this.AcademicProgressTowardGenEd = false;
            this.ReadingNoConcern = true;
            this.ReadingProgress = false;
            this.ReadingNeed = false;
            this.MathNoConcern = true;
            this.MathProgress = false;
            this.MathNeed = false;
            this.WrittenNoConcern = true;
            this.WrittenProgress = false;
            this.WrittenNeed = false;
            this.isDOC = false;
            this.RequireAssistiveTechnology = false;
        }

        // Read the student's plan
        public StudentPlan(int stid)
        {

            tblIEP studentIEP = db.tblIEPs.FirstOrDefault(i => i.UserID == stid && i.IsActive);
            if (studentIEP != null)
            {
                if (studentIEP.IepStatus == IEPStatus.PLAN)
                {
                    studentIEP.IepStatus = IEPStatus.DRAFT;
                }

                tblIEPHealth studentHealth = db.tblIEPHealths.FirstOrDefault(h => h.IEPHealthID == studentIEP.IEPHealthID);
                if (studentHealth != null)
                {
                    this.HealthDiagnosis = studentHealth.Diagnosis;
                    this.HealthHearingDate = studentHealth.HearingDate;
                    this.HealthHearingResult = studentHealth.HearingResult;
                    this.HealthMedications = studentHealth.Medications;
                    this.HealthNoConcern = studentHealth.NoConcerns;
                    this.HealthVisionDate = studentHealth.VisionDate;
                    this.HealthVisionImpaired = studentHealth.VisionImpaired;
                    this.HealthVisionResult = studentHealth.VisionResult;
                    this.HealthProgressTowardGenEd = studentHealth.ProgressTowardGenEd;
                    this.HealthCarePlan = studentHealth.HealthCarePlan ?? false;
                }

                tblIEPMotor studentMotor = db.tblIEPMotors.FirstOrDefault(m => m.IEPMotorID == studentIEP.IEPMotorID);
                if (studentMotor != null)
                {
                    this.MotorNeeds = studentMotor.Needs;
                    this.MotorNoConcern = studentMotor.NoConcerns;
                    this.MotorParticipation = studentMotor.Participation;
                    this.MotorProgress = studentMotor.ProgressTowardGenEd;
                }

                tblIEPCommunication studentCommunication = db.tblIEPCommunications.FirstOrDefault(s => s.IEPCommunicationID == studentIEP.IEPCommunicationID);
                if (studentCommunication != null)
                {
                    this.CommunicationDeaf = studentCommunication.Deaf;
                    this.CommunicationEnglish = studentCommunication.LimitedEnglish;
                    this.CommunicationNoConcern = studentCommunication.NoConcerns;
                    this.CommunicationAreaOfNeed = studentCommunication.AreaOfNeed == true ? true : false;
                    this.CommunicationProgressTowardGenEd = studentCommunication.ProgressTowardGenEd;
                }

                tblIEPSocial studentSocial = db.tblIEPSocials.FirstOrDefault(s => s.IEPSocialID == studentIEP.IEPSocialID);
                if (studentSocial != null)
                {
                    this.SocialBehaviorImpede = studentSocial.BehaviorImepedeLearning;
                    this.SocialBehaviorSignificant = studentSocial.SignificantBehaviors;
                    this.SocialInterventionPlan = studentSocial.BehaviorInterventionPlan;
                    this.SocialMental = studentSocial.MentalHealthDiagnosis;
                    this.SocialNoConcern = studentSocial.NoConcerns;
                    this.SocialProgress = studentSocial.ProgressTowardGenEd;
                    this.SocialAreaOfNeed = studentSocial.AreaOfNeed == true ? true : false;
                    this.SocialSkillsDeficit = studentSocial.SkillDeficit;
                }

                tblIEPIntelligence studentInt = db.tblIEPIntelligences.FirstOrDefault(i => i.IEPIntelligenceID == studentIEP.IEPIntelligenceID);
                if (studentInt != null)
                {
                    this.IntelligenceNoConcern = !studentInt.Concerns;
                    this.IntelligenceProgressTowardGenEd = studentInt.ProgressTowardGenEd;
                    this.IntelligenceAreaOfNeed = studentInt.AreaOfNeed == true ? true : false;
                }

                tblIEPAcademic studentAcademic = db.tblIEPAcademics.FirstOrDefault(a => a.IEPAcademicID == studentIEP.IEPAcademicID);
                if (studentAcademic != null)
                {
                    this.AcademicNeeds = studentAcademic.AreaOfNeed;
                    this.AcademicNoConcern = studentAcademic.NoConcerns;
                    this.AcademicProgressTowardGenEd = studentAcademic.ProgressTowardGenEd;
                }

                tblIEPReading studentReading = db.tblIEPReadings.FirstOrDefault(r => r.IEPReadingID == studentIEP.IEPReadingID);
                if (studentReading != null)
                {
                    this.ReadingNeed = studentReading.AreaOfNeed;
                    this.ReadingNoConcern = studentReading.NoConcerns;
                    this.ReadingProgress = studentReading.ProgressTowardGenEd;
                }

                tblIEPMath studentMath = db.tblIEPMaths.FirstOrDefault(m => m.IEPMathID == studentIEP.IEPReadingID);
                if (studentMath != null)
                {
                    this.MathNeed = studentMath.AreaOfNeed;
                    this.MathNoConcern = studentMath.NoConcerns;
                    this.MathProgress = studentMath.ProgressTowardGenEd;
                }

                tblIEPWritten studentWritten = db.tblIEPWrittens.FirstOrDefault(s => s.IEPWrittenID == studentIEP.IEPWrittenID);
                if (studentWritten != null)
                {
                    this.WrittenNeed = studentWritten.AreaOfNeed;
                    this.WrittenNoConcern = studentWritten.NoConcerns;
                    this.WrittenProgress = studentWritten.ProgressTowardGenEd;
                }

                tblStudentInfo info = db.tblStudentInfoes.Where(si => si.UserID == stid).FirstOrDefault();
                if(info != null)
                {
                    tblDistrict district = db.tblDistricts.Where(d => d.USD == info.USD).FirstOrDefault();
                    if(district != null)
                    {
                        isDOC = district.DOC;
                    }
                }

                tblOtherConsideration otherConsideration = db.tblOtherConsiderations.FirstOrDefault(s => s.IEPid == studentIEP.IEPid);
                if(otherConsideration != null)
                {
                    int extendYear = 0;
                    Int32.TryParse(otherConsideration.ExtendedSchoolYear_Necessary, out extendYear);
                    this.ExtendYear = extendYear;
                    this.RequireAssistiveTechnology = otherConsideration.AssistiveTechnology_Require.HasValue ? otherConsideration.AssistiveTechnology_Require.Value : false;
                }
            }
        }

        // Update the student's plan.
        public void Update(int stid)
        {
            tblIEP studentIEP = db.tblIEPs.FirstOrDefault(i => i.UserID == stid);

            if (studentIEP != null)
            {
                if (studentIEP.IepStatus == IEPStatus.PLAN) {
                    studentIEP.IepStatus = IEPStatus.DRAFT;
                }

                tblIEPHealth studentHealth = db.tblIEPHealths.FirstOrDefault(h => h.IEPHealthID == studentIEP.IEPHealthID);

                if (studentHealth != null)
                {
                    studentHealth.Diagnosis = this.HealthDiagnosis;
                    studentHealth.HearingDate = this.HealthHearingDate;
                    studentHealth.HearingResult = this.HealthHearingResult;
                    studentHealth.Medications = this.HealthMedications;
                    studentHealth.NoConcerns = this.HealthNoConcern;
                    studentHealth.VisionDate = this.HealthVisionDate;
                    studentHealth.VisionImpaired = this.HealthVisionImpaired;
                    studentHealth.VisionResult = this.HealthVisionResult;
                    studentHealth.ProgressTowardGenEd = this.HealthProgressTowardGenEd;
                    studentHealth.HealthCarePlan = this.HealthCarePlan;
                    studentHealth.Completed = this.HealthNoConcern;
                }
                db.SaveChanges();

                tblIEPMotor studentMotor = db.tblIEPMotors.FirstOrDefault(m => m.IEPMotorID == studentIEP.IEPMotorID);

                if (studentMotor != null)
                {
                    studentMotor.Needs = this.MotorNeeds;
                    studentMotor.NoConcerns = this.MotorNoConcern;
                    studentMotor.Participation = this.MotorParticipation;
                    studentMotor.ProgressTowardGenEd = this.MotorProgress;
                    studentMotor.Completed = this.MotorNoConcern;
                }
                db.SaveChanges();

                tblIEPCommunication studentCommunication = db.tblIEPCommunications.FirstOrDefault(s => s.IEPCommunicationID == studentIEP.IEPCommunicationID);

                if (studentCommunication != null)
                {
                    studentCommunication.Deaf = this.CommunicationDeaf;
                    studentCommunication.LimitedEnglish = this.CommunicationEnglish;
                    studentCommunication.NoConcerns = this.CommunicationNoConcern;
                    studentCommunication.AreaOfNeed = this.CommunicationAreaOfNeed;
                    studentCommunication.ProgressTowardGenEd = this.CommunicationProgressTowardGenEd;
                    studentCommunication.Completed = this.CommunicationNoConcern;
                }
                db.SaveChanges();

                tblIEPSocial studentSocial = db.tblIEPSocials.FirstOrDefault(s => s.IEPSocialID == studentIEP.IEPSocialID);

                if (studentSocial != null)
                {
                    studentSocial.BehaviorImepedeLearning = this.SocialBehaviorImpede;
                    studentSocial.SignificantBehaviors = this.SocialBehaviorSignificant;
                    studentSocial.BehaviorInterventionPlan = this.SocialInterventionPlan;
                    studentSocial.MentalHealthDiagnosis = this.SocialMental;
                    studentSocial.NoConcerns = this.SocialNoConcern;
                    studentSocial.ProgressTowardGenEd = this.SocialProgressTowardGenEd;
                    studentSocial.AreaOfNeed = this.SocialAreaOfNeed;
                    studentSocial.SkillDeficit = this.SocialSkillsDeficit;
                    studentSocial.Completed = this.SocialNoConcern;
                }
                db.SaveChanges();

                tblIEPIntelligence studentInt = db.tblIEPIntelligences.FirstOrDefault(i => i.IEPIntelligenceID == studentIEP.IEPIntelligenceID);

                if (studentInt != null)
                {
                    studentInt.Concerns = !this.IntelligenceNoConcern;
                    studentInt.ProgressTowardGenEd = this.IntelligenceProgressTowardGenEd;
                    studentInt.AreaOfNeed = this.IntelligenceAreaOfNeed;
                    studentInt.Completed = this.IntelligenceNoConcern;
                }
                db.SaveChanges();

                tblIEPAcademic studentAcademic = db.tblIEPAcademics.FirstOrDefault(a => a.IEPAcademicID == studentIEP.IEPAcademicID);

                if (studentAcademic != null)
                {
                    studentAcademic.AreaOfNeed = this.AcademicNeeds;
                    studentAcademic.NoConcerns = this.AcademicNoConcern;
                    studentAcademic.ProgressTowardGenEd = this.AcademicProgressTowardGenEd;
                    studentAcademic.Completed = this.AcademicNoConcern;
                }
                db.SaveChanges();

                tblIEPReading studentReading = db.tblIEPReadings.FirstOrDefault(r => r.IEPReadingID == studentIEP.IEPReadingID);

                if (studentReading != null)
                {
                    studentReading.AreaOfNeed = this.ReadingNeed;
                    studentReading.NoConcerns = this.ReadingNoConcern;
                    studentReading.ProgressTowardGenEd = this.ReadingProgress;
                }
                db.SaveChanges();

                tblIEPMath studentMath = db.tblIEPMaths.FirstOrDefault(m => m.IEPMathID == studentIEP.IEPReadingID);

                if (studentMath != null)
                {
                    studentMath.AreaOfNeed = this.MathNeed;
                    studentMath.NoConcerns = this.MathNoConcern;
                    studentMath.ProgressTowardGenEd = this.MathProgress;
                }
                db.SaveChanges();

                tblIEPWritten studentWritten = db.tblIEPWrittens.FirstOrDefault(s => s.IEPWrittenID == studentIEP.IEPWrittenID);

                if (studentWritten != null)
                {
                    studentWritten.AreaOfNeed = this.WrittenNeed;
                    studentWritten.NoConcerns = this.WrittenNoConcern;
                    studentWritten.ProgressTowardGenEd = this.WrittenProgress;
                }
                db.SaveChanges();

                tblOtherConsideration otherConsideration = db.tblOtherConsiderations.FirstOrDefault(s => s.IEPid == studentIEP.IEPid);

                if (otherConsideration != null)
                {
                    otherConsideration.ExtendedSchoolYear_Necessary = this.ExtendYear.ToString();
                    otherConsideration.AssistiveTechnology_Require = this.RequireAssistiveTechnology;
                    otherConsideration.Completed = this.CommunicationNoConcern;
                    
                }
                db.SaveChanges();
            }

        }

        public object this[string propertyName]
        {
            set
            {
                Type myType = typeof(StudentPlan);

                PropertyInfo myPropInfo = myType.GetProperty(propertyName);

                //Convert.ChangeType does not handle conversion to nullable types
                //if the property type is nullable, we need to get the underlying type of the property
                var targetType = IsNullableType(myPropInfo.PropertyType) ? Nullable.GetUnderlyingType(myPropInfo.PropertyType) : myPropInfo.PropertyType;

                //Returns an System.Object with the specified System.Type and whose value is
                //equivalent to the specified object.
                value = Convert.ChangeType(value, targetType);

                myPropInfo.SetValue(this, value, null);
            }
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }
    }
}