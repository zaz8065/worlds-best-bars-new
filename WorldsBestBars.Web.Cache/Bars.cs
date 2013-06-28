using System;
using System.Collections.Generic;
using System.Linq;

namespace WorldsBestBars.Web.Cache
{
	public class Bars : Base<Model.Bar>
	{
		public Bars() : base() { }

		static Bars instance = new Bars();
		public static Bars Instance { get { return instance; } }

		protected override void Populate()
		{
			Add(Model.Bar.GetAll());

			foreach (var bar in GetAll())
			{
				bar.Lists = Cache.Lists.Instance.GetByBar(bar.Id);
			}
		}

		public override void RefreshEntity(Guid id)
		{
			var bar = Model.Bar.GetById(id);

			if (bar != null)
			{
				bar.Lists = Cache.Lists.Instance.GetByBar(bar.Id);
			}

			if (Contains(id))
			{
				if (bar == null)
				{
					// remove
					if (Contains(id))
					{
						Remove(id);
					}
				}
				else
				{
					// update
					Update(id, bar);
				}
			}
			else
			{
				if (bar != null)
				{
					// add
					Add(bar);
				}
			}

			if (bar == null || !bar.IsActive)
			{
				Search.Lucene.DeleteFromIndex(id);
			}
			else
			{
				Search.Lucene.UpdateIndex(bar);
			}
		}

		public Model.Bar[] GetByParent(Guid? location)
		{
			return GetAll().Where(b => (location == null && b.Parent == null) || (b.Parent != null && b.Parent.Id == location)).ToArray();
		}

		public Model.Bar[] GetByBounds(double north, double south, double east, double west)
		{
			return GetAll().Where(b => b.Geo != null &&
				(b.Geo.Long > west &&
				b.Geo.Long < east &&
				b.Geo.Lat < south &&
				b.Geo.Lat > north)).ToArray();
		}

		public Model.Bar[] GetByParentRecursive(Guid? location)
		{
			var result = new List<Model.Bar>();

			result.AddRange(GetAll().Where(b => (location == null && b.Parent == null) || (b.Parent != null && b.Parent.Id == location)).ToArray());

			if (location != null)
			{
				foreach (var subloc in Locations.Instance.GetByParent(location))
				{
					result.AddRange(GetByParentRecursive(subloc.Id));
				}
			}

			return result.ToArray();
		}

		public Model.SearchResult[] GetClosest(Model.Geo location)
		{
			return GetAll().Where(b => b.Geo != null).Select(b => new Model.SearchResult()
			{
				Id = b.Id,
				DisplayText = b.Description,
				Distance = b.Geo.DistanceTo(location),
				Name = b.Name,
				Score = 1,
				Type = "Bar",
				Url = b.Url,
				UrlKey = b.UrlKey
			})
			.OrderBy(s => s.Distance)
			.ToArray();
		}

		public void UpdateSearchIndex()
		{
			Search.Lucene.AddToIndex(GetAll().Where(b => b.IsActive));
		}
	}
}