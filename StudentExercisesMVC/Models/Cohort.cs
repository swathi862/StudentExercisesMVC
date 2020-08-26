using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesAPI.Models
{
    public class Cohort
    {
        public int Id { get; set; }
        [Required]
        [StringLength(11, MinimumLength = 5)]
        [RegularExpression(@"([Dd]ay|[Ee]vening|[Cc]ohort)\s?[0-9]{1,2}", ErrorMessage = "Cohort name should be in the format of [Day|Evening|Cohort] [number]")]
        public string Name { get; set; }
        public List<Student> listOfStudents { get; set; } = new List<Student>();
        public List<Instructor> listOfInstructors { get; set; } = new List<Instructor>();
    }
}
