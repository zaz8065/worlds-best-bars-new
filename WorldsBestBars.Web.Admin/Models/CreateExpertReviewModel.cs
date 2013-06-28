using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorldsBestBars.Web.Admin.Models
{
    public class CreateExpertReviewModel
    {
        public Guid Bar { get; set; }
        public string Comment { get; set; }
    }
}