using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Caldendar.Models
{
    public class Event
    {
        //TODO check for valid entries
        public int ID { get; set; }
        public int UserID { get; set; }
        public DateTime dt { get; set; }
        public int duration { get; set; }
        public string name { get; set; }

        public string ToSqlInsert()
        {
            string command = @"INSERT INTO Events (ID,U_ID,""Name"",Duration,""Date"",""Time"")
                          VALUES(" + this.ID + "," + this.UserID + ",'" + this.name + "'," + this.duration + ",'" + this.dt.Month+"/"+this.dt.Day+"/"+this.dt.Year + "','" + this.dt.TimeOfDay.ToString() + "');";

            return command;
        }


        public override string ToString()
        {
            return "Name: " + name +
                   "Date: " + dt.Date +
                   "Time: " + dt.TimeOfDay;
        }
    }

}