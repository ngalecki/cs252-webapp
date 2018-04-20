using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Caldendar.Models
{
    public class Event
    {
        //TODO check for valid entries
        public DateTime dt;
        public int duration { get; set; }
        public string name { get; set; }
    }
}