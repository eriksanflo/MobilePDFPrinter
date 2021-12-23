using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace PDFPrinter.Droid.PrinterService
{
    public class EscPrinter
    {
        public IEnumerable<byte> GetImageBytes(string imagePath)
        {
            BitmapData data = GetBitmapData(imagePath);
            BitArray dots = data.Dots;

            byte[] width = BitConverter.GetBytes(data.Width);
            byte[] bytes;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write((char)0x1B); // ESC Space
                    binaryWriter.Write('@'); //Initialize printer
                    binaryWriter.Write((char)0x1B); // ESC Space
                    binaryWriter.Write('3'); // Set line feed amount
                    binaryWriter.Write((byte)24);

                    int offset = 0;
                    while (offset < data.Height)
                    {
                        binaryWriter.Write((char)0x1B);
                        binaryWriter.Write('*'); // bit-image mode
                        binaryWriter.Write((byte)33); // 24-dot double-density
                        binaryWriter.Write(width[0]); // width low byte
                        binaryWriter.Write(width[1]); // width high byte

                        for (int x = 0; x < data.Width; ++x)
                        {
                            for (int k = 0; k < 3; ++k)
                            {
                                byte slice = 0;
                                for (int b = 0; b < 8; ++b)
                                {
                                    int y = (offset / 8 + k) * 8 + b;
                                    // Calculate the location of the pixel we want in the bit array.
                                    // It'll be at (y * width) + x.
                                    int i = (y * data.Width) + x;

                                    // If the image is shorter than 24 dots, pad with zero.
                                    bool v = false;
                                    if (i < dots.Length)
                                    {
                                        v = dots[i];
                                    }

                                    slice |= (byte)((v ? 1 : 0) << (7 - b));
                                }

                                binaryWriter.Write(slice);
                            }
                        }

                        offset += 24;
                        binaryWriter.Write((char)0x0A);
                    }

                    // Restore the line spacing to the default of 30 dots.
                    binaryWriter.Write((char)0x1B);
                    binaryWriter.Write('3');
                    binaryWriter.Write((byte)10);
                }

                bytes = stream.ToArray();
            }

            return bytes;
        }
        //public IEnumerable<byte> GetImageBytes(string imagePath)
        //{
        //    BitmapData data = GetBitmapData(imagePath);
        //    BitArray dots = data.Dots;

        //    byte[] width = BitConverter.GetBytes(data.Width);
        //    byte[] bytes;
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        using (BinaryWriter binaryWriter = new BinaryWriter(stream))
        //        {
        //            binaryWriter.Write((char)0x1B);
        //            binaryWriter.Write('@');
        //            binaryWriter.Write((char)0x1B);
        //            binaryWriter.Write('3');
        //            binaryWriter.Write((byte)24);

        //            int offset = 0;
        //            while (offset < data.Height)
        //            {
        //                binaryWriter.Write((char)0x1B);
        //                binaryWriter.Write('*'); // bit-image mode
        //                binaryWriter.Write((byte)33); // 24-dot double-density
        //                binaryWriter.Write(width[0]); // width low byte
        //                binaryWriter.Write(width[1]); // width high byte

        //                for (int x = 0; x < data.Width; ++x)
        //                {
        //                    for (int k = 0; k < 3; ++k)
        //                    {
        //                        byte slice = 0;
        //                        for (int b = 0; b < 8; ++b)
        //                        {
        //                            int y = (offset / 8 + k) * 8 + b;
        //                            // Calculate the location of the pixel we want in the bit array.
        //                            // It'll be at (y * width) + x.
        //                            int i = (y * data.Width) + x;

        //                            // If the image is shorter than 24 dots, pad with zero.
        //                            bool v = false;
        //                            if (i < dots.Length)
        //                            {
        //                                v = dots[i];
        //                            }

        //                            slice |= (byte)((v ? 1 : 0) << (7 - b));
        //                        }

        //                        binaryWriter.Write(slice);
        //                    }
        //                }

        //                offset += 24;
        //                binaryWriter.Write((char)0x0A);
        //            }

        //            // Restore the line spacing to the default of 30 dots.
        //            binaryWriter.Write((char)0x1B);
        //            binaryWriter.Write('3');
        //            binaryWriter.Write((byte)30);
        //        }

        //        bytes = stream.ToArray();
        //    }

        //    return bytes;
        //}
        private static BitmapData GetBitmapData(string imagePath)
        {
            byte[] bytes = File.ReadAllBytes(imagePath);

            // Wrap the bytes in a stream
            using (SKBitmap imageBitmap = SKBitmap.Decode(bytes))
            {
                int index = 0;
                const int threshold = 127;
                const double multiplier = 350; // This depends on your printer model. for Beiyang you should use 1000
                double scale = multiplier / imageBitmap.Width;

                int height = (int)(imageBitmap.Height * scale);
                int width = (int)(imageBitmap.Width * scale);
                int dimensions = width * height;

                BitArray dots = new BitArray(dimensions);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int scaledX = (int)(x / scale);
                        int scaledY = (int)(y / scale);

                        SKColor color = imageBitmap.GetPixel(scaledX, scaledY);

                        // Luminance means Lightning, We can make output brighter or darker with it.
                        int luminance = (int)(color.Red * 0.3 + color.Green * 0.16 + color.Blue * 0.114);
                        dots[index] = luminance < threshold;
                        index++;
                    }
                }

                return new BitmapData
                {
                    Dots = dots,
                    Height = (int)(imageBitmap.Height * scale),
                    Width = (int)(imageBitmap.Width * scale)
                };
            }
        }
        //private static BitmapData GetBitmapData(string imagePath)
        //{
        //    byte[] bytes = File.ReadAllBytes(imagePath);

        //    // Wrap the bytes in a stream
        //    using (SKBitmap imageBitmap = SKBitmap.Decode(bytes))
        //    {
        //        int index = 0;
        //        const int threshold = 127;
        //        const double multiplier = 570; // This depends on your printer model. for Beiyang you should use 1000
        //        double scale = multiplier / imageBitmap.Width;

        //        int height = (int)(imageBitmap.Height * scale);
        //        int width = (int)(imageBitmap.Width * scale);
        //        int dimensions = width * height;

        //        BitArray dots = new BitArray(dimensions);

        //        for (int y = 0; y < height; y++)
        //        {
        //            for (int x = 0; x < width; x++)
        //            {
        //                int scaledX = (int)(x / scale);
        //                int scaledY = (int)(y / scale);

        //                SKColor color = imageBitmap.GetPixel(scaledX, scaledY);

        //                // Luminance means Lightning, We can make output brighter or darker with it.
        //                int luminance = (int)(color.Red * 0.3 + color.Green * 0.16 + color.Blue * 0.114);
        //                dots[index] = luminance < threshold;
        //                index++;
        //            }
        //        }

        //        return new BitmapData
        //        {
        //            Dots = dots,
        //            Height = (int)(imageBitmap.Height * scale),
        //            Width = (int)(imageBitmap.Width * scale)
        //        };
        //    }
        //}
    }
}