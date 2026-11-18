
using System.ComponentModel.DataAnnotations;

namespace portfolio.Models
{
    public class ProjectImage
    {
        public long ImageId { get; set; }

        [Required]
        public long ProjectId { get; set; }

        [Required(ErrorMessage = "Image URL is required.")]
        [StringLength(1000)]
        public string ImageUrl { get; set; } = string.Empty;

        [Range(0, int.MaxValue,
            ErrorMessage = "Sort order cannot be negative.")]
        public int SortOrder { get; set; }

        public Project? Project { get; set; }
    }
}
