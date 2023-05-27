

using System.Drawing;
using System.Drawing.Imaging;
using StringArt.Generator;
using StringArt.ImageProcessor;

Console.WriteLine("Hello, World!");

// Bitmap myBitmap = new Bitmap(@"C:\Users\kadde\Downloads\Telegram Desktop\IMG_3575.JPG");
var bytes = File.ReadAllBytes(@"C:\Users\kadde\Desktop\output.png");
IImageProcessor imageProcessor = new ImageProcessor(bytes);
var myBitmap = imageProcessor.GetProcessedImage();
SaveBitmap(myBitmap, "output.jpeg", ImageFormat.Jpeg);

 static void SaveBitmap(Bitmap bitmap, string filePath, ImageFormat format)
{
    bitmap.Save(filePath, format);
}

using var memoryStream = new MemoryStream(bytes);
var bmp = new Bitmap(memoryStream);
var gen = new StringArtGenerator();

gen.LoadImage(@"C:\Users\kadde\Desktop\output.png");
gen.Preprocess();
gen.SetNails(200);
gen.SetSeed(42);
gen.SetIterations(2000);
var pattern = gen.Generate();
foreach (var index in pattern)
{
    Console.Write($"{index}, ");
}


