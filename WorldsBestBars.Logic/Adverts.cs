using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace WorldsBestBars.Logic
{
    public static class Adverts
    {
        static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Adverts));

        static Random Rand = new Random();
        static Timer StoreTimer = new Timer((_) => Store(), null, new TimeSpan(0, 0, 0), new TimeSpan(0, 5, 0));

        static Dictionary<Guid, int> CacheImpression = new Dictionary<Guid, int>();
        static Dictionary<Guid, int> CacheClick = new Dictionary<Guid, int>();

        public static void RecordImpression(Guid id)
        {
            if (CacheImpression.ContainsKey(id))
            {
                CacheImpression[id]++;
            }
            else
            {
                CacheImpression.Add(id, 1);
            }
        }

        public static void RecordClick(Guid id)
        {
            if (CacheClick.ContainsKey(id))
            {
                CacheClick[id]++;
            }
            else
            {
                CacheClick.Add(id, 1);
            }
        }

        public static Model.Advert GetRandomAdvert(System.Web.HttpSessionStateBase session, bool recordImpression = true, string key = null)
        {
            if (key != null)
            {
                var _advert = Web.Cache.Adverts.Instance.GetAll().FirstOrDefault(a => a.Id.ToString() == key);
                if (_advert != null)
                {
                    return _advert;
                }
            }

            var currentAdvert = Guid.Empty;
            if (session != null && session["advert"] != null)
            {
                currentAdvert = ((Model.Advert)session["advert"]).Id;
            }

            var advert = GetRandomAdvert(session["user:country"] as string);

            if (advert == null)
            {
                return null;
            }

            if (advert.Id == currentAdvert)
            {
                for (var i = 0; i < 10; i++)
                {
                    advert = GetRandomAdvert(session["user:country"] as string);
                    if (advert.Id != currentAdvert)
                    {
                        break;
                    }
                }
            }

            if (recordImpression)
            {
                RecordImpression(advert.Id);
            }

            if (session != null)
            {
                session["advert"] = advert;
            }
            else
            {
                return advert;
            }


            return (Model.Advert)session["advert"];
        }

        public static Model.Advert GetRandomAdvert(string targetCountry)
        {
            var original = Web.Cache.Adverts.Instance.GetAll()
                .Where(a =>
                    a.IsActive &&
                    (a.Start == null || a.Start < DateTime.Now) && (a.Finish == null || a.Finish > DateTime.Now) &&
                    (a.TargetCountries == null || targetCountry == null || a.TargetCountries.Split(',').Contains(targetCountry)));

            if (!original.Any())
            {
                return null;
            }

            var weightedList = new List<Model.Advert>();

            foreach (var advert in original)
            {
                for (var i = 0; i < advert.Weight + 1; i++)
                {
                    weightedList.Add(advert);
                }
            }

            return weightedList.OrderBy(_ => Guid.NewGuid()).First();
        }

        private static void Store()
        {
            Log.Info("Writing advertising statistics");

            var period = DateTime.Now;
            period = period.AddSeconds(-period.Second);
            period = period.AddMinutes(-period.Minute);
            period = period.AddMilliseconds(-period.Millisecond);

            foreach (var id in CacheImpression.Keys.Union(CacheClick.Keys).ToArray())
            {
                try
                {
                    Model.Advert.IncrementStats(id, period, CacheClick.ContainsKey(id) ? CacheClick[id] : 0, CacheImpression.ContainsKey(id) ? CacheImpression[id] : 0);
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to write advertising statistics", ex);
                }
                finally
                {
                    if (CacheImpression.ContainsKey(id))
                    {
                        CacheImpression[id] = 0;
                    }
                    if (CacheClick.ContainsKey(id))
                    {
                        CacheClick[id] = 0;
                    }
                }
            }
        }
    }
}