using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.Helpers
{
    public class ConverterHelper
    {
        public static string HourToDateTime(string getDay,string getHour)
        {
            var today = DateTime.Now;
            var todayInWeek = Convert.ToInt32(today.DayOfWeek);
            if (todayInWeek == 0)
                today = today.AddDays(-todayInWeek - 6);
            else
                today = today.AddDays(-todayInWeek + 1);

            switch (getDay)
            {
                case "Tuesday":
                    today = today.AddDays(1);
                    break;
                case "Wednesday":
                    today = today.AddDays(2);
                    break;
                case "Thursday":
                    today = today.AddDays(3);
                    break;
                case "Friday":
                    today = today.AddDays(4);
                    break;
                default:
                    break;
            }

            var date = today.ToString("yyyy-MM-dd");
            var hour = getHour.Substring(0, 2);
            var minute = getHour.Substring(3, 2);

            return $"{date}T{hour}:{minute}:00";
        }
    }
}