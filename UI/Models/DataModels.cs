using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.Models
{
    public class DepartmentModel
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class CourseModel
    {
        public string depId { get; set; }
        public string code { get; set; }
        public string codename { get; set; }
        public string name { get; set; }
        public string ects { get; set; }
        public string credit { get; set; }
        public string level { get; set; }
        public string type { get; set; }
    }

    public class SectionModel
    {
        public string department { get; set; }
        public string course { get; set; }
        public string sectionId { get; set; }
        public string instructor { get; set; }
        public List<DateInfo> dateinfo { get; set; }
    }

    public class DateInfo
    {
        public string day { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string place { get; set; }
    }

    public class DepartmentShort
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class CalendarData
    {
        public string title { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string backgroundColor { get; set; }

    }
}