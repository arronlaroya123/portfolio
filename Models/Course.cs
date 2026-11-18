
using System.ComponentModel.DataAnnotations;

namespace portfolio.Models
{
    public class Course
    {
        public long CourseId { get; set; }

        [Required(ErrorMessage = "Course code is required.")]
        [StringLength(20)]
        [RegularExpression(@"^[A-Za-z0-9_-]+$",
            ErrorMessage = "Course code can only contain letters, numbers, hyphens, and underscores.")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course title is required.")]
        [StringLength(120)]
        public string CourseTitle { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
