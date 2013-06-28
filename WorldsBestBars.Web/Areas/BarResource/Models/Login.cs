using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WorldsBestBars.Web.Areas.BarResource.Models
{
    public class Login
    {
        [Required, DisplayName("Login")]
        public string Username { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}