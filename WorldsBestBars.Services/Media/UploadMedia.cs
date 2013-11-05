using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace WorldsBestBars.Services.Media
{
    public class UploadMedia : BaseService
    {
        #region Constants

        static readonly Size[] MediaSizes = new Size[] {
            new Size(155, 105),
            new Size(245, 165),
            new Size(365, 270)
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Formats the uploaded media to the correct dimensions and moves it to the appropriate destination.
        /// </summary>
        /// <param name="input">The uploaded file.</param>
        /// <param name="destination">The relative destination directory.</param>
        /// <param name="process">If set to <c>true</c> process the image into the correct dimensions and colour schemes; otherwise just move the file to the correct location.</param>
        public void Execute(Stream input, string destination, bool process = true)
        {
            destination = Shared.GetMediaPathPhysical(destination);

            var index = 0;
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }
            else
            {
                var files = Directory.GetFiles(destination);
                while (files.Where(f => f.EndsWith("jpg")).Any(f => { var parts = f.Split('.'); return int.Parse(parts[parts.Length - 2]) == index; }))
                {
                    index++;
                }
            }

            if (process)
            {
                input.Seek(0, SeekOrigin.Begin);
                using (var original = new FileStream(Path.Combine(destination, string.Format("original.{0:00}.jpg", index)), FileMode.CreateNew))
                {
                    var read = 0;
                    var buffer = new byte[128];
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        original.Write(buffer, 0, read);
                    }
                }

                input.Seek(0, SeekOrigin.Begin);
                using (var image = Image.FromStream(input))
                {
                    var codec = ImageCodecInfo.GetImageEncoders().Single(c => c.MimeType == "image/jpeg");
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 90L);

                    foreach (var size in MediaSizes)
                    {
                        var filename = Path.Combine(destination, string.Format("{0}x{1}.orig.{2:00}.jpg", size.Width, size.Height, index));
                        using (var output = Resize(image, size.Width, size.Height))
                        {
                            output.Save(filename, codec, encoderParameters);

                            using (var sepia = ColorMatrix.ConvertSepiaTone(output))
                            {
                                sepia.Save(filename.Replace(".orig.", ".sep."), codec, encoderParameters);
                            }

                            using (var greyscale = ColorMatrix.ConvertBlackAndWhite(output))
                            {
                                greyscale.Save(filename.Replace(".orig.", ".bw."), codec, encoderParameters);
                            }
                        }
                    }
                }
            }
            else
            {
                using (var original = new FileStream(Path.Combine(destination, string.Format("image.{0:00}.jpg", index)), FileMode.CreateNew))
                {
                    var read = 0;
                    var buffer = new byte[128];
                    while ((read = input.Read(buffer, 0, 128)) > 0)
                    {
                        original.Write(buffer, 0, read);
                    }
                }
            }
        }

        #endregion

        #region Private Methods

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

        #endregion

        #region Private Classes

        internal class ColorMatrix
        {
            internal static Image ConvertSepiaTone(Image image)
            {
                var matrix = new ColorMatrix();
                matrix.Matrix = new float[][]{
                      new float[] {.393f, .349f, .272f, 0, 0},
                      new float[] {.769f, .686f, .534f, 0, 0},
                      new float[] {.189f, .168f, .131f, 0, 0},
                      new float[] {0, 0, 0, 1, 0},
                      new float[] {0, 0, 0, 0, 1}
                  };
                return matrix.Apply(image);
            }

            internal static Image ConvertBlackAndWhite(Image Image)
            {
                var matrix = new ColorMatrix();
                matrix.Matrix = new float[][]{
                     new float[] {.3f, .3f, .3f, 0, 0},
                     new float[] {.59f, .59f, .59f, 0, 0},
                     new float[] {.11f, .11f, .11f, 0, 0},
                     new float[] {0, 0, 0, 1, 0},
                     new float[] {0, 0, 0, 0, 1}
                 };
                return matrix.Apply(Image);
            }

            float[][] Matrix { get; set; }

            internal Image Apply(Image input)
            {
                var output = new Bitmap(input.Width, input.Height);
                using (Graphics g = Graphics.FromImage(output))
                {
                    var colorMatrix = new System.Drawing.Imaging.ColorMatrix(Matrix);
                    using (var attributes = new ImageAttributes())
                    {
                        attributes.SetColorMatrix(colorMatrix);
                        g.DrawImage(input,
                            new System.Drawing.Rectangle(0, 0, input.Width, input.Height),
                            0, 0, input.Width, input.Height,
                              GraphicsUnit.Pixel,
                              attributes);
                    }
                }
                return output;
            }
        }

        #endregion
    }
}
