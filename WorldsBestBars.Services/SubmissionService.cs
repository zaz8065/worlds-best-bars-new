using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using WorldsBestBars.Services.Models;

namespace WorldsBestBars.Services
{
    public class SubmissionService : BaseService
    {
        #region Public Methods

        /// <summary>
        /// Gets the bars that have been submitted to be included on the site.
        /// </summary>
        public IEnumerable<BarCreateSubmission> GetCreateSubmissions()
        {
            var root = Path.Combine(ConfigurationManager.AppSettings["path:media:physical"], "_bar-resource");

            var files = Directory.GetDirectories(root).Select(d => Path.Combine(root, d, "content.xml"));
            var result = new List<BarCreateSubmission>();

            foreach (var file in files)
            {
                if (!File.Exists(file)) { continue; }

                var doc = XDocument.Load(file);
                var path = Path.GetDirectoryName(file);
                var id = path.Split('\\').Last();

                var media = Directory.GetFiles(path, "*.jpg");

                if (doc.Root.Name == "CreateBar")
                {
                    result.Add(new BarCreateSubmission
                    {
                        Id = Guid.Parse(id),
                        Name = doc.Root.Element("Name").Value,
                        ContactEmail = doc.Root.Element("ContactEmail").Value,
                        ContactName = doc.Root.Element("ContactName").Value,
                        Location = doc.Root.Element("Location").Value,
                        Reasoning = doc.Root.Element("Reasoning").Value,
                        Created = File.GetCreationTime(file),
                        Media = media.Select(m => Path.GetFileName(m)).ToArray()
                    });
                }
            }

            return result.OrderByDescending(e => e.Created).ToArray();
        }

        /// <summary>
        /// Purges the submission and it's associated data.
        /// </summary>
        /// <param name="id">The id.</param>
        public void PurgeSubmission(Guid id)
        {
            var root = Path.Combine(ConfigurationManager.AppSettings["path:media:physical"], "_bar-resource");
            var path = Path.Combine(root, id.ToString());

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        /// <summary>
        /// Gets the update submissions.
        /// </summary>
        /// <returns>A list of submitted bar updates.</returns>
        public IEnumerable<BarUpdateSubmission> GetUpdateSubmissions()
        {
            var root = Path.Combine(ConfigurationManager.AppSettings["path:media:physical"], "_bar-resource");

            var files = Directory.GetDirectories(root).Select(d => Path.Combine(root, d, "content.xml"));
            var result = new List<BarUpdateSubmission>();

            foreach (var file in files)
            {
                if (!File.Exists(file)) { continue; }

                var doc = XDocument.Load(file);
                if (doc.Root.Name == "EditBarContainer")
                {
                    var serializer = new XmlSerializer(typeof(EditBarContainer));

                    var updates = (EditBarContainer)serializer.Deserialize(doc.CreateReader());

                    result.Add(new BarUpdateSubmission
                    {
                        Id = updates.Id,
                        BarId = updates.BarId.Value,
                        Name = updates.Name,
                        Created = File.GetCreationTime(file),
                        Updates = updates
                    });
                }
            }

            return result.OrderByDescending(e => e.Created).ToArray();
        }

        /// <summary>
        /// Gets the update submission.
        /// </summary>
        /// <param name="id">The identifier of the update.</param>
        /// <returns>A single bar update submission with the identifier.</returns>
        public BarUpdateSubmission GetUpdateSubmission(Guid id)
        {
            var root = Path.Combine(ConfigurationManager.AppSettings["path:media:physical"], "_bar-resource");

            var file = Path.Combine(root, id.ToString(), "content.xml");
            if (!File.Exists(file)) { return null; }


            var doc = XDocument.Load(file);
            if (doc.Root.Name == "EditBarContainer")
            {
                var serializer = new XmlSerializer(typeof(EditBarContainer));

                var updates = (EditBarContainer)serializer.Deserialize(doc.CreateReader());

                return new BarUpdateSubmission
                {
                    Id = updates.Id,
                    BarId = updates.BarId.Value,
                    Name = updates.Name,
                    Created = File.GetCreationTime(file),
                    Updates = updates
                };
            }

            return null;
        }

        #endregion
    }
}
