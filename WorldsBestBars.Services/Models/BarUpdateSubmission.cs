using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WorldsBestBars.Services.Models
{
    public class BarUpdateSubmission
    {
        public Guid Id { get; set; }
        public Guid BarId { get; set; }

        public string Name { get; set; }
        public DateTime Created { get; set; }

        public EditBarContainer Updates { get; set; }
    }

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
    }
}
