using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using UI.Models;
using UI.Helpers;

namespace UI.Controllers
{
    public class HomeController : Controller
    {
        private WebDriverMetu _webmetu;
        private List<DepartmentModel> _departmentList;
        private List<CourseModel> _courseList;
        private List<SectionModel> _sectionList;
        private SorterHelper _sorterHelper;
        public HomeController()
        {
            _webmetu = new WebDriverMetu();
            _sorterHelper = new SorterHelper();
            var pathDep = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/json/department_data.json");
            var pathCour = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/json/course_data.json");
            var pathSec = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/json/section_data.json");
            using (StreamReader srD = new StreamReader(pathDep, Encoding.UTF8, true))
            {
                _departmentList = JsonConvert.DeserializeObject<List<DepartmentModel>>(srD.ReadToEnd());
            }
            using (StreamReader srC = new StreamReader(pathCour, Encoding.UTF8, true))
            {
                _courseList = JsonConvert.DeserializeObject<List<CourseModel>>(srC.ReadToEnd());
            }
            using (StreamReader srS = new StreamReader(pathSec, Encoding.UTF8, true))
            {
                _sectionList = JsonConvert.DeserializeObject<List<SectionModel>>(srS.ReadToEnd());
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PartialAddCourse(int id)
        {
            var courseCode = id.ToString().Substring(0, 7);
            var colorCode = id.ToString().Substring(7, 1);
            var selectedCourse = _courseList.FirstOrDefault(a => a.code == courseCode);
            var getSections = _sectionList.FindAll(a => a.course == courseCode);

            ViewBag.ColorCode = Convert.ToInt32(colorCode)%5;
            ViewBag.CourseCode = selectedCourse.code;
            ViewBag.CourseName = selectedCourse.name;
            ViewBag.CourseCodename = selectedCourse.codename;
            return PartialView("_AddCourse", getSections);
        }

        public ActionResult CreateCalendarData (string data)
        {
            var result = _sorterHelper.SortSections(data);
            var sendResult = new {
                totalResult = result.Count(),
                current = result
            };
            return Json(sendResult,JsonRequestBehavior.AllowGet);
        }

        public JsonResult CourseSelectListData()
        {
            var result = new List<CourseModel>();
            foreach (var item in _courseList)
            {
                if (_sectionList.FirstOrDefault(a => a.course == item.code) != null)
                    result.Add(item);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}
