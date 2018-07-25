﻿using GreenBushIEP.Helper;
using GreenBushIEP.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GreenBushIEP.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();

        // GET: Manage
        public ActionResult Index()
        {
            return View();
        }

        // GET: Manage/Details/5
        [HttpPost]
        public ActionResult Details(int id)
        {
            tblUser user = db.tblUsers.SingleOrDefault(u => u.UserID == id);
            var districts = db.tblDistricts.ToList();
            var buildings = from b in db.tblBuildings
                            join bm in db.tblBuildingMappings on b.BuildingID equals bm.BuildingID
                            where bm.UserID == id & b.Active == 1
                            select new { BuildingName = b.BuildingName, BuildingID = b.BuildingID, USD = b.USD };

            if (user != null)
            {
                return Json(new { Result = "success", User = user, Districts = districts, Buildings = buildings });
            }

            return Json(new { Result = "error", User = user });
        }

        // GET: Manage/Create
        public ActionResult Create()
        {
            UserDetailsViewModel model = new UserDetailsViewModel();

            model.submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            model.districts = model.submitter.RoleID == "1" ? db.tblDistricts.Where(d => d.Active == 1).ToList() : (from d in db.tblDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where model.submitter.UserID == bm.UserID select d).Distinct().ToList();

            ViewBag.RoleName = ConvertToRoleName(model.submitter.RoleID);
            return View("~/Views/Home/CreateUser.cshtml", model);
        }

        // POST: Manage/Create
        [HttpPost]
        public ActionResult Create(HttpPostedFileBase adminpersona, FormCollection collection)
        {
            try
            {
                tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

                // CREATE new user
                tblUser user = new tblUser
                {
                    TeacherID = (!string.IsNullOrEmpty(collection["teacherid"]) ? collection["teacherid"] : null),
                    RoleID = collection["role"],
                    FirstName = collection["firstname"],
                    LastName = collection["lastname"],
                    Email = collection["email"],
                    Create_Date = DateTime.Now,
                    Update_Date = DateTime.Now,
                    Password = RandomPassword.Generate(8),
                };

                // UPLOAD the image
                if (adminpersona != null && adminpersona.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(adminpersona.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Images/Uploads/"), fileName);
                    user.ImageURL = fileName;
                    adminpersona.SaveAs(path);
                }

                if (db.tblUsers.Any(o => o.Email == user.Email))
                {
                    return Json(new { Result = "error", Message = "The email address is already in use, please use a different email address." });
                }
                
                
                // Add to Database
                db.tblUsers.Add(user);
                db.SaveChanges();

                // save the user to all the districts that was selected.
                foreach (string usd in collection["misDistrict"].ToString().Split(','))
                {
                    tblOrganizationMapping org = new tblOrganizationMapping();
                    org.AdminID = submitter.UserID;
                    org.UserID = user.UserID;
                    org.USD = usd;

                    db.tblOrganizationMappings.Add(org);
                    db.SaveChanges();

                    tblBuildingMapping district = new tblBuildingMapping();
                    district.BuildingID = "0";
                    district.USD = usd;
                    district.UserID = user.UserID;

                    db.tblBuildingMappings.Add(district);
                    db.SaveChanges();
                }

                // Email the new password to the user.
                EmailPassword.Send(user, user.Password);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return Json(new { Result = "error", Message = e.Message + " Contact an adminstrator for additional help"});
            }

            return RedirectToAction("Portal", "Home");
        }

        // GET: Manage/CreateStudent
        [HttpGet]
        public ActionResult CreateStudent()
        {
            StudentDetailsViewModel model = new StudentDetailsViewModel();

            model.submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            model.districts = model.submitter.RoleID == "1" ? db.tblDistricts.Where(d => d.Active == 1).ToList() : (from d in db.tblDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where model.submitter.UserID == bm.UserID select d).Distinct().ToList();
            model.allDistricts = db.tblDistricts.ToList();
            model.student.DateOfBirth = DateTime.Now.AddYears(-5);
            model.placementCode = db.tblPlacementCodes.ToList();
            model.primaryDisabilities = db.vw_PrimaryDisabilities.ToList();
            model.secondaryDisabilities = db.vw_SecondaryDisabilities.ToList();
            model.contacts.Add(new tblStudentRelationship() { Realtionship = "parent", State = "KS" });
            model.statusCode = db.tblStatusCodes.ToList();

            ViewBag.RoleName = ConvertToRoleName(model.submitter.RoleID);

            return View("~/Views/Home/CreateStudent.cshtml", model);
        }

        // POST: Manage/CreateStudent
        [HttpPost]
        public JsonResult CreateStudent(HttpPostedFileBase adminpersona, FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

                    // Create New User 
                    tblUser student = new tblUser()
                    {
                        RoleID = "5",
                        FirstName = collection["firstname"],
                        LastName = collection["lastname"],
                        Email = ((!string.IsNullOrEmpty(collection["email"])) ? collection["email"].ToString() : null ),
                        Create_Date = DateTime.Now,
                        Update_Date = DateTime.Now,
                        TeacherID = submitter.UserID.ToString(),
                    };

                    // try catch. If the email is the same as another student show error gracefully.
                    try
                    {

                        if (!String.IsNullOrEmpty(student.Email) && db.tblUsers.Any(o => o.Email == student.Email))
                        {
                            return Json(new { Result = "error", Message = "The email address is already in use, please use a different email address." });
                        }
                        else
                        {
                            db.tblUsers.Add(student);
                            db.SaveChanges();
                        }
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + e.InnerException.ToString() });
                    }


                    // Create New StudentInfo
                    // tblStudentInfo
                    tblStudentInfo studentInfo = new tblStudentInfo()
                    {
                        UserID = student.UserID,
                        KIDSID = Convert.ToInt64(collection["kidsid"]),
                        DateOfBirth = Convert.ToDateTime(collection["dob"]),
                        Primary_DisabilityCode = collection["primaryDisability"].ToString(),
                        Secondary_DisabilityCode = collection["secondaryDisability"].ToString(),
                        AssignedUSD = collection["assignChildCount"].ToString(),
                        USD = collection["misDistrict"],
                        BuildingID = collection["AttendanceBuildingId"],
                        NeighborhoodBuildingID = collection["NeighborhoodBuildingID"],
                        Status = "PENDING",
                        Gender = (String.IsNullOrEmpty(collection["gender"])) ? "M" : "F",
                        Create_Date = DateTime.Now,
                        Update_Date = DateTime.Now,
                    };

                    try
                    {
                        db.tblStudentInfoes.Add(studentInfo);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + e.InnerException.ToString() });
                    }

                    // save to organization chart
                    // save the user to all the districts that was selected.
                    // tblOrganizationMapping and tblBuildingMapping
                    var districtValues = collection["misDistrict"];

                    if (!string.IsNullOrEmpty(districtValues))
                    {
                        string[] districtArray = districtValues.Split(','); ;

                        foreach (string usd in districtArray)
                        {
                            tblOrganizationMapping org = new tblOrganizationMapping();
                            org.AdminID = submitter.UserID;
                            org.UserID = student.UserID;
                            org.USD = usd;

                            db.tblOrganizationMappings.Add(org);
                            db.SaveChanges();

                            tblBuildingMapping district = new tblBuildingMapping();
                            district.BuildingID = "0";
                            district.USD = usd;
                            district.UserID = student.UserID;

                            try
                            {
                                db.tblBuildingMappings.Add(district);
                                db.SaveChanges();
                            }
                            catch (Exception e)
                            {
                                return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + e.InnerException.ToString() });
                            }
                        }
                    }

                    return Json(new { Result = "success", Message = student.UserID });
                }
                catch (DbEntityValidationException ex)
                {
                    // Retrieve the error messages as a list of strings.
                    var errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);

                    // Join the list to a single string.
                    var fullErrorMessage = string.Join("; ", errorMessages);

                    // Combine the original exception message with the new one.
                    var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                    Console.Write(exceptionMessage);
                }

            }

            return Json(new { Result = "error", Message = "There was an error while trying to create the user. Please try again or contact your administrator." });
        }

        public JsonResult CreateStudentOptions(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int studentId = Convert.ToInt32(collection["studentId"]);

                    tblUser user = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
                    if(user != null)
                    {
                        user.Address1 = collection["studentStreetAddress1"].ToString();
                        user.Address2 = collection["studentStreetAddress2"].ToString();
                        user.City = collection["studentCity"].ToString();
                        user.State = collection["studentState"].ToString();
                        user.Zip = collection["studentZipCode"].ToString();
                    }
                    db.SaveChanges();

                    tblStudentInfo info = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();
                    if(info != null)
                    {
                        info.County = collection["studentCounty"].ToString();
                        info.Grade = Convert.ToInt32(collection["studentGrade"]);
                        info.Race = collection["studentRace"].ToString();
                        info.Ethicity = collection["studentEthnic"].ToString();
                        info.StudentLanguage = collection["studentLanguage"].ToString();
                        info.ParentLanguage = collection["parentLanguage"].ToString();
                        info.ClaimingCode = collection["claimingCode"] == "on" ? true : false;
                        info.FullDayKG = collection["fullDayKindergarten"] == "on" ? true : false;
                        info.StatusCode = collection["statusCode"].ToString();

                        if (!String.IsNullOrEmpty(collection["initialIEPDate"]))
                        {
                            info.InitialIEPDate = Convert.ToDateTime(collection["initialIEPDate"]);
                        }

                        if(!String.IsNullOrEmpty(collection["exitDate"]))
                        {
                            info.ExitDate = Convert.ToDateTime(collection["exitDate"]);
                        }

                        if(!String.IsNullOrEmpty(collection["initialConsentSignature"]))
                        {
                            info.InitialEvalConsentSigned = Convert.ToDateTime(collection["initialConsentSignature"]);
                        }
                        
                        if(!String.IsNullOrEmpty(collection["initialEvaluationDetermination"]))
                        {
                            info.InitialEvalDetermination = Convert.ToDateTime(collection["initialEvaluationDetermination"]);
                        }

                        if(!String.IsNullOrEmpty(collection["reEvaluationSignature"]))
                        {
                            info.ReEvalConsentSigned = Convert.ToDateTime(collection["reEvaluationSignature"]);
                        }
                    }
                    db.SaveChanges();

                    return Json(new { Result = "success", Message = studentId });
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "There was an error while trying to add the student's contacts. \n\n" + e.InnerException.ToString() });
                }
            }

            return Json(new { Result = "error", Message = "There was an error while trying to update the user's options. Please try again or contact your administrator." });
        }

        [HttpPost]
        public JsonResult CreateStudentContacts(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int studentId = Convert.ToInt32(collection["studentId"]);

                    tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
                    tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();

                    int j = 0;
                    int loopCounter = 0;
                    while (j < collection.Count - 1)
                    {
                        tblStudentRelationship contact = new tblStudentRelationship()
                        {
                            RealtionshipID = 0,
                            UserID = studentId,
                            FirstName = collection[++j].ToString(),
                            MiddleName = collection[++j].ToString(),
                            LastName = collection[++j].ToString(),
                            Realtionship = collection[++j].ToString(),
                            Address1 = collection[++j].ToString(),
                            Address2 = collection[++j].ToString(),
                            City = collection[++j].ToString(),
                            State = collection[++j].ToString(),
                            Zip = collection[++j].ToString(),
                            Phone = collection[++j].ToString(),
                            Email = collection[++j].ToString(),
                        };

                        /////////////////////////////
                        // This whole if block is due to the fact that checkbox false values are NOT passed to our collection
                        // and the checkbox is the last value in the collection fields.
                        /////////////////////////////
                        if (++j <= collection.Count - 1) // test if this is the end of the collection i.e. out of range issues.
                        {
                            if (collection.GetKey(j) == string.Format("contacts[{0}].PrimaryContact", loopCounter))
                            {
                                contact.PrimaryContact = collection[j] == "on" ? 1 : 0;
                            }
                            else { j--; }
                        }

                        try
                        {
                            db.tblStudentRelationships.Add(contact);
                            db.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            return Json(new { Result = "error", Message = "There was an error while trying to add the student's contacts. \n\n" + e.InnerException.ToString() });
                        }

                        loopCounter++;
                    }

                    return Json(new { Result = "success", Message = student.UserID });
                }
                catch (DbEntityValidationException ex)
                {
                    // Retrieve the error messages as a list of strings.
                    var errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);

                    // Join the list to a single string.
                    var fullErrorMessage = string.Join("; ", errorMessages);

                    // Combine the original exception message with the new one.
                    var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                    Console.Write(exceptionMessage);
                }
            }

            return Json(new { Result = "error", Message = "There was an error while trying to create the student's contacts. Please try again or contact your administrator." });
        }

        [HttpPost]
        public ActionResult CreateStudentAvatar(HttpPostedFileBase adminpersona, FormCollection collection)
        {
            int studentId = Convert.ToInt32(collection["studentId"]);

            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
            if (student != null)
            {
                //// UPLOAD the image
                if (adminpersona != null && adminpersona.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(adminpersona.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Images/Uploads/"), fileName);
                    student.ImageURL = fileName;
                    adminpersona.SaveAs(path);

                    db.SaveChanges();
                }
            }

            return RedirectToAction("Portal", "Home");
        }

        // GET: Manage/EditStudent/5
        [HttpGet]
        public ActionResult EditStudent(int id)
        {
            StudentDetailsViewModel model = new StudentDetailsViewModel();

            model.student = new Student();
            tblUser student = db.tblUsers.Where(u => u.UserID == id).FirstOrDefault();
            if (student != null)
            {
                model.student.UserID = id;
                model.student.FirstName = student.FirstName;
                model.student.LastName = student.LastName;
                model.student.City = student.City;
                model.student.State = student.State;
                model.student.Email = student.Email;
                model.student.RoleID = "5";
                model.student.ImageURL = student.ImageURL;
            }

            tblStudentInfo studentinfo = db.tblStudentInfoes.Where(i => i.UserID == id).FirstOrDefault();
            if (studentinfo != null)
            {
                model.student.KidsID = studentinfo.KIDSID;
                model.student.DateOfBirth = studentinfo.DateOfBirth;
                model.student.USD = studentinfo.USD;
                model.student.BuildingID = studentinfo.BuildingID;
                model.student.NeighborhoodBuildingID = studentinfo.NeighborhoodBuildingID;
                model.info = studentinfo;
            }

            List<tblStudentRelationship> relationships = db.tblStudentRelationships.Where(r => r.UserID == student.UserID).ToList();
            if (relationships != null)
            {
                foreach (tblStudentRelationship relationship in relationships)
                {
                    model.contacts.Add(new tblStudentRelationship()
                    {
                        FirstName = relationship.FirstName,
                        MiddleName = relationship.MiddleName,
                        LastName = relationship.LastName,
                        Address1 = relationship.Address1,
                        Address2 = relationship.Address2,
                        City = relationship.City,
                        State = relationship.State,
                        Zip = relationship.Zip,
                        Email = relationship.Email,
                        Phone = relationship.Phone,
                        Realtionship = relationship.Realtionship,
                        UserID = relationship.UserID,
                        RealtionshipID = relationship.RealtionshipID,
                        PrimaryContact = relationship.PrimaryContact
                    });
                }
            }

            model.submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            model.selectedDistrict = (from d in db.tblDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where model.student.UserID == bm.UserID select d).Distinct().ToList();
            model.districts = (model.submitter.RoleID == "1") ? db.tblDistricts.Where(d => d.Active == 1).ToList() : (from d in db.tblDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where model.submitter.UserID == bm.UserID select d).Distinct().ToList();

            foreach (var d in model.selectedDistrict)
            {
                var districtBuildings = (from b in db.tblBuildings
                                         where b.USD == d.USD && b.Active == 1
                                         select new BuildingsViewModel { BuildingName = b.BuildingName, BuildingID = b.BuildingID, BuildingUSD = b.USD }).Distinct().ToList();

                model.buildings.AddRange(districtBuildings);
            }

            ViewBag.RoleName = ConvertToRoleName(model.submitter.RoleID);

            return View("~/Views/Home/EditStudent.cshtml", model);
        }

        // POST: Manage/EditStudent
        [HttpPost]
        public JsonResult EditStudent(FormCollection collection)
        {
            int studentId = Convert.ToInt32(collection["id"]);

            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
            if (student != null)
            {
                student.FirstName = collection["firstname"];
                student.LastName = collection["lastname"];
                student.Email = collection["email"];
                student.RoleID = "5";
            }
            else
            {
                return Json(new { Result = "error", Message = "There was an error while trying to edit the user." });
            }

            tblStudentInfo info = db.tblStudentInfoes.Where(u => u.UserID == studentId).FirstOrDefault();
            if (info != null)
            {
                // remove all mapping. Blow it all away.
                db.tblBuildingMappings.RemoveRange(db.tblBuildingMappings.Where(x => x.UserID == studentId));
                db.SaveChanges();

                List<tblBuildingMapping> mappings = new List<tblBuildingMapping>();
                if (collection["misDistrict"] != null)
                {
                    foreach (string usd in collection["misDistrict"].ToString().Split(','))
                    {
                        mappings.Add(new tblBuildingMapping()
                        {
                            BuildingID = "0",
                            UserID = student.UserID,
                            USD = usd
                        });
                    }
                }
                // add back the connections to the database.
                db.tblBuildingMappings.AddRange(mappings);

                info.UserID = student.UserID;
                info.KIDSID = Convert.ToInt64(collection["kidsid"]);
                info.DateOfBirth = Convert.ToDateTime(collection["dob"]);
                info.BuildingID = collection["AttendanceBuildingId"];
                info.NeighborhoodBuildingID = collection["NeighborhoodBuildingID"];
                info.Ethicity = collection["ethnic"];
                info.StudentLanguage = collection["studentLanguage"];
                info.ParentLanguage = collection["parentLanguage"];
                info.Race = collection["race"];
                info.Status = "PENDING";
                info.Grade = Convert.ToInt32(collection["studentGrade"]);
                info.Gender = (String.IsNullOrEmpty(collection["gender"])) ? "M" : "F";
            }
            else
            {
                return Json(new { Result = "error", Message = "There was an error while trying to edit the user." });
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + e.InnerException.ToString() });
            }

            return Json(new { Result = "success", Message = student.UserID });
        }

        // POST: Manage/EditStudentContancts
        [HttpPost]
        public JsonResult EditStudentContacts(FormCollection collection)
        {
            int studentId = Convert.ToInt32(collection["id"]);
            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();

            if (student != null)
            {
                try
                {
                    // BLOW AWAY all the relationships to the students.
                    List<tblStudentRelationship> relationships = db.tblStudentRelationships.Where(r => r.UserID == student.UserID).ToList();
                    db.tblStudentRelationships.RemoveRange(relationships);
                    db.SaveChanges();

                    int j = 0;
                    while (++j < collection.Count - 1)
                    {
                        int startIndex = collection.GetKey(j).ToString().IndexOf("[") + 1;
                        int endIndex = collection.GetKey(j).ToString().IndexOf("]");
                        string relId = collection.GetKey(j).ToString().Substring(startIndex, endIndex - startIndex);

                        tblStudentRelationship contact = new tblStudentRelationship()
                        {
                            RealtionshipID = 0,
                            UserID = studentId,
                            FirstName = collection[j].ToString(),
                            MiddleName = collection[++j].ToString(),
                            LastName = collection[++j].ToString(),
                            Realtionship = collection[++j].ToString(),
                            Address1 = collection[++j].ToString(),
                            Address2 = collection[++j].ToString(),
                            City = collection[++j].ToString(),
                            State = collection[++j].ToString(),
                            Zip = collection[++j].ToString(),
                            Phone = collection[++j].ToString(),
                            Email = collection[++j].ToString(),
                        };

                        /////////////////////////////
                        // This whole if block is due to the fact that checkbox false values are NOT passed to our collection
                        // and the checkbox is the last value in the collection fields.
                        /////////////////////////////
                        if (++j <= collection.Count - 1) // test if this is the end of the collection i.e. out of range issues.
                        {
                            if (collection.GetKey(j) == string.Format("contact[{0}].PrimaryContact", relId))
                            {
                                contact.PrimaryContact = collection[j] == "on" ? 1 : 0;
                            }
                            else { j--; }
                        }

                        try
                        {
                            db.tblStudentRelationships.Add(contact);
                            db.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            return Json(new { Result = "error", Message = "There was an error while trying to edit the student's contacts. \n\n" + e.InnerException.ToString() });
                        }
                    }
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "There was an error while trying to edit the student's contacts. \n\n" + e.InnerException.ToString() });
                }

                return Json(new { Result = "success", Message = "yup" });
            }

            return Json(new { Result = "error", Message = "There was an error while trying to edit the student's contacts." });
        }

        [HttpPost]
        public ActionResult EditStudentAvatar(HttpPostedFileBase adminpersona, FormCollection collection)
        {
            int studentId = Convert.ToInt32(collection["id"]);

            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
            if (student != null)
            {
                // UPLOAD the image
                if (adminpersona != null && adminpersona.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(adminpersona.FileName);
                    var random = Guid.NewGuid() + fileName;
                    var path = Path.Combine(Server.MapPath("~/Content/Images/Uploads/"), random);
                    if (!Directory.Exists(Server.MapPath("~/Content/Files/")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Content/Files/"));
                    }
                    student.ImageURL = random;
                    adminpersona.SaveAs(path);
                }

                db.SaveChanges();
            }

            return RedirectToAction("Portal", "Home");
        }

        // GET: Manage/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            UserDetailsViewModel model = new UserDetailsViewModel();

            model.user = db.tblUsers.Where(u => u.UserID == id).SingleOrDefault();
            model.submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            model.selectedDistrict = (from d in db.tblDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where model.user.UserID == bm.UserID && bm.BuildingID == "0" select d).Distinct().ToList();
            model.districts = (model.submitter.RoleID == "1") ? db.tblDistricts.Where(d => d.Active == 1).ToList() : (from d in db.tblDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where model.submitter.UserID == bm.UserID select d).Distinct().ToList();
            model.buildings = (from bm in db.tblBuildingMappings
                               join b in db.tblBuildings on bm.USD equals b.USD
                               where bm.UserID == model.user.UserID && b.Active == 1 && bm.BuildingID == b.BuildingID
                               select new BuildingsViewModel { BuildingName = b.BuildingName, BuildingID = b.BuildingID, BuildingUSD = b.USD }).Distinct().ToList();

            ViewBag.RoleName = ConvertToRoleName(model.submitter.RoleID);
            return View("~/Views/Home/EditUser.cshtml", model);
        }

        // POST: Manage/Edit
        [HttpPost]
        public ActionResult Edit(int id, HttpPostedFileBase adminpersona, FormCollection collection)
        {
            try
            {
                tblUser user = db.tblUsers.SingleOrDefault(u => u.UserID == id);
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(collection["teacherid"])) { user.TeacherID = collection["teacherId"]; }
                    if (!string.IsNullOrEmpty(collection["role"])) { user.RoleID = collection["role"]; }
                    user.FirstName = collection["FirstName"];
                    user.LastName = collection["LastName"];
                    user.Email = collection["Email"];

                    List<tblBuildingMapping> mappings = new List<tblBuildingMapping>();

                    if (collection["buildingIds"] != null)
                    {
                        foreach (string building in collection["buildingIds"].ToString().Split(','))
                        {
                            mappings.Add(new tblBuildingMapping()
                            {
                                BuildingID = building,
                                UserID = id,
                                USD = db.tblBuildings.Where(b => b.BuildingID == building).Single().USD
                            });
                        }
                    }

                    if (collection["misDistrict"] != null)
                    {
                        foreach (string usd in collection["misDistrict"].ToString().Split(','))
                        {
                            if (mappings.Where(m => m.USD == usd).Count() == 0)
                            {
                                mappings.Add(new tblBuildingMapping()
                                {
                                    BuildingID = "0",
                                    UserID = id,
                                    USD = usd
                                });
                            }
                        }
                    }

                    // remove all relationships. Blow it all away.
                    db.tblBuildingMappings.RemoveRange(db.tblBuildingMappings.Where(x => x.UserID == id));
                    db.SaveChanges();

                    // add back the connections to the database.
                    db.tblBuildingMappings.AddRange(mappings);
                    db.SaveChanges();

                    // if they are updating their avatar
                    if (adminpersona != null)
                    {
                        if (!string.IsNullOrEmpty(user.ImageURL))
                        {
                            // Delete exiting file
                            System.IO.File.Delete(Path.Combine(Server.MapPath("~/Content/Images/Uploads/"), user.ImageURL));
                        }

                        // Save new file
                        string filename = Guid.NewGuid() + Path.GetFileName(adminpersona.FileName);
                        string path = Path.Combine(Server.MapPath("~/Content/Images/Uploads/"), filename);
                        adminpersona.SaveAs(path);
                        user.ImageURL = filename;
                    }

                    db.SaveChanges();
                }

                return RedirectToAction("Portal", "Home");
            }
            catch (Exception e)
            {
                return RedirectToAction("Edit", "Manage", new { id = id, message = e.Message.ToString() });
            }
        }

        // GET: Manage/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Manage/Delete
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // RULES:
                // 1) Cannot delete yourself.
                // 2) Cannot delete if have staff
                // 3) Cannot delete a teacher if they have IEP data opened.
                // 4) Cannot delete a student based on these rules have data associated with them. Otherwise - archive

                tblUser user = db.tblUsers.SingleOrDefault(u => u.UserID == id);
                if (user != null)
                {
                    // Cannot delete yourself foo.
                    if (User.Identity.Name == user.Email)
                    {
                        return Json(new { Result = "warning", Message = "<strong>Warning!</ strong > You cannot delete yourself from the system." });
                    }

                    // Cannot delete if the user has staff
                    if (db.tblOrganizationMappings.Any(u => u.AdminID == id))
                    {
                        return Json(new { Result = "warning", Message = "<strong>Warning!</strong> You must first remove all of the users under " + user.FirstName.ToString() + " " + user.LastName.ToString() + " before you can delete them." });
                    }

                    // TODO: Cannot delete the teacher if they have opened/associated IEPs

                    // Cannot delete a student if they have any IEP associated to them
                    if (db.tblIEPs.Any(u => u.UserID == id))
                    {
                        user.Archive = true; // you can archieve a student, not delete them.
                        db.SaveChanges();

                        return Json(new { Result = "warning", Message = "<strong>Warning!</strong> The student was archived." });
                    }

                    //delete studentInfo table if the userid is there.
                    var info = db.tblStudentInfoes.FirstOrDefault(u => u.UserID == id);
                    if (info != null)
                    {
                        db.tblStudentInfoes.Remove(info);
                        db.SaveChanges();
                    }

                    //delete from tlbOrganizationMapping all userId references.
                    var mappings = db.tblOrganizationMappings.Where(u => u.UserID == user.UserID).ToList();
                    if (mappings.Count > 0)
                    {
                        db.tblOrganizationMappings.RemoveRange(mappings);
                        db.SaveChanges();
                    }

                    //delete from tblBuildingMapping all userId references.
                    var buildings = db.tblBuildingMappings.Where(u => u.UserID == user.UserID).ToList();
                    if (buildings.Count > 0)
                    {
                        db.tblBuildingMappings.RemoveRange(buildings);
                        db.SaveChanges();
                    }

                    //delete from tblStudentRelationships all userId references.
                    var relatioships = db.tblStudentRelationships.Where(r => r.UserID == user.UserID).ToList();
                    if (relatioships.Count > 0)
                    {
                        db.tblStudentRelationships.RemoveRange(relatioships);
                        db.SaveChanges();
                    }

                    db.tblUsers.Remove(user);
                    db.SaveChanges();
                    return Json(new { Result = "success", Message = "<strong>Success!</strong> The user was successfully deleted from the system." });
                }

                return Json(new { Result = "error", Message = "<strong>Error!</strong> An error happened while trying to find the user. Contact your admin." });
            }
            catch
            {
                return Json(new { Result = "error", Message = "<strong>Error!</strong> An error happened while trying to delete the user. Contact your admin." });
            }
        }

        // POST: Manage/RemoveFromList/5
        [HttpPost]
        public ActionResult RemoveFromList(int id)
        {
            tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
            tblOrganizationMapping boss = db.tblOrganizationMappings.Where(u => u.UserID == submitter.UserID).FirstOrDefault();
            tblOrganizationMapping userToRemove = db.tblOrganizationMappings.Where(u => u.AdminID == submitter.UserID && u.UserID == id).SingleOrDefault();

            if (userToRemove != null && boss != null)
            {
                db.tblOrganizationMappings.Add(
                    new tblOrganizationMapping
                    {
                        AdminID = boss.AdminID,
                        UserID = id,
                        USD = userToRemove.USD
                    });
                db.SaveChanges();

                db.tblOrganizationMappings.Remove(userToRemove);
                db.SaveChanges();

                return Json(new { Result = "Success", Message = "User successfully removed from your list." });
            }

            return Json(new { Result = "Error", Message = "An error happened while removing the user from your list. Please contact your admin." });
        }

        // POST: Manage/RemoveFromTeacherList
        [HttpPost]
        public ActionResult RemoveFromTeacherList(int id, int teacherid)
        {
            tblUser teacher = db.tblUsers.FirstOrDefault(u => u.UserID == teacherid);
            tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

            tblOrganizationMapping userToRemove = db.tblOrganizationMappings.Where(u => u.AdminID == teacher.UserID && u.UserID == id).SingleOrDefault();
            if (userToRemove != null)
            {
                db.tblOrganizationMappings.Remove(userToRemove);
                db.SaveChanges();

                return Json(new { Result = "Success", Message = "User successfully removed from your list." });
            }

            return Json(new { Result = "Error", Message = "An error happened while removing the user from your list. Please contact your admin." });
        }

        [HttpGet]
        public ActionResult GetDistricts(int id)
        {
            try
            {
                tblUser user = db.tblUsers.FirstOrDefault(u => u.UserID == id);
                List<tblDistrict> districts = (from d in db.tblDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where user.UserID == bm.UserID select d).Distinct().ToList();
                if (districts != null)
                {
                    return Json(new { Result = "success", Message = districts }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to get districts. Contact Greenbush admin." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetAllStudentsInDistrict(int id)
        {
            tblUser submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            tblUser teacher = db.tblUsers.SingleOrDefault(u => u.UserID == id);

            try
            {
                var innerQuery = (from o in db.tblOrganizationMappings where o.AdminID == teacher.UserID select o.UserID).Distinct().ToList();
                var teacherDistricts = (from bm in db.tblBuildingMappings where bm.UserID == teacher.UserID select bm.USD).Distinct().ToList();

                List<tblUser> students = (from o in db.tblOrganizationMappings
                                          join u in db.tblUsers on o.UserID equals u.UserID
                                          where u.RoleID == "5" && !innerQuery.Contains(o.UserID) // the user is a student, the user is NOT already in the teachers list, and the user is in the Teachers's District
                                                                                                  //where u.USD == submitter.USD && u.RoleID == "5" && !innerQuery.Contains(o.UserID) //&& o.AdminID != teacher.UserID  
                                          select u).Distinct().ToList();

                foreach (tblUser student in students.ToList())
                {
                    var studentDistricts = (from bm in db.tblBuildingMappings where bm.UserID == student.UserID select bm.USD).Distinct().ToList();
                    if (teacherDistricts.Except(studentDistricts).Any())
                    {
                        students.Remove(student);
                    }
                }

                return Json(new { Result = "success", Message = students }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddStudentsToTeacher(int id, int[] students)
        {
            try
            {
                tblUser teacher = db.tblUsers.Where(u => u.UserID == id).SingleOrDefault();
                foreach (int student in students)
                {
                    tblUser studentUser = db.tblUsers.Where(u => u.UserID == student).SingleOrDefault();
                    tblOrganizationMapping newRelation = new tblOrganizationMapping()
                    {
                        AdminID = teacher.UserID,
                        UserID = student,
                        USD = (from bms in db.tblBuildingMappings join bmt in db.tblBuildingMappings on bms.USD equals bmt.USD select bms.USD).FirstOrDefault(),
                    };
                    db.tblOrganizationMappings.Add(newRelation);
                    db.SaveChanges();
                }

                return Json(new { Result = "Success", Message = "Wooty Woot" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetBuildings(string id, int userId)
        {
            try
            {
                // A list of all the building ids the current user has joined.
                var mappings = from m in db.tblBuildingMappings
                               where m.UserID == userId
                               select m.BuildingID;

                // Give me the list of all the buildings in the current district that are user is NOT already in.
                var listOfBuildings = from b in db.tblBuildings
                                      where !(mappings).Contains(b.BuildingID)
                                      && b.USD == id && b.Active == 1
                                      orderby b.BuildingName
                                      select b;

                if (listOfBuildings != null)
                {
                    return Json(new { Result = "success", Message = listOfBuildings }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to get buildings. Contact Greenbush admin." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetBuildingsByDistrictId(string ids)
        {
            try
            {
                List<tblBuilding> buildings = new List<tblBuilding>();

                if (!string.IsNullOrEmpty(ids))
                {


                    string[] usdIds = ids.Split(',');
                    foreach (var usdId in usdIds)
                    {
                        // Give me the list of all the buildings in the current district 
                        var listOfBuildings = from b in db.tblBuildings
                                              where b.USD == usdId && b.Active == 1
                                              orderby b.BuildingName
                                              select b;

                        buildings.AddRange(listOfBuildings.ToList());
                    }

                }


                if (buildings != null)
                {
                    return Json(new { Result = "success", Message = buildings }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to get buildings. Contact Greenbush admin." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SaveBuildingsToUser(string USD, int userId, string[] buildings)
        {
            try
            {
                foreach (string building in buildings)
                {
                    tblBuildingMapping mapping = new tblBuildingMapping() { USD = USD, UserID = userId, BuildingID = building };
                    db.tblBuildingMappings.Add(mapping);
                    db.SaveChanges();
                }

                return Json(new { Result = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            // Redirect on error:
            filterContext.Result = RedirectToAction("Index", "Home");

            // OR set the result without redirection:
            //filterContext.Result = new ViewResult
            //{
            //    ViewName = "~/Views/Error/Index.cshtml"
            //};
        }

        #region helpers

        [NonAction]
        public string ConvertToRoleName(string roleId)
        {
            switch (roleId)
            {
                case "1":
                    return "Owner";
                case "2":
                    return "Managed Information System";
                case "3":
                    return "Special Education Administrator";
                case "4":
                    return "Teacher";
                default:
                    return "Student";
            }
        }

        #endregion
    }
}