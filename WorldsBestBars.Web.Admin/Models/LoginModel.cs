using System.ComponentModel.DataAnnotations;

namespace WorldsBestBars.Web.Admin.Models
{
    public class LoginModel
    {
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}