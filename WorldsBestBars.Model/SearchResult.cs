using System;
using System.Linq;

using Newtonsoft.Json;

namespace WorldsBestBars.Model
{
	[Serializable()]
	[JsonObject()]
	public class SearchResult : DatabaseObject
	{
		[JsonProperty("text")]
		public string DisplayText { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("score")]
		public double Score { get; set; }

		[JsonProperty("distance")]
		public double? Distance { get; set; }
	}
}
