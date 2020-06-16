using System;
using System.Collections.Generic;
using System.Linq;

namespace GreenBushIEP.Models
{
    public class IEP
    {
        private readonly IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

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
        public bool isAllCompleted { get; set; }
        public bool hasAccommodations { get; set; }
        public bool hasBehavior { get; set; }
        public bool hasPlan { get; set; }

        public bool anyStudentIEPActive { get; set; }
        public bool anyStudentIEPAmendment { get; set; }
        public bool anyStudentIEPDraft { get; set; }
        public bool anyStudentIEPAnnual { get; set; }

        public tblIEPHealth studentHealth { get; set; }
        public tblIEPMotor studentMotor { get; set; }
        public tblIEPCommunication studentCommunication { get; set; }
        public tblIEPSocial studentSocial { get; set; }
        public tblIEPIntelligence studentIntelligence { get; set; }
        public tblIEPAcademic studentAcademic { get; set; }
        public tblIEPReading studentReading { get; set; }
        public tblIEPMath studentMath { get; set; }
        public tblIEPWritten studentWritten { get; set; }
        public tblOtherConsideration studentOtherConsiderations { get; set; }

        public List<tblGoal> studentGoals { get; set; }
        public List<tblGoalBenchmark> studentGoalBenchmarks { get; set; }
        public List<tblGoalBenchmarkMethod> studentGoalBenchmarkMethods { get; set; }
        public List<tblGoalEvaluationProcedure> studentGoalEvalProcs { get; set; }
        public List<tblService> studentServices { get; set; }
        public List<tblLocation> locations { get; set; }
        public List<tblServiceType> serviceTypes { get; set; }
        public List<tblProvider> serviceProviders { get; set; }
        public List<tblAccommodation> accommodations { get; set; }
        public List<tblIEP> listOfStudentsIEPs { get; set; }

        public tblIEP current { get; set; }
        public BehaviorViewModel studentBehavior { get; set; }
        public StudentTransitionViewModel studentTransition { get; set; }

        public string iepStatusType { get; set; }
        public string displayIEPStatus { get; set; }
        public DateTime? iepStartTime { get; set; }
        public string studentFirstName { get; set; }
        public string studentLastName { get; set; }
        public int studentAge { get; set; }
        public bool isServerRender { get; set; }

        //for printing
        public StudentDetailsPrintViewModel studentDetails { get; set; }

        public IEP()
        {
            isHealthCompleted = false;
            isMotorCompleted = false;
            isCommunicationCompleted = false;
            isSocialCompleted = false;
            isIntelligenceCompleted = false;
            isOtherCompleted = false;
            isAcademicCompleted = false;
            isBehaviorCompleted = false;
            isTransitionCompleted = false;
            isServiceCompleted = false;
            hasAccommodations = false;
            isAllCompleted = false;
            hasAccommodations = false;
            hasBehavior = false;
            hasPlan = false;
            isServerRender = false;

            listOfStudentsIEPs = new List<tblIEP>();
            current = new tblIEP();
            anyStudentIEPActive = false;
            anyStudentIEPAmendment = false;
            anyStudentIEPDraft = false;
            anyStudentIEPAnnual = false;
            iepStatusType = IEPStatus.DRAFT;
            displayIEPStatus = IEPStatus.DRAFT;
            iepStartTime = new DateTime();

            studentGoals = new List<tblGoal>();
            studentGoalBenchmarks = new List<tblGoalBenchmark>();
            studentGoalEvalProcs = new List<tblGoalEvaluationProcedure>();
            studentGoalBenchmarkMethods = new List<tblGoalBenchmarkMethod>();
            studentServices = new List<tblService>();
            locations = new List<tblLocation>();
            serviceTypes = new List<tblServiceType>();
            serviceProviders = new List<tblProvider>();
            accommodations = new List<tblAccommodation>();
            studentDetails = new StudentDetailsPrintViewModel();
        }

