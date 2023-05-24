using System.Drawing;
namespace StringArt.ImageProcessor;

public interface IImageProcessor
{
    Bitmap GetProcessedImage();
}