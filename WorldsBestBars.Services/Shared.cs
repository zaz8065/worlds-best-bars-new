using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace WorldsBestBars.Services
{
    internal static class Shared
    {
        internal static string GetMediaPathPhysicalRoot()
        {
            return ConfigurationManager.AppSettings["path:media:physical"];
        }

        internal static string GetMediaPathPhysical(string url)
        {
            if (url == null) { return null; }

            return Path.Combine(GetMediaPathPhysicalRoot(), url);
        }

        internal static string GetMediaPathRelative(string url)
        {
            if (url == null) { return null; }

            return string.Format("{0}/{1}", ConfigurationManager.AppSettings["path:media:relative"], url);
        }

        internal static IEnumerable<string> GetImages(string url, string key = "365x270")
        {
            var fullPath = GetMediaPathPhysical(url);
            if (fullPath == null) { return new string[0]; }

            var ret = new List<string>();
            if (Directory.Exists(fullPath))
            {
                var files = Directory.GetFiles(fullPath);

                foreach (var file in files)
                {
                    if (key == null || Path.GetFileNameWithoutExtension(file).StartsWith(key))
                    {
                        ret.Add(file);
                    }
                }
            }

            return ret.Select(e => GetMediaPathRelative(url + e.Replace(fullPath, string.Empty))).ToArray();
        }
    }
}
