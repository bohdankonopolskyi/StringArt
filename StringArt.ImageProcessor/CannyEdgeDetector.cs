using System.Drawing;

namespace StringArt.ImageProcessor;

public class CannyEdgeDetector : IEdgeDetector
{
    private double[,] _gradientDirections;
    private double[,] _gradients;
    
    private int _height;
    private int _width;
    
    private double _highThreshold;
    private double _lowThreshold;
    private double _sigma;
    
    private Bitmap _image;

    public Bitmap EdgeMap { get; private set; }
    
    public CannyEdgeDetector(Bitmap image, double sigma = 1.4, double highThreshold = 20, double lowThreshold = 10)
    {
        _image = image;
        _sigma = sigma;
        _highThreshold = highThreshold;
        _lowThreshold = lowThreshold;

        _width = image.Width;
        _height = image.Height;

        _gradientDirections = new double[_width, _height];
        _gradients = new double[_width, _height];
    }

    public void Apply()
    {
        var grayImage = ConvertToGrayscale(_image);
        
        var blurredImage = ApplyGaussianBlur(grayImage, _sigma);
        
        CalculateGradients(blurredImage);
        
        var suppressedImage = ApplyNonMaximumSuppression();
        
        var thresholdedImage = ApplyDoubleThresholding(suppressedImage);
        
        var edgeMap = PerformEdgeTracking(thresholdedImage);
        
        EdgeMap = edgeMap;
    }

