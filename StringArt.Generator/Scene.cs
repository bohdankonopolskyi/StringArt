using System.Drawing;
using StringArt.Generator.Structs;

namespace StringArt.Generator;

public class Scene
{
    private readonly int x0 = 0;
    private readonly int y0 = 0;
    private Bitmap _image;

    public Scene(int nailCount, int radius = 1)
    {
        NailCount = nailCount;
        Radius = radius;
    }

    public int NailCount { get; set; }
    public List<Hook> Hooks { get; set; }
    private double[,] _hooks;
    public int Radius { get; set; }

    public void InitializeNailsOnCircleBorder()
    {
        Hooks = new List<Hook>();

        var angleIncrement = 2 * Math.PI / NailCount;
        double currentAngle = 0;

        for (var i = 0; i < NailCount; i++)
        {
            var x = x0 + Radius * Math.Cos(currentAngle);
            var y = y0 + Radius * Math.Sin(currentAngle);

            var nail = new Hook
            {
                Id = i + 1,
                X = (int)x,
                Y = (int)y
            };

            Hooks.Add(nail);

            currentAngle += angleIncrement;
        }
    }

    /// <summary>
    /// Generates the position of hooks, given a particular number of hooks, wheel pixel size, and hook pixel size.
    /// Creates 2 lists of positions (one for the anticlockwise side, one for the clockwise),
    /// and weaves them together so that the order they appear in
    /// the final output is the order of nodes anticlockwise around the frame.
    /// </summary>
    /// <param name="nailsCount"></param>
    /// <param name="wheelPixelSize"></param>
    /// <param name="hookPixelSize"></param>
    /// <returns></returns>
    public static double[,] GenerateHooks(int nailsCount, double wheelPixelSize, double hookPixelSize)
    {
        double r = (wheelPixelSize / 2) - 1;

        double[] theta = new double[nailsCount];
        for (int i = 0; i < nailsCount; i++)
        {
            theta[i] = (double)i / nailsCount * (2 * Math.PI);
        }

        double epsilon = Math.Asin(hookPixelSize / wheelPixelSize);

        double[] thetaAcw = new double[nailsCount];
        double[] thetaCw = new double[nailsCount];
        for (int i = 0; i < nailsCount; i++)
        {
            thetaAcw[i] = theta[i] + epsilon;
            thetaCw[i] = theta[i] - epsilon;
        }

        double[] thetaCombined = new double[2 * nailsCount];
        for (int i = 0; i < nailsCount; i++)
        {
            thetaCombined[i] = thetaCw[i];
            thetaCombined[i + nailsCount] = thetaAcw[i];
        }

        // anticlockwise
        double[] x = new double[2 * nailsCount];
        // clockwise
        double[] y = new double[2 * nailsCount];

        // FOR DEBUG
        // !!! Tetha acw cw
        for (int i = 0; i < 2 * nailsCount; i++)
        {
            x[i] = r * (1 + Math.Cos(thetaCombined[i])) + 0.5;
            y[i] = r * (1 + Math.Sin(thetaCombined[i])) + 0.5;
        }

        double[,] hooks = new double[2 * nailsCount, 2];
        for (int i = 0; i < 2 * nailsCount; i++)
        {
            hooks[i, 0] = x[i];
            hooks[i, 1] = y[i];
        }

        return hooks;
    }

    // change parameters to double
    public static List<Hook> GenerateHooks(int nailsCount, int wheelPixelSize, double hookPixelSize)
    {
        double r = (wheelPixelSize / 2) - 1;

        double[] theta = new double[nailsCount];
        for (int i = 0; i < nailsCount; i++)
        {
            theta[i] = (double)i / nailsCount * (2 * Math.PI);
        }

        double epsilon = Math.Asin(hookPixelSize / wheelPixelSize);

        double[] thetaAcw = new double[nailsCount];
        double[] thetaCw = new double[nailsCount];
        for (int i = 0; i < nailsCount; i++)
        {
            thetaAcw[i] = theta[i] + epsilon;
            thetaCw[i] = theta[i] - epsilon;
        }

        double[] thetaCombined = new double[2 * nailsCount];
        for (int i = 0; i < nailsCount; i++)
        {
            thetaCombined[i] = thetaCw[i];
            thetaCombined[i + nailsCount] = thetaAcw[i];
        }

        double[] x = new double[2 * nailsCount];
        double[] y = new double[2 * nailsCount];
        for (int i = 0; i < 2 * nailsCount; i++)
        {
            x[i] = r * (1 + Math.Cos(thetaCombined[i])) + 0.5;
            y[i] = r * (1 + Math.Sin(thetaCombined[i])) + 0.5;
        }

        List<Hook> hooks = new List<Hook>();
        for (int i = 0; i < 2 * nailsCount; i++)
        {
            hooks.Add(new Hook { Id = i, X = (int)x[i], Y = (int)y[i] });
        }

        return hooks;
    }

