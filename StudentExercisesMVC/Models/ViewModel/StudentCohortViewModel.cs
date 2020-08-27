using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentExercisesAPI.Models;

namespace StudentExercisesMVC.Models.ViewModel
{
    public class StudentCohortViewModel
    {
        public Student student { get; set; }

        public List<SelectListItem> cohorts { get; set; } = new List<SelectListItem>();
    }
}
