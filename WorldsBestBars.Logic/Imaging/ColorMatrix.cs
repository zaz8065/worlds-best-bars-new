using System.Drawing;
using System.Drawing.Imaging;

namespace WorldsBestBars.Logic.Imaging
{
    public class ColorMatrix
    {
        public static Image ConvertSepiaTone(Image image)
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

        public static Image ConvertBlackAndWhite(Image Image)
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

        public float[][] Matrix { get; set; }

        public Image Apply(Image input)
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
}