        public IEP(int stid, int? iepId = null, int? startNew = null)
        {
            bool iepStarted = true;
            listOfStudentsIEPs = db.tblIEPs.Where(i => i.UserID == stid && i.IsActive && i.IepStatus != IEPStatus.ARCHIVE).OrderBy(i => i.IepStatus).ThenBy(i => i.Amendment).ToList();

            if (listOfStudentsIEPs.Count > 0)
            {
                if (iepId.HasValue)
                {
                    // get the first iep by id, else it must be an ammendment.
                    tblIEP studentIEP = listOfStudentsIEPs.Where(i => i.IEPid == iepId).FirstOrDefault();
                    tblIEP studentAmendment = listOfStudentsIEPs.Where(i => i.AmendingIEPid == iepId).FirstOrDefault();
                    current = studentIEP ?? studentAmendment;
                }
                else
                {
                    current = listOfStudentsIEPs.FirstOrDefault();
                }

                hasPlan = current.IepStatus != IEPStatus.PLAN;
            }
            else
            {
                if (startNew.HasValue)
                {
                    IEP studentIEP = CreateNewIEP(stid);
                    current = studentIEP.current;
                    hasPlan = false;
                }
                else
                {
                    current = new tblIEP();
                    iepStarted = false;
                }
            }

            anyStudentIEPActive = listOfStudentsIEPs.Any(i => i.IepStatus.ToUpper() == IEPStatus.ACTIVE && i.IsActive);
            anyStudentIEPAmendment = listOfStudentsIEPs.Any(i => i.IepStatus.ToUpper() == IEPStatus.DRAFT && i.Amendment && i.IsActive);
            anyStudentIEPDraft = listOfStudentsIEPs.Any(i => i.IepStatus.ToUpper() == IEPStatus.DRAFT && !i.Amendment && i.IsActive);
            anyStudentIEPAnnual = listOfStudentsIEPs.Any(i => i.IepStatus.ToUpper() == IEPStatus.ANNUAL && i.IsActive);

            if (iepStarted)
            {
                studentHealth = db.tblIEPHealths.Where(h => h.IEPHealthID == current.IEPHealthID).FirstOrDefault();
                studentMotor = db.tblIEPMotors.Where(m => m.IEPMotorID == current.IEPMotorID).FirstOrDefault();
                studentCommunication = db.tblIEPCommunications.Where(c => c.IEPCommunicationID == current.IEPCommunicationID).FirstOrDefault();
                studentSocial = db.tblIEPSocials.Where(s => s.IEPSocialID == current.IEPSocialID).FirstOrDefault();
                studentIntelligence = db.tblIEPIntelligences.Where(i => i.IEPIntelligenceID == current.IEPIntelligenceID).FirstOrDefault();
                studentAcademic = db.tblIEPAcademics.Where(a => a.IEPAcademicID == current.IEPAcademicID).FirstOrDefault();
                studentWritten = db.tblIEPWrittens.Where(w => w.IEPWrittenID == current.IEPWrittenID).FirstOrDefault();
                studentReading = db.tblIEPReadings.Where(r => r.IEPReadingID == current.IEPReadingID).FirstOrDefault();
                studentMath = db.tblIEPMaths.Where(m => m.IEPMathID == current.IEPMathID).FirstOrDefault();
                studentOtherConsiderations = db.tblOtherConsiderations.Where(o => o.IEPid == current.IEPid).FirstOrDefault();
                studentGoals = db.tblGoals.Where(g => g.IEPid == current.IEPid).ToList();
                studentServices = db.tblServices.Where(s => s.IEPid == current.IEPid).ToList();
                accommodations = db.tblAccommodations.Where(a => a.IEPid == current.IEPid).ToList();
                List<tblTransition> transitions = db.tblTransitions.Where(a => a.IEPid == current.IEPid).ToList();

                // all our database information should be loaded by now. Just query our student lists.
                isHealthCompleted = studentHealth != null ? studentHealth.Completed : false;
                isMotorCompleted = studentMotor != null ? studentMotor.Completed : false;
                isCommunicationCompleted = studentCommunication != null ? studentCommunication.Completed : false;
                isSocialCompleted = studentSocial != null ? studentSocial.Completed : false;
                isIntelligenceCompleted = studentIntelligence != null ? studentIntelligence.Completed : false;
                isAcademicCompleted = studentAcademic != null ? studentAcademic.Completed : false;
                isOtherCompleted = studentOtherConsiderations != null ? studentOtherConsiderations.Completed : false;

                bool allModulesCompleted = (isHealthCompleted || studentHealth.NoConcerns) && (isMotorCompleted || studentMotor.NoConcerns) && (isCommunicationCompleted || studentCommunication.NoConcerns) && (isSocialCompleted || studentSocial.NoConcerns) && (isIntelligenceCompleted || !studentIntelligence.Concerns) && (isAcademicCompleted || studentAcademic.NoConcerns);
                
				CheckCompleted();
				
				isServiceCompleted = studentServices != null ? studentServices.All(s => s.Completed) && studentServices.Count > 0 : false;                
                isBehaviorCompleted = db.tblBehaviors.Where(b => b.IEPid == current.IEPid).FirstOrDefault() != null ? db.tblBehaviors.Where(b => b.IEPid == current.IEPid).FirstOrDefault().Completed || studentSocial.Completed : !(studentSocial != null && (studentSocial.BehaviorInterventionPlan));
                isAllCompleted = isHealthCompleted & isMotorCompleted & isCommunicationCompleted && isSocialCompleted && isIntelligenceCompleted && isAcademicCompleted && isOtherCompleted && isGoalCompleted && isServiceCompleted && isAccommodationsCompleted && isBehaviorCompleted;
                isTransitionCompleted = transitions != null ? transitions.Any(o => o.Completed) : false;

                bool healthNeeds = (studentHealth != null && (studentHealth.NeedMetByAccommodation.HasValue && studentHealth.NeedMetByAccommodation.Value));
                bool motorNeeds = (studentMotor != null && (studentMotor.NeedMetByAccommodation.HasValue && studentMotor.NeedMetByAccommodation.Value));
                bool communicationNeeds = (studentCommunication != null && (studentCommunication.NeedMetByAccommodation.HasValue && studentCommunication.NeedMetByAccommodation.Value));
                bool socialNeeds = (studentSocial != null && (studentSocial.NeedMetByAccommodation.HasValue && studentSocial.NeedMetByAccommodation.Value));
                bool academicNeeds = (studentAcademic != null && (studentAcademic.NeedMetByAccommodation.HasValue && studentAcademic.NeedMetByAccommodation.Value));
                bool intelligenceNeeds = (studentIntelligence != null && (studentIntelligence.NeedMetByAccommodation.HasValue && studentIntelligence.NeedMetByAccommodation.Value));
                bool readingNeeds = (studentReading != null && (studentReading.NeedMetByAccommodation.HasValue && studentReading.NeedMetByAccommodation.Value));
                bool writtensNeeds = (studentWritten != null && (studentWritten.NeedMetByAccommodation.HasValue && studentWritten.NeedMetByAccommodation.Value));
                bool mathNeeds = (studentMath != null && (studentMath.NeedMetByAccommodation.HasValue && studentMath.NeedMetByAccommodation.Value));

                tblIEP originalIEP = db.tblIEPs.Where(i => i.IEPid == current.OriginalIEPid).FirstOrDefault();
                hasAccommodations = healthNeeds | motorNeeds | communicationNeeds | socialNeeds | academicNeeds | intelligenceNeeds | readingNeeds | writtensNeeds | mathNeeds;
                hasBehavior = (studentSocial != null && studentSocial.BehaviorInterventionPlan);
                iepStatusType = (current.Amendment & current.IsActive & current.IepStatus.ToUpper() == IEPStatus.DRAFT) ? IEPStatus.AMENDMENT : ((!current.IsActive) ? IEPStatus.ARCHIVE : current.IepStatus).ToUpper();
                displayIEPStatus = (iepStatusType == IEPStatus.DRAFT && anyStudentIEPActive && !current.Amendment ? "ANNUAL" : string.Empty) + " " + iepStatusType + " " + (current.Amendment && iepStatusType != IEPStatus.ACTIVE ? "DRAFT" : string.Empty);
                iepStartTime = (iepStatusType != IEPStatus.ANNUAL && originalIEP != null) ? originalIEP.begin_date : current.begin_date.HasValue ? current.begin_date : DateTime.Now;
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
            int OtherID;

            // Check that we don't already have a draft copy being used by this user. 
            // If we do we need to return an error.
            if (current != null)
            {
                throw new System.ArgumentException("There is already a draft IEP for this user");
            }

            tblStudentInfo studentDetails = db.tblStudentInfoes.Where(o => o.UserID == stid).FirstOrDefault();

            current = new tblIEP
            {
                UserID = stid,
                IepStatus = IEPStatus.PLAN,
                Create_Date = DateTime.Now,
                end_Date = null,
                Update_Date = DateTime.Now,
                begin_date = null,
                Amendment = false,
                StateAssessment = string.Empty,
                IsActive = true
            };

            if (studentDetails != null)
            {
                current.StatusCode = studentDetails.StatusCode;
            }

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
            studentHealth = new tblIEPHealth
            {
                IEPid = current.IEPid,
                NoConcerns = true,
                Concerns = false,
                Diagnosis = false,
                HearingDate = DateTime.Now,
                HearingResult = -1,
                VisionDate = DateTime.Now,
                VisionResult = -1,
                VisionImpaired = false,
                HearingImpaired = false,
                HealthCarePlan = false,
                Completed = false,
                Notes = string.Empty,
                Update_Date = DateTime.Now,
                Create_Date = DateTime.Now
            };

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

            // Adding Other Considerations Table
            studentOtherConsiderations = new tblOtherConsideration
            {
                IEPid = current.IEPid,
                Completed = false,
                Create_Date = DateTime.Now,
                Parental_CopyIEP_flag = true,
                Parental_RightsBook_flag = true
            };
            studentOtherConsiderations.Create_Date = DateTime.Now;
            studentOtherConsiderations.Update_Date = DateTime.Now;

            try
            {
                db.tblOtherConsiderations.Add(studentOtherConsiderations);
                db.SaveChanges();

                OtherID = studentOtherConsiderations.OtherConsiderationID;
            }
            catch (Exception e)
            {
                current.IepStatus = IEPStatus.DELETED;
                throw new System.ArgumentException("Failed to create the Other Considerations " + e.InnerException.Message);
            }


            // Adding Motor Table
            studentMotor = new tblIEPMotor
            {
                IEPid = current.IEPid,
                NoConcerns = true,
                ProgressTowardGenEd = false,
                Needs = false,
                Participation = -1,
                Completed = false,
                Create_Date = DateTime.Now,
                Update_Date = DateTime.Now
            };

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
            studentCommunication = new tblIEPCommunication
            {
                IEPid = current.IEPid,
                NoConcerns = true,
                ProgressTowardGenEd = false,
                SpeechImpactPerformance = false,
                Deaf = false,
                LimitedEnglish = false,
                Completed = false,
                Create_Date = DateTime.Now,
                Update_Date = DateTime.Now
            };

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
            studentSocial = new tblIEPSocial
            {
                IEPid = current.IEPid,
                NoConcerns = true,
                ProgressTowardGenEd = false,
                AreaOfNeed = false,
                MentalHealthDiagnosis = false,
                SignificantBehaviors = false,
                BehaviorImepedeLearning = false,
                BehaviorInterventionPlan = false,
                Completed = false,
                Create_Date = DateTime.Now,
                Update_Date = DateTime.Now
            };

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
            studentIntelligence = new tblIEPIntelligence
            {
                IEPid = current.IEPid,
                Concerns = false,
                Completed = false,
                Create_Date = DateTime.Now,
                Update_Date = DateTime.Now
            };

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
            studentAcademic = new tblIEPAcademic
            {
                IEPid = current.IEPid,
                NoConcerns = true,
                AreaOfNeed = false,
                Completed = false,
                Create_Date = DateTime.Now,
                Update_Date = DateTime.Now
            };

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
            studentReading = new tblIEPReading
            {
                IEPid = current.IEPid,
                NoConcerns = true,
                ProgressTowardGenEd = false,
                InstructionalTier1 = false,
                InstructionalTier2 = false,
                InstructionalTier3 = false,
                AreaOfNeed = false,
                Create_Date = DateTime.Now,
                Update_Date = DateTime.Now
            };

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
            studentMath = new tblIEPMath
            {
                IEPid = current.IEPid,
                NoConcerns = true,
                ProgressTowardGenEd = false,
                InstructionalTier1 = false,
                InstructionalTier2 = false,
                InstructionalTier3 = false,
                AreaOfNeed = false,
                Create_Date = DateTime.Now,
                Update_Date = DateTime.Now
            };

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
            studentWritten = new tblIEPWritten
            {
                IEPid = current.IEPid,
                NoConcerns = true,
                ProgressTowardGenEd = false,
                InstructionalTier1 = false,
                InstructionalTier2 = false,
                InstructionalTier3 = false,
                AreaOfNeed = false,
                Create_Date = DateTime.Now,
                Update_Date = DateTime.Now
            };

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
            current.Create_Date = DateTime.Now;
            current.Update_Date = DateTime.Now;

            db.SaveChanges();

            hasPlan = false;
            current.IepStatus = IEPStatus.PLAN;


            return this;
        }


        public int GetCalculatedAge(DateTime dateOfBirth, bool isDoc)
        {
            if (current.begin_date != null && !isDoc)
            {
                //check student age for transition plan using the begin date plus one year
                int now = int.Parse(iepStartTime.Value.AddYears(1).ToString("yyyyMMdd"));
                int dob = int.Parse(dateOfBirth.ToString("yyyyMMdd"));
                return (now - dob) / 10000;

                //var endDate = theIEP.current.begin_date.Value.AddYears(1);
                //model.studentAge = (endDate.Year - info.DateOfBirth.Year - 1) + (((endDate.Month > info.DateOfBirth.Month) || ((endDate.Month == info.DateOfBirth.Month) && (endDate.Day >= info.DateOfBirth.Day))) ? 1 : 0);
            }
            else
            {
                //use current date
                int now = int.Parse(iepStartTime.Value.AddYears(1).ToString("yyyyMMdd"));
                int dob = int.Parse(dateOfBirth.ToString("yyyyMMdd"));
                return (now - dob) / 10000;

                //model.studentAge = (DateTime.Now.Year - info.DateOfBirth.Year - 1) + (((DateTime.Now.Month > info.DateOfBirth.Month) || ((DateTime.Now.Month == info.DateOfBirth.Month) && (DateTime.Now.Day >= info.DateOfBirth.Day))) ? 1 : 0);
            }
        }

		private void CheckCompleted()
		{
			
				int accommodationsCount = 0;

				if (studentHealth != null && (studentHealth.NeedMetByAccommodation.HasValue && studentHealth.NeedMetByAccommodation.Value))
				{
					accommodationsCount++;
				}
				if (studentMotor != null && (studentMotor.NeedMetByAccommodation.HasValue && studentMotor.NeedMetByAccommodation.Value))
				{
					accommodationsCount++;
				}
				if (studentCommunication != null && (studentCommunication.NeedMetByAccommodation.HasValue && studentCommunication.NeedMetByAccommodation.Value))
				{
					accommodationsCount++;
				}
				if (studentSocial != null && (studentSocial.NeedMetByAccommodation.HasValue && studentSocial.NeedMetByAccommodation.Value))
				{
					accommodationsCount++;
				}
				if (studentAcademic != null && (studentAcademic.NeedMetByAccommodation.HasValue && studentAcademic.NeedMetByAccommodation.Value))
				{
					accommodationsCount++;
				}
				if (studentWritten != null && (studentWritten.NeedMetByAccommodation.HasValue && studentWritten.NeedMetByAccommodation.Value))
				{
					accommodationsCount++;
				}
				if (studentReading != null && (studentReading.NeedMetByAccommodation.HasValue && studentReading.NeedMetByAccommodation.Value))
				{
					accommodationsCount++;
				}
				if (studentMath != null && (studentMath.NeedMetByAccommodation.HasValue && studentMath.NeedMetByAccommodation.Value))
				{
					accommodationsCount++;
				}

				if (accommodations.Count > 0)
				{
					//how many modules have accommodations that are complete
					int completedCount = accommodations.Count(g => g.Completed);
					if (completedCount >= accommodationsCount)
						isAccommodationsCompleted = true;
				}

			int goalsCount = 0;

			if (studentHealth != null && (studentHealth.NeedMetByGoal.HasValue && studentHealth.NeedMetByGoal.Value))
			{
				goalsCount++;
			}
			if (studentMotor != null && (studentMotor.NeedMetByGoal.HasValue && studentMotor.NeedMetByGoal.Value))
			{
				goalsCount++;
			}
			if (studentCommunication != null && (studentCommunication.NeedMetByGoal.HasValue && studentCommunication.NeedMetByGoal.Value))
			{
				goalsCount++;
			}
			if (studentSocial != null && (studentSocial.NeedMetByGoal.HasValue && studentSocial.NeedMetByGoal.Value))
			{
				goalsCount++;
			}
			if (studentAcademic != null && (studentAcademic.NeedMetByGoal.HasValue && studentAcademic.NeedMetByGoal.Value))
			{
				goalsCount++;
			}
			if (studentWritten != null && (studentWritten.NeedMetByGoal.HasValue && studentWritten.NeedMetByGoal.Value))
			{
				goalsCount++;
			}
			if (studentReading != null && (studentReading.NeedMetByGoal.HasValue && studentReading.NeedMetByGoal.Value))
			{
				goalsCount++;
			}
			if (studentMath != null && (studentMath.NeedMetByGoal.HasValue && studentMath.NeedMetByGoal.Value))
			{
				goalsCount++;
			}

			if (studentGoals.Count > 0)
			{
				//how many modules have goals that are complete
				int completedGoalCount = studentGoals.Count(g => g.Completed);
				if (completedGoalCount >= goalsCount)
					isGoalCompleted = true;
			}

		}
	}
}