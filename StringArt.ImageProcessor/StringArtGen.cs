using System;
using System.Collections.Generic;
using System.Drawing;
namespace StringArt.ImageProcessor;

using System;
using System.Collections.Generic;
using System.Drawing;

public class StringArtGenerator1
{
    private Bitmap image;
    private List<Point> nails;
    private List<int> sequence;

    public int[] GenerateStringArt(Bitmap bitmap)
    {
        // Initialize variables
        image = bitmap;
        nails = new List<Point>();
        sequence = new List<int>();

        // Initialize nails
        InitNails();

        // Generate string art
        Generate();

        // Return the sequence of nail indexes
        return sequence.ToArray();
    }

    private void InitNails()
    {
        // Add nails at regular intervals on the image
        int width = image.Width;
        int height = image.Height;
        int spacing = 10; // Adjust the spacing as desired

        for (int y = 0; y < height; y += spacing)
        {
            for (int x = 0; x < width; x += spacing)
            {
                nails.Add(new Point(x, y));
            }
        }
    }

    private void Generate()
    {
        // Generate the string art by connecting nails based on the image brightness
        for (int i = 0; i < nails.Count; i++)
        {
            Point nail = nails[i];
            int nextNail = GetNextNail(nail);
            sequence.Add(nextNail);
        }
    }

    private int GetNextNail(Point nail)
    {
        int nextNail = -1;
        double minBrightness = double.MaxValue;

        for (int i = 0; i < nails.Count; i++)
        {
            if (i == nails.IndexOf(nail))
                continue;

            Point nextNailPos = nails[i];
            double brightness = GetBrightness(nail, nextNailPos);

            if (brightness < minBrightness)
            {
                minBrightness = brightness;
                nextNail = i;
            }
        }

        return nextNail;
    }

    private double GetBrightness(Point p1, Point p2)
    {
        // Calculate the brightness difference between two points
        Color color1 = image.GetPixel(p1.X, p1.Y);
        Color color2 = image.GetPixel(p2.X, p2.Y);

        double brightness1 = (color1.R + color1.G + color1.B) / 3.0;
        double brightness2 = (color2.R + color2.G + color2.B) / 3.0;

        return Math.Abs(brightness1 - brightness2);
    }
}

