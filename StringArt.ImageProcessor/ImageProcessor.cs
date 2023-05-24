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
        ApplyGrayscale();
        return new Bitmap(_bitmapImage);
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
    
}