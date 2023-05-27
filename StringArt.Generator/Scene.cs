using System.Drawing;

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
    public List<Nail> Nails { get; set; }

    public int Radius { get; set; }

    public void InitializeNailsOnCircleBorder()
    {
        Nails = new List<Nail>();

        var angleIncrement = 2 * Math.PI / NailCount;
        double currentAngle = 0;

        for (var i = 0; i < NailCount; i++)
        {
            var x = x0 + Radius * Math.Cos(currentAngle);
            var y = y0 + Radius * Math.Sin(currentAngle);

            var nail = new Nail
            {
                Id = i + 1,
                X = (int)x,
                Y = (int)y
            };

            Nails.Add(nail);

            currentAngle += angleIncrement;
        }
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

    public int GetLineLightness(List<Point> line)
    {
        float lightness = 0;

        foreach (var point in line)
            lightness += _image.GetPixel(point.X, point.Y).GetBrightness();

        return (int)(lightness / line.Count);
    }

    public (List<Point>?, Nail) GetNextNail(Nail nail)
    {
        var nextNail = nail;
        List<Point> nextLine = null;
        var minLightness = long.MaxValue;

        foreach (var borderNail in Nails)
            if (borderNail.Id != nail.Id)
            {
                var line = RasterizeLine(borderNail.X, borderNail.Y, nail.X, nail.Y);
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
    
    

}

public enum ScenePattern
{
    Circle,
    Rectangle
}