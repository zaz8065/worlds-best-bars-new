using System.ComponentModel.DataAnnotations;

namespace WorldsBestBars.Web.Models
{
    public class UpdateProfile
    {
        #region Public Properties

        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public string City { get; set; }
        
        public string DateOfBirth { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        public string FavouriteCities { get; set; }
        public string FavouriteBars { get; set; }
        public string FavouriteBrands { get; set; }
        public string FavouriteCocktails { get; set; }

        #endregion
    }
}