    /// <summary>
    /// Given 2 hooks, generates a list of pixels that the line connecting them runs through.
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <returns></returns>
    public static int[,] ThroughPixels(int[] p0, int[] p1)
    {
        int dx = p1[0] - p0[0];
        int dy = p1[1] - p0[1];
        int d = Math.Max((int)Math.Sqrt(dx * dx + dy * dy), 1);

        int[,] pixels = new int[d + 1, 2];

        for (int i = 0; i <= d; i++)
        {
            double ratio = (double)i / d;
            pixels[i, 0] = p0[0] + (int)(dx * ratio);
            pixels[i, 1] = p0[1] + (int)(dy * ratio);
        }

        return pixels;
    }

    public static int[,] ThroughPixels(Hook? startHook, Hook? endHook)
    {
        if (startHook is null || endHook is null)
            throw new Exception();

        int dx = endHook.X - startHook.X;
        int dy = endHook.Y - startHook.Y;

        // d = max(int(((p0[0]-p1[0])**2 + (p0[1]-p1[1])**2) ** 0.5), 1)

        int d = Math.Max((int)Math.Sqrt(dx * dx + dy * dy), 1);

        int[,] pixels = new int[d + 1, 2];

        for (int i = 0; i <= d; i++)
        {
            double ratio = (double)i / d;
            pixels[i, 0] = startHook.X + (int)(dx * ratio);
            pixels[i, 1] = startHook.Y + (int)(dy * ratio);
        }

        return pixels;
    }

    /// <summary>
    /// Uses ThroughPixels method to build up a dictionary of all possible lines connecting 2 hooks.
    /// Can be run at the start of a project, and doesn't need to be run again.
    /// Prints out an ongoing estimate of how much time is left.
    /// </summary>
    /// <param name="nHooks"></param>
    /// <param name="wheelPixelSize"></param>
    /// <param name="hookPixelSize"></param>
    /// <returns></returns>
    public Dictionary<Line, int[,]> BuildThroughPixelsDict(int nHooks, double wheelPixelSize,
        double hookPixelSize)
    {
        int nHookSides = nHooks * 2;

        List<Line> lines = new List<Line>();
        lines.Add(new Line(0, 1));
        for (int j = 0; j < nHookSides; j++)
        {
            for (int i = 0; i < j; i++)
            {
                if (j - i > 10 && j - i < (nHookSides - 10))
                {
                    lines.Add(new Line(i, j));
                }
            }
        }

        //int[] randomOrder = GetRandomOrder(lines.Count);
        var shuffledLines = GetShuffledList(lines);

        Dictionary<Line, int[,]> linePixels = new Dictionary<Line, int[,]>();
        double t0 = DateTime.Now.TimeOfDay.TotalSeconds;

        for (int i = 0; i < lines.Count; i++)
        {
            // fetch line with random index
            //(int, int) line = lines[randomOrder[i]];
            Line line = shuffledLines[i];

            // int[] p0 = _hooks[pair.Item1];
            // int[] p1 = _hooks[pair.Item2];

            var startHook = Hooks.FirstOrDefault(hook => hook.Id == line.Start);
            var endHook = Hooks.FirstOrDefault(hook => hook.Id == line.End);
            int[,] pixels = ThroughPixels(startHook, endHook);
            linePixels[line] = pixels;

            double t = DateTime.Now.TimeOfDay.TotalSeconds - t0;
            double tLeft = t * (lines.Count - i - 1) / (i + 1);
            Console.Write($"time left = {TimeSpan.FromSeconds(tLeft).ToString(@"mm\:ss")}\r");
        }

        return linePixels;
    }

