using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Caldendar.Models
{
    public class DueDate
    {
        //TODO ensure validity
        public int ID { get; set; }
        public int UserID { get; set; }
        public DateTime dt { get; set; }
        public string name { get; set; }
        public int requiredHours { get; set; }

        public string ToSqlInsert()
        {
            string command = @"INSERT INTO DueDates (ID,U_ID,""Name"",""Required Hours"",""Date"",""Time"")
                          VALUES(" + this.ID + "," + this.UserID + ",'" + this.name + "'," + this.requiredHours + ",'" + this.dt.Month + "/" + this.dt.Day + "/" + this.dt.Year + "','" + this.dt.TimeOfDay.ToString() + "');";

            return command;
        }
    }
}