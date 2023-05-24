using System.Drawing;

namespace StringArt.ImageProcessor;

public interface IEdgeDetector
{
    Bitmap EdgeMap { get; }

    void Apply();
}