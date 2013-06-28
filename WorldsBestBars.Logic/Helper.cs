using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace WorldsBestBars.Logic
{
    public static class Helper
    {
        public static readonly Size[] MediaSizes = new Size[] {
            new Size(155, 105),
            new Size(245, 165),
            new Size(365, 270)
        };

        public static bool ValidEmail(string input)
        {
            return Regex.IsMatch(input,
              @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
              @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
              RegexOptions.IgnoreCase);
        }

        public static IList<Model.UrlMap> Breadcrumbs(Guid id)
        {
            var ret = new List<Model.UrlMap>();

            var entity = Web.Cache.UrlMap.Instance.GetById(id);
            while (entity != null)
            {
                ret.Add(entity);

                entity = entity.Parent == null ? null : (Web.Cache.UrlMap.Instance.Contains((Guid)entity.Parent) ? Web.Cache.UrlMap.Instance.GetById((Guid)entity.Parent) : null);
            }

            ret.Reverse();

            return ret;
        }

        public static bool AgeVerified(this HttpSessionStateBase session)
        {
            return (session != null && session["age.check"] != null);
        }

        public static Model.User CurrentUser(this HttpSessionStateBase session)
        {
            if (session == null) { return null; }

            return (Model.User)session["user"];
        }

        public static void SetCurrentUser(this HttpSessionStateBase session, Model.User user)
        {
            if (user == null)
            {
                session.Remove("user");
            }
            else
            {
                session["user"] = user;
            }
        }

        public static string Slugify(this string input, int maxLength = 32)
        {
            var slug = input.ToLower();
            slug = RemoveDiacritics(slug);
            slug = slug.Replace("&", "and");
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"[\s-]+", " ").Trim();
            slug = Regex.Replace(slug, @"\s", "-");
            slug = slug.Substring(0, slug.Length <= maxLength ? slug.Length : maxLength).Trim();

            return slug;
        }

        static string RemoveDiacritics(string stIn)
        {
            var stFormD = stIn.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }


        const string Default = "__default";

        public static string[] GetMedia(this UrlHelper helper, string path, string key, bool excludeDefault = false)
        {
            var mediaRoot = helper.RequestContext.HttpContext.Server.MapPath(ConfigurationManager.AppSettings["mediaRoot"]);
            var fullPath = Path.Combine(mediaRoot, path);
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

            if (ret.Count == 0)
            {
                if (path != Default && !excludeDefault)
                {
                    return GetMedia(helper, Default, key);
                }
            }

            return ret.Select(e => helper.Content(PhysicalToVirtual(e))).ToArray();
        }

        public static string[] GetMedia(this UrlHelper helper, Guid id, string key, bool excludeDefault = false)
        {
            var map = Web.Cache.UrlMap.Instance.GetById(id);
            if (map == null)
            {
                return GetMedia(helper, Default, key);
            }

            var path = Web.Cache.UrlMap.Instance.GetById(id).Url;

            return GetMedia(helper, path, key, excludeDefault);
        }

        public static string GetFirstMedia(this UrlHelper helper, Guid id, string key, bool excludeDefault = false)
        {
            return GetMedia(helper, id, key, excludeDefault).OrderBy(_ => Guid.NewGuid()).FirstOrDefault() ?? string.Empty;
        }

        public static string GetMediaPath(Guid id)
        {
            var map = Web.Cache.UrlMap.Instance.GetById(id);
            if (map == null)
            {
                return null;
            }

            var path = Web.Cache.UrlMap.Instance.GetById(id).Url;
            var mediaRootPhysical = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["mediaRoot"]);

            return Path.Combine(mediaRootPhysical, path);
        }

        public static string GetFeatureMediaPath(Guid id)
        {
            var mediaRootPhysical = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["mediaRoot"]);

            return Path.Combine(mediaRootPhysical, "_features", id.ToString());
        }

        public static string[] GetFeatureMedia(this UrlHelper helper, Guid id)
        {
            var fullPath = GetFeatureMediaPath(id);
            var ret = new List<string>();
            if (Directory.Exists(fullPath))
            {
                var files = Directory.GetFiles(fullPath);

                foreach (var file in files)
                {
                    ret.Add(file);
                }
            }

            return ret.Select(e => helper.Content(PhysicalToVirtual(e))).ToArray();
        }

        public static string GetFirstFeatureMedia(this UrlHelper helper, Guid id)
        {
            return GetFeatureMedia(helper, id).OrderBy(_ => Guid.NewGuid()).FirstOrDefault() ?? string.Empty;
        }

        public static void MoveMedia(string source, string dest)
        {
            if (source != null)
            {
                if (Directory.Exists(source))
                {
                    if (dest != null)
                    {
                        if ((dest == null) || !Directory.Exists(dest))
                        {
                            Directory.Move(source, dest);
                        }
                        else
                        {
                            foreach (var file in Directory.GetFiles(source))
                            {
                                File.Move(file, Path.Combine(dest, Path.GetFileName(file)));
                            }

                            Directory.Delete(source);
                        }
                    }
                }
            }
        }

        public static string[] GetMediaGroups(this UrlHelper helper, Guid id)
        {
            var path = GetMediaPath(id);
            if (path != null && Directory.Exists(path))
            {
                var ret = new List<string>();

                ret.AddRange(Directory.EnumerateFiles(path, string.Format("{0}x{1}.orig.*.jpg", MediaSizes[0].Width, MediaSizes[0].Height)).Select(f => helper.Content(PhysicalToVirtual(f))).ToArray());
                ret.AddRange(Directory.EnumerateFiles(path, "image.*.jpg").Select(f => helper.Content(PhysicalToVirtual(f))).ToArray());

                return ret.ToArray();
            }

            return null;
        }

        public static void DeleteMediaGroup(Guid id, int index)
        {
            var path = GetMediaPath(id);
            if (path != null && Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    if (file.EndsWith(string.Format("{0:00}.jpg", index)))
                    {
                        File.Delete(file);
                    }
                }

                if (Directory.GetFiles(path).Length + Directory.GetDirectories(path).Length == 0)
                {
                    Directory.Delete(path);
                }
            }
        }

        public static void UploadImage(Guid id, Stream data, bool process)
        {
            var mediaPath = GetMediaPath(id);
            if (mediaPath == null) { throw new Exception("Cannot upload media to an inactive bar."); }

            var index = 0;
            if (!Directory.Exists(mediaPath))
            {
                Directory.CreateDirectory(mediaPath);
            }
            else
            {
                var files = Directory.GetFiles(mediaPath);
                while (files.Any(f => { var parts = f.Split('.'); return int.Parse(parts[parts.Length - 2]) == index; }))
                {
                    index++;
                }
            }

            if (process)
            {
                data.Seek(0, SeekOrigin.Begin);
                using (var original = new FileStream(Path.Combine(mediaPath, string.Format("original.{0:00}.jpg", index)), FileMode.CreateNew))
                {
                    var read = 0;
                    var buffer = new byte[128];
                    while ((read = data.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        original.Write(buffer, 0, read);
                    }
                }

                data.Seek(0, SeekOrigin.Begin);
                using (var image = Image.FromStream(data))
                {
                    var codec = ImageCodecInfo.GetImageEncoders().Single(c => c.MimeType == "image/jpeg");
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);

                    foreach (var size in MediaSizes)
                    {
                        var filename = Path.Combine(mediaPath, string.Format("{0}x{1}.orig.{2:00}.jpg", size.Width, size.Height, index));
                        using (var output = Resize(image, size))
                        {
                            output.Save(filename, codec, encoderParameters);

                            using (var sepia = Imaging.ColorMatrix.ConvertSepiaTone(output))
                            {
                                sepia.Save(filename.Replace(".orig.", ".sep."), codec, encoderParameters);
                            }

                            using (var greyscale = Imaging.ColorMatrix.ConvertBlackAndWhite(output))
                            {
                                greyscale.Save(filename.Replace(".orig.", ".bw."), codec, encoderParameters);
                            }
                        }
                    }
                }
            }
            else
            {
                using (var original = new FileStream(Path.Combine(mediaPath, string.Format("image.{0:00}.jpg", index)), FileMode.CreateNew))
                {
                    var read = 0;
                    var buffer = new byte[128];
                    while ((read = data.Read(buffer, 0, 128)) > 0)
                    {
                        original.Write(buffer, 0, read);
                    }
                }
            }
        }

        public static void FeatureUploadImage(Guid id, Stream data)
        {
            var mediaPath = Helper.GetFeatureMediaPath(id);

            var index = 0;
            if (!Directory.Exists(mediaPath))
            {
                Directory.CreateDirectory(mediaPath);
            }
            else
            {
                var files = Directory.GetFiles(mediaPath);
                while (files.Any(f => { var parts = f.Split('.'); return int.Parse(parts[parts.Length - 2]) == index; }))
                {
                    index++;
                }
            }

            using (var original = new FileStream(Path.Combine(mediaPath, string.Format("image.{0:00}.jpg", index)), FileMode.CreateNew))
            {
                var read = 0;
                var buffer = new byte[128];
                while ((read = data.Read(buffer, 0, 128)) > 0)
                {
                    original.Write(buffer, 0, read);
                }
            }
        }

        public static bool CheckRedirect(Uri requestedUrl, out string redirectedUrl)
        {
            if (requestedUrl.AbsolutePath == "/age-check.htm" || requestedUrl.AbsolutePath == "/agecheck.aspx")
            {
                if (requestedUrl.Query != null && requestedUrl.Query.StartsWith("?ReturnUrl="))
                {
                    redirectedUrl = string.Format("/age-gate?redirect={0}", requestedUrl.Query.Substring(11));
                }
                else
                {
                    redirectedUrl = "/age-gate";
                }

                return true;
            }

            redirectedUrl = Model.Redirect.GetByInbound(requestedUrl.AbsolutePath, true);

            return redirectedUrl != null;
        }

        static Image Resize(Image source, Size size)
        {
            return Resize(source, size.Width, size.Height);
        }

        static Image Resize(Image source, int width, int height)
        {
            var ratio = (double)source.Width / (double)source.Height;

            var newWidth = width;
            var newHeight = height;

            if ((double)width / (double)height > (double)ratio)
            {
                width = (int)((double)height * ratio);
            }
            else
            {
                height = (int)((double)width / ratio);
            }

            var destination = new Bitmap(newWidth, newHeight);

            using (var g = Graphics.FromImage(destination))
            {
                g.FillRectangle(Brushes.White, 0, 0, destination.Width, destination.Height);

                var destPoint = new Point((newWidth / 2) - (width / 2), (newHeight / 2) - (height / 2));
                var destSize = new Size(width, height);
                var destRect = new Rectangle(destPoint, destSize);

                g.DrawImage(source, destRect);
            }

            return destination;
        }

        private static string PhysicalToVirtual(string physical)
        {
            var mediaRootPhysical = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["mediaRoot"]);
            var mediaRootRelative = ConfigurationManager.AppSettings["mediaRoot"];

            return physical.Replace(mediaRootPhysical, mediaRootRelative).Replace("\\", "/");
        }
    }
}
