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

        public static Model.Advert GetRandomAdvert(System.Web.HttpSessionStateBase session, bool recordImpression = true)
        {
            var issued = session == null ? DateTime.Now.AddMinutes(-2) : (DateTime?)session["advert.issued"] ?? DateTime.MinValue;
            if (issued < DateTime.Now.AddMinutes(-1))
            {
                var currentAdvert = Guid.Empty;
                if (session != null && session["advert"] != null)
                {
                    currentAdvert = ((Model.Advert)session["advert"]).Id;
                }

                var _advert = GetRandomAdvert();

                if (_advert == null)
                {
                    return null;
                }

                if (_advert.Id == currentAdvert)
                {
                    for (var i = 0; i < 10; i++)
                    {
                        _advert = GetRandomAdvert();
                        if (_advert.Id != currentAdvert)
                        {
                            break;
                        }
                    }
                }

                if (recordImpression)
                {
                    RecordImpression(_advert.Id);
                }

                if (session != null)
                {
                    session["advert"] = _advert;
                    session["advert.issued"] = DateTime.Now;
                }
                else
                {
                    return _advert;
                }
            }

            return (Model.Advert)session["advert"];
        }

        public static Model.Advert GetRandomAdvert()
        {
            var original = Web.Cache.Adverts.Instance.GetAll().Where(a => a.IsActive && (a.Start == null || a.Start < DateTime.Now) && (a.Finish == null || a.Finish > DateTime.Now));

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