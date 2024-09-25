using System.ComponentModel.DataAnnotations;

namespace ST10140587_CLDV6212_Part2.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }  // Optional field for remembering the login session
    }
}
