using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

class EnhanceImage
{
    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: EnhanceImage <inputPath> <outputPath>");
            return;
        }
        string inputPath = args[0];
        string outputPath = args[1];
        if (!File.Exists(inputPath))
        {
            Console.WriteLine($"Input file not found: {inputPath}");
            return;
        }
        using (var src = new Bitmap(inputPath))
        {
            // Scale factor (e.g., 2.5x)
            float scaleFactor = 2.5f;
            int newWidth = (int)(src.Width * scaleFactor);
            int newHeight = (int)(src.Height * scaleFactor);
            var scaled = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(scaled))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(src, 0, 0, newWidth, newHeight);
            }
            // Simple unsharp mask: blur then combine
            var blurred = GaussianBlur(scaled, 2);
            var sharpened = new Bitmap(scaled.Width, scaled.Height);
            float amount = 1.5f; // sharpening strength
            for (int y = 0; y < scaled.Height; y++)
            {
                for (int x = 0; x < scaled.Width; x++)
                {
                    Color original = scaled.GetPixel(x, y);
                    Color blur = blurred.GetPixel(x, y);
                    int r = Clamp((int)(original.R + amount * (original.R - blur.R)));
                    int g = Clamp((int)(original.G + amount * (original.G - blur.G)));
                    int b = Clamp((int)(original.B + amount * (original.B - blur.B)));
                    sharpened.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            sharpened.Save(outputPath, ImageFormat.Png);
        }
    }

    static Bitmap GaussianBlur(Bitmap image, int radius)
    {
        // Very naive box blur approximation (for demo purposes)
        var blurred = new Bitmap(image.Width, image.Height);
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                int r = 0, g = 0, b = 0, count = 0;
                for (int ky = -radius; ky <= radius; ky++)
                {
                    int ny = y + ky;
                    if (ny < 0 || ny >= image.Height) continue;
                    for (int kx = -radius; kx <= radius; kx++)
                    {
                        int nx = x + kx;
                        if (nx < 0 || nx >= image.Width) continue;
                        Color c = image.GetPixel(nx, ny);
                        r += c.R; g += c.G; b += c.B; count++;
                    }
                }
                r /= count; g /= count; b /= count;
                blurred.SetPixel(x, y, Color.FromArgb(r, g, b));
            }
        }
        return blurred;
    }

    static int Clamp(int value)
    {
        if (value < 0) return 0;
        if (value > 255) return 255;
        return value;
    }
}
