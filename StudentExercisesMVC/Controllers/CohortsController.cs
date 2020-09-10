using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using StudentExercisesMVC.Models;

namespace StudentExercisesMVC.Controllers
{
    public class CohortsController : Controller
    {
        private readonly IConfiguration _config;

        public CohortsController(IConfiguration config)
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

        // GET: CohortsController
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT c.Id AS CohortID, c.Name, 
                                               s.Id AS StudentID, i.Id AS InstructorID
                                        FROM Cohort c LEFT JOIN Student s ON c.Id = s.CohortId LEFT JOIN Instructor i ON c.Id = i.CohortId
                                    ";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Cohort> cohorts = new List<Cohort>();

                    while (reader.Read())
                    {
                        Cohort cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("CohortID")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };



                        if (cohorts.Any(c => c.Id == cohort.Id) == false)
                        {
                            if (cohort.listOfStudents.Any(stud => stud.Id == reader.GetInt32(reader.GetOrdinal("StudentID"))) == false && !reader.IsDBNull(reader.GetOrdinal("StudentID")))
                            {
                                Student student = new Student
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentID"))
                                };

                                cohort.listOfStudents.Add(student);
                            }

                            if (cohort.listOfInstructors.Any(i => i.Id == reader.GetInt32(reader.GetOrdinal("InstructorID"))) == false && !reader.IsDBNull(reader.GetOrdinal("InstructorID")))
                            {
                                Instructor instructor = new Instructor
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorID"))
                                };

                                cohort.listOfInstructors.Add(instructor);
                            }

                            cohorts.Add(cohort);
                        }
                        else
                        {
                            if (cohorts.FirstOrDefault(c => c.Id == cohort.Id).listOfStudents.Any(stud => stud.Id == reader.GetInt32(reader.GetOrdinal("StudentID"))) == false && !reader.IsDBNull(reader.GetOrdinal("StudentID")))
                            {
                                Student student = new Student
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentID"))
                                };

                                cohorts.FirstOrDefault(c => c.Id == cohort.Id).listOfStudents.Add(student);

                            }

                            if (cohorts.FirstOrDefault(c => c.Id == cohort.Id).listOfInstructors.Any(i => i.Id == reader.GetInt32(reader.GetOrdinal("InstructorID"))) == false && !reader.IsDBNull(reader.GetOrdinal("InstructorID")))
                            {
                                Instructor instructor = new Instructor
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorID")),
                                };

                                cohorts.FirstOrDefault(c => c.Id == cohort.Id).listOfInstructors.Add(instructor);
                            }
                        }
                    }

                    reader.Close();

                    return View(cohorts);
                }
            }
        }

        // GET: CohortsController/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT c.Id,
                                            c.Name
                                        FROM Cohort c
                                        WHERE c.Id = @id
                                    ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = null;

                    if (reader.Read())
                    {
                        cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };

                    }

                    reader.Close();

                    if (cohort != null)
                    {
                        return View(cohort);
                    }
                    else
                    {
                        return RedirectToAction(nameof(NotFound));
                    }
                }
            }
        }

        // GET: CohortsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CohortsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Cohort cohort)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Cohort
                                            ( Name )
                                            VALUES
                                            ( @name )";
                        cmd.Parameters.Add(new SqlParameter("@name", cohort.Name));

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

        // GET: CohortsController/Edit/5
        public ActionResult Edit(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT c.Id,
                                            c.Name
                                        FROM Cohort c
                                        WHERE c.Id = @id
                                    ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = null;

                    if (reader.Read())
                    {
                        cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };

                    }

                    reader.Close();

                    if (cohort != null)
                    {
                        return View(cohort);
                    }
                    else
                    {
                        return RedirectToAction(nameof(NotFound));
                    }
                }
            }
        }

        // POST: CohortsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Cohort cohort)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Cohort
                                            SET Name = @name
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@name", cohort.Name));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(cohort);
            }
        }

        // GET: CohortsController/Delete/5
        public ActionResult Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT c.Id,
                                            c.Name
                                        FROM Cohort c
                                        WHERE c.Id = @id
                                    ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = null;

                    if (reader.Read())
                    {
                        cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };

                    }

                    reader.Close();

                    if (cohort != null)
                    {
                        return View(cohort);
                    }
                    else
                    {
                        return RedirectToAction(nameof(NotFound));
                    }
                }
            }
        }

        // POST: CohortsController/Delete/5
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
                        cmd.CommandText = @"DELETE FROM Cohort WHERE Id = @id";
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
