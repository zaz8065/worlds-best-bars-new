using System;
using System.IO;

namespace WorldsBestBars.Services.Media
{
    public class Delete
    {
        #region Constants

        const string VirtualMediaRoot = "/content/media/";

        #endregion

        #region Public Methods

        /// <summary>
        /// Deletes a file from the specified virtual path.
        /// </summary>
        /// <param name="virtualPath">The virtual path of the file to be deleted.</param>
        public void Execute(string virtualPath)
        {
            if (virtualPath == null) { return; }

            if (virtualPath.StartsWith("http"))
            {
                virtualPath = new Uri(virtualPath).AbsolutePath;
            }

            if (!virtualPath.StartsWith(VirtualMediaRoot))
            {
                throw new ArgumentException("Path must be media content", "virtualPath");
            }

            var root = Shared.GetMediaPathPhysicalRoot();

            virtualPath = virtualPath.Substring(VirtualMediaRoot.Length);

            var path = Path.Combine(root, virtualPath);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        #endregion
    }
}
