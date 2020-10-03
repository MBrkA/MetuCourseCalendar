using Newtonsoft.Json;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using UI.Models;

namespace UI.Helpers
{
    public class WebDriverMetu
    {
        public void GetDepartments()
        {
            using (IWebDriver driver = new ChromeDriver())
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                driver.Navigate().GoToUrl("https://oibs2.metu.edu.tr/View_Program_Course_Details_64/main.php");

                var getSelect = driver.FindElement(By.Name("select_dept"));
                var getOptions = getSelect.FindElements(By.TagName("option"));
                var idValues = new List<DepartmentModel>();
                foreach (var item in getOptions)
                {
                    idValues.Add(new DepartmentModel
                    {
                        id = item.GetAttribute("value"),
                        name = item.Text
                    });
                }
                idValues = idValues.Skip(1).OrderBy(a => a.id).ToList();
                var idJson = JsonConvert.SerializeObject(idValues);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "Files/json/department_data.json", idJson);

            }
        }

        public void GetCourses()
        {
            var pathDepartment = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/json/department_data.json");
            var departmentData = new List<DepartmentModel>();
            using (StreamReader srD = new StreamReader(pathDepartment, Encoding.UTF8, true))
            {
                departmentData = JsonConvert.DeserializeObject<List<DepartmentModel>>(srD.ReadToEnd());
            }

            using (IWebDriver driver = new ChromeDriver())
            {
                driver.Navigate().GoToUrl("https://oibs2.metu.edu.tr/View_Program_Course_Details_64/main.php");

                var courseData = new List<CourseModel>();
                var courseCount = 0;
                foreach (var item in departmentData)
                {
                    var courseSelect = driver.FindElement(By.Name("select_dept"));
                    var selectEl = new SelectElement(courseSelect);
                    selectEl.SelectByValue(item.id);
                    driver.FindElement(By.Name("submit_CourseList")).Click();
                    try
                    {
                        var table = driver.FindElement(By.XPath("(//table)[4]"));
                        try
                        {
                            var tableTR = table.FindElements(By.TagName("tr"));
                            courseCount = 0;
                            foreach (var itemTR in tableTR)
                            {
                                if (courseCount == 0) { courseCount++; continue; }
                                var tableTD = itemTR.FindElements(By.TagName("td"));

                                courseData.Add(new CourseModel
                                {
                                    depId = item.id,
                                    code = tableTD[1].Text,
                                    name = tableTD[2].Text,
                                    ects = tableTD[3].Text,
                                    credit = tableTD[4].Text.Substring(0, 4),
                                    level = tableTD[5].Text,
                                    type = tableTD[6].Text
                                });
                            }
                            driver.Navigate().Back();
                        }
                        catch (Exception)
                        {
                            driver.Navigate().Back();
                        }
                    }
                    catch (Exception)
                    {

                    }

                }
                var courseJson = JsonConvert.SerializeObject(courseData);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "Files/json/course_data.json", courseJson);
            }
        }

        public void GetSections()
        {
            using (IWebDriver driver = new ChromeDriver())
            {
                driver.Navigate().GoToUrl("https://oibs2.metu.edu.tr/View_Program_Course_Details_64/main.php");
                var departmentData = new List<DepartmentModel>();
                var courseData = new List<CourseModel>();
                var sectionData = new List<SectionModel>();
                var pathDepartment = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/json/department_data.json");
                var pathCourse = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/json/course_data.json");
                using (StreamReader srD = new StreamReader(pathDepartment, Encoding.UTF8, true))
                {
                    departmentData = JsonConvert.DeserializeObject<List<DepartmentModel>>(srD.ReadToEnd());
                }
                using (StreamReader srC = new StreamReader(pathCourse, Encoding.UTF8, true))
                {
                    courseData = JsonConvert.DeserializeObject<List<CourseModel>>(srC.ReadToEnd());
                }

                foreach (var itemDep in departmentData)
                {
                    var courseSelect = driver.FindElement(By.Name("select_dept"));
                    var selectEl = new SelectElement(courseSelect);
                    selectEl.SelectByValue(itemDep.id);
                    driver.FindElement(By.Name("submit_CourseList")).Click();
                    var depCourse = courseData.FindAll(a => a.depId == itemDep.id);

                    foreach (var itemCour in depCourse)
                    {
                        driver.FindElement(By.XPath($"//input[@name='text_course_code' and @value='{itemCour.code}']")).Click();
                        driver.FindElement(By.Name("SubmitCourseInfo")).Click();
                        try
                        {
                            var allSecTable = driver.FindElements(By.TagName("table"))[2].
                                       FindElements(By.TagName("tr")).Skip(2);
                            var cntTR = 0;
                            var _sectionModel = new SectionModel();
                            foreach (var itemSec in allSecTable)
                            {
                                cntTR = cntTR % 7;
                                if (cntTR == 0)
                                {
                                    _sectionModel = new SectionModel();
                                    _sectionModel.dateinfo = new List<DateInfo>();
                                    _sectionModel.department = itemDep.id;
                                    _sectionModel.course = itemCour.code;
                                    _sectionModel.sectionId = itemSec.FindElement(By.Name("submit_section")).GetAttribute("value");
                                    _sectionModel.instructor = itemSec.FindElements(By.TagName("td"))[1].Text;
                                }
                                else if (cntTR > 1 && cntTR < 7)
                                {
                                    var tdinfo = itemSec.FindElements(By.TagName("td"));
                                    if (tdinfo[0].Text != "")
                                    {
                                        _sectionModel.dateinfo.Add(new DateInfo
                                        {
                                            day = tdinfo[0].Text,
                                            start = tdinfo[1].Text,
                                            end = tdinfo[2].Text,
                                            place = tdinfo[3].Text
                                        });
                                    }
                                    if (cntTR == 6 && _sectionModel.dateinfo.Count != 0)
                                        sectionData.Add(_sectionModel);
                                }
                                cntTR++;
                            }
                        }
                        catch (Exception)
                        {
                        }
                        driver.Navigate().Back();
                    }
                    driver.Navigate().Back();
                }


                var sectionJson = JsonConvert.SerializeObject(sectionData);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "Files/json/section_data.json", sectionJson);

            }
        }

        public void SetCourseCodename()
        {
            var pathCourse = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/json/course_data.json");
            var pathShort = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files/json/department_short.json");
            var courseData = new List<CourseModel>();
            var shortData = new List<DepartmentShort>();
            using (StreamReader srC = new StreamReader(pathCourse, Encoding.UTF8, true))
            {
                courseData = JsonConvert.DeserializeObject<List<CourseModel>>(srC.ReadToEnd());
            }
            using (StreamReader srS = new StreamReader(pathShort, Encoding.UTF8, true))
            {
                shortData = JsonConvert.DeserializeObject<List<DepartmentShort>>(srS.ReadToEnd());
            }
            foreach (var item in courseData)
            {
                var depCode = shortData.FirstOrDefault(a => a.id == item.code.Substring(0, 3));
                item.codename = depCode == null ? "" : depCode.name + item.code.Substring(4, 3);
            }
            var courseJson = JsonConvert.SerializeObject(courseData);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "Files/json/course_data.json", courseJson);
        }
    }

}