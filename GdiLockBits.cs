// using System.Diagnostics;
// using System.Drawing;
// using System.Drawing.Imaging;

// public class GdiLockBit
// {
//     public static void Do()
//     {
//         int GetPixelIndex(int x, int y, int stride) =>
//                         y * stride + x * 4;

//         int GetPixelColor(Span<byte> span, int x, int y, int stride)
//         {
//             var index = GetPixelIndex(x, y, stride);

//             var r = span[index + 3];
//             var g = span[index + 2];
//             var b = span[index + 1];
//             var a = span[index + 0];

//             return a << 24 | r << 16 | g << 8 | b;
//         }

//         void SetPixelColor(Span<byte> span, int index)
//         {
//             span[index + 0] = (byte)000; // B
//             span[index + 1] = (byte)000; // G
//             span[index + 2] = (byte)255; // R
//             span[index + 3] = (byte)255; // A
//         }

//         unsafe
//         {
//             var sw = Stopwatch.StartNew();
//             var input = new Bitmap("Test2.png");
//             var output = new Bitmap(input);

//             Console.WriteLine($"Image size: {input.Width}x{input.Height}");
//             var rect = new Rectangle(0, 0, input.Width, input.Height);

//             var stencil = new Dictionary<(int, int), bool>(10000);

//             var inputBitmapData = input.LockBits(rect, ImageLockMode.ReadWrite, input.PixelFormat);
//             var outputBitmapData = output.LockBits(rect, ImageLockMode.ReadWrite, input.PixelFormat);

//             var inputSpan = new Span<byte>(inputBitmapData.Scan0.ToPointer(), Math.Abs(inputBitmapData.Stride) * input.Height);
//             var outputSpan = new Span<byte>(inputBitmapData.Scan0.ToPointer(), Math.Abs(inputBitmapData.Stride) * input.Height);


//             for (int y = 0; y < input.Height - 2; y++)
//             {
//                 for (int x = 0; x < input.Width - 2; x++)
//                 {
//                     try
//                     {
//                         var currentPixelColor = GetPixelColor(inputSpan, x, y, inputBitmapData.Stride);
//                         var rightPixelColor = GetPixelColor(inputSpan, x + 1, y, inputBitmapData.Stride);
//                         var bottomPixelColor = GetPixelColor(inputSpan, x, y + 1, inputBitmapData.Stride);

//                         if (currentPixelColor != rightPixelColor || currentPixelColor != bottomPixelColor)
//                         {
//                             if (!stencil.TryGetValue((x, y), out var marked))
//                             {
//                                 stencil[(x, y)] = true;
//                             }

//                             if (currentPixelColor != rightPixelColor && !stencil.TryGetValue((x + 1, y), out var marked2))
//                             {
//                                 stencil[(x + 1, y)] = true;
//                             }
//                             else if (!stencil.TryGetValue((x, y + 1), out var marked3))
//                             {
//                                 stencil[(x, y + 1)] = true;
//                             }
//                         }
//                     }
//                     catch (Exception ex)
//                     {
//                         Console.WriteLine(ex.Message);
//                     }
//                 }
//             }

//             foreach (var (x, y) in stencil.Keys)
//             {
//                 var pixelIndex = GetPixelIndex(x, y, outputBitmapData.Stride);
//                 SetPixelColor(outputSpan, pixelIndex);
//             }

//             input.UnlockBits(inputBitmapData);

//             Console.WriteLine("Before save: {0}", sw.Elapsed);
//             input.Save("Outlined.png");
//             Console.WriteLine("After Save: {0}", sw.Elapsed);
//         }
//     }
// }