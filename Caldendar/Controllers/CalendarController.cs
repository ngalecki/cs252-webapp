using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Caldendar.Models;

namespace Caldendar.Controllers
{
    public class CalendarController : Controller
    {
        // Start Page
        public ActionResult Index()
        {
            return View();
        }

        // Calendar Page
        public ActionResult Calendar()
        {
            return View();
        }

        // Calendar Page with date already selected
        [HttpPost]
        public ActionResult CalendarWithInitDate(string date)
        {
            ViewBag.InitDate = date;
            return View("Calendar");
        }

        // Create Page
        [HttpPost]
        public ActionResult CreateEvent(string datebox)
        {
            if (String.IsNullOrWhiteSpace(datebox))
            {
                ViewBag.Error = "Invalid Date";
                return View("Calendar");
            }
            DateTime dt = DateTime.Parse(datebox);
            ViewBag.DateTime = dt;
            return View();
        }

        // Create Page after inputing info
        [HttpPost]
        public ActionResult SubmitCreateEvent(string name,string time ,string duration)
        {
            Event ev = new Event();
            ev.name = name;
            ev.duration = Int32.Parse(duration);
            return View("Calendar");
        }
    }

}