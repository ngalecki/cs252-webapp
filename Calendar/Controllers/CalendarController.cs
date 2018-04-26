using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
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
            DateTime dt = DateTime.Now;
            string dateString = dt.Month + "/" + dt.Day + "/" + dt.Year;

            ViewBag.DueDates = GetDueDates(1, dateString);
            ViewBag.WorkAssignments = GetWorkAssignments(1, dateString);
            ViewBag.InitDate = dt.ToString("MM/dd/yyy");
            ViewBag.Events = getEvents(1, dateString);
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

                ViewBag.DueDates = GetDueDates(1, dateString);
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
                wa.User_ID = 1; //dummy data
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

        [HttpPost]
        public ActionResult EditEvent(string name,string date,int id,string time,int duration)
        {
            ViewBag.Name = name;
            ViewBag.Date = date;
            ViewBag.ID = id;
            ViewBag.Time = time;
            ViewBag.Duration = duration;
            return View();
        }

        [HttpPost]
        public ActionResult SubmitEditEvent(string submit,string delete,string cancel,string name,string date,int id,string time,int duration)
        {
            if (!String.IsNullOrEmpty(submit))
            {
                //submit button
                string command = @"UPDATE ""Events"" SET Name='" + name + "', Date='" + date + "', Time='" + time + "', Duration=" + duration + " WHERE ID=" + id + ";";

                SqlConnection connection = SQLGetConnection();
                connection.Open();
                SQLNonQuery(command, connection);
                connection.Close();

                return View("Calendar");
            }else if (!String.IsNullOrEmpty(delete))
            {
                //delete button
                string command = @"DELETE FROM ""Events"" WHERE ID="+id+";";

                SqlConnection connection = SQLGetConnection();
                connection.Open();
                SQLNonQuery(command, connection);
                connection.Close();

                return View("Calendar");
            }
            else
            {
                //cancel button
                return View("Calendar");
            }
        }

        [HttpPost]
        public ActionResult EditDueDate(string name,string date,string time,int id)
        {
            ViewBag.Name = name;
            ViewBag.Date = date;
            ViewBag.Time = time;
            ViewBag.ID = id;
            return View();
        }

        [HttpPost]
        public ActionResult SubmitEditDueDate(string submit, string delete, string cancel, string name, string date, int id, string time, int hours)
        {
            if (!String.IsNullOrEmpty(submit))
            {
                //submit button

                //delete all work assignments attached to the dd
                string command = @"DELETE FROM WorkAssignments WHERE DueDate_ID=" + id + ";";
                SqlConnection connection = SQLGetConnection();
                connection.Open();
                SQLNonQuery(command, connection);
                connection.Close();

                //delete dd
                command = @"DELETE FROM DueDates WHERE ID=" + id + ";";
                SqlConnection connection2 = SQLGetConnection();
                connection.Open();
                SQLNonQuery(command, connection2);
                connection2.Close();

                //submit new duedate, works just as well
                return SubmitCreateDueDate(name, date, time, hours.ToString());
            }
            else if (!String.IsNullOrEmpty(delete))
            {
                //delete button

                //delete all work assignments attached to the dd
                string command = @"DELETE FROM WorkAssignments WHERE DueDate_ID=" + id + ";";
                SqlConnection connection = SQLGetConnection();
                connection.Open();
                SQLNonQuery(command, connection);
                connection.Close();

                //delete dd
                command = @"DELETE FROM DueDates WHERE ID=" + id + ";";
                return View("Calendar");
            }
            else
            {
                //cancel button
                return View("Calendar");
            }
        }

        public ActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SubmitSignIn(string signIn,string newAccount,string userName,string password)
        {
            if (!String.IsNullOrEmpty(signIn))
            {
                //sign in
                string checkForUsername = @"SELECT * FROM Users WHERE UserName='" + userName + "';";
                SqlConnection connection = SQLGetConnection();
                connection.Open();
                SqlDataReader reader = SQLCommandReader(checkForUsername, connection);

                if (reader.HasRows)
                {
                    //username found
                    reader.Read();
                    User u = new User();
                    u.name = (string)reader["UserName"];
                    u.ID = (int)reader["ID"];
                    string saltedPass = (string)reader["Hash"];
                    byte[] saltArray = Convert.FromBase64String((string)reader["Salt"]);
                    Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(password, saltArray, 5000);
                    byte[] enteredHash = rfc.GetBytes(20);

                    if(saltedPass == Convert.ToBase64String(enteredHash))
                    {
                        //correct password
                        ViewBag.Success = "Logged in";
                        return View("SignIn");
                    }
                    else
                    {
                        //incorrect password
                        ViewBag.Error = "Incorrect password";
                        return View("SignIn");
                    }

                }
                else
                {
                    //not found, spit back error
                    reader.Close();
                    connection.Close();
                    ViewBag.Error = "User name not found";
                    return View("SignIn");
                }
            }
            else
            {
                //create new account
                string checkForUsername = @"SELECT * FROM Users WHERE UserName='" + userName + "';";
                SqlConnection connection = SQLGetConnection();
                connection.Open();
                SqlDataReader reader = SQLCommandReader(checkForUsername, connection);

                if (reader.HasRows)
                {
                    //username found,spit back error
                    reader.Close();
                    connection.Close();
                    ViewBag.Error = "User name already taken";
                    return View("SignIn");
                }
                else
                {
                    //not found, proceed
                    reader.Close();
                    connection.Close();

                    //create salt, get salted hash
                    byte[] saltArray;
                    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                    rng.GetBytes(saltArray = new byte[16]);
                    Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(password,saltArray,5000);
                    byte[] hashed = rfc.GetBytes(20);

                    //convert salt and salted hash to base 64 string to store
                    string saltedHash = Convert.ToBase64String(hashed);
                    string salt = Convert.ToBase64String(saltArray);

                    Random rand = new Random();
                    int randID = rand.Next();
                    string newUserCommand = @"INSERT INTO Users(ID,UserName,Salt,Hash) VALUES(" + randID + ",'" + userName + "','" + salt + "','" + saltedHash + "');";
                    connection.Open();
                    SQLNonQuery(newUserCommand, connection);
                    connection.Close();

                    ViewBag.Success = "Successfully created new user!";
                    return View("SignIn");
                }
            }
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

        public List<WorkAssignment> GetWorkAssignments(int User_ID,string dateString)
        {
            string commandWork = @"SELECT * FROM WorkAssignments WHERE U_ID=" + User_ID + @" AND ""Date""='" + dateString + "'";

            SqlConnection connection = SQLGetConnection();
            connection.Open();
            SqlDataReader reader2 = SQLCommandReader(commandWork, connection);

            List<WorkAssignment> waList = new List<WorkAssignment>();

            while (reader2.Read())
            {
                WorkAssignment wa = new WorkAssignment();
                wa.name = (string)reader2["Name"];
                wa.ID = (int)reader2["ID"];
                wa.duration = (int)reader2["Duration"];
                wa.dt = DateTime.Parse((string)reader2["Date"]);
                wa.DueDate_ID = (int)reader2["DueDate_ID"];
                wa.User_ID = (int)reader2["U_ID"];

                waList.Add(wa);
            }

            reader2.Close();
            connection.Close();

            return waList;
        }

        public List<DueDate> GetDueDates(int User_ID,string dateString)
        {
            string commandWork = @"SELECT * FROM DueDates WHERE U_ID=" + User_ID + @" AND ""Date""='" + dateString + "';";

            SqlConnection connection = SQLGetConnection();
            connection.Open();
            SqlDataReader reader2 = SQLCommandReader(commandWork, connection);

            List<DueDate> ddList = new List<DueDate>();

            while (reader2.Read())
            {
                DueDate dd = new DueDate();

                dd.name = (string)reader2["Name"];
                dd.ID = (int)reader2["ID"];
                dd.dt = DateTime.Parse((string)reader2["Date"]+" "+(string)reader2["Time"]);
                dd.UserID = (int)reader2["U_ID"];
                dd.requiredHours = (int)reader2["Required Hours"];

                ddList.Add(dd);
            }

            reader2.Close();
            connection.Close();

            return ddList;
        }
    }

}