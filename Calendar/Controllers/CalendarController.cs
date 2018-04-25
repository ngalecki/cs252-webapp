using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
        public ActionResult CalendarEvent(string datebox,string CreateEvent,string ShowEvents,string CreateDueDate)
        {
            if (!String.IsNullOrWhiteSpace(CreateEvent))
            {
                //create new event
                if (String.IsNullOrWhiteSpace(datebox))
                {
                    ViewBag.Error = "Invalid Date";
                    return View("Calendar");
                }
                ViewBag.Date = datebox;
                return View("CreateEvent");
            }
            else if(!String.IsNullOrWhiteSpace(ShowEvents))
            {
                //show events
                if (String.IsNullOrWhiteSpace(datebox))
                {
                    ViewBag.Error = "Invalid Date";
                    return View("Calendar");
                }

                DateTime dt = DateTime.Parse(datebox);
                string dateString = dt.Month + "/" + dt.Day + "/" + dt.Year;

                ViewBag.WorkAssignments = GetWorkAssignments(1,dateString);
                ViewBag.InitDate = datebox;
                ViewBag.Events = getEvents(1,dateString);
                return View("Calendar");
            }
            else
            {
                if (String.IsNullOrWhiteSpace(datebox))
                {
                    ViewBag.Error = "Invalid Date";
                    return View("Calendar");
                }
                ViewBag.Date = datebox;
                return View("CreateDueDate");
            }
        }

        // Create Page after inputing info
        [HttpPost]
        public ActionResult SubmitCreateEvent(string name,string date,string time,string duration)
        {
            //populate event object with data
            Event ev = new Event();

            //dummy data
            Random rand = new Random();
            ev.ID = rand.Next();
            ev.UserID = 1;

            ev.name = name;
            ev.duration = Int32.Parse(duration);

            ev.dt = DateTime.Parse(date + " " + time);

            //update database
            SqlConnection connection = SQLGetConnection();
            connection.Open();
            SQLNonQuery(ev.ToSqlInsert(), connection);
            connection.Close();

            return View("Calendar");
        }
        
        // Create Due Date, then go back to Calendar Page
        [HttpPost]
        public ActionResult SubmitCreateDueDate(string name,string date,string time,string hours)
        {
            //Create Due Date model object, populate with data
            DueDate dd = new DueDate();

            //dummy data
            Random rand = new Random();
            dd.ID = rand.Next();
            dd.UserID = 1;

            dd.name = name;
            dd.requiredHours = Int32.Parse(hours);

            dd.dt = DateTime.Parse(date + " " + time);

            //update database
            SqlConnection connection = SQLGetConnection();
            connection.Open();
            SQLNonQuery(dd.ToSqlInsert(), connection);
            connection.Close();

            //add Workassignments for DueDate
            DateTime today = DateTime.Now;
            double daysUntilDue = (dd.dt.Date - today.Date).TotalDays;

            //split required hours over as many days as possible at minimum 2 hour intervals
            //defalt to finishing a due date 1 day early
            DateTime iterator = dd.dt;
            if (daysUntilDue != 1)
            {
                daysUntilDue--; //for finishing 1 day early
                iterator = iterator.AddDays(-1);
            }
            double hoursEachDay = dd.requiredHours / daysUntilDue;
            if (hoursEachDay < 2) hoursEachDay = 2;
            if (hoursEachDay > 18){
                //not possible throw error
                //TODO
            }

            //distribute days
            double hoursAllocated = 0;
            List<WorkAssignment> waList = new List<WorkAssignment>();
            while (hoursAllocated < dd.requiredHours)
            {
                string itDate = iterator.Month + "/" + iterator.Day + "/" + iterator.Year;

                WorkAssignment wa = new WorkAssignment();
                wa.name = dd.name;
                wa.duration = hoursEachDay;
                wa.DueDate_ID = dd.ID;
                wa.dt = DateTime.Parse(itDate);
                wa.ID = rand.Next();

                waList.Add(wa);

                hoursAllocated += hoursEachDay;
                iterator = iterator.AddDays(-1);
            }

            connection.Open();
            foreach(WorkAssignment wa in waList)
            {
                SQLNonQuery(wa.ToSqlInsert(), connection);
            }
            connection.Close();

            return View("Calendar");
        }

        public SqlConnection SQLGetConnection()
        {
            string connectionString = "Server=tcp:o3418.database.windows.net,1433;Initial Catalog=o3418_db;Persist Security Info=False;User ID=ngalecki;Password=Azure3418;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            SqlConnection connection = new SqlConnection(connectionString);
            return connection;
        }

        public SqlDataReader SQLCommandReader(string command, SqlConnection connection)
        {
                SqlCommand cmd = new SqlCommand(command, connection);

                SqlDataReader reader = cmd.ExecuteReader();
                return reader;
        }

        public void SQLNonQuery(string command, SqlConnection connection)
        {
            SqlCommand cmd = new SqlCommand(command, connection);

            cmd.ExecuteNonQuery();
        }

        public List<Event> getEvents(int UserID,string dateString)
        {
            string commandEvents = @"SELECT * FROM Events WHERE U_ID="+UserID+@" AND ""Date""='" + dateString + "'";

            SqlConnection connection = SQLGetConnection();
            connection.Open();
            SqlDataReader reader = SQLCommandReader(commandEvents, connection);

            List<Event> eventList = new List<Event>();
            //List<Event> eventList = getEvents(1,dateString);

            while (reader.Read())
            {
                Event ev = new Event();
                ev.name = (string)reader["Name"];
                ev.duration = (int)reader["duration"];
                ev.ID = (int)reader["ID"];
                ev.UserID = (int)reader["U_ID"];
                string date = (string)reader["Date"];
                string time = (string)reader["Time"];

                ev.dt = DateTime.Parse(date + " " + time);

                eventList.Add(ev);
            }

            reader.Close();
            connection.Close();

            return eventList;
        }

        public List<WorkAssignment> GetWorkAssignments(int UserID,string dateString)
        {
            string commandEvents = @"SELECT * FROM Events WHERE U_ID=" + UserID + @" AND ""Date""='" + dateString + "'";

            SqlConnection connection = SQLGetConnection();
            connection.Open();
            SqlDataReader reader2 = SQLCommandReader(commandEvents, connection);

            List<WorkAssignment> waList = new List<WorkAssignment>();

            while (reader2.Read())
            {
                WorkAssignment wa = new WorkAssignment();
                wa.name = (string)reader2["Name"];
                wa.ID = (int)reader2["ID"];
                wa.duration = (int)reader2["Duration"];
                wa.dt = DateTime.Parse((string)reader2["Date"]);
                wa.DueDate_ID = (int)reader2["DueDate_ID"];

                waList.Add(wa);
            }

            reader2.Close();
            connection.Close();

            return waList;
        }
    }

}