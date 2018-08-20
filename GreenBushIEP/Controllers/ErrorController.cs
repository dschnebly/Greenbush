using System.Web.Mvc;

namespace GreenBushIEP.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index()
        {
            ViewBag.Error = TempData["Error"];
            return View();
        }
    }
}