    public double Fitness(int [,] image, Line line, int darkness, double lightnessPenalty, bool w, bool wPos,
        bool wNeg, string lineNormMode)
    {
        return 0;
    }
    
    public  Tuple<int[,], Line> OptimiseFitness(int[,] image, int previousEdge, int darkness,
        double lightnessPenalty,
        List<Line> listOfLines, bool w, bool wPos, bool wNeg, string lineNormMode, double timeSaver)
    {
        int nHooks = listOfLines.Count / 2;

        int startingEdge = (previousEdge % 2 == 0) ? previousEdge + 1 : previousEdge - 1;

        // int[] sidesA = Enumerable.Repeat(startingEdge, nHooks * 2).ToArray();
        // int[] sidesB = Enumerable.Range(0, nHooks * 2).ToArray();
        //
        // // array of lines 
        // var nextLines = sidesA.Zip(sidesB, (a, b) => new[] { a, b })
        //     .Where(line => Math.Abs(line[1] - line[0]) > 10 && Math.Abs(line[1] - line[0]) < nHooks * 2 - 10)
        //     .ToArray();

        // optimized
        List<Line> nextLines = Enumerable.Range(0, nHooks * 2)
            .Select(index => new Line(startingEdge, index))
            .Where(line => Math.Abs(line.End - line.Start) > 10 && Math.Abs(line.End - line.Start) < nHooks * 2 - 10)
            .ToList();


        if (timeSaver == 1)
        {
            nextLines = nextLines.ToList();
        }
        else
        {
            int nLinesToKeep = (int)(nextLines.Count * timeSaver);
            nextLines = nextLines.OrderBy(_ => Guid.NewGuid()).Take(nLinesToKeep).ToList();
        }

        // List<double> fitnessList
        var fitnessList = nextLines
            .Select(line => Fitness(image, line, darkness, lightnessPenalty, w, wPos, wNeg, lineNormMode))
            .ToList();

        int bestLineIdx = fitnessList.IndexOf(fitnessList.Max());
        Line bestLine = nextLines[bestLineIdx];

        int[,] pixels = ThroughPixelsDict[Tuple.Create(bestLine[0], bestLine[1])];

        for (int i = 0; i < pixels.GetLength(0); i++)
        {
            image[pixels[i, 0], pixels[i, 1]] -= darkness;
        }

        return Tuple.Create(image, bestLine);
    }
    

    public List<int[]> FindLines(int[,] image, int nLines, double darkness, double lightnessPenalty,
        string lineNormMode, bool[,] w = null, bool[,] wPos = null, bool[,] wNeg = null, double timeSaver = 1)
    {
        List<int[]> listOfLines = new List<int[]>();
        int previousEdge = new Random().Next(NailCount * 2);

        int[,] imageCopy = (int[,])image.Clone();

        double penalty;
        // string avgPenalty;
        for (int i = 0; i < nLines; i++)
        {
            // if (i == 0)
            // {
            //     // DateTime t0 = DateTime.Now;
            //     double initialPenalty = GetPenalty(imageCopy, lightnessPenalty, w, wPos, wNeg);
            //     string initialAvgPenalty = (initialPenalty / (wheelPixelSize * wheelPixelSize)).ToString("0.00");
            // }
            // else if (i % 100 == 0)
            // {
            //     // TimeSpan tSoFar = DateTime.Now - t0;
            //     // TimeSpan tLeft = TimeSpan.FromTicks((long)(tSoFar.Ticks * (nLines - i) / i));
            //     penalty = GetPenalty(image, lightnessPenalty, w, wPos, wNeg);
            //     // avgPenalty = (penalty / (wheelPixelSize * wheelPixelSize)).ToString("0.00");
            //     // Console.Write($"{i}/{nLines}, average penalty = {avgPenalty}/{initialAvgPenalty}, " +
            //     //               $"time = {tSoFar:mm\\:ss}, time left = {tLeft:mm\\:ss}    \r");
            // }

            Tuple<int[,], int[]> result = OptimiseFitness(imageCopy, previousEdge, darkness, lightnessPenalty,
                listOfLines,
                w, wPos, wNeg, lineNormMode, timeSaver);

            image = result.Item1;
            int[] line = result.Item2;
            previousEdge = line[1];

            listOfLines.Add(line);
        }

        // Console.Clear();

        // penalty = GetPenalty(imageCopy, lightnessPenalty, w, wPos, wNeg);
        // avgPenalty = (penalty / (wheelPixelSize * wheelPixelSize)).ToString("0.00");
        // Console.WriteLine($"{listOfLines.Count}/{nLines}, average penalty = {avgPenalty}/{initialAvgPenalty}");
        // Console.WriteLine("time = " + (DateTime.Now - t0).ToString(@"mm\:ss"));

        return listOfLines;
    }

