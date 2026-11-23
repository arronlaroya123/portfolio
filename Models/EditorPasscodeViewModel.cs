
using System.ComponentModel.DataAnnotations;

namespace portfolio.Models
{
    public class EditorPasscodeViewModel
    {
        [Required(ErrorMessage = "Please enter your passcode.")]
        public string Passcode { get; set; } = "";
    }
}
