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
    
    public partial class tblOtherConsideration
    {
        public int OtherConsiderationID { get; set; }
        public int IEPid { get; set; }
        public Nullable<bool> AssistiveTechnology_Require { get; set; }
        public string AssistiveTechnology_Description { get; set; }
        public Nullable<bool> DistrictAssessment_NoAccommodations_flag { get; set; }
        public string DistrictAssessment_NoAccommodations_desc { get; set; }
        public Nullable<bool> DistrictAssessment_WithAccommodations_flag { get; set; }
        public string DistrictAssessment_WithAccommodations_desc { get; set; }
        public Nullable<bool> DistrictAssessment_Alternative_flag { get; set; }
        public string DistrictAssessment_Alternative_desc { get; set; }
        public Nullable<bool> DistrictAssessment_GradeNotAssessed { get; set; }
        public Nullable<bool> StateAssessment_NoAccommodations_flag { get; set; }
        public string StateAssessment_NoAccommodations_desc { get; set; }
        public Nullable<bool> StateAssessment_WithAccommodations_flag { get; set; }
        public string StateAssessment_WithAccommodations_desc { get; set; }
        public Nullable<bool> StateAssessment_RequiredCompleted { get; set; }
        public Nullable<bool> Transporation_NotEligible { get; set; }
        public Nullable<bool> Transporation_Required { get; set; }
        public Nullable<bool> Transporation_AttendOtherBuilding { get; set; }
        public Nullable<bool> Transporation_Other_flag { get; set; }
        public string Transporation_Other_desc { get; set; }
        public string RegularEducation_NotParticipate { get; set; }
        public string ExtendedSchoolYear_Necessary { get; set; }
        public Nullable<bool> ExtendedSchoolYear_RegressionRisk { get; set; }
        public Nullable<bool> ExtendedSchoolYear_SeverityRisk { get; set; }
        public string ExtendedSchoolYear_Justification { get; set; }
        public Nullable<bool> Parental_Concerns_flag { get; set; }
        public string Parental_Concerns_Desc { get; set; }
        public System.DateTime Create_Date { get; set; }
        public Nullable<bool> Transporation_Disability_flag { get; set; }
        public string Transporation_Disability_desc { get; set; }
        public bool Completed { get; set; }
        public Nullable<bool> StateAssesment_Alternative_flag { get; set; }
        public string StateAssesment_Alternative_Desc { get; set; }
        public bool Parental_CopyIEP_flag { get; set; }
        public bool Parental_RightsBook_flag { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<bool> Potential_HarmfulEffects_flag { get; set; }
        public string Potential_HarmfulEffects_desc { get; set; }
        public string Module { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public System.DateTime Update_Date { get; set; }
        public string EducationalPlacement { get; set; }
    }
}
