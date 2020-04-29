using System;
using System.Linq;
using System.Reflection;

namespace GreenBushIEP.Models
{
    public class StudentPlan
    {
        // connection to our database.
        private readonly IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

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
        public bool AcademicModuleNoConcern { get; set; }
        public bool AcademicNoConcern { get; set; }
        public bool AcademicCompleted { get; set; }
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
        public bool RequireAssistiveTechnology { get; set; }
        public int ExtendYear { get; set; }
        public bool isDOC { get; set; }

        // Create the student's plan
        public StudentPlan()
        {
            HealthHearingDate = DateTime.Now;
            HealthVisionDate = DateTime.Now;
            HealthNoConcern = true;
            HealthDiagnosis = false;
            HealthMedications = false;
            HealthHearingResult = -1;
            HealthVisionResult = -1;
            HealthVisionImpaired = false;
            HealthProgressTowardGenEd = false;
            HealthCarePlan = false;
            MotorNoConcern = true;
            MotorProgress = false;
            MotorNeeds = false;
            MotorParticipation = -1;
            CommunicationNoConcern = true;
            CommunicationAreaOfNeed = false;
            CommunicationDeaf = false;
            CommunicationEnglish = false;
            CommunicationProgressTowardGenEd = false;
            SocialNoConcern = true;
            SocialProgress = false;
            SocialAreaOfNeed = false;
            SocialMental = false;
            SocialBehaviorSignificant = false;
            SocialBehaviorImpede = false;
            SocialInterventionPlan = false;
            SocialProgressTowardGenEd = false;
            SocialSkillsDeficit = false;
            IntelligenceNoConcern = false;
            IntelligenceProgressTowardGenEd = false;
            IntelligenceAreaOfNeed = false;
            AcademicModuleNoConcern = true;
            AcademicCompleted = false;
            AcademicNoConcern = true;
            AcademicNeeds = false;
            AcademicProgressTowardGenEd = false;
            ReadingNoConcern = true;
            ReadingProgress = false;
            ReadingNeed = false;
            MathNoConcern = true;
            MathProgress = false;
            MathNeed = false;
            WrittenNoConcern = true;
            WrittenProgress = false;
            WrittenNeed = false;
            RequireAssistiveTechnology = false;
            ExtendYear = 0;
            isDOC = false;
        }

