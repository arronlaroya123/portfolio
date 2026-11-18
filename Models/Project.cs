
using System.ComponentModel.DataAnnotations;

namespace portfolio.Models
{
    public class Project
    {
        public long ProjectId { get; set; }

        [Required(ErrorMessage = "Please select a course.")]
        public long CourseId { get; set; }

        [Required(ErrorMessage = "Project title is required.")]
        [StringLength(150)]
        public string ProjectTitle { get; set; } = string.Empty;

        public string? Description { get; set; }

        [StringLength(500)]
        [RegularExpression(@"^$|^https?://.+",
            ErrorMessage = "Enter a valid video URL.")]
        public string? VideoUrl { get; set; }

        public Course? Course { get; set; }

        public List<ProjectImage> Images { get; set; } = new();
    }
}
