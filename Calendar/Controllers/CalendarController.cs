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
        public ActionResult CalendarEvent(string datebox,string Create,string ShowEvents)
        {
            if (!String.IsNullOrWhiteSpace(Create))
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
            else
            {
                //show events
                if (String.IsNullOrWhiteSpace(datebox))
                {
                    ViewBag.Error = "Invalid Date";
                    return View("Calendar");
                }
                //get all events
                SqlConnection connection = SQLGetConnection();

                DateTime dt = DateTime.Parse(datebox);
                string dateString = dt.Month + "/" + dt.Day + "/" + dt.Year;

                //dummy data, setting user id to 1 for now
                string command = @"SELECT ""Name"",duration,""Date"",""Time"" FROM Events WHERE U_ID=1 AND ""Date""='"+dateString+"'";

                connection.Open();
                SqlDataReader reader = SQLCommandReader(command, connection);

                List<Event> eventList = new List<Event>();

                while (reader.Read())
                {
                    Event ev = new Event();
                    ev.name = (string) reader["Name"];
                    ev.duration = (int) reader["duration"];
                    string date = (string) reader["Date"];
                    string time = (string)reader["Time"];

                    ev.dt = DateTime.Parse(date +" "+ time);

                    eventList.Add(ev);
                }

                connection.Close();
                reader.Close();

                ViewBag.InitDate = datebox;
                ViewBag.Events = eventList;
                return View("Calendar");
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

            int indexOfColon = time.IndexOf(':');
            int indexOfSpace = time.IndexOf(' ');

            string hourString = time.Substring(0, indexOfColon);
            int hour = Int32.Parse(hourString);

            string minuteString = time.Substring(indexOfColon + 1, indexOfSpace - indexOfColon - 1);
            int minute = Int32.Parse(minuteString);

            ev.dt = DateTime.Parse(date + " " + time);

            //update database
            SqlConnection connection = SQLGetConnection();
            connection.Open();
            SQLNonQuery(ev.ToSqlInsert(), connection);
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
    }

}