        // Read the student's plan
        public StudentPlan(int stid, int? iepID = null)
        {

            tblIEP studentIEP = null;

            if (iepID.HasValue)
            {
                studentIEP = db.tblIEPs.FirstOrDefault(i => i.UserID == stid && i.IEPid == iepID.Value);
            }
            else
            {
                studentIEP = db.tblIEPs.FirstOrDefault(i => i.UserID == stid && i.IsActive);
            }

            if (studentIEP != null)
            {

                if (studentIEP.IepStatus == IEPStatus.PLAN)
                {
                    studentIEP.IepStatus = IEPStatus.DRAFT;
                    db.SaveChanges();
                }

                tblIEPHealth studentHealth = db.tblIEPHealths.FirstOrDefault(h => h.IEPHealthID == studentIEP.IEPHealthID);
                if (studentHealth != null)
                {
                    HealthDiagnosis = studentHealth.Diagnosis;
                    HealthHearingDate = studentHealth.HearingDate;
                    HealthHearingResult = studentHealth.HearingResult;
                    HealthMedications = studentHealth.Medications;
                    HealthNoConcern = studentHealth.NoConcerns;
                    HealthVisionDate = studentHealth.VisionDate;
                    HealthVisionImpaired = studentHealth.VisionImpaired;
                    HealthVisionResult = studentHealth.VisionResult;
                    HealthProgressTowardGenEd = studentHealth.ProgressTowardGenEd;
                    HealthCarePlan = studentHealth.HealthCarePlan ?? false;
                }

                tblIEPMotor studentMotor = db.tblIEPMotors.FirstOrDefault(m => m.IEPMotorID == studentIEP.IEPMotorID);
                if (studentMotor != null)
                {
                    MotorNeeds = studentMotor.Needs;
                    MotorNoConcern = studentMotor.NoConcerns;
                    MotorParticipation = studentMotor.Participation;
                    MotorProgress = studentMotor.ProgressTowardGenEd;
                }

                tblIEPCommunication studentCommunication = db.tblIEPCommunications.FirstOrDefault(s => s.IEPCommunicationID == studentIEP.IEPCommunicationID);
                if (studentCommunication != null)
                {
                    CommunicationDeaf = studentCommunication.Deaf;
                    CommunicationEnglish = studentCommunication.LimitedEnglish;
                    CommunicationNoConcern = studentCommunication.NoConcerns;
                    CommunicationAreaOfNeed = studentCommunication.AreaOfNeed == true ? true : false;
                    CommunicationProgressTowardGenEd = studentCommunication.ProgressTowardGenEd;
                }

                tblIEPSocial studentSocial = db.tblIEPSocials.FirstOrDefault(s => s.IEPSocialID == studentIEP.IEPSocialID);
                if (studentSocial != null)
                {
                    SocialBehaviorImpede = studentSocial.BehaviorImepedeLearning;
                    SocialBehaviorSignificant = studentSocial.SignificantBehaviors;
                    SocialInterventionPlan = studentSocial.BehaviorInterventionPlan;
                    SocialMental = studentSocial.MentalHealthDiagnosis;
                    SocialNoConcern = studentSocial.NoConcerns;
                    SocialProgress = studentSocial.ProgressTowardGenEd;
                    SocialAreaOfNeed = studentSocial.AreaOfNeed == true ? true : false;
                    SocialSkillsDeficit = studentSocial.SkillDeficit;
                }

                tblIEPIntelligence studentInt = db.tblIEPIntelligences.FirstOrDefault(i => i.IEPIntelligenceID == studentIEP.IEPIntelligenceID);
                if (studentInt != null)
                {
                    IntelligenceNoConcern = !studentInt.Concerns;
                    IntelligenceProgressTowardGenEd = studentInt.ProgressTowardGenEd;
                    IntelligenceAreaOfNeed = studentInt.AreaOfNeed == true ? true : false;
                }

                tblIEPAcademic studentAcademic = db.tblIEPAcademics.FirstOrDefault(a => a.IEPAcademicID == studentIEP.IEPAcademicID);
                if (studentAcademic != null)
                {
                    AcademicCompleted = studentAcademic.Completed;
                    AcademicNeeds = studentAcademic.AreaOfNeed;
                    AcademicNoConcern = studentAcademic.NoConcerns;
                    AcademicProgressTowardGenEd = studentAcademic.ProgressTowardGenEd;
                }

                tblIEPReading studentReading = db.tblIEPReadings.FirstOrDefault(r => r.IEPReadingID == studentIEP.IEPReadingID);
                if (studentReading != null)
                {
                    ReadingNeed = studentReading.AreaOfNeed;
                    ReadingNoConcern = studentReading.NoConcerns;
                    ReadingProgress = studentReading.ProgressTowardGenEd;
                }

                tblIEPMath studentMath = db.tblIEPMaths.FirstOrDefault(m => m.IEPMathID == studentIEP.IEPReadingID);
                if (studentMath != null)
                {
                    MathNeed = studentMath.AreaOfNeed;
                    MathNoConcern = studentMath.NoConcerns;
                    MathProgress = studentMath.ProgressTowardGenEd;
                }

                tblIEPWritten studentWritten = db.tblIEPWrittens.FirstOrDefault(s => s.IEPWrittenID == studentIEP.IEPWrittenID);
                if (studentWritten != null)
                {
                    WrittenNeed = studentWritten.AreaOfNeed;
                    WrittenNoConcern = studentWritten.NoConcerns;
                    WrittenProgress = studentWritten.ProgressTowardGenEd;
                }

                AcademicModuleNoConcern = AcademicNoConcern & ReadingNoConcern & MathNoConcern & WrittenNoConcern;

                tblStudentInfo info = db.tblStudentInfoes.Where(si => si.UserID == stid).FirstOrDefault();
                if (info != null)
                {
                    tblDistrict district = db.tblDistricts.Where(d => d.USD == info.USD).FirstOrDefault();
                    if (district != null)
                    {
                        isDOC = district.DOC;
                    }
                }

                tblOtherConsideration otherConsideration = db.tblOtherConsiderations.FirstOrDefault(s => s.IEPid == studentIEP.IEPid);
                if (otherConsideration != null)
                {
                    int extendYear = 0;
                    int.TryParse(otherConsideration.ExtendedSchoolYear_Necessary, out extendYear);
                    ExtendYear = extendYear;
                    RequireAssistiveTechnology = otherConsideration.AssistiveTechnology_Require.HasValue ? otherConsideration.AssistiveTechnology_Require.Value : false;
                }
            }
        }

