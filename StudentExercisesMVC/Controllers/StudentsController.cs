using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

using StudentExercisesMVC.Models;

using StudentExercisesMVC.Models.ViewModel;

namespace StudentExercisesMVC.Controllers
{
    public class StudentsController : Controller
    {
        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // GET: StudentsController
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT s.Id,
                                            s.FirstName,
                                            s.LastName,
                                            s.SlackHandle,
                                            s.CohortId, c.Name
                                        FROM Student s LEFT JOIN Cohort c ON c.Id = s.CohortId
                                    ";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Student> students = new List<Student>();
                    while (reader.Read())
                    {
                        Student student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort { Name = reader.GetString(reader.GetOrdinal("Name"))}
                        };

                        students.Add(student);
                    }

                    reader.Close();

                    return View(students);
                }
            }
        }

        // GET: StudentsController/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                    SELECT s.Id AS StudentID,
                                        s.FirstName,
                                        s.LastName,
                                        s.SlackHandle,
                                        s.CohortId, c.Name AS CohortName, e.Id AS ExerciseID, e.Name AS ExerciseName, e.Language
                                    FROM Student s LEFT JOIN Cohort c ON c.Id = s.CohortId LEFT JOIN AssignedExercise a ON a.StudentId = s.Id LEFT JOIN Exercise e ON e.Id = a.ExerciseId
                                WHERE s.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;

                    while (reader.Read())
                    {
                        if (student == null)
                        {
                            student = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohort { Name = reader.GetString(reader.GetOrdinal("CohortName")) }
                            };

                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("ExerciseID")))
                        {
                                Exercise exercise = new Exercise
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseID")),
                                    Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                    Language = reader.GetString(reader.GetOrdinal("Language"))
                                };

                                student.assignedExercises.Add(exercise);
                        }
                        
                    }

                    reader.Close();

                    if (student != null)
                    {
                        return View(student);
                    }
                    else
                    {
                        return RedirectToAction(nameof(NotFound));
                    }
                }
            }
        }

        // GET: StudentsController/Create
        public ActionResult Create()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Cohort.Id, Cohort.Name FROM Cohort";

                    SqlDataReader reader = cmd.ExecuteReader();

                    // Create a new instance of our view model
                    StudentCohortViewModel viewModel = new StudentCohortViewModel();
                    while (reader.Read())
                    {
                        // Map the raw data to our cohort model
                        Cohort cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };

                        // Use the info to build our SelectListItem
                        SelectListItem cohortOptionTag = new SelectListItem()
                        {
                            Text = cohort.Name,
                            Value = cohort.Id.ToString()
                        };

                        // Add the select list item to our list of dropdown options
                        viewModel.cohorts.Add(cohortOptionTag);

                    }

                    reader.Close();


                    // send it all to the view
                    return View(viewModel);
                }
            }
        }

        // POST: StudentsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentCohortViewModel viewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Student
                ( FirstName, LastName, SlackHandle, CohortId )
                VALUES
                ( @firstName, @lastName, @slackHandle, @cohortId )";
                        cmd.Parameters.Add(new SqlParameter("@firstName", viewModel.student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", viewModel.student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", viewModel.student.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", viewModel.student.CohortId));
                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: StudentsController/Edit/5
        public ActionResult Edit(int id)
        {
            StudentCohortViewModel viewModel = new StudentCohortViewModel();
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                    SELECT s.Id,
                                        s.FirstName,
                                        s.LastName,
                                        s.SlackHandle,
                                        s.CohortId
                                    FROM Student s
                                WHERE s.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    viewModel.student = null;

                    if (reader.Read())
                    {
                        viewModel.student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                        };
                    }

                    reader.Close();

                }

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Cohort.Id, Cohort.Name FROM Cohort";

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Cohort cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };

                        SelectListItem cohortOptionTag = new SelectListItem()
                        {
                            Text = cohort.Name,
                            Value = cohort.Id.ToString()
                        };

                        viewModel.cohorts.Add(cohortOptionTag);
                    }

                    reader.Close();
                }

                if (viewModel.student != null)
                {
                    return View(viewModel);
                }
                else
                {
                    return RedirectToAction(nameof(NotFound));
                }
            }
        }

        // POST: StudentsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, StudentCohortViewModel viewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        // When we submit the edit form, go ahead and update the db with the info we passed in
                        cmd.CommandText = @"UPDATE Student
                                            SET firstName=@firstName, 
                                            lastName=@lastName, 
                                            slackHandle=@slackHandle, 
                                            cohortId=@cohortId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@firstName", viewModel.student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", viewModel.student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", viewModel.student.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", viewModel.student.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();


                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(viewModel.student);
            }
        }

        // GET: StudentsController/Delete/5
        public ActionResult Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = @"
                                        SELECT s.Id,
                                            s.FirstName,
                                            s.LastName,
                                            s.SlackHandle,
                                            s.CohortId
                                        FROM Student s
                                        WHERE id = @id
                                    ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;
                    if (reader.Read())
                    {
                        student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                        };

                    }

                    reader.Close();

                    if (student != null)
                    {
                        return View(student);
                    }
                    else
                    {
                        return RedirectToAction(nameof(NotFound));
                    }

                }
            }
        }

        // POST: StudentsController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM AssignedExercise WHERE studentId = @id
                        DELETE FROM Student WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();

                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public new ActionResult NotFound()
        {
            return View();
        }

    }
}
