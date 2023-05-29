using System.Drawing;

namespace StringArt.ImageProcessor;

public class ImageProcessor : IImageProcessor
{
    private Bitmap _bitmapImage;

    public ImageProcessor(byte[] imageBytes)
    {
        _bitmapImage = ByteArrayToBitmap(imageBytes);
    }

    public Bitmap GetProcessedImage()
    {
        var edgeDetector = new CannyEdgeDetector(_bitmapImage);
        edgeDetector.Apply();
        
  
        return new Bitmap(edgeDetector.EdgeMap);
    }

    private static Bitmap ByteArrayToBitmap(byte[] byteArray)
    {
        using var memoryStream = new MemoryStream(byteArray);
        return new Bitmap(memoryStream);
    }

    private void ApplyGrayscale()
    {
        using var grayscaleImage = new Bitmap(_bitmapImage.Width, _bitmapImage.Height);

        for (var y = 0; y < _bitmapImage.Height; y++)
        for (var x = 0; x < _bitmapImage.Width; x++)
        {
            var originalColor = _bitmapImage.GetPixel(x, y);
            var grayscaleValue = (int)((originalColor.R + originalColor.G + originalColor.B) / 3.0);
            var grayscaleColor = Color.FromArgb(grayscaleValue, grayscaleValue, grayscaleValue);
            grayscaleImage.SetPixel(x, y, grayscaleColor);
        }

        _bitmapImage.Dispose();
        _bitmapImage = new Bitmap(grayscaleImage);
    }
    
    private void ApplyThreshold(int threshold = 128)
    {
        for (var y = 0; y < _bitmapImage.Height; y++)
        {
            for (var x = 0; x < _bitmapImage.Width; x++)
            {
                var pixel = _bitmapImage.GetPixel(x, y);
                var intensity = (pixel.R + pixel.G + pixel.B) / 3;

                var color = intensity > threshold ? Color.White : Color.Black;

                _bitmapImage.SetPixel(x, y, color);
                
                // if (intensity > threshold)
                // {
                //     _bitmapImage.SetPixel(x, y, Color.White);
                // }
                // else
                // {
                //     _bitmapImage.SetPixel(x, y, Color.Black);
                // }
            }
        }
    }
    
    public Bitmap InvertBlackAndWhite(Bitmap image)
    {
        Bitmap invertedImage = new Bitmap(image.Width, image.Height);

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Color pixelColor = image.GetPixel(x, y);

                // Calculate the inverted color
                Color invertedColor = Color.FromArgb(255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B);

                // Set the inverted color in the new image
                invertedImage.SetPixel(x, y, invertedColor);
            }
        }

        return invertedImage;
    }
    
    public List<Point> RasterizeLine(int x0, int y0, int x1, int y1)
    {
        var points = new List<Point>();

        var dx = Math.Abs(x1 - x0);
        var dy = Math.Abs(y1 - y0);
        var sx = x0 < x1 ? 1 : -1;
        var sy = y0 < y1 ? 1 : -1;
        var err = dx - dy;

        while (true)
        {
            points.Add(new Point(x0, y0));

            if (x0 == x1 && y0 == y1)
                break;

            var err2 = 2 * err;

            if (err2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (err2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return points;
    }
}