    public static double GetPenalty(int[,] image, double lightnessPenalty, bool[,] w = null, bool[,] wPos = null,
        bool[,] wNeg = null)
    {
        int[] shape = new int[] { image.GetLength(0), image.GetLength(1) };

        int size = shape[0] * shape[1];

        int[] flattenedImage = new int[size];

        for (int i = 0; i < shape[0]; i++)
        {
            for (int j = 0; j < shape[1]; j++)
            {
                flattenedImage[i * shape[1] + j] = image[i, j];
            }
        }

        double penalty = 0;

        if (w == null && wPos == null)
        {
            penalty = flattenedImage.Sum() - (1 + lightnessPenalty) * flattenedImage.Where(x => x < 0).Sum();
        }
        else if (wPos == null)
        {
            double[] weightedImage = new double[size];

            for (int i = 0; i < size; i++)
            {
                weightedImage[i] = flattenedImage[i] * (w[i / shape[1], i % shape[1]] ? 1 : 0);
            }

            penalty = weightedImage.Sum() - (1 + lightnessPenalty) * weightedImage.Where(x => x < 0).Sum();
        }
        else if (w == null)
        {
            double[] weightedPositiveImage = new double[size];
            double[] weightedNegativeImage = new double[size];

            for (int i = 0; i < size; i++)
            {
                weightedPositiveImage[i] = flattenedImage[i] * (wPos[i / shape[1], i % shape[1]] ? 1 : 0);
                weightedNegativeImage[i] = flattenedImage[i] * (wNeg[i / shape[1], i % shape[1]] ? 1 : 0);
            }

            penalty = weightedPositiveImage.Where(x => x > 0).Sum() -
                      lightnessPenalty * weightedNegativeImage.Where(x => x < 0).Sum();
        }

        return penalty;
    }


    public int GetLineLightness(List<Point> line)
    {
        float lightness = 0;

        foreach (var point in line)
            lightness += _image.GetPixel(point.X, point.Y).GetBrightness();

        return (int)(lightness / line.Count);
    }

    public (List<Point>?, Hook) GetNextNail(Hook hook)
    {
        var nextNail = hook;
        List<Point> nextLine = null;
        var minLightness = long.MaxValue;

        foreach (var borderNail in Hooks)
            if (borderNail.Id != hook.Id)
            {
                var line = RasterizeLine(borderNail.X, borderNail.Y, hook.X, hook.Y);
                var lightness = GetLineLightness(line);
                if (lightness < minLightness)
                {
                    minLightness = lightness;
                    nextNail.Id = borderNail.Id;
                    nextLine = line;
                }
            }

        return (nextLine, nextNail);
    }

    /// <summary>
    /// Returns a shuffled version of that list using LINQ
    /// </summary>
    /// <param name="list"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<T> GetShuffledList<T>(List<T> list)
    {
        Random random = new Random();
        return list.OrderBy(x => random.Next()).ToList();
    }
    
    /// <summary>
    /// Is intended to generate a random order of numbers from 0 to count - 1.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public static int[] GetRandomOrder(int count)
    {
        Random random = new Random();
        int[] order = new int[count];
        for (int i = 0; i < count; i++)
        {
            order[i] = i;
        }

        for (int i = count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            int temp = order[i];
            order[i] = order[j];
            order[j] = temp;
        }

        return order;
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