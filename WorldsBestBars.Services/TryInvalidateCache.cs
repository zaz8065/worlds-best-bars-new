using System;
using System.Configuration;
using System.Net;
using System.Threading;

namespace WorldsBestBars.Services
{
    public class TryInvalidateCache : BaseService
    {
        public void Execute(Guid id, string type)
        {
            new Thread(() =>
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        client.Headers.Add("user-agent", "WBB Admin/1.0");

                        var url = ConfigurationManager.AppSettings["path:root:relative"] + "/cache/invalidate/" + type + "/" + id.ToString();

                        client.DownloadString(url);

                    }
                }
                catch { }
            }).Start();
        }
    }
}
