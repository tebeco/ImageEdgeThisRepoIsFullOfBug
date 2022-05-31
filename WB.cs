using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

public class WB
{
    static int GetPixelIndex(int x, int y, int stride) =>
                y * stride + x * 4;

    unsafe static int GetPixelColor(IntPtr backBuffer, int x, int y, int stride)
    {
        var index = GetPixelIndex(x, y, stride);
        var data = (byte*)backBuffer.ToPointer();
        byte b = data[index + 0];
        byte g = data[index + 1];
        byte r = data[index + 2];
        byte a = data[index + 3];

        return a << 24 | r << 16 | g << 8 | b;
    }

    private static Random rnd = new Random();
    unsafe static void SetPixelColor(IntPtr backBuffer, int index)
    {
        var data = (byte*)backBuffer.ToPointer();

        data[index + 0] = 0;
        data[index + 1] = 0;
        data[index + 2] = 255;
        data[index + 3] = (byte)255; // A
    }

    public static void Do()
    {
        var sw = Stopwatch.StartNew();

        BitmapImage inputBitmap = new BitmapImage(new Uri("Test2.png", UriKind.Relative));
        WriteableBitmap inputWb = new WriteableBitmap(inputBitmap);
        var stencil = new Dictionary<(int, int), bool>(10000);

        inputWb.Lock();
        IntPtr backBuffer = inputWb.BackBuffer;


        for (int y = 0; y < inputWb.Height - 2; y++)
        {
            for (int x = 0; x < inputWb.Width - 2; x++)
            {
                var currentPixelColor = GetPixelColor(inputWb.BackBuffer, x, y, inputWb.BackBufferStride);
                var rightPixelColor = GetPixelColor(inputWb.BackBuffer, x + 1, y, inputWb.BackBufferStride);
                var bottomPixelColor = GetPixelColor(inputWb.BackBuffer, x, y + 1, inputWb.BackBufferStride);

                if (currentPixelColor != rightPixelColor || currentPixelColor != bottomPixelColor)
                {
                    if (!stencil.TryGetValue((x, y), out var marked))
                    {
                        stencil[(x, y)] = true;
                    }

                    if (currentPixelColor != rightPixelColor && !stencil.TryGetValue((x + 1, y), out var marked2))
                    {
                        stencil[(x + 1, y)] = true;
                    }
                    else if (!stencil.TryGetValue((x, y + 1), out var marked3))
                    {
                        stencil[(x, y + 1)] = true;
                    }
                }
            }
        }

        inputWb.Unlock();

        BitmapImage outputBitmap = new BitmapImage(new Uri("Test2.png", UriKind.Relative));
        WriteableBitmap outputWb = new WriteableBitmap(outputBitmap);

        outputWb.Lock();
        foreach (var (x, y) in stencil.Keys)
        {
            var pixelIndex = GetPixelIndex(x, y, outputWb.BackBufferStride);
            SetPixelColor(outputWb.BackBuffer, pixelIndex);
        }
        outputWb.Unlock();

        Console.WriteLine("Before save: {0}", sw.Elapsed);
        using var fileStream = new FileStream("output.png", FileMode.Create);
        PngBitmapEncoder encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(outputWb));
        encoder.Save(fileStream);

        Console.WriteLine("After Save: {0}", sw.Elapsed);


        // outputWb.Save("Outlined.png");
    }
}