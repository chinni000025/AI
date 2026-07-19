
namespace AIEngineConnectivity.DTOs
{
    using System.ComponentModel.DataAnnotations;
    public class UserRegister
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Email { get; set; }

		[Required]
		[MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
		[MaxLength(64, ErrorMessage = "Password must be atmost 64 characters long")]
		[RegularExpression(
				@"^(?=[a-zA-Z0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).+$",
				ErrorMessage = "Password must contain uppercase, lowercase, number, special character, and must not start with a special character")]
		public string Password { get; set; }

		[Required]
        [Compare("Password", ErrorMessage = "Password and Confirm Password did not match")]
        public string ConfirmPassword { get; set; }
    }
}
