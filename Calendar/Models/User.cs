using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Caldendar.Models
{
    public class User
    {
        public int ID { get; set; }
        public string name;
        public int mondayAvailableTime { get; set; }
        public int tuesdayAvailableTime { get; set; }
        public int wednesdayAvailableTime { get; set; }
        public int thursdayAvailableTime { get; set; }
        public int fridayAvailableTime { get; set; }
        public int saturdayAvailableTime { get; set; }
        public int sundayAvailableTime { get; set; }
    }
}