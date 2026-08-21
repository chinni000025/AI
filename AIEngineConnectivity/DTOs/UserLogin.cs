using System.ComponentModel.DataAnnotations;

namespace AIEngineConnectivity.DTOs
{
    public class UserLogin
    {
        [Required(ErrorMessage = "UserName is Required")]
        [MaxLength(100)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is Required")]
        public string Password { get; set; }

        public string SessionId { get; set; }
    }
}