        // Update the student's plan.
        public void Update(int stid, int iepId)
        {
            tblIEP studentIEP = db.tblIEPs.FirstOrDefault(i => i.UserID == stid && i.IEPid == iepId);

            if (studentIEP != null)
            {
                if (studentIEP.IepStatus == IEPStatus.PLAN)
                {
                    studentIEP.IepStatus = IEPStatus.DRAFT;
                    db.SaveChanges();
                }

                tblIEPHealth studentHealth = db.tblIEPHealths.FirstOrDefault(h => h.IEPHealthID == studentIEP.IEPHealthID);

                if (studentHealth != null)
                {
                    if (HealthNoConcern)
                    {
                        HealthProgressTowardGenEd = false;
                        HealthDiagnosis = false;
                        studentHealth.Completed = true;
                    }
                    else
                    {
                        if (studentHealth.Completed == false)
                        {
                            //only update if it hasn't already been marked as completed
                            studentHealth.Completed = HealthNoConcern;
                        }

                    }

                    studentHealth.Diagnosis = HealthDiagnosis;
                    studentHealth.HearingDate = HealthHearingDate;
                    studentHealth.HearingResult = HealthHearingResult;
                    studentHealth.Medications = HealthMedications;
                    studentHealth.VisionDate = HealthVisionDate;
                    studentHealth.VisionImpaired = HealthVisionImpaired;
                    studentHealth.VisionResult = HealthVisionResult;
                    studentHealth.ProgressTowardGenEd = HealthProgressTowardGenEd;
                    studentHealth.HealthCarePlan = HealthCarePlan;

                    if (HealthProgressTowardGenEd || HealthDiagnosis)
                    {
                        studentHealth.NoConcerns = false;
                        studentHealth.ProgressTowardGenEd = HealthProgressTowardGenEd;
                        studentHealth.Diagnosis = HealthDiagnosis;
                        studentHealth.Concerns = true;
                    }
                    else
                    {
                        studentHealth.NoConcerns = HealthNoConcern;
                        studentHealth.ProgressTowardGenEd = false;
                        studentHealth.Diagnosis = false;
                    }
                }
                db.SaveChanges();

                tblIEPMotor studentMotor = db.tblIEPMotors.FirstOrDefault(m => m.IEPMotorID == studentIEP.IEPMotorID);

                if (studentMotor != null)
                {
                    studentMotor.Needs = MotorNeeds;
                    studentMotor.NoConcerns = MotorNoConcern;
                    studentMotor.Participation = MotorParticipation;
                    studentMotor.ProgressTowardGenEd = MotorProgress;

                    if (studentMotor.Completed == false)
                    {
                        studentMotor.Completed = MotorNoConcern;
                    }
                }
                db.SaveChanges();

                tblIEPCommunication studentCommunication = db.tblIEPCommunications.FirstOrDefault(s => s.IEPCommunicationID == studentIEP.IEPCommunicationID);

                if (studentCommunication != null)
                {
                    studentCommunication.Deaf = CommunicationDeaf;
                    studentCommunication.LimitedEnglish = CommunicationEnglish;
                    studentCommunication.NoConcerns = CommunicationNoConcern;
                    studentCommunication.AreaOfNeed = CommunicationAreaOfNeed;
                    studentCommunication.ProgressTowardGenEd = CommunicationProgressTowardGenEd;
                    if (studentCommunication.Completed == false)
                    {
                        studentCommunication.Completed = CommunicationNoConcern;
                    }
                }
                db.SaveChanges();

                tblIEPSocial studentSocial = db.tblIEPSocials.FirstOrDefault(s => s.IEPSocialID == studentIEP.IEPSocialID);

                if (studentSocial != null)
                {
                    studentSocial.BehaviorImepedeLearning = SocialBehaviorImpede;
                    studentSocial.SignificantBehaviors = SocialBehaviorSignificant;
                    studentSocial.BehaviorInterventionPlan = SocialInterventionPlan;
                    studentSocial.MentalHealthDiagnosis = SocialMental;
                    studentSocial.NoConcerns = SocialNoConcern;
                    studentSocial.ProgressTowardGenEd = SocialProgressTowardGenEd;
                    studentSocial.AreaOfNeed = SocialAreaOfNeed;
                    studentSocial.SkillDeficit = SocialSkillsDeficit;
                    if (studentSocial.Completed == false)
                    {
                        studentSocial.Completed = SocialNoConcern;
                    }
                }
                db.SaveChanges();

                tblIEPIntelligence studentInt = db.tblIEPIntelligences.FirstOrDefault(i => i.IEPIntelligenceID == studentIEP.IEPIntelligenceID);

                if (studentInt != null)
                {
                    studentInt.Concerns = !IntelligenceNoConcern;
                    studentInt.ProgressTowardGenEd = IntelligenceProgressTowardGenEd;
                    studentInt.AreaOfNeed = IntelligenceAreaOfNeed;
                    if (studentInt.Completed == false)
                    {
                        studentInt.Completed = IntelligenceNoConcern;
                    }
                }
                db.SaveChanges();

                tblIEPAcademic studentAcademic = db.tblIEPAcademics.FirstOrDefault(a => a.IEPAcademicID == studentIEP.IEPAcademicID);

                if (studentAcademic != null)
                {
                    studentAcademic.AreaOfNeed = AcademicNeeds;
                    studentAcademic.NoConcerns = AcademicNoConcern;
                    studentAcademic.ProgressTowardGenEd = AcademicProgressTowardGenEd;
                }
                db.SaveChanges();

                tblIEPReading studentReading = db.tblIEPReadings.FirstOrDefault(r => r.IEPReadingID == studentIEP.IEPReadingID);

                if (studentReading != null)
                {
                    studentReading.AreaOfNeed = ReadingNeed;
                    studentReading.NoConcerns = ReadingNoConcern;
                    studentReading.ProgressTowardGenEd = ReadingProgress;
                }
                db.SaveChanges();

                tblIEPMath studentMath = db.tblIEPMaths.FirstOrDefault(m => m.IEPMathID == studentIEP.IEPReadingID);

                if (studentMath != null)
                {
                    studentMath.AreaOfNeed = MathNeed;
                    studentMath.NoConcerns = MathNoConcern;
                    studentMath.ProgressTowardGenEd = MathProgress;
                }
                db.SaveChanges();

                tblIEPWritten studentWritten = db.tblIEPWrittens.FirstOrDefault(s => s.IEPWrittenID == studentIEP.IEPWrittenID);

                if (studentWritten != null)
                {
                    studentWritten.AreaOfNeed = WrittenNeed;
                    studentWritten.NoConcerns = WrittenNoConcern;
                    studentWritten.ProgressTowardGenEd = WrittenProgress;
                }
                db.SaveChanges();

                AcademicModuleNoConcern = AcademicNoConcern & ReadingNoConcern & MathNoConcern & WrittenNoConcern;


                if (!studentAcademic.Completed)
                {

                    if (!AcademicModuleNoConcern) // if any of the four types has a problem than the module is not completed
                    {
                        studentAcademic.Completed = false;
                    }
                    else
                    {
                        studentAcademic.Completed = true;
                    }

                    db.SaveChanges();
                }

                tblOtherConsideration otherConsideration = db.tblOtherConsiderations.FirstOrDefault(s => s.IEPid == studentIEP.IEPid);

                if (otherConsideration != null)
                {
                    otherConsideration.ExtendedSchoolYear_Necessary = ExtendYear.ToString();
                    otherConsideration.AssistiveTechnology_Require = RequireAssistiveTechnology;
                    //otherConsideration.Completed = !(this.RequireAssistiveTechnology | this.ExtendYear > 0);
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
                Type targetType = IsNullableType(myPropInfo.PropertyType) ? Nullable.GetUnderlyingType(myPropInfo.PropertyType) : myPropInfo.PropertyType;

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