using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Caldendar.Models
{
    public class WorkAssignment
    {
        public int ID { get; set; }
        public int DueDate_ID { get; set; }
        public int User_ID { get; set; }
        public string name { get; set; }
        public double duration { get; set; }
        public DateTime dt { get; set; }

        public string ToSqlInsert()
        {
            string command = @"INSERT INTO WorkAssignments (ID,U_ID,DueDate_ID,""Name"",Duration,""Date"")
                          VALUES(" + this.ID + "," +this.User_ID+ ","+ this.DueDate_ID + ",'" + this.name + "'," + this.duration + ",'" + this.dt.Month + "/" + this.dt.Day + "/" + this.dt.Year + "');";

            return command;
        }
    }
}