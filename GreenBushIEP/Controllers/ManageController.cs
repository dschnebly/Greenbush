using GreenBushIEP.Helper;
using GreenBushIEP.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace GreenBushIEP.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private IndividualizedEducationProgramEntities db = new IndividualizedEducationProgramEntities();
        private const string owner = "1"; //level 5
        private const string mis = "2"; //level 4
        private const string admin = "3"; //level 3
        private const string teacher = "4"; //level 2
        private const string student = "5";
        private const string nurse = "6"; //level 1

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
                var emailPassword = RandomPassword.Generate(10);
                PasswordHash hash = new PasswordHash(emailPassword);

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
                    Password = hash.Hash,
                    Salt = hash.Salt
                };

                // UPLOAD the image
                if (adminpersona != null && adminpersona.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(adminpersona.FileName);
                    var path = Path.Combine(Server.MapPath("~/Avatar/"), fileName);
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
                EmailPassword.Send(user, emailPassword);

                return Json(new { Result = "success", Message = "Successfully created a new user." });
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return Json(new { Result = "error", Message = e.Message + " Contact an adminstrator for additional help" });
            }
        }


        [HttpPost]
        public ActionResult FilterReferrals(int searchType)
        {
            var currentUser = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            var districts = (from org in db.tblOrganizationMappings
                             join user in db.tblUsers
                                 on org.UserID equals user.UserID
                             where (user.UserID == currentUser.UserID)
                             select org).Distinct();

            List<ReferralViewModel> referralList = new List<ReferralViewModel>();
            if (districts != null)
            {
                bool? completeType = null;

                if (searchType == 1)
                    completeType = false;
                else if (searchType == 2)
                    completeType = true;

                foreach (var district in districts)
                {

                    var referrals = (from refInfo in db.tblReferralInfoes
                                     join rr in db.tblReferralRequests
                                         on refInfo.ReferralID equals rr.ReferralID
                                     where
                                     (refInfo.AssignedUSD == district.USD)
                                     && ((completeType == null) || (rr.Complete == completeType.Value))
                                     select refInfo).Distinct();

                    foreach (var referral in referrals)
                    {
                        //if duplicated skip

                        ReferralViewModel model = new ReferralViewModel();
                        tblReferralRequest request = null;

                        if (db.tblReferralRequests.Where(o => o.ReferralID == referral.ReferralID).Count() > 0 && completeType != null)
                            request = db.tblReferralRequests.Where(o => o.ReferralID == referral.ReferralID && o.Complete == completeType).FirstOrDefault();
                        else if (db.tblReferralRequests.Where(o => o.ReferralID == referral.ReferralID).Count() > 0 && completeType == null)
                            request = db.tblReferralRequests.Where(o => o.ReferralID == referral.ReferralID).OrderByDescending(o => o.Complete).FirstOrDefault();
                        else
                            request = db.tblReferralRequests.Where(o => o.ReferralID == referral.ReferralID).FirstOrDefault();

                        if (request != null)
                        {
                            model.submitDate = request.Create_Date.ToShortDateString();
                            model.isComplete = request.Complete;
                        }
                        model.referralId = referral.ReferralID;
                        model.lastName = referral.LastName;
                        model.firstName = referral.FirstName;
                        model.kidsId = referral.KIDSID.HasValue && referral.KIDSID > 0 ? referral.KIDSID.ToString() : "";
                        model.notes = referral.ReferralNotes;
                        referralList.Add(model);
                    }
                }

            }

            return Json(new { Result = "success", FilterList = referralList.OrderBy(o => o.lastName).ThenBy(o => o.firstName).ToList() });
        }

        // GET: Manage/Referrals
        [HttpGet]
        public ActionResult Referrals()
        {
            var currentUser = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);

            var districts = (from org in db.tblOrganizationMappings
                             join user in db.tblUsers
                                 on org.UserID equals user.UserID
                             where (user.UserID == currentUser.UserID)
                             select org).Distinct();

            List<ReferralViewModel> referralList = new List<ReferralViewModel>();
            if (districts != null)
            {
                foreach (var district in districts)
                {

                    var referrals = (from refInfo in db.tblReferralInfoes
                                     join rr in db.tblReferralRequests
                                         on refInfo.ReferralID equals rr.ReferralID
                                     where
                                     (refInfo.AssignedUSD == district.USD)
                                     && rr.Complete == false
                                     select refInfo).Distinct();

                    foreach (var referral in referrals)
                    {
                        //if duplicated skip
                        if (!referral.tblReferralRequests.Any(o => o.Complete == true))
                        {
                            ReferralViewModel model = new ReferralViewModel();

                            var request = db.tblReferralRequests.Where(o => o.ReferralID == referral.ReferralID).FirstOrDefault();
                            if (request != null)
                            {
                                model.submitDate = request.Create_Date.ToShortDateString();
                                model.isComplete = request.Complete;
                            }
                            model.referralId = referral.ReferralID;
                            model.lastName = referral.LastName;
                            model.firstName = referral.FirstName;
                            model.kidsId = referral.KIDSID.HasValue && referral.KIDSID > 0 ? referral.KIDSID.ToString() : "";
                            model.notes = referral.ReferralNotes;
                            referralList.Add(model);
                        }

                    }
                }

            }

            ViewBag.Referrals = referralList.OrderBy(o => o.lastName).ThenBy(o => o.firstName).ToList();

            return View("~/Views/Home/Referrals.cshtml");

        }

        // GET: Manage/EditReferral
        [HttpGet]
        public ActionResult EditReferral(int id)
        {
            StudentDetailsViewModel model = new StudentDetailsViewModel();
            bool isComplete = false;
            model.student = new Student();

            tblReferralRequest referralReq = db.tblReferralRequests.Where(o => o.ReferralID == id).FirstOrDefault();
            if (referralReq != null)
            {
                isComplete = referralReq.Complete;

            }

            tblReferralInfo student = db.tblReferralInfoes.Where(u => u.ReferralID == id).FirstOrDefault();
            if (student != null)
            {

                model.referralId = student.ReferralID;
                model.student.FirstName = student.FirstName;
                model.student.MiddleName = student.MiddleInitial;
                model.student.LastName = student.LastName;
                model.student.City = student.City;
                model.student.State = student.State;
                model.student.RoleID = "5";
                model.student.Address1 = student.Address1;
                model.student.Address2 = student.Address2;
                model.student.Zip = student.Zip;
                model.student.KidsID = student.KIDSID;
                model.student.DateOfBirth = student.DateOfBirth.HasValue ? student.DateOfBirth.Value : DateTime.MinValue;
                model.student.USD = student.AssignedUSD;
                model.student.BuildingID = student.ResponsibleBuildingID;
                model.student.NeighborhoodBuildingID = student.NeighborhoodBuildingID;
                model.student.County = student.County;
                model.student.USD = student.AssignedUSD;

                tblStudentInfo studentinfo = new tblStudentInfo();
                studentinfo.County = student.County;
                studentinfo.AssignedUSD = student.AssignedUSD;

                studentinfo.BuildingID = student.ResponsibleBuildingID;
                studentinfo.NeighborhoodBuildingID = student.NeighborhoodBuildingID;
                studentinfo.ParentLanguage = student.ParentLanguage;
                studentinfo.StudentLanguage = student.StudentLanguage;
                studentinfo.RaceCode = student.RaceCode; //db.tblRaces.Where(r => r.RaceCode == student.RaceCode).FirstOrDefault().RaceDescription;
                studentinfo.Ethicity = student.Ethicity;
                studentinfo.Gender = student.Gender;
                studentinfo.Grade = student.Grade;


                if (student.InitialEvalConsentSigned != null)
                    studentinfo.InitialEvalConsentSigned = student.InitialEvalConsentSigned;

                model.info = studentinfo;
            }


            List<tblReferralRelationship> relationships = db.tblReferralRelationships.Where(r => r.ReferralID == id).ToList();
            if (relationships != null)
            {
                foreach (tblReferralRelationship relationship in relationships)
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
                        RealtionshipID = relationship.RealtionshipID,
                        PrimaryContact = relationship.PrimaryContact.HasValue && relationship.PrimaryContact == 1 ? 1 : 0
                    });
                }
            }

            model.submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
			model.allDistricts = db.tblDistricts.ToList();
			model.districts = model.submitter.RoleID == "1" ? model.allDistricts.Where(d => d.Active == 1).ToList() : (from d in model.allDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where model.submitter.UserID == bm.UserID select d).Distinct().ToList();
			model.placementCode = db.tblPlacementCodes.ToList();
            model.primaryDisabilities = db.vw_PrimaryDisabilities.ToList();
            model.secondaryDisabilities = db.vw_SecondaryDisabilities.ToList();
            model.statusCode = db.tblStatusCodes.ToList();
            model.grades = db.tblGrades.ToList();            
			model.selectedDistrict = (from d in db.tblDistricts join o in db.tblOrganizationMappings on d.USD equals o.USD where model.student.UserID == o.UserID select d).Distinct().ToList();

			model.races = db.tblRaces.ToList();


			ViewBag.SelectedDistrictBuildings = (from b in db.vw_BuildingList
												 where b.USD == student.AssignedUSD
												 select new BuildingsViewModel
												 {
													 BuildingName = b.BuildingName,
													 BuildingID = b.BuildingID,
													 BuildingUSD = b.USD
												 }).OrderBy(b => b.BuildingName).ToList();

			
			ViewBag.AllBuildings = (from b in db.vw_BuildingList
									where b.isServiceOnly == false
									select new BuildingsViewModel
									{
										BuildingName = b.BuildingName,
										BuildingID = b.BuildingID,
										BuildingUSD = b.USD
									}).OrderBy(b => b.BuildingName).ToList();

			

			ViewBag.RoleName = ConvertToRoleName(model.submitter.RoleID);
            ViewBag.ReferralComplete = isComplete;
            return View("~/Views/Home/EditReferral.cshtml", model);
        }


        [HttpPost]
        public JsonResult DeleteReferral(int referralId)
        {
            try
            {
                List<tblReferralRelationship> relationships = db.tblReferralRelationships.Where(r => r.ReferralID == referralId).ToList();
                if (relationships != null && relationships.Count > 0)
                {
                    db.tblReferralRelationships.RemoveRange(relationships);
                }

                var referral = db.tblReferralInfoes.Where(r => r.ReferralID == referralId).FirstOrDefault();
                if (referral != null)
                {
                    db.tblReferralInfoes.Remove(referral);
                }

                var referralReq = db.tblReferralRequests.Where(r => r.ReferralID == referralId).FirstOrDefault();
                if (referralReq != null)
                {
                    db.tblReferralRequests.Remove(referralReq);
                }

                db.SaveChanges();

            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = "There was an error while trying to delete the Referral. \n\n" + e.InnerException.ToString() });
            }

            return Json(new { Result = "success", Message = "The student was successfully deleted." });


        }

        [HttpPost]
        public JsonResult EditReferral(HttpPostedFileBase adminpersona, FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

                    // check that the kidsIS doesn't already exsist in the system.
                    long kidsID = Convert.ToInt64(collection["kidsid"]);
                    int referralId = Convert.ToInt32(collection["referralId"]);

                    if (kidsID == 0)
                    {
                        return Json(new { Result = "error", Message = "The KIDS ID is invalid. Please enter another KIDS ID." });
                    }

                    tblStudentInfo exsistingStudent = db.tblStudentInfoes.Where(i => i.KIDSID == kidsID).FirstOrDefault();
                    tblReferralRequest referralReq = db.tblReferralRequests.Where(o => o.ReferralID == referralId).FirstOrDefault();

                    if (exsistingStudent != null)
                    {
                        if (referralReq != null && referralReq.Complete)
                            return Json(new { Result = "error", Message = "The student is already in the Greenbush system. Please contact Greenbush." });

                        //student has been created but it is not complete - don't create new record


                        return Json(new { Result = "success", Message = exsistingStudent.UserID });
                    }

                    // Create New User 
                    tblUser student = new tblUser()
                    {
                        RoleID = "5",
                        FirstName = collection["firstname"],
                        MiddleName = collection["middlename"],
                        LastName = collection["lastname"],
                        Email = ((!string.IsNullOrEmpty(collection["email"])) ? collection["email"].ToString() : null),
                        Create_Date = DateTime.Now,
                        Update_Date = DateTime.Now,
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
                    DateTime dobDate = DateTime.MinValue;

                    DateTime.TryParse(collection["dob"], out dobDate);

                    if (dobDate == DateTime.MinValue)
                    {
                        return Json(new { Result = "error", Message = "The Birthdate supplied in not a valid date." });
                    }

                    // tblStudentInfo
                    tblStudentInfo studentInfo = new tblStudentInfo()
                    {
                        UserID = student.UserID,
                        KIDSID = kidsID,
                        DateOfBirth = dobDate,
                        Primary_DisabilityCode = collection["primaryDisability"] != null ? collection["primaryDisability"].ToString() : "",
                        Secondary_DisabilityCode = collection["secondaryDisability"] != null ? collection["secondaryDisability"].ToString() : "",
                        AssignedUSD = collection["assignChildCount"] != null ? collection["assignChildCount"].ToString() : "",
                        USD = collection["misDistrict"],
                        BuildingID = collection["AttendanceBuildingId"],
                        NeighborhoodBuildingID = collection["NeighborhoodBuildingID"],
                        Status = "PENDING",
                        Gender = (String.IsNullOrEmpty(collection["gender"])) ? "M" : "F",
                        CreatedBy = submitter.UserID,
                        Create_Date = DateTime.Now,
                        Update_Date = DateTime.Now,
                        PlacementCode = collection["studentPlacement"],
                        ClaimingCode = true, // set to default true unless they change it on the second page.
                        isGifted = collection["Is_Gifted"] != null && collection["Is_Gifted"] == "on" ? true : false
                    };

                    // map the buildings in the building mapping table
                    try
                    {
                        db.tblBuildingMappings.Add(new tblBuildingMapping() { BuildingID = studentInfo.BuildingID, USD = studentInfo.AssignedUSD, UserID = studentInfo.UserID });
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + e.InnerException.ToString() });
                    }

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

        [HttpPost]
        public JsonResult EditReferralOptions(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int studentId = Convert.ToInt32(collection["studentId"]);

                    tblUser user = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
                    if (user != null)
                    {
                        user.Address1 = collection["studentStreetAddress1"].ToString();
                        user.Address2 = collection["studentStreetAddress2"].ToString();
                        user.City = collection["studentCity"].ToString();
                        user.State = collection["studentState"].ToString();
                        user.Zip = collection["studentZipCode"].ToString();
                    }
                    db.SaveChanges();

                    tblStudentInfo info = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();
                    if (info != null)
                    {
                        info.County = collection["studentCounty"].ToString();
                        info.Grade = Convert.ToInt32(collection["studentGrade"]);
                        info.RaceCode = collection["studentRace"].ToString();
                        info.Race = db.tblRaces.Where(r => r.RaceCode == info.RaceCode).FirstOrDefault().RaceDescription;
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

                        if (!String.IsNullOrEmpty(collection["exitDate"]))
                        {
                            info.ExitDate = Convert.ToDateTime(collection["exitDate"]);
                        }

                        if (!String.IsNullOrEmpty(collection["initialConsentSignature"]))
                        {
                            info.InitialEvalConsentSigned = Convert.ToDateTime(collection["initialConsentSignature"]);
                        }

                        if (!String.IsNullOrEmpty(collection["initialEvaluationDetermination"]))
                        {
                            info.InitialEvalDetermination = Convert.ToDateTime(collection["initialEvaluationDetermination"]);
                        }

                        if (!String.IsNullOrEmpty(collection["reEvaluationSignature"]))
                        {
                            info.ReEvalConsentSigned = Convert.ToDateTime(collection["reEvaluationSignature"]);
                        }
                    }
                    db.SaveChanges();

                    if (info != null && info.ReEvalConsentSigned.HasValue)
                        CreateReevalArchive(studentId, info.ReEvalConsentSigned.Value);


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
        public JsonResult EditReferralContacts(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int studentId = Convert.ToInt32(collection["studentId"]);

                    tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
                    tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();

                    int j = 1;
                    int loopCounter = 1;
                    while (++j < collection.Count - 2)
                    {
                        tblStudentRelationship contact = new tblStudentRelationship()
                        {
                            RealtionshipID = 0,
                            UserID = studentId,
                            FirstName = collection[j].ToString(),
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
                        catch (DbEntityValidationException e)
                        {
                            var sb = new StringBuilder();
                            foreach (var eve in e.EntityValidationErrors)
                            {
                                sb.Append(string.Format("Error in \"{0}\": ",
                                    eve.Entry.Entity.GetType().Name, eve.Entry.State));
                                foreach (var ve in eve.ValidationErrors)
                                {
                                    sb.Append(string.Format("- Property: \"{0}\", Error: \"{1}\"",
                                        ve.PropertyName, ve.ErrorMessage));
                                }
                            }

                            return Json(new { Result = "error", Message = "There was an error while trying to add the student's contacts. \n\n" + sb.ToString() });

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
        public ActionResult EditReferralAvatar(HttpPostedFileBase adminpersona, FormCollection collection)
        {
            int studentId = Convert.ToInt32(collection["studentId"]);
            int referralID = Convert.ToInt32(collection["referralId"]);
            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
            if (student != null)
            {
                //// UPLOAD the image
                if (adminpersona != null && adminpersona.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(adminpersona.FileName);
                    var random = Guid.NewGuid() + fileName;
                    var path = Path.Combine(Server.MapPath("~/Avatar/"), random);
                    if (!Directory.Exists(Server.MapPath("~/Avatar/")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Avatar/"));
                    }

                    student.ImageURL = random;
                    adminpersona.SaveAs(path);

                    db.SaveChanges();
                }
            }


            var rrList = db.tblReferralRequests.Where(o => o.ReferralID == referralID);
            if (rrList != null)
            {
                foreach (var rr in rrList)
                {
                    rr.Complete = true;
                    rr.Update_Date = DateTime.Now;
                }

                db.SaveChanges();

            }

            return RedirectToAction("Referrals", "Manage");

        }


        // GET: Manage/CreateReferral
        [HttpGet]
        public ActionResult CreateReferral()
        {
            StudentDetailsViewModel model = new StudentDetailsViewModel();

            model.submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            model.allDistricts = db.tblDistricts.Where(d => d.Active == 1).ToList();
            model.student.DateOfBirth = DateTime.Now.AddYears(-5);
            model.placementCode = db.tblPlacementCodes.ToList();
            model.primaryDisabilities = db.vw_PrimaryDisabilities.ToList();
            model.secondaryDisabilities = db.vw_SecondaryDisabilities.ToList();
            model.contacts.Add(new tblStudentRelationship() { Realtionship = "parent", State = "KS" });
            model.statusCode = db.tblStatusCodes.ToList();
            model.grades = db.tblGrades.ToList();
            model.races = db.tblRaces.ToList();

            ViewBag.RoleName = ConvertToRoleName(model.submitter.RoleID);

			ViewBag.SelectedDistrictBuildings = (from b in db.vw_BuildingList
												 where b.USD == "101"
												 select new BuildingsViewModel
												 {
													 BuildingName = b.BuildingName,
													 BuildingID = b.BuildingID,
													 BuildingUSD = b.USD
												 }).OrderBy(b => b.BuildingName).ToList();

			ViewBag.AllBuildings = (from b in db.vw_BuildingList
									where b.isServiceOnly == false
									select new BuildingsViewModel
									{
										BuildingName = b.BuildingName,
										BuildingID = b.BuildingID,
										BuildingUSD = b.USD
									}).OrderBy(b => b.BuildingName).ToList();

			return View("~/Views/Home/CreateReferral.cshtml", model);
        }

        // POST: Manage/CreateReferral
        [HttpPost]
        public JsonResult CreateReferral(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

                    // check that the kidsIS doesn't already exsist in the system.
                    string kidsIdStr = collection["kidsid"].ToString();
                    long kidsID = 0;

                    if (!string.IsNullOrEmpty(kidsIdStr))
                    {
                        kidsID = Convert.ToInt64(kidsIdStr);
                        tblStudentInfo exsistingStudent = db.tblStudentInfoes.Where(i => i.KIDSID == kidsID).FirstOrDefault();
                        if (exsistingStudent != null)
                        {
                            return Json(new { Result = "error", Message = "The student is already in the system. Please contact Greenbush." });
                        }


                        tblReferralInfo exsistingReferral = db.tblReferralInfoes.Where(i => i.KIDSID == kidsID).FirstOrDefault();
                        if (exsistingReferral != null)
                        {
                            return Json(new { Result = "error", Message = "A referral with the same KIDS ID has already been submitted. Please contact Greenbush if you need more information." });
                        }
                    }

                    string raceCodeVal = collection["studentRace"].ToString();
                    // Create New					
                    tblReferralInfo studentInfo = new tblReferralInfo()
                    {
                        //UserID = student.UserID,
                        FirstName = collection["firstname"],
                        MiddleInitial = collection["middlename"],
                        LastName = collection["lastname"],
                        KIDSID = kidsID,
                        Gender = (String.IsNullOrEmpty(collection["gender"])) ? "M" : "F",
                        Address1 = collection["studentStreetAddress1"].ToString(),
                        Address2 = collection["studentStreetAddress2"].ToString(),
                        City = collection["studentCity"].ToString(),
                        State = collection["studentState"].ToString(),
                        Zip = collection["studentZipCode"].ToString(),
                        County = collection["studentCounty"].ToString(),
                        Grade = Convert.ToInt32(collection["studentGrade"]),
                        RaceCode = raceCodeVal,
                        Race = db.tblRaces.Where(r => r.RaceCode == raceCodeVal).FirstOrDefault().RaceDescription,
                        Ethicity = collection["studentEthnic"].ToString(),
                        StudentLanguage = collection["studentLanguage"].ToString(),
                        ParentLanguage = collection["parentLanguage"].ToString(),
                        CreatedBy = submitter.UserID,
                        Create_Date = DateTime.Now,
                        Update_Date = DateTime.Now
                    };



                    if (!String.IsNullOrEmpty(collection["dob"]))
                    {
                        studentInfo.DateOfBirth = Convert.ToDateTime(collection["dob"]);
                    }


                    try
                    {
                        db.tblReferralInfoes.Add(studentInfo);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "There was an error while trying to create the referral. \n\n" + e.InnerException.ToString() });
                    }

                    return Json(new { Result = "success", Message = studentInfo.ReferralID });
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

            return Json(new { Result = "error", Message = "There was an error while trying to create the referral. Please try again or contact your administrator." });
        }

        // POST: Manage/CreateStudent
        [HttpPost]
        public JsonResult CreateReferralSchoolData(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
                    int studentId = Convert.ToInt32(collection["studentId"]);

                    tblReferralInfo student = db.tblReferralInfoes.Where(u => u.ReferralID == studentId).FirstOrDefault();
                    if (student != null)
                    {
                        student.AssignedUSD = collection["assignChildCount"].ToString();
                        student.ResponsibleBuildingID = collection["AttendanceBuildingId"];
                        student.NeighborhoodBuildingID = collection["NeighborhoodBuildingID"];
                        student.ReferralNotes = collection["ReferralNotes"];

                        if (!String.IsNullOrEmpty(collection["initialConsentSignature"]))
                        {
                            student.InitialEvalConsentSigned = Convert.ToDateTime(collection["initialConsentSignature"]);
                        }

                        db.SaveChanges();
                    }
                    return Json(new { Result = "success", Message = student.ReferralID });
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

        [HttpPost]
        public JsonResult CreateReferralContacts(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int studentId = Convert.ToInt32(collection["studentId"]);

                    tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
                    tblReferralInfo student = db.tblReferralInfoes.Where(u => u.ReferralID == studentId).FirstOrDefault();

                    //delete old
                    var relationships = db.tblReferralRelationships.Where(o => o.ReferralID == student.ReferralID).ToList();
                    if (relationships.Any())
                    {
                        foreach (var existingRelationship in relationships)
                        {
                            db.tblReferralRelationships.Remove(existingRelationship);

                        }
                        db.SaveChanges();
                    }

                    int j = 0;
                    int loopCounter = 1;
                    while (++j < collection.Count - 1)
                    {
                        tblReferralRelationship contact = new tblReferralRelationship()
                        {
                            RealtionshipID = 0,
                            ReferralID = studentId,
                            FirstName = collection[j].ToString(),
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
                            //add new
                            db.tblReferralRelationships.Add(contact);
                            db.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            return Json(new { Result = "error", Message = "There was an error while trying to add the referral's contacts. \n\n" + e.InnerException.ToString() });
                        }

                        loopCounter++;
                    }

                    //create summary
                    string summaryText = CreateSummary(student);

                    return Json(new { Result = "success", Message = student.ReferralID, Summary = summaryText });
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

            return Json(new { Result = "error", Message = "There was an error while trying to create the referral's contacts. Please try again or contact your administrator." });
        }

        private string CreateSummary(tblReferralInfo student)
        {
            //create summary
            string neighborhoodBuilding = "";
            if (student.NeighborhoodBuildingID != null)
            {
                var nb = db.tblBuildings.Where(o => o.BuildingID == student.NeighborhoodBuildingID).FirstOrDefault();
                neighborhoodBuilding = nb.BuildingName;
            }

            string responsibleBuilding = "";
            if (student.ResponsibleBuildingID != null)
            {
                var nb = db.tblBuildings.Where(o => o.BuildingID == student.ResponsibleBuildingID).FirstOrDefault();
                responsibleBuilding = nb.BuildingName;
            }

            string grade = "";
            if (student.Grade != null)
            {
                var nb = db.tblGrades.Where(o => o.gradeID == student.Grade).FirstOrDefault();
                grade = nb.description;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<h3>Student Information</h3>");
            sb.Append("<br/>");
            sb.AppendFormat("<b>KIDSID:</b> {0} ", student.KIDSID.HasValue && student.KIDSID.Value != 0 ? student.KIDSID.ToString() : "");
            sb.Append("<br/>");
            sb.AppendFormat("<b>Student Name:</b> {0} {1} {2}", student.FirstName, student.MiddleInitial, student.LastName);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Birthdate:</b> {0}", student.DateOfBirth.HasValue ? student.DateOfBirth.Value.ToShortDateString() : "");
            sb.Append("<br/>");
            sb.AppendFormat("<b>Gender:</b> {0}", student.Gender);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Address:</b> {0} {1} {2}, {3} {4}", student.Address1, student.Address2, student.City, student.State, student.Zip);
            sb.Append("<br/>");
            sb.AppendFormat("<b>County of Residence:</b> {0}", student.County);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Grade:</b> {0}", grade);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Race:</b> {0}", student.Race);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Hispanic Ethnicity:</b> {0}", student.Ethicity);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Student Language:</b> {0}", student.StudentLanguage);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Parents Language:</b> {0}", student.ParentLanguage);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Assign Child Count:</b> {0}", student.AssignedUSD);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Neighborhood School:</b> {0}", neighborhoodBuilding);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Responsible School:</b> {0}", responsibleBuilding);
            sb.Append("<br/>");
            sb.AppendFormat("<b>Initial Evaluation Consent Signed:</b> {0}", student.InitialEvalConsentSigned.HasValue ? student.InitialEvalConsentSigned.Value.ToShortDateString() : "");

            sb.Append("<br/>");
            sb.Append("<br/>");
            sb.AppendFormat("<h3>Parent/Guardian Information</h3>");
            sb.Append("<br/>");
            foreach (var pc in db.tblReferralRelationships.Where(o => o.ReferralID == student.ReferralID))
            {
                sb.AppendFormat("<b>Relationship:</b> {0}", pc.Realtionship);
                sb.Append("<br/>");
                sb.AppendFormat("<b>Parent/Guardian:</b> {0} {1}", pc.FirstName, pc.LastName);
                sb.Append("<br/>");
                sb.AppendFormat("<b>Address:</b> {0} {1} {2}, {3} {4}", pc.Address1, pc.Address2, pc.City, pc.State, pc.Zip);
                sb.Append("<br/>");
                sb.AppendFormat("<b>Email:</b> {0}", pc.Email);
                sb.Append("<br/>");
                sb.Append("<br/>");
            }

            sb.Append("<br/>");
            sb.AppendFormat("<h3>Additional Information</h3>");
            sb.AppendFormat("{0}", student.ReferralNotes);

            return sb.ToString();
        }

        [HttpPost]
        public JsonResult SubmitReferral(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int studentId = Convert.ToInt32(collection["studentId"]);

                    tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
                    tblReferralInfo student = db.tblReferralInfoes.Where(u => u.ReferralID == studentId).FirstOrDefault();
                    List<tblUser> list = new List<tblUser>();

                    string misRole = "2"; //level 4
                    list = (from org in db.tblOrganizationMappings
                            join user in db.tblUsers
                                on org.UserID equals user.UserID
                            where !(user.Archive ?? false) && (user.RoleID == misRole) && org.USD == student.AssignedUSD
                            select user).Distinct().ToList();

                    int userDistrictId = 0;
                    if (list != null && list.Any())
                    {

                        SmtpClient smtpClient = new SmtpClient();
                        MailMessage mailMessage = new MailMessage();
                        mailMessage.ReplyToList.Add(new System.Net.Mail.MailAddress("GreenbushIEP@greenbush.org"));

                        foreach (var misUser in list)
                        {
                            if (userDistrictId == 0)
                                userDistrictId = misUser.UserID;

                            if (!string.IsNullOrEmpty(misUser.Email))
                            {
                                mailMessage.To.Add(misUser.Email);
                            }
                        }


                        //create summary
                        string summaryText = CreateSummary(student);
                        StringBuilder sb = new StringBuilder();
                        sb.Append("The following new Referral Request has been created. Please log into the IEP Backpack to review the details.<br/><br/>");
                        sb.AppendFormat("<b>Submitted by:</b> {0}, {1}<br/><br/>", submitter.FirstName, submitter.LastName);
                        sb.Append(summaryText);
                        sb.Append("<br/><br/>Contact melanie.johnson@greenbush.org or (620) 724-6281 if you need any assistance.<br/>URL: https://greenbushbackpack.org ");
                        mailMessage.IsBodyHtml = true;
                        mailMessage.Subject = "Referral Request";
                        mailMessage.Body = sb.ToString();

                        smtpClient.Send(mailMessage);

                    }

                    tblReferralRequest request = new tblReferralRequest();
                    request.UserID_Requster = submitter.UserID;
                    request.UserID_District = userDistrictId;
                    request.ReferralID = student.ReferralID;
                    request.Create_Date = DateTime.Now;
                    request.Update_Date = DateTime.Now;
                    db.tblReferralRequests.Add(request);
                    db.SaveChanges();

                    return Json(new { Result = "success", Message = student.ReferralID });
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

            return Json(new { Result = "error", Message = "There was an error while trying to send the referral email. Please try again or contact your administrator." });
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
            model.grades = db.tblGrades.ToList();
            model.races = db.tblRaces.ToList();

            ViewBag.SelectedDistrictBuildings = (from b in db.vw_BuildingList
                                                 where b.USD == "101"
                                                 select new BuildingsViewModel
                                                 {
                                                     BuildingName = b.BuildingName,
                                                     BuildingID = b.BuildingID,
                                                     BuildingUSD = b.USD
                                                 }).OrderBy(b => b.BuildingName).ToList();

            ViewBag.AllBuildings = (from b in db.vw_BuildingList
                                    where b.isServiceOnly == false
                                    select new BuildingsViewModel
                                    {
                                        BuildingName = b.BuildingName,
                                        BuildingID = b.BuildingID,
                                        BuildingUSD = b.USD
                                    }).OrderBy(b => b.BuildingName).ToList();

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

                    // check that the kidsIS doesn't already exsist in the system.
                    long kidsID = Convert.ToInt64(collection["kidsid"]);
                    tblStudentInfo exsistingStudent = db.tblStudentInfoes.Where(i => i.KIDSID == kidsID).FirstOrDefault();
                    if (exsistingStudent != null)
                    {
                        return Json(new { Result = "error", Message = "The student is already in the Greenbush system. Please contact Greenbush." });
                    }

                    // Create New User 
                    tblUser student = new tblUser()
                    {
                        RoleID = "5",
                        FirstName = collection["firstname"],
                        MiddleName = collection["middlename"],
                        LastName = collection["lastname"],
                        Email = ((!string.IsNullOrEmpty(collection["email"])) ? collection["email"].ToString() : null),
                        Create_Date = DateTime.Now,
                        Update_Date = DateTime.Now,
                    };

                    // try catch. If the email is the same as another student show error gracefully.
                    try
                    {
                        if (!String.IsNullOrEmpty(student.Email) && db.tblUsers.Any(o => o.Email == student.Email))
                        {
                            return Json(new { Result = "error", Message = "The email address is already in use, please use a different email address." });
                        }
                        else if (collection["misDistrict"] == null)
                        {
                            return Json(new { Result = "error", Message = "Please choose an attending district." });
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
                        KIDSID = kidsID,
                        DateOfBirth = Convert.ToDateTime(collection["dob"]),
                        Primary_DisabilityCode = collection["primaryDisability"] != null ? collection["primaryDisability"].ToString() : "",
                        Secondary_DisabilityCode = collection["secondaryDisability"] != null ? collection["secondaryDisability"].ToString() : "",
                        AssignedUSD = collection["assignChildCount"].ToString(),
                        USD = collection["misDistrict"],
                        BuildingID = collection["AttendanceBuildingId"],
                        NeighborhoodBuildingID = collection["NeighborhoodBuildingID"],
                        Status = "PENDING",
                        Gender = (String.IsNullOrEmpty(collection["gender"])) ? "M" : "F",
                        CreatedBy = submitter.UserID,
                        Create_Date = DateTime.Now,
                        Update_Date = DateTime.Now,
                        PlacementCode = collection["studentPlacement"],
                        ClaimingCode = true, // set to default true unless they change it on the second page.
                        isGifted = collection["Is_Gifted"] != null && collection["Is_Gifted"] == "on" ? true : false
                    };

                    // map the buildings in the building mapping table
                    try
                    {
                        db.tblBuildingMappings.Add(new tblBuildingMapping() { BuildingID = studentInfo.BuildingID, USD = studentInfo.AssignedUSD, UserID = studentInfo.UserID });
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + e.InnerException.ToString() });
                    }

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

        [HttpPost]
        public JsonResult CreateStudentOptions(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int studentId = Convert.ToInt32(collection["studentId"]);

                    tblUser user = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
                    if (user != null)
                    {
                        user.Address1 = collection["studentStreetAddress1"].ToString();
                        user.Address2 = collection["studentStreetAddress2"].ToString();
                        user.City = collection["studentCity"].ToString();
                        user.State = collection["studentState"].ToString();
                        user.Zip = collection["studentZipCode"].ToString();
                    }
                    db.SaveChanges();

                    tblStudentInfo info = db.tblStudentInfoes.Where(i => i.UserID == studentId).FirstOrDefault();
                    if (info != null)
                    {
                        info.County = collection["studentCounty"].ToString();
                        info.Grade = Convert.ToInt32(collection["studentGrade"]);
                        info.RaceCode = collection["studentRace"].ToString();
                        info.Race = db.tblRaces.Where(r => r.RaceCode == info.RaceCode).FirstOrDefault().RaceDescription;
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

                        if (!String.IsNullOrEmpty(collection["exitDate"]))
                        {
                            info.ExitDate = Convert.ToDateTime(collection["exitDate"]);
                        }

                        if (!String.IsNullOrEmpty(collection["initialConsentSignature"]))
                        {
                            info.InitialEvalConsentSigned = Convert.ToDateTime(collection["initialConsentSignature"]);
                        }

                        if (!String.IsNullOrEmpty(collection["initialEvaluationDetermination"]))
                        {
                            info.InitialEvalDetermination = Convert.ToDateTime(collection["initialEvaluationDetermination"]);
                        }

                        if (!String.IsNullOrEmpty(collection["reEvaluationSignature"]))
                        {
                            info.ReEvalConsentSigned = Convert.ToDateTime(collection["reEvaluationSignature"]);
                        }


                        if (!String.IsNullOrEmpty(collection["reEvalCompleted"]))
                        {
                            info.ReEvalCompleted = Convert.ToDateTime(collection["reEvalCompleted"]);
                        }
                    }
                    db.SaveChanges();

                    if (info != null && info.ReEvalConsentSigned.HasValue)
                        CreateReevalArchive(studentId, info.ReEvalConsentSigned.Value);


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
                    int loopCounter = 1;
                    while (++j < collection.Count - 1)
                    {
                        tblStudentRelationship contact = new tblStudentRelationship()
                        {
                            RealtionshipID = 0,
                            UserID = studentId,
                            FirstName = collection[j].ToString(),
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
                    var random = Guid.NewGuid() + fileName;
                    var path = Path.Combine(Server.MapPath("~/Avatar/"), random);
                    if (!Directory.Exists(Server.MapPath("~/Avatar/")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Avatar/"));
                    }

                    student.ImageURL = random;
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
                model.student.MiddleName = student.MiddleName;
                model.student.LastName = student.LastName;
                model.student.City = student.City;
                model.student.State = student.State;
                model.student.Email = student.Email;
                model.student.RoleID = "5";
                model.student.ImageURL = student.ImageURL;
                model.student.Address1 = student.Address1;
                model.student.Address2 = student.Address2;
                model.student.Zip = student.Zip;
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
            model.races = db.tblRaces.ToList();
            model.selectedRace = db.tblRaces.Where(r => r.RaceCode == studentinfo.RaceCode).FirstOrDefault();
            model.allDistricts = db.tblDistricts.ToList();
            model.districts = model.submitter.RoleID == "1" ? model.allDistricts.Where(d => d.Active == 1).ToList() : (from d in model.allDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where model.submitter.UserID == bm.UserID select d).Distinct().ToList();
            model.placementCode = db.tblPlacementCodes.ToList();
            model.primaryDisabilities = db.vw_PrimaryDisabilities.ToList();
            model.secondaryDisabilities = db.vw_SecondaryDisabilities.ToList();
            model.statusCode = db.tblStatusCodes.ToList();
            model.grades = db.tblGrades.ToList();
            model.selectedDistrict = (from d in db.tblDistricts join o in db.tblOrganizationMappings on d.USD equals o.USD where model.student.UserID == o.UserID select d).Distinct().ToList();

            string districtList = string.Join(", ", model.districts.Select(o => o.USD).Distinct());

            ViewBag.SelectedDistrictBuildings = (from b in db.vw_BuildingList
                                                 where b.USD == studentinfo.AssignedUSD
                                                 select new BuildingsViewModel
                                                 {
                                                     BuildingName = b.BuildingName,
                                                     BuildingID = b.BuildingID,
                                                     BuildingUSD = b.USD
                                                 }).OrderBy(b => b.BuildingName).ToList();

            ViewBag.RoleName = ConvertToRoleName(model.submitter.RoleID);
            ViewBag.AllBuildings = (from b in db.vw_BuildingList
                                    where b.isServiceOnly == false
                                    select new BuildingsViewModel
                                    {
                                        BuildingName = b.BuildingName,
                                        BuildingID = b.BuildingID,
                                        BuildingUSD = b.USD
                                    }).OrderBy(b => b.BuildingName).ToList();

            return View("~/Views/Home/EditStudent.cshtml", model);
        }

        // POST: Manage/EditStudent
        [HttpPost]
        public JsonResult EditStudent(FormCollection collection)
        {
            tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);

            int studentId = Convert.ToInt32(collection["id"]);
            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();
            if (student != null)
            {
                student.FirstName = collection["firstname"];
                student.MiddleName = collection["middlename"];
                student.LastName = collection["lastname"];
                student.Email = String.IsNullOrEmpty(collection["email"]) ? null : collection["email"].ToString();
                student.RoleID = "5";
            }
            else
            {
                return Json(new { Result = "error", Message = "There was an error while trying to edit the user." });
            }

            tblStudentInfo info = db.tblStudentInfoes.Where(u => u.UserID == studentId).FirstOrDefault();
            if (info != null)
            {
                // remove all the buildingId. Blow it all away.
                db.tblBuildingMappings.RemoveRange(db.tblBuildingMappings.Where(b => b.UserID == studentId));

                db.SaveChanges();

                // save to organization chart
                // save the user to all the districts that was selected.
                // tblOrganizationMapping and tblBuildingMapping
                var districtValues = collection["misDistrict"];

                if (!string.IsNullOrEmpty(districtValues))
                {
                    string[] districtArray = districtValues.Split(',');

                    List<tblOrganizationMapping> fullList = db.tblOrganizationMappings.Where(o => o.UserID == studentId).ToList();
                    List<tblOrganizationMapping> removeList = fullList.Where(o => !districtArray.Contains(o.USD)).ToList();
                    db.tblOrganizationMappings.RemoveRange(removeList);
                    db.SaveChanges();

                    try
                    {
                        foreach (string usd in districtArray)
                        {
                            if (fullList.Any(l => !l.USD.Contains(usd)))
                            {
                                var mappingCount = db.tblOrganizationMappings.Where(o => o.AdminID == submitter.UserID && o.UserID == student.UserID && o.USD == usd).Count();

                                if (mappingCount == 0)
                                {
                                    db.tblOrganizationMappings.Add(new tblOrganizationMapping()
                                    {
                                        AdminID = submitter.UserID,
                                        UserID = student.UserID,
                                        USD = usd
                                    });

                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        return Json(new { Result = "error", Message = "There was an error while trying to add the student to the Attending USD. \n\n" + e.InnerException.ToString() });
                    }
                }

                // map the buildings in the building mapping table
                try
                {
                    db.tblBuildingMappings.Add(new tblBuildingMapping()
                    {
                        BuildingID = collection["AttendanceBuildingId"],
                        USD = db.tblBuildings.Where(b => b.BuildingID == info.BuildingID).Select(b => b.USD).FirstOrDefault(), //info.USD,
                        UserID = info.UserID
                    });

                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "There was an error while trying to create the user. \n\n" + e.InnerException.ToString() });
                }

                info.UserID = student.UserID;
                info.KIDSID = Convert.ToInt64(collection["kidsid"]);
                info.DateOfBirth = Convert.ToDateTime(collection["dob"]);
                info.AssignedUSD = collection["assignChildCount"];
                info.BuildingID = collection["AttendanceBuildingId"];
                info.NeighborhoodBuildingID = collection["NeighborhoodBuildingID"];
                info.Status = "PENDING";
                info.Gender = (String.IsNullOrEmpty(collection["gender"])) ? "M" : "F";
                info.Primary_DisabilityCode = collection["primaryDisability"] != null ? collection["primaryDisability"].ToString() : "";
                info.Secondary_DisabilityCode = collection["secondaryDisability"] != null ? collection["secondaryDisability"].ToString() : "";
                info.PlacementCode = collection["studentPlacement"];
                info.USD = collection["misDistrict"];
                info.isGifted = collection["Is_Gifted"] != null && collection["Is_Gifted"] == "on" ? true : false;
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
        public JsonResult EditStudentOptions(FormCollection collection)
        {

            int studentId = Convert.ToInt32(collection["studentId"]);
            tblUser student = db.tblUsers.Where(u => u.UserID == studentId).FirstOrDefault();

            if (student != null)
            {
                try
                {
                    student.Address1 = collection["studentStreetAddress1"].ToString();
                    student.Address2 = collection["studentStreetAddress2"].ToString();
                    student.City = collection["studentCity"].ToString();
                    student.State = collection["studentState"].ToString();
                    student.Zip = collection["studentZipCode"].ToString();
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "There was an error while trying to edit the student's options. \n\n" + e.InnerException.ToString() });
                }
            }

            tblStudentInfo info = db.tblStudentInfoes.Where(u => u.UserID == studentId).FirstOrDefault();


            if (info != null)
            {
                try
                {
                    string currentStatusCode = info.StatusCode;

                    info.County = collection["studentCounty"].ToString();
                    info.Grade = Convert.ToInt32(collection["studentGrade"]);
                    info.RaceCode = collection["studentRace"].ToString();
                    info.Race = db.tblRaces.Where(r => r.RaceCode == info.RaceCode).FirstOrDefault().RaceDescription;
                    info.Ethicity = collection["studentEthnic"].ToString();
                    info.StudentLanguage = collection["studentLanguage"].ToString();
                    info.ParentLanguage = collection["parentLanguage"].ToString();
                    info.ClaimingCode = collection["claimingCode"] == "on" ? true : false;
                    info.FullDayKG = collection["fullDayKindergarten"] == "on" ? true : false;
                    info.StatusCode = collection["statusCode"].ToString();
                    info.ExitNotes = !String.IsNullOrEmpty(collection["exitNotes"]) ? collection["exitNotes"].ToString() : "";

					if (!String.IsNullOrEmpty(collection["initialIEPDate"]))
					{
						info.InitialIEPDate = Convert.ToDateTime(collection["initialIEPDate"]);
					}
					else
					{
						info.InitialIEPDate = null;
					}

					if (!String.IsNullOrEmpty(collection["exitDate"]))
					{
						info.ExitDate = Convert.ToDateTime(collection["exitDate"]);
					}
					else
					{
						info.ExitDate = null;
					}

					if (!String.IsNullOrEmpty(collection["initialConsentSignature"]))
					{
						info.InitialEvalConsentSigned = Convert.ToDateTime(collection["initialConsentSignature"]);
					}
					else
					{
						info.InitialEvalConsentSigned = null;
					}

					if (!String.IsNullOrEmpty(collection["initialEvaluationDetermination"]))
					{
						info.InitialEvalDetermination = Convert.ToDateTime(collection["initialEvaluationDetermination"]);
					}
					else
					{
						info.InitialEvalDetermination = null;
					}

					if (!String.IsNullOrEmpty(collection["reEvaluationSignature"]))
					{
						info.ReEvalConsentSigned = Convert.ToDateTime(collection["reEvaluationSignature"]);
					}
					else
					{
						info.ReEvalConsentSigned = null;
					}

					if (!String.IsNullOrEmpty(collection["reEvalCompleted"]))
					{
						info.ReEvalCompleted = Convert.ToDateTime(collection["reEvalCompleted"]);
					}
					else
					{
						info.ReEvalCompleted = null;
					}

					db.SaveChanges();

                    if (info != null && info.ReEvalConsentSigned.HasValue)
                        CreateReevalArchive(studentId, info.ReEvalConsentSigned.Value);



                    //check for exit code and send email if it was just changed to an exist code
                    if (currentStatusCode != info.StatusCode)
                    {
                        var statusCodeObj = db.tblStatusCodes.Where(o => o.StatusCode == info.StatusCode).FirstOrDefault();

                        if (statusCodeObj != null && statusCodeObj.Type.ToLower() == "inactive")
                            SendExitEmail(info.AssignedUSD
                                , string.Format("{0}, {1}", student.LastName, student.FirstName)
                                , info.ExitDate.HasValue ? info.ExitDate.Value.ToShortDateString() : ""
                                , string.Format("({0}) {1}", info.StatusCode, statusCodeObj.Description)
                                , info.ExitNotes
                                );
                    }



                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "There was an error while trying to edit the student's options. \n\n" + e.InnerException.ToString() });
                }


                return Json(new { Result = "success", Message = student.UserID });
            }

            return Json(new { Result = "error", Message = "There was an error while trying to edit students. \n\n" });
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
                    if (relationships != null && relationships.Count > 0)
                    {
                        db.tblStudentRelationships.RemoveRange(relationships);
                        db.SaveChanges();
                    }

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
                var filePath = Server.MapPath("~/Avatar/" + student.ImageURL);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    db.SaveChanges();
                }

                // UPLOAD the image
                if (adminpersona != null && adminpersona.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(adminpersona.FileName);
                    var random = Guid.NewGuid() + fileName;
                    var path = Path.Combine(Server.MapPath("~/Avatar/"), random);
                    if (!Directory.Exists(Server.MapPath("~/Avatar/")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Avatar/"));
                    }
                    student.ImageURL = random;
                    adminpersona.SaveAs(path);
                    db.SaveChanges();
                }
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

            if (model.user.UserID == model.submitter.UserID)
            {
                return View("~/Views/Home/EditMe.cshtml", model);
            }

            return View("~/Views/Home/EditUser.cshtml", model);
        }

        // POST: Manage/Edit
        [HttpPost]
        public ActionResult Edit(int id, HttpPostedFileBase adminpersona, FormCollection collection)
        {
            try
            {
                //tblUser submitter = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);				
                tblUser user = db.tblUsers.SingleOrDefault(u => u.UserID == id);
                tblUser misUser = FindSupervisor.GetUSersMIS(user);
                if (user.UserID == misUser.UserID)
                {  // I'm my own grandpapa 
                    misUser = db.tblUsers.Where(u => u.UserID == 1).FirstOrDefault();
                }

                // EDIT the user
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(collection["teacherID"])) { user.TeacherID = collection["teacherID"]; }
                    if (!string.IsNullOrEmpty(collection["role"])) { user.RoleID = collection["role"]; }
                    user.FirstName = collection["FirstName"];
                    user.LastName = collection["LastName"];
                    user.Email = collection["userEmail"];

                    if (collection.AllKeys.Contains("password"))
                    {
                        string password = collection["password"].ToString();

                        PasswordHash hash = new PasswordHash(password);

                        user.Password = hash.Hash;
                        user.Salt = hash.Salt;

                        db.SaveChanges();
                    }
                }

                // EDIT their avatar
                if (adminpersona != null)
                {
                    if (!string.IsNullOrEmpty(user.ImageURL))
                    {
                        // Delete exiting file
                        System.IO.File.Delete(Path.Combine(Server.MapPath("~/Avatar/"), user.ImageURL));
                    }

                    // Save new file
                    string filename = Guid.NewGuid() + Path.GetFileName(adminpersona.FileName);
                    string path = Path.Combine(Server.MapPath("~/Avatar/"), filename);
                    adminpersona.SaveAs(path);
                    user.ImageURL = filename;
                }

                db.SaveChanges();

                List<tblOrganizationMapping> districtMappings = new List<tblOrganizationMapping>();
                List<tblBuildingMapping> buildingMappings = new List<tblBuildingMapping>();
                List<string> districts = new List<string>();
                List<string> buildings = new List<string>();

                if (collection["misDistrict"] != null)
                {
                    districts = new List<string>(collection["misDistrict"].ToString().Split(','));
                }

                if (collection["buildingIds"] != null)
                {
                    buildings = new List<string>(collection["buildingIds"].ToString().Split(','));
                }

                // removes any buildings not in the current list of usd's.
                List<tblBuilding> userBuildings = db.tblBuildings.Where(b => buildings.Contains(b.BuildingID) && districts.Contains(b.USD) && b.BuildingID != "0").ToList();

                if (districts != null)
                {
                    foreach (string district in districts)
                    {
                        districtMappings.Add(new tblOrganizationMapping()
                        {
                            AdminID = misUser.UserID,
                            UserID = id,
                            USD = district
                        });

                        buildingMappings.Add(new tblBuildingMapping()
                        {
                            BuildingID = "0",
                            UserID = id,
                            USD = district,
                        });
                    }
                }

                if (buildings != null)
                {
                    foreach (tblBuilding building in userBuildings)
                    {
                        buildingMappings.Add(new tblBuildingMapping()
                        {
                            BuildingID = building.BuildingID,
                            UserID = id,
                            USD = building.USD
                        });
                    }
                }

                //remove all the district relationships. Blow it all away.
                db.tblOrganizationMappings.RemoveRange(db.tblOrganizationMappings.Where(o => o.UserID == id));

                // remove all building relationships. Blow it all away.
                db.tblBuildingMappings.RemoveRange(db.tblBuildingMappings.Where(b => b.UserID == id));

                db.SaveChanges();

                // add back the connections to the database.
                db.tblOrganizationMappings.AddRange(districtMappings);
                db.tblBuildingMappings.AddRange(buildingMappings);
                db.SaveChanges();

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

                    // archive user
                    user.Archive = true;

                    //db.tblUsers.Remove(user);
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

        // POST: Manage/FilterUserList
        [HttpPost]
        public ActionResult FilterOwnerUserList(string DistrictId, string BuildingId, string RoleId, int? userId)
        {
            tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (submitter != null)
            {
                int? searchUserId = null;

                if (userId != null)
                {
                    if (userId.Value > -1)
                    {
                        searchUserId = userId.Value;
                    }
                }

                List<String> myDistricts = new List<string>();
                List<String> myBuildings = new List<string>();
                List<String> myRoles = new List<string>() { "2", "3", "4", "5", "6" };

                Dictionary<string, object> NewPortalObject = new Dictionary<string, object>();
                NewPortalObject.Add("selectedDistrict", DistrictId);
                NewPortalObject.Add("selectedBuilding", BuildingId);
                NewPortalObject.Add("selectedRole", RoleId);

                if (DistrictId == "-1")
                {
                    var districts = (from district in db.tblDistricts select new { district.USD, district.DistrictName }).Distinct().ToList();
                    myDistricts = districts.Select(d => d.USD).ToList();
                }
                else
                {
                    var districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.USD == DistrictId select new { district.USD, district.DistrictName }).Distinct().ToList();
                    myDistricts = districts.Select(d => d.USD).ToList();
                }

                if (BuildingId == "-1")
                {
                    var buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where myDistricts.Contains(buildingMap.USD) select building).Distinct().ToList();
                    NewPortalObject.Add("buildings", buildings);
                    myBuildings = buildings.Select(b => b.BuildingID).ToList();
                    myBuildings.Add("0");
                }
                else
                {
                    var buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.BuildingID == BuildingId select building).Distinct().ToList();
                    NewPortalObject.Add("buildings", buildings);
                    myBuildings = buildings.Select(b => b.BuildingID).ToList();
                }

                if (RoleId != "-1")
                {
                    var members = db.vw_UserList.Where(ul => myRoles.Contains(ul.RoleID) && myDistricts.Contains(ul.USD) && myBuildings.Contains(ul.BuildingID) && ((searchUserId == null) || (ul.UserID == searchUserId.Value)) && ul.RoleID == RoleId).Select(u => new { u.UserID, u.FirstName, u.LastName, u.RoleID }).Distinct().ToList();

                    NewPortalObject.Add("members", members);
                }
                else
                {
                    var members = db.vw_UserList.Where(ul => myRoles.Contains(ul.RoleID) && myDistricts.Contains(ul.USD) && myBuildings.Contains(ul.BuildingID) && ((searchUserId == null) || (ul.UserID == searchUserId.Value))).Select(u => new { u.UserID, u.FirstName, u.LastName, u.RoleID }).Distinct().ToList();

                    NewPortalObject.Add("members", members);
                }
                // var members = (from buildingMap in db.tblBuildingMappings
                //  join user in db.tblUsers on buildingMap.UserID equals user.UserID
                //  where myRoles.Contains(user.RoleID) 
                //  && !(user.Archive ?? false)
                //  && ((searchUserId == null) || (user.UserID == searchUserId.Value))
                //  && myDistricts.Contains(buildingMap.USD)
                //  && myBuildings.Contains(buildingMap.BuildingID)
                //  select new { user.UserID, user.FirstName, user.LastName, user.RoleID }).Distinct().ToList();


                return Json(new { Result = "success", Message = NewPortalObject }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "An error happened while removing the user from your list. Please contact your admin." });
        }

        // POST: Manage/FilterUserList
        [HttpPost]
        public ActionResult FilterUserList(string DistrictId, string BuildingId, string RoleId, int? userId)
        {
            tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (submitter != null)
            {
                int? searchUserId = null;

                if (userId != null)
                {
                    if (userId.Value > -1)
                    {
                        searchUserId = userId.Value;
                    }
                }
                List<String> myDistricts = new List<string>();
                List<String> myBuildings = new List<string>();
                List<String> myRoles = new List<string>() { "3", "4", "5", "6" };

                Dictionary<string, object> NewPortalObject = new Dictionary<string, object>();
                NewPortalObject.Add("selectedDistrict", DistrictId);
                NewPortalObject.Add("selectedBuilding", BuildingId);
                NewPortalObject.Add("selectedRole", RoleId);

                if (DistrictId == "-1")
                {
                    var districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == submitter.UserID select new { district.USD, district.DistrictName }).Distinct().ToList();
                    myDistricts = districts.Select(d => d.USD).ToList();
                }
                else
                {
                    var districts = (from org in db.tblOrganizationMappings join district in db.tblDistricts on org.USD equals district.USD where org.UserID == submitter.UserID && org.USD == DistrictId select new { district.USD, district.DistrictName }).Distinct().ToList();
                    myDistricts = districts.Select(d => d.USD).ToList();
                }

                if (BuildingId == "-1")
                {
                    var buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == submitter.UserID && myDistricts.Contains(buildingMap.USD) select building).Distinct().ToList();
                    NewPortalObject.Add("buildings", buildings);
                    myBuildings = buildings.Select(b => b.BuildingID).ToList();
                    myBuildings.Add("0");
                }
                else
                {
                    var buildings = (from buildingMap in db.tblBuildingMappings join building in db.tblBuildings on new { buildingMap.USD, buildingMap.BuildingID } equals new { building.USD, building.BuildingID } where buildingMap.UserID == submitter.UserID && buildingMap.BuildingID == BuildingId select building).Distinct().ToList();
                    NewPortalObject.Add("buildings", buildings);
                    myBuildings = buildings.Select(b => b.BuildingID).ToList();
                }

                if (submitter.RoleID == "2" || submitter.RoleID == "1")
                {
                    myRoles.Add("2");
                }

                //model.members = db.vw_UserList.Where(ul => (ul.RoleID == teacher || ul.RoleID == student || ul.RoleID == nurse) && (myBuildings.Contains(ul.BuildingID) && myDistricts.Contains(ul.USD))).Select(u => new StudentIEPViewModel() { UserID = u.UserID, FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, RoleID = u.RoleID, hasIEP = u.IsActive ?? false }).OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList().OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList();


                var members = db.vw_UserList.Where(ul => myRoles.Contains(ul.RoleID) && myDistricts.Contains(ul.USD) && myBuildings.Contains(ul.BuildingID) && ((searchUserId == null) || (ul.UserID == searchUserId.Value))).Select(u => new { u.UserID, u.FirstName, u.LastName, u.RoleID }).GroupBy(u => u.UserID).Select(u => u.FirstOrDefault()).ToList();

                //var members = (from buildingMap in db.tblBuildingMappings
                //			   join user in db.tblUsers on buildingMap.UserID equals user.UserID
                //			   where myRoles.Contains(user.RoleID)
                //			   && ((searchUserId == null) || (user.UserID == searchUserId.Value))
                //			   && !(user.Archive ?? false) 
                //			   && (myDistricts.Contains(buildingMap.USD) 
                //			   && myBuildings.Contains(buildingMap.BuildingID)) select new { user.UserID, user.FirstName, user.LastName, user.RoleID }).Distinct().ToList();

                if (RoleId != "-1")
                {
                    foreach (var user in members.ToList())
                    {
                        if (user.RoleID != RoleId)
                        {
                            members.Remove(user);
                        }
                    }
                }


                NewPortalObject.Add("members", members);
                return Json(new { Result = "success", Message = NewPortalObject }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "An error happened while removing the user from your list. Please contact your admin." });
        }

        // POST: Manage/RemoveFromTeacherList
        [HttpPost]
        public ActionResult RemoveFromTeacherList(int id, int teacherid)
        {
            tblUser teacher = db.tblUsers.FirstOrDefault(u => u.UserID == teacherid);

            tblOrganizationMapping userToRemove = db.tblOrganizationMappings.Where(u => u.AdminID == teacher.UserID && u.UserID == id).SingleOrDefault();
            if (userToRemove != null)
            {
                db.tblOrganizationMappings.Remove(userToRemove);
                db.SaveChanges();

                return Json(new { Result = "Success", Message = "User successfully removed from your list." });
            }

            return Json(new { Result = "Error", Message = "An error happened while removing the user from your list. Please contact your admin." });
        }

        // POST: Manage/AddDistrictContact
        [HttpPost]
        [Authorize]
        public ActionResult AddDistrictContact(FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
                    string USD = collection["districtUSD"].ToString();

                    tblContact districtContact = db.tblContacts.Where(c => c.USD == USD).FirstOrDefault();
                    if (districtContact != null)
                    {
                        districtContact.ContactName = collection["districtContact"].ToString();
                        districtContact.Email = collection["districtEmail"].ToString();
                        districtContact.Address1 = collection["districtContactAddress1"].ToString();
                        districtContact.Address2 = collection["districtContactAddress2"].ToString();
                        districtContact.City = collection["districtCity"].ToString();
                        districtContact.State = collection["districtState"].ToString();
                        districtContact.Zip = collection["districtContactZipCode"].ToString();
                        districtContact.Phone = collection["districtContactPhone"].ToString();
                        districtContact.Active = 1;
                        districtContact.Update_Date = DateTime.Now;
                    }
                    else
                    {
                        districtContact = new tblContact();
                        tblDistrict district = db.tblDistricts.Where(d => d.USD == USD).FirstOrDefault();

                        districtContact.ContactName = collection["districtContact"].ToString();
                        districtContact.Email = collection["districtEmail"].ToString();
                        districtContact.Address1 = collection["districtContactAddress1"].ToString();
                        districtContact.Address2 = collection["districtContactAddress2"].ToString();
                        districtContact.City = collection["districtCity"].ToString();
                        districtContact.State = collection["districtState"].ToString();
                        districtContact.Zip = collection["districtContactZipCode"].ToString();
                        districtContact.Phone = collection["districtContactPhone"].ToString();
                        districtContact.Active = 1;
                        districtContact.USD = district.USD;
                        districtContact.Create_Date = DateTime.Now;
                        districtContact.Update_Date = DateTime.Now;

                        db.tblContacts.Add(districtContact);
                    }

                    db.SaveChanges();

                    return Json(new { Result = "success", Message = "Your district contact has been updated." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "There was an error while trying to edit/add the district contact. \n\n" + e.InnerException.ToString() });
                }
            }

            return Json(new { Result = "error", Message = "There was an error while saving the district contact. Please try again or contact your administrator." }, JsonRequestBehavior.AllowGet);
        }

        // POST : Manage/ContactsInDistricts
        [HttpPost]
        [Authorize]
        public ActionResult ContactsInDistricts(string USD)
        {
            tblUser MIS = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
            if (MIS != null)
            {
                tblContact districtContact = (from contact in db.tblContacts where contact.Active == 1 && contact.USD == USD select contact).FirstOrDefault();
                if (districtContact != null)
                {
                    return Json(new { Result = "success", Message = new { ContactID = districtContact.ContactID, USD = districtContact.USD, ContactName = districtContact.ContactName, Email = districtContact.Email, Phone = districtContact.Phone, Address1 = districtContact.Address1, Address2 = districtContact.Address2, City = districtContact.City, State = districtContact.State, Zip = districtContact.Zip } }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Result = "success", Message = districtContact }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "There was an error while getting contacts in the district" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult RemoveDistrictContact(string USD)
        {
            tblContact districtContact = db.tblContacts.Where(c => c.USD == USD).FirstOrDefault();
            if (districtContact != null)
            {
                districtContact.Active = 0;

                db.SaveChanges();

                return Json(new { Result = "success", Message = "The district contact was removed." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to edit contacts. Contact Greenbush admin." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDistricts(int id)
        {
            try
            {
                tblUser user = db.tblUsers.FirstOrDefault(u => u.UserID == id);
                //List<tblDistrict> districts = (from d in db.tblDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where user.UserID == bm.UserID select d).Distinct().ToList();
                var districts = (from d in db.tblDistricts join bm in db.tblBuildingMappings on d.USD equals bm.USD where user.UserID == bm.UserID select new { USD = d.USD, DistrictName = d.DistrictName, }).Distinct().ToList();
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
        public ActionResult CreateIEPAnnual(int Stid, int Iepid)
        {
            try
            {
                tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
                db.uspCopyIEP(Iepid, user.UserID, false);
                int AnnualId = db.tblIEPs.Where(i => i.UserID == Stid && i.Amendment == false && i.IepStatus.ToUpper() == IEPStatus.DRAFT).FirstOrDefault().IEPid;

                return Json(new { Result = "success", Message = AnnualId }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened :" + e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult CreateIEPAmendment(int Stid, int IepId, bool? amend = true)
        {
            try
            {
                tblUser user = db.tblUsers.SingleOrDefault(o => o.Email == User.Identity.Name);
                db.uspCopyIEP(IepId, user.UserID, amend);
                int AmendmentId = db.tblIEPs.Where(i => i.UserID == Stid && i.Amendment == true && i.AmendingIEPid == IepId).FirstOrDefault().IEPid;

                return Json(new { Result = "success", Message = AmendmentId }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened :" + e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult GetAllStudentsInBuildings(int id, string filterStudentName)
        {
            tblUser teacher = db.tblUsers.SingleOrDefault(u => u.UserID == id);

            try
            {
                var teacherBuildings = (from bm in db.tblBuildingMappings where bm.UserID == teacher.UserID select bm.BuildingID).Distinct().ToList();
                var studentsInTheBuildings = (from bm in db.tblBuildingMappings
                                              join user in db.tblUsers on bm.UserID
            equals user.UserID
                                              where user.RoleID == "5"
                                              && ((filterStudentName == null) || (user.FirstName.Contains(filterStudentName) || user.LastName.Contains(filterStudentName)))
                                              && teacherBuildings.Contains(bm.BuildingID)
                                              select bm.UserID).ToList();
                var alreadyAssignedStudents = (from o in db.tblOrganizationMappings where o.AdminID == teacher.UserID select o.UserID).Distinct().ToList();

                // Get all users that are students NOT archive, NOT already in the teachers list and in the Teachers's building!!!!
                //var students = db.tblUsers.Where(u => u.Archive != true && studentsInTheBuildings.Contains(u.UserID) && !alreadyAssignedStudents.Contains(u.UserID)).ToList();

                var students = (from u in db.tblUsers
                                join su in db.tblStudentInfoes on u.UserID equals su.UserID
                                join b in db.tblBuildings on su.BuildingID equals b.BuildingID
                                where
                                u.RoleID == "5"
                                && !(u.Archive ?? false)
                                && studentsInTheBuildings.Contains(u.UserID)
                                && !alreadyAssignedStudents.Contains(u.UserID)
                                && teacherBuildings.Contains(b.BuildingID)
                                select new Student
                                {
                                    UserID = u.UserID,
                                    FirstName = u.FirstName,
                                    LastName = u.LastName,
                                    ImageURL = u.ImageURL,
                                    BuildingName = b.BuildingName
                                }).Distinct().OrderBy(o => o.LastName).ThenBy(o => o.FirstName).ToList();


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
                        USD = (from bm in db.tblBuildingMappings where bm.UserID == studentUser.UserID select bm.USD).FirstOrDefault()
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
        public ActionResult GetBuildingsByDistrictId(string districtId)
        {
            try
            {
                if (!string.IsNullOrEmpty(districtId))
                {
                    var buildings = db.vw_BuildingList.Where(b => b.USD == districtId).ToList();
                    return Json(new { Result = "success", DistrictBuildings = buildings }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Result = "error", Message = e.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to get buildings. Contact Greenbush admin." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize]
        public ActionResult deleteUploadForm(int studentId, int formId)
        {
            tblFormArchive form = db.tblFormArchives.Where(f => f.Student_UserID == studentId && f.FormArchiveID == formId).FirstOrDefault();
            if (form != null)
            {
                //delete the form in the database
                db.tblFormArchives.Remove(form);

                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to delete the file from the database: " + e.InnerException.ToString() }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Result = "success", Message = "The uploaded file was removed from the database." }, JsonRequestBehavior.AllowGet);
            }


            return Json(new { Result = "error", Message = "<strong>Error!</strong> An unknown error happened while trying to delete the uploaded form." }, JsonRequestBehavior.AllowGet);
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
        }

        protected void CreateReevalArchive(int studentId, DateTime reEvaldate)
        {
            if (reEvaldate != null)
            {
                var archives = db.tblArchiveEvaluationDates.Where(i => i.userID == studentId && DbFunctions.TruncateTime(i.evalutationDate) == reEvaldate.Date).AsQueryable();
                if (archives.Count() == 0)
                {
                    db.tblArchiveEvaluationDates.Add(new tblArchiveEvaluationDate { evalutationDate = reEvaldate.Date, Create_Date = DateTime.Now, userID = studentId });
                    db.SaveChanges();
                }


            }
        }


        protected void SendExitEmail(string assignedUSD, string studentName, string exitDate, string exitCode, string exitNotes)
        {
            tblUser submitter = db.tblUsers.FirstOrDefault(u => u.Email == User.Identity.Name);
            int userDistrictId = 0;
            string usdName = assignedUSD;
            string misRole = "2"; //level 4

            var list = (from org in db.tblOrganizationMappings
                        join user in db.tblUsers
                            on org.UserID equals user.UserID
                        where !(user.Archive ?? false) && (user.RoleID == misRole) && org.USD == assignedUSD
                        select user).Distinct().ToList();

            var usd = db.tblDistricts.Where(o => o.USD == assignedUSD).FirstOrDefault();

            if (list != null && list.Any())
            {

                SmtpClient smtpClient = new SmtpClient();
                MailMessage mailMessage = new MailMessage();
                mailMessage.ReplyToList.Add(new System.Net.Mail.MailAddress("GreenbushIEP@greenbush.org"));

                foreach (var misUser in list)
                {
                    if (userDistrictId == 0)
                        userDistrictId = misUser.UserID;

                    if (!string.IsNullOrEmpty(misUser.Email))
                    {
                        mailMessage.To.Add(misUser.Email);
                    }


                }


                StringBuilder sb = new StringBuilder();
                sb.Append("The following student was updated with an Exit status: ");
                sb.AppendFormat("\n\nStudent Name: {0}", studentName);
                sb.AppendFormat("\nAssigned USD: {0}", usd == null ? assignedUSD : usd.DistrictName);
                sb.AppendFormat("\nCode: {0}", exitCode);
                sb.AppendFormat("\nExit Date: {0}", exitDate);
                sb.AppendFormat("\nExit Notes: {0}", exitNotes);
                sb.AppendFormat("\nSubmitted By: {0}", string.Format("{0} {1}", submitter.FirstName, submitter.LastName));

                sb.Append("\n\nContact melanie.johnson@greenbush.org or (620) 724 - 6281 if you need any assistance.");
                sb.Append("\n\nURL: https://greenbushbackpack.org");

                mailMessage.Subject = "Student Exit";

                mailMessage.Body = sb.ToString();

                smtpClient.Send(mailMessage);

            }
        }


        #region helpers

        [NonAction]
        public string ConvertToRoleName(string roleId)
        {
            switch (roleId)
            {
                case "1":
                    return "Level 5";
                case "2":
                    return "Level 4";
                case "3":
                    return "Level 3";
                case "4":
                    return "Level 2";
                default:
                    return "Student";
            }
        }



        #endregion
    }
}