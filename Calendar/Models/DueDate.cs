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
    }
}