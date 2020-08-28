using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;

using StudentExercisesAPI.Models;
using StudentExercisesMVC.Models.ViewModel;

namespace StudentExercisesMVC.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly IConfiguration _config;

        public InstructorsController(IConfiguration config)
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

        // GET: InstructorsController
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                    SELECT i.Id,
                                        i.FirstName,
                                        i.LastName,
                                        i.SlackHandle,
                                        i.CohortId,
                                        i.Speciality
                                    FROM Instructor i
                                ";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Instructor> instructors = new List<Instructor>();
                    while (reader.Read())
                    {
                        Instructor instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Specialty = reader.GetString(reader.GetOrdinal("Speciality"))
                        };

                        instructors.Add(instructor);
                    }

                    reader.Close();

                    return View(instructors);
                }
            }
        }

        // GET: InstructorsController/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                    SELECT i.Id,
                                        i.FirstName,
                                        i.LastName,
                                        i.SlackHandle,
                                        i.CohortId,
                                        i.Speciality
                                    FROM Instructor i
                                WHERE i.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Instructor instructor = null;

                    if(reader.Read())
                    {
                        instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Specialty = reader.GetString(reader.GetOrdinal("Speciality"))
                        };
                    }

                    reader.Close();

                    if (instructor != null)
                    {
                        return View(instructor);
                    }
                    else
                    {
                        return RedirectToAction(nameof(NotFound));
                    }
                }
            }
        }

        // GET: InstructorsController/Create
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
                    InstructorCohortViewModel viewModel = new InstructorCohortViewModel();
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

        // POST: InstructorsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(InstructorCohortViewModel viewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Instructor
                ( FirstName, LastName, SlackHandle, CohortId, Speciality )
                VALUES
                ( @firstName, @lastName, @slackHandle, @cohortId, @speciality )";
                        cmd.Parameters.Add(new SqlParameter("@firstName", viewModel.instructor.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", viewModel.instructor.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", viewModel.instructor.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", viewModel.instructor.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@speciality", viewModel.instructor.Specialty));

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

        // GET: InstructorsController/Edit/5
        public ActionResult Edit(int id)
        {
            InstructorCohortViewModel viewModel = new InstructorCohortViewModel();
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                    SELECT i.Id,
                                        i.FirstName,
                                        i.LastName,
                                        i.SlackHandle,
                                        i.CohortId,
                                        i.Speciality
                                    FROM Instructor i
                                WHERE i.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    viewModel.instructor = null;

                    if (reader.Read())
                    {
                        viewModel.instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Specialty = reader.GetString(reader.GetOrdinal("Speciality"))
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

                if (viewModel.instructor != null)
                {
                    return View(viewModel);
                }
                else
                {
                    return RedirectToAction(nameof(NotFound));
                }
            }
        }

        // POST: InstructorsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, InstructorCohortViewModel viewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Instructor
                                            SET FirstName = @firstName,
                                                LastName = @lastName,
                                                SlackHandle = @slack,
                                                CohortId = @cohortId,
                                                Speciality = @speciality
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@firstName", viewModel.instructor.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", viewModel.instructor.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slack", viewModel.instructor.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", viewModel.instructor.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@speciality", viewModel.instructor.Specialty));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(viewModel.instructor);
            }

        }

        // GET: InstructorsController/Delete/5
        public ActionResult Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                    SELECT i.Id,
                                        i.FirstName,
                                        i.LastName,
                                        i.SlackHandle,
                                        i.CohortId,
                                        i.Speciality
                                    FROM Instructor i
                                WHERE i.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Instructor instructor = null;

                    if (reader.Read())
                    {
                        instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Specialty = reader.GetString(reader.GetOrdinal("Speciality"))
                        };
                    }

                    reader.Close();

                    if (instructor != null)
                    {
                        return View(instructor);
                    }
                    else
                    {
                        return RedirectToAction(nameof(NotFound));
                    }
                }
            }
        }

        // POST: InstructorsController/Delete/5
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
                        cmd.CommandText = @"DELETE FROM Instructor WHERE Id = @id";
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
