//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GreenBushIEP.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblFormSummaryPerformance
    {
        public int FormSummaryPerformanceId { get; set; }
        public int StudentId { get; set; }
        public string Goal_Learning { get; set; }
        public string Goal_LearningRecommendation { get; set; }
        public string Goal_Working { get; set; }
        public string Goal_WorkingRecommendation { get; set; }
        public string Goal_Living { get; set; }
        public string Goal_LivingRecommendation { get; set; }
        public string AC_ReadingPerformance { get; set; }
        public string AC_ReadingAccommodations { get; set; }
        public string AC_MathPerformance { get; set; }
        public string AC_MathAccommodations { get; set; }
        public string AC_LanguagePerformance { get; set; }
        public string AC_LanguageAccommodations { get; set; }
        public string AC_LearningPerformance { get; set; }
        public string AC_LearningAccommodations { get; set; }
        public string AC_OtherPerformance { get; set; }
        public string AC_OtherAccommodations { get; set; }
        public string Functional_SocialPerformance { get; set; }
        public string Functional_SocialAccommodations { get; set; }
        public string Functional_LivingPerformance { get; set; }
        public string Functional_LivingAccommodations { get; set; }
        public string Functional_MobiilityPerformance { get; set; }
        public string Functional_MobiilityAccommodations { get; set; }
        public string Functional_AdvocacyPerformance { get; set; }
        public string Functional_AdvocacyAccommodations { get; set; }
        public string Functional_EmploymentPerformance { get; set; }
        public string Functional_EmploymentAccommodations { get; set; }
        public string Functional_AdditionsPerformance { get; set; }
        public string Functional_AdditionsAccommodations { get; set; }
        public Nullable<System.DateTime> DateCompleted { get; set; }
        public string Documentation_PsychologicalAssementName { get; set; }
        public Nullable<System.DateTime> Documentation_PsychologicalDate { get; set; }
        public string Documentation_NeuropsychologicalAssementName { get; set; }
        public Nullable<System.DateTime> Documentation_NeuropsychologicalDate { get; set; }
        public string Documentation_MedicalAssementName { get; set; }
        public Nullable<System.DateTime> Documentation_MedicalDate { get; set; }
        public string Documentation_CommunicationAssementName { get; set; }
        public Nullable<System.DateTime> Documentation_CommunicationDate { get; set; }
        public string Documentation_AdaptiveBehaviorAssementName { get; set; }
        public Nullable<System.DateTime> Documentation_AdaptiveBehaviorDate { get; set; }
        public string Documentation_InterpersonalAssementName { get; set; }
        public Nullable<System.DateTime> Documentation_InterpersonalDate { get; set; }
        public string Documentation_SpeechAssementName { get; set; }
        public Nullable<System.DateTime> Documentation_SpeechDate { get; set; }
        public string Documentation_MTSSAssementName { get; set; }
        public Nullable<System.DateTime> Documentation_MTSSDate { get; set; }
        public string Documentation_CareerAssementName { get; set; }
        public Nullable<System.DateTime> Documentation_CareerDate { get; set; }
        public string Documentation_CommunityAssementName { get; set; }
        public Nullable<System.DateTime> Documentation_CommunityDate { get; set; }
        public string Documentation_SelfDeterminationAssementName { get; set; }
        public Nullable<System.DateTime> Documentation_SelfDeterminationDate { get; set; }
        public string Documentation_AssistiveTechAssementName { get; set; }
        public Nullable<System.DateTime> Documentation_AssistiveTechDate { get; set; }
        public string Documentation_ClassroomAssementName { get; set; }
        public Nullable<System.DateTime> Documentation_ClassroomDate { get; set; }
        public string Documentation_OtherAssementName { get; set; }
        public Nullable<System.DateTime> Documentation_OtherDate { get; set; }
        public string AdditionalInformation { get; set; }
        public int ModifiedBy { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime Create_Date { get; set; }
        public System.DateTime Update_Date { get; set; }
        public string student_Name { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public string student_phone { get; set; }
        public Nullable<int> GraduationExitYear { get; set; }
        public string CurrentSchool { get; set; }
        public string CurrentCity { get; set; }
        public string PrimaryLanguage { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string ContactSchool { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string Team_StudentName { get; set; }
        public string Team_ParentName { get; set; }
        public string Team_TeacherName1 { get; set; }
        public string Team_TeacherName2 { get; set; }
        public string Team_OtherProvider1 { get; set; }
        public string Team_OtherProvider2 { get; set; }
    }
}
