﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class IndividualizedEducationProgramEntities : DbContext
    {
        public IndividualizedEducationProgramEntities()
            : base("name=IndividualizedEducationProgramEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<tblAccommodation> tblAccommodations { get; set; }
        public virtual DbSet<tblBuildingMapping> tblBuildingMappings { get; set; }
        public virtual DbSet<tblBuilding> tblBuildings { get; set; }
        public virtual DbSet<tblCalendar> tblCalendars { get; set; }
        public virtual DbSet<tblCalendarReporting> tblCalendarReportings { get; set; }
        public virtual DbSet<tblCalendarTemplate> tblCalendarTemplates { get; set; }
        public virtual DbSet<tblDistrict> tblDistricts { get; set; }
        public virtual DbSet<tblGoalBenchmark> tblGoalBenchmarks { get; set; }
        public virtual DbSet<tblGoal> tblGoals { get; set; }
        public virtual DbSet<tblIEPAcademic> tblIEPAcademics { get; set; }
        public virtual DbSet<tblIEPCommunication> tblIEPCommunications { get; set; }
        public virtual DbSet<tblIEPHealth> tblIEPHealths { get; set; }
        public virtual DbSet<tblIEPIntelligence> tblIEPIntelligences { get; set; }
        public virtual DbSet<tblIEPMath> tblIEPMaths { get; set; }
        public virtual DbSet<tblIEPMotor> tblIEPMotors { get; set; }
        public virtual DbSet<tblIEPReading> tblIEPReadings { get; set; }
        public virtual DbSet<tblIEP> tblIEPs { get; set; }
        public virtual DbSet<tblIEPSocial> tblIEPSocials { get; set; }
        public virtual DbSet<tblIEPWritten> tblIEPWrittens { get; set; }
        public virtual DbSet<tblLanguage> tblLanguages { get; set; }
        public virtual DbSet<tblLocation> tblLocations { get; set; }
        public virtual DbSet<tblOrganizationMapping> tblOrganizationMappings { get; set; }
        public virtual DbSet<tblProvider> tblProviders { get; set; }
        public virtual DbSet<tblRole> tblRoles { get; set; }
        public virtual DbSet<tblService> tblServices { get; set; }
        public virtual DbSet<tblServiceType> tblServiceTypes { get; set; }
        public virtual DbSet<tblStudentInfo> tblStudentInfoes { get; set; }
        public virtual DbSet<tblStudentRelationship> tblStudentRelationships { get; set; }
        public virtual DbSet<tblUser> tblUsers { get; set; }
    }
}
