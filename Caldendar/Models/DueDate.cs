using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Caldendar.Models
{
    public class DueDate
    {
        //TODO ensure validity
        public int date { get; set; }
        public int month { get; set; }
        public int hour { get; set; }
        public int minutes { get; set; }
        public string name { get; set; }
    }
}