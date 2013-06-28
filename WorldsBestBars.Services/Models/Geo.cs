using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldsBestBars.Services.Models
{
    public class Geo
    {
        public double Lat { get; set; }
        public double Long { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, {1}", Lat, Long);
        }

        public static Geo FromString(string input)
        {
            if (input == null) { return null; }

            try
            {
                var parts = input.Split(',').Select(e => double.Parse(e));

                return new Geo
                {
                    Lat = parts.ElementAt(0),
                    Long = parts.ElementAt(1)
                };

            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}
