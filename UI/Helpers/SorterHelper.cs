using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using UI.Models;

namespace UI.Helpers
{
    public class SorterHelper
    {
        private List<CourseModel> _courseList;
        private List<SectionModel> _sectionList;
        public SorterHelper()
        {

            var pathCour = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/json/course_data.json");
            var pathSec = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/json/section_data.json");
            using (StreamReader srC = new StreamReader(pathCour, Encoding.UTF8, true))
            {
                _courseList = JsonConvert.DeserializeObject<List<CourseModel>>(srC.ReadToEnd());
            }
            using (StreamReader srS = new StreamReader(pathSec, Encoding.UTF8, true))
            {
                _sectionList = JsonConvert.DeserializeObject<List<SectionModel>>(srS.ReadToEnd());
            }
        }

        public List<List<CalendarData>> SortSections(string data)
        {
            var checkCourse = new List<string>();
            var listSections = new List<SectionModel>();
            var result = new List<List<CalendarData>>();
            bool checkOverlap = false;
            if (data != "")
            {
                var getSections = data.Split(',');
                if (getSections[0] == "CheckOverlap")
                {
                    checkOverlap = true;
                    getSections = getSections.Skip(1).ToArray();
                }

                foreach (var item in getSections)
                {
                    var itemCourse = item.Substring(0, 7);
                    var itemSection = item.Substring(7, 1);
                    if (checkCourse.FirstOrDefault(a => a == itemCourse) == null)
                    {
                        checkCourse.Add(itemCourse);

                    }
                    var retrievedSection = _sectionList.FirstOrDefault(a => a.course == itemCourse && a.sectionId == itemSection);
                    if (retrievedSection != null)
                        listSections.Add(retrievedSection);

                }

                result = SectionSortHelper(checkCourse, listSections, checkOverlap);

            }
            return result;
        }


        private List<List<CalendarData>> SectionSortHelper(List<string> checkCourse, List<SectionModel> listSections, bool check)
        {
            var totalList = new List<List<CalendarData>>();
            var list = new List<CalendarData>();

            foreach (var item in checkCourse)
            {
                var itemSecs = listSections.FindAll(a => a.course == item);
                if (itemSecs.Count > 1)
                {
                    var sendList = new List<SectionModel>(listSections);
                    sendList.Remove(itemSecs[0]);
                    totalList.AddRange(SectionSortHelper(checkCourse, sendList, check));

                    listSections.RemoveAll(a => a.course == item);
                    listSections.Add(itemSecs[0]);
                }
            }

            foreach (var item in listSections)
            {
                var coursename = _courseList.FirstOrDefault(a => a.code == item.course).codename;
                foreach (var secdate in item.dateinfo)
                {
                    list.Add(new CalendarData
                    {
                        title = $"{coursename} - {item.sectionId} ({secdate.place})",
                        start = ConverterHelper.HourToDateTime(secdate.day, secdate.start),
                        end = ConverterHelper.HourToDateTime(secdate.day, secdate.end),
                        backgroundColor = "#" + item.course.Substring(0, 6)
                    });
                }
            }

            if (check)
            {
                if (!OverlapChecker(list))
                    totalList.Add(list);
            }
            else
                totalList.Add(list);

            return totalList;
        }

        private bool OverlapChecker(List<CalendarData> dates)
        {
            var searchList = new List<CalendarData>(dates);
            var overlapped = false;

            foreach (var item in dates)
            {
                var itemDate = item.start.Substring(0, 10);
                int itemStartHour = Convert.ToInt32(item.start.Substring(11, 2));
                int itemEndHour = Convert.ToInt32(item.end.Substring(11, 2));
                searchList.Remove(item);
                foreach (var src in searchList)
                {
                    var srcDate = src.start.Substring(0, 10);
                    if(itemDate == srcDate)
                    {
                        int srcStartHour = Convert.ToInt32(src.start.Substring(11, 2));
                        int srcEndHour = Convert.ToInt32(src.end.Substring(11, 2));

                        if (itemStartHour == srcStartHour) { overlapped = true;break; }
                        if (itemEndHour == srcEndHour) { overlapped = true;break; }
                        if (srcStartHour > itemStartHour && srcStartHour < itemEndHour) { overlapped = true;break; }
                        if (srcEndHour > itemStartHour && srcEndHour < itemEndHour) { overlapped = true;break; }
                    }
                }
                if (overlapped)
                    break;
            }

            return overlapped;
        }
    }
}