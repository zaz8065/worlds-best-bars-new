using System.IO;

namespace WorldsBestBars.Services.Media
{
    public static class Helper
    {
        /// <summary>
        /// Formats the uploaded media to the correct dimensions and moves it to the appropriate destination.
        /// </summary>
        /// <param name="input">The uploaded file.</param>
        /// <param name="destination">The relative destination directory.</param>
        /// <param name="process">If set to <c>true</c> process the image into the correct dimensions and colour schemes; otherwise just move the file to the correct location.</param>
        public static void UploadMedia(Stream input, string destination, bool process = true)
        {
            var service = new UploadMedia();

            service.Execute(input, destination, process);
        }

        /// <summary>
        /// Deletes a file from the specified virtual path.
        /// </summary>
        /// <param name="virtualPath">The virtual path of the file to be deleted.</param>
        public static void Delete(string virtualPath)
        {
            var service = new Delete();

            service.Execute(virtualPath);
        }
    }
}
