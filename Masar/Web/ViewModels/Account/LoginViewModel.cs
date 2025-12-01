using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="Email Is Required")]
        [EmailAddress(ErrorMessage ="This Email is Not Valid")]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
