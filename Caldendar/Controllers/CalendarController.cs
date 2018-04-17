using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Caldendar.Controllers
{
    public class CalendarController : Controller
    {
        // GET: Start Page
        public ActionResult Index()
        {
            return View();
        }

        // GET: Calendar Page
        public ActionResult Calendar()
        {
            return View();
        }
    }

}