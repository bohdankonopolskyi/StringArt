using System.Drawing;

namespace StringArt.Generator;


    public class StringArtGenerator
    {
        private int iterations = 1000;
        private string shape = "circle";
        private Bitmap image;
        private double[,] data;
        private double[,] residual;
        private int seed = 0;
        private int nails = 100;
        private int weight = 20;
        private List<Point> nodes;
        private List<List<List<Point>>> paths;

        public void SetSeed(int seed)
        {
            this.seed = seed;
        }

        public void SetWeight(int weight)
        {
            this.weight = weight;
        }

        public void SetShape(string shape)
        {
            this.shape = shape;
        }

        public void SetNails(int nails)
        {
            this.nails = nails;
            if (shape == "circle")
            {
                SetNodesCircle();
            }
            else if (shape == "rectangle")
            {
                SetNodesRectangle();
            }
        }

        public void SetIterations(int iterations)
        {
            this.iterations = iterations;
        }

        public void SetNodesRectangle()
        {
            double perimeter = GetPerimeter();
            double spacing = perimeter / nails;
            int width = data.GetLength(0);
            int height = data.GetLength(1);

            List<double> pnails = Enumerable.Range(0, nails).Select(t => t * spacing).ToList();

            List<double> xarr = new List<double>();
            List<double> yarr = new List<double>();
            foreach (double p in pnails)
            {
                double x, y;
                if (p < width) // top edge
                {
                    x = p;
                    y = 0;
                }
                else if (p < width + height) // right edge
                {
                    x = width;
                    y = p - width;
                }
                else if (p < 2 * width + height) // bottom edge
                {
                    x = width - (p - width - height);
                    y = height;
                }
                else // left edge
                {
                    x = 0;
                    y = height - (p - 2 * width - height);
                }
                xarr.Add(x);
                yarr.Add(y);
            }

            nodes = xarr.Zip(yarr, (x, y) => new Point((int)x, (int)y)).ToList();
        }

        public double GetPerimeter()
        {
            return 2.0 * (data.GetLength(0) + data.GetLength(1));
        }

        public void SetNodesCircle()
        {
            double spacing = 2 * Math.PI / nails;
            int radius = (int)(0.5 * Math.Max(data.GetLength(0), data.GetLength(1)));

            List<double> x = Enumerable.Range(0, nails).Select(t => radius + radius * Math.Cos(t * spacing)).ToList();
            List<double> y = Enumerable.Range(0, nails).Select(t => radius + radius * Math.Sin(t * spacing)).ToList();

            nodes = x.Zip(y, (xVal, yVal) => new Point((int)xVal, (int)yVal)).ToList();
        }

        public void LoadImage(string path)
        {
            image = new Bitmap(path);
            data = GetImageData(image);
        }

        public void Preprocess()
        {
            image = Grayscale(image);
            image = Invert(image);
            image = EdgeEnhanceMore(image);
            image = EnhanceContrast(image);
            data = GetImageData(image);
        }

        public List<Point> Generate()
        {
            CalculatePaths();
            double delta = 0.0;
            List<Point> pattern = new List<Point>();
            int nail = seed;
            double[,] datacopy = CopyArray(data);

            for (int i = 0; i < iterations; i++)
            {
                int darkestNail;
                double[,] darkestPath = ChooseDarkestPath(nail, out darkestNail);

                pattern.Add(nodes[darkestNail]);

                SubtractPathFromData(darkestPath);

                if (SumArray(data) <= 0.0)
                {
                    Console.WriteLine("Stopping iterations. No more data or residual unchanged.");
                    break;
                }

                delta = SumArray(data);
                nail = darkestNail;
            }

            residual = CopyArray(data);
            data = datacopy;

            return pattern;
        }

        private double[,] GetImageData(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            double[,] imageData = new double[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    double luminosity = (pixelColor.R + pixelColor.G + pixelColor.B) / 3.0;
                    imageData[x, y] = luminosity;
                }
            }

            return imageData;
        }

        private Bitmap Grayscale(Bitmap image)
        {
            Bitmap grayscaleImage = new Bitmap(image.Width, image.Height);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    byte luminosity = (byte)((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
                    grayscaleImage.SetPixel(x, y, Color.FromArgb(luminosity, luminosity, luminosity));
                }
            }

            return grayscaleImage;
        }

        private Bitmap Invert(Bitmap image)
        {
            Bitmap invertedImage = new Bitmap(image.Width, image.Height);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    byte invertedLuminosity = (byte)(255 - pixelColor.R);
                    invertedImage.SetPixel(x, y, Color.FromArgb(invertedLuminosity, invertedLuminosity, invertedLuminosity));
                }
            }

            return invertedImage;
        }

        private Bitmap EdgeEnhanceMore(Bitmap image)
        {
            Bitmap edgeEnhancedImage = new Bitmap(image.Width, image.Height);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    edgeEnhancedImage.SetPixel(x, y, pixelColor);
                }
            }

            return edgeEnhancedImage;
        }

        private Bitmap EnhanceContrast(Bitmap image)
        {
            return image;
        }

        private void CalculatePaths()
        {
            paths = new List<List<List<Point>>>();

            foreach (Point nail in nodes)
            {
                List<List<Point>> nailPaths = new List<List<Point>>();

                foreach (Point node in nodes)
                {
                    List<Point> path = BresenhamPath(nail, node);
                    nailPaths.Add(path);
                }

                paths.Add(nailPaths);
            }
        }

        private List<Point> BresenhamPath(Point start, Point end)
        {
            int x1 = start.X;
            int y1 = start.Y;
            int x2 = end.X;
            int y2 = end.Y;

            x1 = Math.Max(0, Math.Min(x1, data.GetLength(0) - 1));
            y1 = Math.Max(0, Math.Min(y1, data.GetLength(1) - 1));
            x2 = Math.Max(0, Math.Min(x2, data.GetLength(0) - 1));
            y2 = Math.Max(0, Math.Min(y2, data.GetLength(1) - 1));

            int dx = x2 - x1;
            int dy = y2 - y1;

            List<Point> path = new List<Point>();

            if (start == end)
            {
                return path;
            }

            bool isSteep = Math.Abs(dy) > Math.Abs(dx);

            if (isSteep)
            {
                Swap(ref x1, ref y1);
                Swap(ref x2, ref y2);
            }

            if (x1 > x2)
            {
                Swap(ref x1, ref x2);
                Swap(ref y1, ref y2);
            }

            dx = x2 - x1;
            dy = y2 - y1;

            int error = dx / 2;
            int ystep = (y1 < y2) ? 1 : -1;
            int y = y1;

            for (int x = x1; x <= x2; x++)
            {
                Point point = isSteep ? new Point(y, x) : new Point(x, y);
                path.Add(point);
                error -= Math.Abs(dy);
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }

            return path;
        }

        private void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        private double[,] ChooseDarkestPath(int nail, out int darkestNail)
        {
            double maxDarkness = -1.0;
            double[,] darkestPath = null;
            darkestNail = -1;

            for (int index = 0; index < paths[nail].Count; index++)
            {
                List<Point> path = paths[nail][index];
                double darkness = path.Sum(point => data[point.X, point.Y]);

                if (darkness > maxDarkness)
                {
                    darkestPath = new double[data.GetLength(0), data.GetLength(1)];
                    foreach (Point point in path)
                    {
                        darkestPath[point.X, point.Y] = 1.0;
                    }
                    darkestNail = index;
                    maxDarkness = darkness;
                }
            }

            return darkestPath;
        }

        private void SubtractPathFromData(double[,] path)
        {
            int rows = path.GetLength(0);
            int cols = path.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    data[i, j] -= weight * path[i, j];
                    if (data[i, j] < 0.0)
                    {
                        data[i, j] = 0.0;
                    }
                }
            }
        }

        private double SumArray(double[,] array)
        {
            double sum = 0.0;
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    sum += array[i, j];
                }
            }

            return sum;
        }

        private double[,] CopyArray(double[,] array)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            double[,] copy = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    copy[i, j] = array[i, j];
                }
            }

            return copy;
        }
    }

