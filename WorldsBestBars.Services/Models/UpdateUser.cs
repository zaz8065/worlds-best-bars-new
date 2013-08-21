using System;

namespace WorldsBestBars.Services.Models
{
    public class UpdateUser
    {
        #region Public Properties

        public string Name { get; set; }
        public string Email { get; set; }
        public string City { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string Password { get; set; }

        public string FavouriteCities { get; set; }
        public string FavouriteBars { get; set; }
        public string FavouriteBrands { get; set; }
        public string FavouriteCocktails { get; set; }

        #endregion
    }
}
