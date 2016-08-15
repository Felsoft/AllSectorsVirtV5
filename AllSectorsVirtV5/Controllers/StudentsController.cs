using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AllSectorsVirtV5.DAL;
using AllSectorsVirtV5.Models;
using System.Data.Entity.Infrastructure;
using System.Collections.Generic;
using System.Web;
using System;
using PagedList;

namespace AllSectorsVirtV5.Controllers
{
    public class StudentsController : Controller
    {
            private AllSectorsDBContext db = new AllSectorsDBContext();

            public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
            {
                ViewBag.CurrentSort = sortOrder;
                ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

                if (searchString != null)
                {
                    page = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                ViewBag.CurrentFilter = searchString;

                var students = from s in db.Students
                               select s;
                if (!String.IsNullOrEmpty(searchString))
                {
                    students = students.Where(s => s.LastName.Contains(searchString)
                                           || s.FirstMidName.Contains(searchString));
                }
                switch (sortOrder)
                {
                    case "name_desc":
                        students = students.OrderByDescending(s => s.LastName);
                        break;
                    case "Date":
                        students = students.OrderBy(s => s.EnrollmentDate);
                        break;
                    case "date_desc":
                        students = students.OrderByDescending(s => s.EnrollmentDate);
                        break;
                    default:  // Name ascending 
                        students = students.OrderBy(s => s.LastName);
                        break;
                }

                int pageSize = 6;
                int pageNumber = (page ?? 1);
                return View(students.ToPagedList(pageNumber, pageSize));
            }


            // GET: Student/Details/5
            public ActionResult Details(int? id)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Student student = db.Students.Include(s => s.Files).SingleOrDefault(s => s.ID == id);
                if (student == null)
                {
                    return HttpNotFound();
                }
                return View(student);
            }

            // GET: Student/Create
            public ActionResult Create()
            {
                return View();
            }

            // POST: Student/Create

            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Create([Bind(Include = "LastName, FirstMidName, EnrollmentDate")]Student student, HttpPostedFileBase upload)
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (upload != null && upload.ContentLength > 0)
                        {
                            var avatar = new File
                            {
                                FileName = System.IO.Path.GetFileName(upload.FileName),
                                FileType = FileType.Avatar,
                                ContentType = upload.ContentType
                            };
                            using (var reader = new System.IO.BinaryReader(upload.InputStream))
                            {
                                avatar.Content = reader.ReadBytes(upload.ContentLength);
                            }
                            student.Files = new List<File> { avatar };
                        }
                        db.Students.Add(student);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
                return View(student);
            }


            // GET: Student/Edit/5
            public ActionResult Edit(int? id)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Student student = db.Students.Include(s => s.Files).SingleOrDefault(s => s.ID == id);
                if (student == null)
                {
                    return HttpNotFound();
                }
                return View(student);
            }

            // POST: Student/Edit/5

            [HttpPost, ActionName("Edit")]
            [ValidateAntiForgeryToken]
            public ActionResult EditPost(int? id, HttpPostedFileBase upload)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var studentToUpdate = db.Students.Find(id);
                if (TryUpdateModel(studentToUpdate, "",
                   new string[] { "LastName", "FirstMidName", "EnrollmentDate" }))
                {
                    try
                    {
                        if (upload != null && upload.ContentLength > 0)
                        {
                            if (studentToUpdate.Files.Any(f => f.FileType == FileType.Avatar))
                            {
                                db.Files.Remove(studentToUpdate.Files.First(f => f.FileType == FileType.Avatar));
                            }
                            var avatar = new File
                            {
                                FileName = System.IO.Path.GetFileName(upload.FileName),
                                FileType = FileType.Avatar,
                                ContentType = upload.ContentType
                            };
                            using (var reader = new System.IO.BinaryReader(upload.InputStream))
                            {
                                avatar.Content = reader.ReadBytes(upload.ContentLength);
                            }
                            studentToUpdate.Files = new List<File> { avatar };
                        }
                        db.Entry(studentToUpdate).State = EntityState.Modified;
                        db.SaveChanges();

                        return RedirectToAction("Index");
                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        //Log the error (uncomment dex variable name and add a line here to write a log.
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                    }
                }
                return View(studentToUpdate);
            }

            // GET: Student/Delete/5
            public ActionResult Delete(int? id, bool? saveChangesError = false)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                if (saveChangesError.GetValueOrDefault())
                {
                    ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
                }
                Student student = db.Students.Find(id);
                if (student == null)
                {
                    return HttpNotFound();
                }
                return View(student);
            }

            // POST: Student/Delete/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Delete(int id)
            {
                try
                {
                    Student student = db.Students.Find(id);
                    db.Students.Remove(student);
                    db.SaveChanges();
                }
                catch (RetryLimitExceededException/* dex */)
                {
                    //Log the error to be implemented in semester 2
                    return RedirectToAction("Delete", new { id = id, saveChangesError = true });
                }
                return RedirectToAction("Index");
            }
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    db.Dispose();
                }
                base.Dispose(disposing);
            }
        }
}