    private Bitmap ConvertToGrayscale(Bitmap image)
    {
        var grayscaleImage = new Bitmap(_width, _height);

        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                var color = image.GetPixel(x, y);
                var grayValue = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
                grayscaleImage.SetPixel(x, y, Color.FromArgb(grayValue, grayValue, grayValue));
            }
        }

        return grayscaleImage;
    }

    private Bitmap ApplyGaussianBlur(Bitmap image, double sigma)
    {
        var kernelSize = (int)Math.Ceiling(sigma * 3) * 2 + 1;
        var kernel = CreateGaussianKernel(sigma, kernelSize);

        var blurredImage = new Bitmap(_width, _height);

        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                var sumR = 0.0;
                var sumG = 0.0;
                var sumB = 0.0;
                var weightSum = 0.0;

                for (var ky = -kernelSize / 2; ky <= kernelSize / 2; ky++)
                {
                    for (var kx = -kernelSize / 2; kx <= kernelSize / 2; kx++)
                    {
                        var pixelX = x + kx;
                        var pixelY = y + ky;

                        if (pixelX >= 0 && pixelX < _width && pixelY >= 0 && pixelY < _height)
                        {
                            var color = image.GetPixel(pixelX, pixelY);
                            var weight = kernel[ky + kernelSize / 2, kx + kernelSize / 2];

                            sumR += color.R * weight;
                            sumG += color.G * weight;
                            sumB += color.B * weight;
                            weightSum += weight;
                        }
                    }
                }

                var blurredColor = Color.FromArgb((int)(sumR / weightSum), (int)(sumG / weightSum), (int)(sumB / weightSum));
                blurredImage.SetPixel(x, y, blurredColor);
            }
        }

        return blurredImage;
    }

    private double[,] CreateGaussianKernel(double sigma, int size)
    {
        var kernel = new double[size, size];
        var kernelSum = 0.0;

        var radius = size / 2;
        var sigmaSquared = sigma * sigma;
        var twoSigmaSquared = 2 * sigmaSquared;
        var coefficient = 1 / (Math.PI * twoSigmaSquared);

        for (var y = -radius; y <= radius; y++)
        {
            for (var x = -radius; x <= radius; x++)
            {
                var exponent = -(x * x + y * y) / twoSigmaSquared;
                var weight = coefficient * Math.Exp(exponent);

                kernel[y + radius, x + radius] = weight;
                kernelSum += weight;
            }
        }

        // Normalize the kernel
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                kernel[y, x] /= kernelSum;
            }
        }

        return kernel;
    }

    private void CalculateGradients(Bitmap image)
    {
        var sobelX = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        var sobelY = new int[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

        for (var y = 1; y < _height - 1; y++)
        {
            for (var x = 1; x < _width - 1; x++)
            {
                var gx = 0.0;
                var gy = 0.0;

                for (var ky = -1; ky <= 1; ky++)
                {
                    for (var kx = -1; kx <= 1; kx++)
                    {
                        var pixelX = x + kx;
                        var pixelY = y + ky;

                        var color = image.GetPixel(pixelX, pixelY);
                        var grayValue = color.R;

                        gx += sobelX[ky + 1, kx + 1] * grayValue;
                        gy += sobelY[ky + 1, kx + 1] * grayValue;
                    }
                }

                _gradientDirections[x, y] = Math.Atan2(gy, gx);
                _gradients[x, y] = Math.Sqrt(gx * gx + gy * gy);
            }
        }
    }

    private Bitmap ApplyNonMaximumSuppression()
    {
        var suppressedImage = new Bitmap(_width, _height);

        for (var y = 1; y < _height - 1; y++)
        {
            for (var x = 1; x < _width - 1; x++)
            {
                var direction = _gradientDirections[x, y];
                var gradient = _gradients[x, y];
                var neighbor1 = 0.0;
                var neighbor2 = 0.0;

                if ((direction >= -Math.PI / 8 && direction < Math.PI / 8) || (direction >= 7 * Math.PI / 8 || direction < -7 * Math.PI / 8))
                {
                    neighbor1 = _gradients[x + 1, y];
                    neighbor2 = _gradients[x - 1, y];
                }
                else if ((direction >= Math.PI / 8 && direction < 3 * Math.PI / 8) || (direction >= -7 * Math.PI / 8 && direction < -5 * Math.PI / 8))
                {
                    neighbor1 = _gradients[x + 1, y + 1];
                    neighbor2 = _gradients[x - 1, y - 1];
                }
                else if ((direction >= 3 * Math.PI / 8 && direction < 5 * Math.PI / 8) || (direction >= -5 * Math.PI / 8 && direction < -3 * Math.PI / 8))
                {
                    neighbor1 = _gradients[x, y + 1];
                    neighbor2 = _gradients[x, y - 1];
                }
                else if ((direction >= 5 * Math.PI / 8 && direction < 7 * Math.PI / 8) || (direction >= -3 * Math.PI / 8 && direction < -Math.PI / 8))
                {
                    neighbor1 = _gradients[x - 1, y + 1];
                    neighbor2 = _gradients[x + 1, y - 1];
                }

                if (gradient >= neighbor1 && gradient >= neighbor2)
                {
                    suppressedImage.SetPixel(x, y, Color.FromArgb((int)gradient, (int)gradient, (int)gradient));
                }
                else
                {
                    suppressedImage.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                }
            }
        }

        return suppressedImage;
    }

    private Bitmap ApplyDoubleThresholding(Bitmap image)
    {
        var thresholdedImage = new Bitmap(_width, _height);

        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                var pixel = image.GetPixel(x, y);
                var grayValue = pixel.R;

                if (grayValue >= _highThreshold)
                {
                    thresholdedImage.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                }
                else if (grayValue >= _lowThreshold)
                {
                    thresholdedImage.SetPixel(x, y, Color.FromArgb(128, 128, 128));
                }
                else
                {
                    thresholdedImage.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                }
            }
        }

        return thresholdedImage;
    }

    private Bitmap PerformEdgeTracking(Bitmap image)
    {
        var edgeMap = new Bitmap(_width, _height);

        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                var pixel = image.GetPixel(x, y);
                var grayValue = pixel.R;

                if (grayValue == 255)
                {
                    edgeMap.SetPixel(x, y, Color.FromArgb(255, 255, 255));

                    // Follow the edge using depth-first search
                    FollowEdge(x, y, edgeMap);
                }
                else
                {
                    edgeMap.SetPixel(x, y, Color.FromArgb(0, 0, 0));
                }
            }
        }

        return edgeMap;
    }

    private void FollowEdge(int x, int y, Bitmap edgeMap)
    {
        var stack = new Stack<Point>();
        stack.Push(new Point(x, y));

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            var cx = current.X;
            var cy = current.Y;

            for (var dy = -1; dy <= 1; dy++)
            {
                for (var dx = -1; dx <= 1; dx++)
                {
                    var nx = cx + dx;
                    var ny = cy + dy;

                    if (nx >= 0 && nx < _width && ny >= 0 && ny < _height)
                    {
                        var pixel = _image.GetPixel(nx, ny);
                        var grayValue = pixel.R;

                        if (grayValue == 128)
                        {
                            _image.SetPixel(nx, ny, Color.FromArgb(255, 255, 255));
                            edgeMap.SetPixel(nx, ny, Color.FromArgb(255, 255, 255));
                            stack.Push(new Point(nx, ny));
                        }
                    }
                }
            }
        }
    }
}