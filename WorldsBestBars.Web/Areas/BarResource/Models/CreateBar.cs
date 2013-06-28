using System;
using System.ComponentModel.DataAnnotations;

namespace WorldsBestBars.Web.Areas.BarResource.Models
{
    [Serializable]
    public class CreateBar
    {
        [Required(ErrorMessage = "We can't do anything without the name of your bar...")]
        public string Name { get; set; }

        [Required(ErrorMessage = "We won't know what to call you if you don't give us your name...")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "How will we tell you your bar is one of the best if we can't get on to you?"), DataType(DataType.EmailAddress)]
        public string ContactEmail { get; set; }

        [Required(ErrorMessage = "Surely there must be something that puts your bar among the best...")]
        public string Reasoning { get; set; }

        [Required(ErrorMessage = "Not much use knowing about your bar if you don't tell us where it is...")]
        public string Location { get; set; }
    }
}