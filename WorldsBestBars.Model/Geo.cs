using System;

using Newtonsoft.Json;


namespace WorldsBestBars.Model
{
    [JsonObject()]
    public class Geo
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("long")]
        public double Long { get; set; }

        public double DistanceTo(Geo other)
        {
            return HaversineDistance(this, other, DistanceUnit.Miles);
        }

        private static double HaversineDistance(Geo pos1, Geo pos2, DistanceUnit unit)
        {
            double R = (unit == DistanceUnit.Miles) ? 3960 : 6371;
            var lat = ConvertToRadians(pos2.Lat - pos1.Lat);
            var lng = ConvertToRadians(pos2.Long - pos1.Long);
            var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
                          Math.Cos(ConvertToRadians(pos1.Lat)) * Math.Cos(ConvertToRadians(pos2.Lat)) *
                          Math.Sin(lng / 2) * Math.Sin(lng / 2);
            var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
            return R * h2;
        }

        private enum DistanceUnit { Miles, Kilometers };

        private static double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
    }
}