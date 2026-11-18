
using System.ComponentModel.DataAnnotations;

namespace portfolio.Models
{
    public class ProjectEditViewModel
    {
        public long ProjectId { get; set; }

        [Required(ErrorMessage = "Please select a course.")]
        [Range(1, long.MaxValue, ErrorMessage = "Please select a valid course.")]
        public long CourseId { get; set; }

        [Required(ErrorMessage = "Project title is required.")]
        [StringLength(150)]
        public string ProjectTitle { get; set; } = string.Empty;

        public string? Description { get; set; }

        [StringLength(500)]
        [RegularExpression(@"^$|^https?://[^\s]+$",
            ErrorMessage = "Enter a valid video URL.")]
        public string? VideoUrl { get; set; }

        [Required(ErrorMessage = "Enter at least one image URL.")]
        public string ImageUrls { get; set; } = string.Empty;
    }
}
