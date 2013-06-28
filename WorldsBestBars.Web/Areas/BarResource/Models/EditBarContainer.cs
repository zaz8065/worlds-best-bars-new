using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace WorldsBestBars.Web.Areas.BarResource.Models
{
    [Serializable]
    public class EditBarContainer
    {
        public Guid Id { get; set; }
        public Guid? BarId { get; set; }
        public string Name { get; set; }
        public bool IsNew { get; set; }
        public bool IsModerated { get; set; }

        public EditBarPictures Pictures { get; set; }
        public EditBarOpenHours OpeningHours { get; set; }
        public EditBarMenus Menus { get; set; }
        public EditBarContact Contact { get; set; }
        public EditBarTypeOfBar TypeOfBar { get; set; }
        public EditBarFeatures Features { get; set; }
        public EditSignatureCocktail SignatureCocktail { get; set; }

        public EditBarContainer()
        {
            Pictures = new EditBarPictures();
            OpeningHours = new EditBarOpenHours();
            Menus = new EditBarMenus();
            Contact = new EditBarContact();
            TypeOfBar = new EditBarTypeOfBar();
            Features = new EditBarFeatures();
            SignatureCocktail = new EditSignatureCocktail();
        }
    }

    [Serializable]
    public class EditBarPictures
    {
        [XmlArray]
        public List<string> Pending { get; set; }
        [XmlArray]
        public List<string> Existing { get; set; }
    }

    [Serializable]
    public class EditBarOpenHours
    {
        [Serializable]
        public class Day
        {
            public string Name { get; set; }

            [Serializable]
            public class TimeFrame
            {
                public string Open { get; set; }
                public string Close { get; set; }
            }

            [XmlArray]
            public List<TimeFrame> TimeFrames { get; set; }
        }

        [XmlArray]
        public List<Day> Days { get; set; }
    }

    [Serializable]
    public class EditBarMenus
    {
        public string MenuUrl { get; set; }
        [XmlArray]
        public List<string> Files { get; set; }
    }

    [Serializable]
    public class EditBarContact
    {
        public string Website { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Geo { get; set; }
    }

    [Serializable]
    public class EditBarTypeOfBar
    {
        [XmlArray]
        public string[] Styles { get; set; }
        [XmlArray]
        public string[] Specialities { get; set; }
        [XmlArray]
        public string[] Atmosphere { get; set; }
    }

    [Serializable]
    public class EditBarFeatures
    {
        [Serializable]
        public class Feature
        {
            [Required]
            public string Title { get; set; }
            public string Image { get; set; }
            public string ImagePhysical { get; set; }
            public string Description { get; set; }
            public Guid Id { get; set; }
        }

        [XmlArray]
        public List<Feature> List { get; set; }
    }

    [Serializable]
    public class EditSignatureCocktail
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Ingredients { get; set; }
    }

    public enum BarTypeCategory { Style, GoodFor, Atmosphere }

    public class BarType
    {
        public BarTypeCategory Category { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public static BarType[] Types = new BarType[] {
                new BarType { Category = BarTypeCategory.Style, Name = "Hotel Bar", Description = "Be it a grand Victorian pile or a hip, city boutique, is your bar in any way affiliated with a hotel?" },
                new BarType { Category = BarTypeCategory.Style, Name = "Amazing Views", Description = "Does the view from your bar pack as much of a punch as your drinks? Rooftop terrace? Floor to ceiling windows? Seascape or cityscape, or maybe even both?" },
                new BarType { Category = BarTypeCategory.Style, Name = "Speakeasy", Description = "Does your bar exude a prohibition-era vibe with drinks to match? Do you ahve an unmarked door? A secret password? Do you favour teacups over tumblers?" },
                new BarType { Category = BarTypeCategory.Style, Name = "Timeless", Description = "Has your bar stood the test of time? Have numerous bar trends come and gone while you've stayed just as you are? Was Ernest Hemingway once a regular habitué of your establishment?" },
                new BarType { Category = BarTypeCategory.Style, Name = "Club", Description = "Are dance floors and DJs an integral part of your establishment? Is your sound-system state of the art?" },
                new BarType { Category = BarTypeCategory.Style, Name = "Tiki", Description = "Is your bar a shrine to all things Tiki? Do your walls overflow with Polynesian paraphernalia? Do you mix a killer Zombie?" },
                new BarType { Category = BarTypeCategory.Style, Name = "Something a little different", Description = "Does your bar have a unique selling point? A particular quirk of location, history or design that sets you apart from the pack?" },

                new BarType { Category = BarTypeCategory.GoodFor, Name = "Cocktails", Description = "Is your bar an exponent of the art of mixology? Is your cocktail list as thick as a phone book? Do you hand-shave your ice?" },
                new BarType { Category = BarTypeCategory.GoodFor, Name = "Wine", Description = "Do you have a particularly well-stocked cellar? A large range of wines by the bottle and glass? Do your staff know everything there is to know about the ways of the grape?" },
                new BarType { Category = BarTypeCategory.GoodFor, Name = "Beer", Description = "Does your bar take beer seriously? Does your drinks list abound with craft ales, imports and other rarities?" },
                new BarType { Category = BarTypeCategory.GoodFor, Name = "Live music", Description = "Do you regularly play host to big name acts or local bands? A spot of late night piano or some weekend jazz?" },
                new BarType { Category = BarTypeCategory.GoodFor, Name = "Food", Description = "Is the skill in the kitchen integral to your bar? Are your bar snacks far, far more than mere beer fodder? Is your menu a draw in its own right?" },
                new BarType { Category = BarTypeCategory.GoodFor, Name = "Open Late", Description = "Do you stay open into the small hours? Is your bar an after-party hot spot?" },                
                new BarType { Category = BarTypeCategory.GoodFor, Name = "Post work", Description = "Is your bar the kind of place where people can go to unwind after a hard day at the office? Is the atmosphere relaxing but with a bit of a buzz?" },
                new BarType { Category = BarTypeCategory.GoodFor, Name = "Business ", Description = "Is your bar the kind of place guaranteed to impress a boss or a business client? If there are deals to be made is your venue the place to make them?" },
                new BarType { Category = BarTypeCategory.GoodFor, Name = "Date", Description = "Is your bar romantic in atmosphere? Low-lit and rich in character? The kind of place you could go for an intimate night out with a current (or prospective) partner?" },
                new BarType { Category = BarTypeCategory.GoodFor, Name = "Sport", Description = "Does your bar boast a big screen or two? Is it the kind of place where people gather to cheer on their team or watch the weekend’s big game?" },
                new BarType { Category = BarTypeCategory.GoodFor, Name = "Big groups", Description = "Is your bar willing and able to accommodate large parties? Is it the kind of place where a group of lads or girls could have a night to remember?" },
                
                new BarType { Category = BarTypeCategory.Atmosphere, Name = "Sophisticated", Description = "Is your bar elegant in both décor and atmosphere? A stylish and cosmopolitan venue with a more mature crowd?" },
                new BarType { Category = BarTypeCategory.Atmosphere, Name = "Laid Back", Description = "Is your bar easy-going? A place people don't need to worry about anything other than enjoying themselves, where you can escape the stresses of the city?" },
                new BarType { Category = BarTypeCategory.Atmosphere, Name = "Intimate", Description = "Is your bar cosy and welcoming? The kind of place where you might take a date or catch up with an old friend; where conversation is more important than the background buzz?" },
                new BarType { Category = BarTypeCategory.Atmosphere, Name = "Lively", Description = "Is your bar the sort where people can come when they feel in the mood to party? Is it the place to hit when you want to make the night go with a bang?" },
                new BarType { Category = BarTypeCategory.Atmosphere, Name = "Contemporary", Description = "Is your bar innovative in terms of design or drinks? Is it a modern, urban space that reflects the latest trends?" },
                new BarType { Category = BarTypeCategory.Atmosphere, Name = "Classic", Description = "Is your bar a place of tradition and history? Do you celebrate old fashioned values and service?" }
            };
    }
}