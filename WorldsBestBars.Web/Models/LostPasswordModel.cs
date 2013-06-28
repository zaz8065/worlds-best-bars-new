using System.ComponentModel.DataAnnotations;

namespace WorldsBestBars.Web.Models
{
    public class LostPasswordModel
    {
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}