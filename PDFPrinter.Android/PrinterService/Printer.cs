using Android.Bluetooth;
using Android.Content;
using Android.Graphics;
using Android.Print;
using Java.Util;
using PDFPrinter.Droid.PrinterService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(PDFPrinter.Droid.Printer))]
namespace PDFPrinter.Droid
{
    public class Printer : IPrinter
    {
        public static byte[] SELECT_BIT_IMAGE_MODE = { 0x1B, 0x2A, 33, (byte)255, 3 };
        public IList<string> GetDeviceList()
        {
            using (BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter)
            {
                var btdevice = bluetoothAdapter?.BondedDevices.Select(i => i.Name).ToList();
                return btdevice;
            }
        }

        public void ClearDirectory()
        {
            var context = Android.App.Application.Context;
            var pathToNewFolder = System.IO.Path.Combine(context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).ToString());
            var files = Directory.GetFiles(pathToNewFolder, "*.pdf");
            foreach (var filePath in files)
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        public void CreateDirectory()
        {
            var context = Android.App.Application.Context;
            var pathToNewFolder = System.IO.Path.Combine(context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).ToString());
            Directory.CreateDirectory(pathToNewFolder);
        }

        public async Task Print(string deviceName, string text)
        {
            using (BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter)
            {
                BluetoothDevice device = (from bd in bluetoothAdapter?.BondedDevices
                                          where bd?.Name == deviceName
                                          select bd).FirstOrDefault();
                try
                {
                    using (BluetoothSocket bluetoothSocket = device?.
                        CreateRfcommSocketToServiceRecord(
                        UUID.FromString("00001101-0000-1000-8000-00805f9b34fb")))
                    {
                        bluetoothSocket?.Connect();
                        byte[] buffer = Encoding.UTF8.GetBytes(text);
                        bluetoothSocket?.OutputStream.Write(buffer, 0, buffer.Length);
                        bluetoothSocket.Close();
                    }
                }
                catch (Exception exp)
                {
                    throw exp;
                }
            }
        }

        public async Task Print(string deviceName, List<string> lines)
        {
            using (BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter)
            {
                BluetoothDevice device = (from bd in bluetoothAdapter?.BondedDevices
                                          where bd?.Name == deviceName
                                          select bd).FirstOrDefault();
                try
                {
                    using (BluetoothSocket bluetoothSocket = device?.
                        CreateRfcommSocketToServiceRecord(
                        UUID.FromString("00001101-0000-1000-8000-00805f9b34fb")))
                    {
                        bluetoothSocket?.Connect();
                        List<byte> outputLines = new List<byte>();
                        outputLines.Add(0x0A);
                        foreach (var txt in lines)
                        {
                            outputLines.AddRange(Encoding.UTF8.GetBytes(txt));
                            outputLines.Add(0x0A);
                        }
                        outputLines.Add(0x0A);
                        outputLines.Add(0x0A);
                        outputLines.Add(0x0A);
                        byte[] buffer = outputLines.ToArray();
                        bluetoothSocket?.OutputStream.Write(buffer, 0, buffer.Length);
                        bluetoothSocket.Close();
                    }
                }
                catch (Exception exp)
                {
                    throw exp;
                }
            }
        }

        public async Task Print(string deviceName, string logoName, List<string> lines)
        {
            using (BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter)
            {
                BluetoothDevice device = (from bd in bluetoothAdapter?.BondedDevices
                                          where bd?.Name == deviceName
                                          select bd).FirstOrDefault();
                try
                {
                    using (BluetoothSocket bluetoothSocket = device?.
                        CreateRfcommSocketToServiceRecord(
                        UUID.FromString("00001101-0000-1000-8000-00805f9b34fb")))
                    {
                        bluetoothSocket?.Connect();
                        List<byte> outputLines = new List<byte>();
                        outputLines.Add(0x0A);
                        if (!string.IsNullOrEmpty(logoName))
                        {
                            var context = Android.App.Application.Context;
                            var folder = System.IO.Path.Combine(context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).ToString());
                            var path = System.IO.Path.Combine(folder, logoName);
                            if (File.Exists(path))
                            {
                                var imageBytes = new EscPrinter();
                                outputLines.AddRange(imageBytes.GetImageBytes(path));
                            }
                        }

                        foreach (var txt in lines)
                        {
                            outputLines.AddRange(Encoding.UTF8.GetBytes(txt));
                            outputLines.Add(0x0A);
                        }
                        outputLines.Add(0x0A);
                        outputLines.Add(0x0A);
                        outputLines.Add(0x0A);
                        byte[] buffer = outputLines.ToArray();
                        bluetoothSocket?.OutputStream.Write(buffer, 0, buffer.Length);
                        bluetoothSocket.Close();
                    }
                }
                catch (Exception exp)
                {
                    throw exp;
                }
            }
        }

        public async Task Print(string deviceName, byte[] data)
        {
            using (BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter)
            {
                BluetoothDevice device = (from bd in bluetoothAdapter?.BondedDevices
                                          where bd?.Name == deviceName
                                          select bd).FirstOrDefault();
                try
                {
                    using (BluetoothSocket bluetoothSocket = device?.
                        CreateRfcommSocketToServiceRecord(
                        UUID.FromString("00001101-0000-1000-8000-00805f9b34fb")))
                    {
                        List<byte> outputList = new List<byte>();
                        outputList.AddRange(SELECT_BIT_IMAGE_MODE);
                        outputList.AddRange(data);
                        var buffer = outputList.ToArray();
                        
                        bluetoothSocket?.Connect();
                        bluetoothSocket?.OutputStream.Write(buffer, 0, buffer.Length);
                        bluetoothSocket.Close();
                    }
                }
                catch (Exception exp)
                {
                    throw exp;
                }
            }
        }

        public async Task PrintByImagePath(string deviceName, string imagePath)
        {
            TimeSpan timeout = TimeSpan.FromSeconds(5);
            using (BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter)
            {
                BluetoothDevice device = (from bd in bluetoothAdapter?.BondedDevices
                                          where bd?.Name == deviceName
                                          select bd).FirstOrDefault();
                try
                {
                    using (BluetoothSocket bluetoothSocket = device?.
                        CreateRfcommSocketToServiceRecord(
                        UUID.FromString("00001101-0000-1000-8000-00805f9b34fb")))
                    {
                        byte[] cutCommand = { 29, 86, 1 };

                        List<byte> outputList = new List<byte>();

                        //outputList.AddRange(GetImageBytes(imagePath));
                        
                        outputList.AddRange(SELECT_BIT_IMAGE_MODE);
                        outputList.AddRange(LoadImage(imagePath));
                        outputList.AddRange(cutCommand);

                        var buffer = outputList.ToArray();

                        bluetoothSocket?.Connect();
                        bluetoothSocket?.OutputStream.Write(buffer, 0, buffer.Length);
                        bluetoothSocket.Close();
                    }
                }
                catch (Exception exp)
                {
                    throw exp;
                }
            }
        }

        #region Download And Print

        
        public void Print(Stream inputStream, string fileName)
        {
            var ticketName = $"{DateTime.Now.ToString("yyyyMMhhmmss")}.pdf";
            var context = Android.App.Application.Context;
            var pathToNewFolder = System.IO.Path.Combine(context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).ToString());
            Directory.CreateDirectory(pathToNewFolder);
            string pathToNewFile = System.IO.Path.Combine(pathToNewFolder, "Ticket.pdf");
            try
            {
                //WebClient webClient = new WebClient();
                //webClient.DownloadFileCompleted += (s, e) =>
                //{
                    
                //};
                //string pathToNewFile = System.IO.Path.Combine(pathToNewFolder, ticketName);
                //webClient.DownloadFileAsync(new Uri(url), pathToNewFile);
            }
            catch (Exception ex)
            {
                //if (OnFileDownloaded != null)
                //{
                //    OnFileDownloaded.Invoke(this, new DownloadEventArgs(false));
                //}
            }

            if (inputStream.CanSeek)
                inputStream.Position = 0;
            string createdFilePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), fileName);
            using (var dest = System.IO.File.OpenWrite(createdFilePath))
                inputStream.CopyTo(dest);
            string filePath = createdFilePath;
            var activity = Xamarin.Essentials.Platform.CurrentActivity;
            PrintManager printManager = (PrintManager)activity.GetSystemService(Context.PrintService);
            PrintDocumentAdapter pda = new CustomPrintDocumentAdapter(filePath);
            //Print with null PrintAttributes
            printManager.Print(fileName, pda, null);
        }

        #endregion
        public void PrintByPath(string path, string fileName)
        {
            var activity = Xamarin.Essentials.Platform.CurrentActivity;
            PrintManager printManager = (PrintManager)activity.GetSystemService(Context.PrintService);
            try
            {
                PrintDocumentAdapter adapter = new DroidPrintPDFAdapter(activity, path, fileName);
                printManager.Print("Document", adapter, null);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public void PrintErik(string deviceName, string uri)
        {
            System.Net.WebClient webClient = new System.Net.WebClient();
            webClient.DownloadDataCompleted += (s, e) =>
            {
                if (e.Error == null)
                {
                    var data = e.Result;
                    Bitmap bitmap = BitmapFactory.DecodeByteArray(e.Result, 0, e.Result.Length);

                    MemoryStream stream = new MemoryStream();
                    byte[] imageData = ImageToByte2(bitmap);
                    stream.Write(imageData, 0, imageData.Length);
                    stream.Write(SELECT_BIT_IMAGE_MODE, 0, SELECT_BIT_IMAGE_MODE.Length);
                    var bytes = stream.ToArray();

                    using (BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter)
                    {
                        BluetoothDevice device = (from bd in bluetoothAdapter?.BondedDevices
                                                  where bd?.Name == deviceName
                                                  select bd).FirstOrDefault();
                        try
                        {
                            using (BluetoothSocket bluetoothSocket = device?.
                                CreateRfcommSocketToServiceRecord(
                                UUID.FromString("00001101-0000-1000-8000-00805f9b34fb")))
                            {
                                bluetoothSocket?.Connect();
                                bluetoothSocket?.OutputStream.Write(bytes, 0, bytes.Length);
                                bluetoothSocket.Close();
                            }
                        }
                        catch (Exception exp)
                        {
                            throw exp;
                        }
                    }
                }
            };
            var url = new Uri(uri);
            webClient.DownloadDataAsync(url);
        }

        private static IEnumerable<byte> GetImageBytes(string imagePath)
        {
            //BitmapData data = GetBitmapData(imagePath);
            //BitArray dots = data.Dots;

            //byte[] width = BitConverter.GetBytes(data.Width);
            byte[] bytes = new byte[1024];
            //using (MemoryStream stream = new MemoryStream())
            //{
            //    using (BinaryWriter binaryWriter = new BinaryWriter(stream))
            //    {
            //        binaryWriter.Write((char)0x1B);
            //        binaryWriter.Write('@');
            //        binaryWriter.Write((char)0x1B);
            //        binaryWriter.Write('3');
            //        binaryWriter.Write((byte)24);

            //        int offset = 0;
            //        while (offset < data.Height)
            //        {
            //            binaryWriter.Write((char)0x1B);
            //            binaryWriter.Write('*'); // bit-image mode
            //            binaryWriter.Write((byte)33); // 24-dot double-density
            //            binaryWriter.Write(width[0]); // width low byte
            //            binaryWriter.Write(width[1]); // width high byte

            //            for (int x = 0; x < data.Width; ++x)
            //            {
            //                for (int k = 0; k < 3; ++k)
            //                {
            //                    byte slice = 0;
            //                    for (int b = 0; b < 8; ++b)
            //                    {
            //                        int y = (offset / 8 + k) * 8 + b;
            //                        // Calculate the location of the pixel we want in the bit array.
            //                        // It'll be at (y * width) + x.
            //                        int i = (y * data.Width) + x;

            //                        // If the image is shorter than 24 dots, pad with zero.
            //                        bool v = false;
            //                        if (i < dots.Length)
            //                        {
            //                            v = dots[i];
            //                        }

            //                        slice |= (byte)((v ? 1 : 0) << (7 - b));
            //                    }

            //                    binaryWriter.Write(slice);
            //                }
            //            }

            //            offset += 24;
            //            binaryWriter.Write((char)0x0A);
            //        }

            //        // Restore the line spacing to the default of 30 dots.
            //        binaryWriter.Write((char)0x1B);
            //        binaryWriter.Write('3');
            //        binaryWriter.Write((byte)30);
            //    }

            //    bytes = stream.ToArray();
            //}

            return bytes;
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

        public string GetLogo()
        {
            string logo = "";
            if (!File.Exists(@"C:\bitmap.bmp"))
                return null;
            BitmapData data = GetBitmapData(@"C:\bitmap.bmp");
            BitArray dots = data.Dots;
            byte[] width = BitConverter.GetBytes(data.Width);

            int offset = 0;
            MemoryStream stream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write((char)0x1B);
            bw.Write('@');

            bw.Write((char)0x1B);
            bw.Write('3');
            bw.Write((byte)24);

            while (offset < data.Height)
            {
                bw.Write((char)0x1B);
                bw.Write('*');         // bit-image mode
                bw.Write((byte)33);    // 24-dot double-density
                bw.Write(width[0]);  // width low byte
                bw.Write(width[1]);  // width high byte

                for (int x = 0; x < data.Width; ++x)
                {
                    for (int k = 0; k < 3; ++k)
                    {
                        byte slice = 0;
                        for (int b = 0; b < 8; ++b)
                        {
                            int y = (((offset / 8) + k) * 8) + b;
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

                        bw.Write(slice);
                    }
                }
                offset += 24;
                bw.Write((char)0x0A);
            }
            // Restore the line spacing to the default of 30 dots.
            bw.Write((char)0x1B);
            bw.Write('3');
            bw.Write((byte)30);

            bw.Flush();
            byte[] bytes = stream.ToArray();
            return logo + Encoding.Default.GetString(bytes);
        }

        public static BitmapData GetBitmapData(string bmpFileName)
        {
            //using (var bitmap = (Bitmap)Bitmap.FromFile(bmpFileName))
            //{
            //    var threshold = 127;
            //    var index = 0;
            //    double multiplier = 570; // this depends on your printer model. for Beiyang you should use 1000
            //    double scale = (double)(multiplier / (double)bitmap.Width);
            //    int xheight = (int)(bitmap.Height * scale);
            //    int xwidth = (int)(bitmap.Width * scale);
            //    var dimensions = xwidth * xheight;
            //    var dots = new BitArray(dimensions);

            //    for (var y = 0; y < xheight; y++)
            //    {
            //        for (var x = 0; x < xwidth; x++)
            //        {
            //            var _x = (int)(x / scale);
            //            var _y = (int)(y / scale);
            //            var color = bitmap.GetPixel(_x, _y);
            //            var luminance = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
            //            dots[index] = (luminance < threshold);
            //            index++;
            //        }
            //    }

            return new BitmapData()
            {
                //Dots = dots,
                //Height = (int)(bitmap.Height * scale),
                //Width = (int)(bitmap.Width * scale)
            };
            //}
        }

        private byte[] LoadImage(string filePath)
        {
            var activity = Xamarin.Essentials.Platform.CurrentActivity;
            Android.Net.Uri uri = Android.Net.Uri.FromFile(new Java.IO.File(filePath));
            Stream input = activity.ContentResolver.OpenInputStream(uri);

            //Use bitarray to use less memory                    
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                input.Close();
                return ms.ToArray();
            }

        }

        public static byte[] ImageToByte2(Bitmap bitmap)
        {
            MemoryStream stream = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
            byte[] bitmapData = stream.ToArray();
            return bitmapData;
        }

        public async Task DownloadAndPrintPdfFile(string uri)
        {
            try
            {
                var pdfFileName = $"{DateTime.Now.ToString("yyyyMMddhhmmss")}.pdf";
                var context = Android.App.Application.Context;
                var pathToNewFolder = System.IO.Path.Combine(context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).ToString());
                Directory.CreateDirectory(pathToNewFolder);
                string pathToNewFile = System.IO.Path.Combine(pathToNewFolder, pdfFileName);
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += (s, e) => {
                    FileStream file = new FileStream(pathToNewFile, FileMode.Open, FileAccess.Read);
                    StreamReader r = new StreamReader(file);

                    if (file.CanSeek)
                        file.Position = 0;

                    var activity = Xamarin.Essentials.Platform.CurrentActivity;
                    PrintManager printManager = (PrintManager)activity.GetSystemService(Context.PrintService);
                    PrintDocumentAdapter pda = new CustomPrintDocumentAdapter(pathToNewFile);
                    printManager.Print("Print File Job", pda, null);
                    file.Dispose();
                };
                webClient.DownloadFileAsync(new Uri(uri), pathToNewFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            await Task.CompletedTask;
        }

        public async Task DownloadAndPrintPdfFile(string deviceName, string uri)
        {
            try
            {
                var pdfFileName = $"{DateTime.Now.ToString("yyyyMMddhhmmss")}.pdf";
                var context = Android.App.Application.Context;
                var pathToNewFolder = System.IO.Path.Combine(context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).ToString());
                Directory.CreateDirectory(pathToNewFolder);
                string pathToNewFile = System.IO.Path.Combine(pathToNewFolder, pdfFileName);
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += (s, e) => {
                    FileStream file = new FileStream(pathToNewFile, FileMode.Open, FileAccess.Read);
                    StreamReader r = new StreamReader(file);

                    byte[] buffer = ReadToEnd(file);
                    using (BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter)
                    {
                        BluetoothDevice device = (from bd in bluetoothAdapter?.BondedDevices
                                                  where bd?.Name == deviceName
                                                  select bd).FirstOrDefault();
                        try
                        {
                            using (BluetoothSocket bluetoothSocket = device?.
                                CreateRfcommSocketToServiceRecord(
                                UUID.FromString("00001101-0000-1000-8000-00805f9b34fb")))
                            {
                                bluetoothSocket?.Connect();
                                bluetoothSocket?.OutputStream.Write(buffer, 0, buffer.Length);
                                bluetoothSocket.Close();
                            }
                        }
                        catch (Exception exp)
                        {
                            throw exp;
                        }
                    }
                };
                webClient.DownloadFileAsync(new Uri(uri), pathToNewFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            await Task.CompletedTask;
        }

        public async Task PrintImageByteArray(string deviceName)
        {
            var context = Android.App.Application.Context;
            var pathToNewFolder = System.IO.Path.Combine(context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).ToString());
            Directory.CreateDirectory(pathToNewFolder);
            string filePath = System.IO.Path.Combine(pathToNewFolder, "elimportador.png");
            using (BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter)
            {
                BluetoothDevice device = (from bd in bluetoothAdapter?.BondedDevices
                                          where bd?.Name == deviceName
                                          select bd).FirstOrDefault();
                try
                {
                    using (BluetoothSocket bluetoothSocket = device?.
                        CreateRfcommSocketToServiceRecord(
                        UUID.FromString("00001101-0000-1000-8000-00805f9b34fb")))
                    {
                        byte[] cutCommand = { 29, 86, 1 };

                        List<byte> outputList = new List<byte>();
                        var imageBytes = new EscPrinter();
                        outputList.AddRange(imageBytes.GetImageBytes(filePath));
                        outputList.AddRange(cutCommand);

                        var buffer = outputList.ToArray();

                        bluetoothSocket?.Connect();
                        bluetoothSocket?.OutputStream.Write(buffer, 0, buffer.Length);
                        bluetoothSocket.Close();
                    }
                }
                catch (Exception exp)
                {
                    throw exp;
                }
            }

            
        }

        public async Task DownloadPDFAndSaveAsPNG(string deviceName, string uri)
        {
            try
            {
                var pdfFileName = DateTime.Now.ToString("yyyyMMddhhmmss");
                var context = Android.App.Application.Context;
                var pathToNewFolder = System.IO.Path.Combine(context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).ToString());
                Directory.CreateDirectory(pathToNewFolder);
                string pathToNewFile = System.IO.Path.Combine(pathToNewFolder, $"{pdfFileName}.pdf");
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += (s, e) => {
                    FileStream file = new FileStream(pathToNewFile, FileMode.Open, FileAccess.Read);
                    StreamReader r = new StreamReader(file);

                    string pathImage = System.IO.Path.Combine(pathToNewFolder, $"{pdfFileName}.png");
                    using (var imageFile = new FileStream(pathImage, FileMode.Create))
                    {
                        //image.CopyTo(fileStream);
                    }

                    //byte[] buffer = ReadToEnd(file);
                    //using (BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter)
                    //{
                    //    BluetoothDevice device = (from bd in bluetoothAdapter?.BondedDevices
                    //                              where bd?.Name == deviceName
                    //                              select bd).FirstOrDefault();
                    //    try
                    //    {
                    //        using (BluetoothSocket bluetoothSocket = device?.
                    //            CreateRfcommSocketToServiceRecord(
                    //            UUID.FromString("00001101-0000-1000-8000-00805f9b34fb")))
                    //        {
                    //            bluetoothSocket?.Connect();
                    //            bluetoothSocket?.OutputStream.Write(buffer, 0, buffer.Length);
                    //            bluetoothSocket.Close();
                    //        }
                    //    }
                    //    catch (Exception exp)
                    //    {
                    //        throw exp;
                    //    }
                    //}
                };
                webClient.DownloadFileAsync(new Uri(uri), pathToNewFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            await Task.CompletedTask;
        }

        public async Task DownloadImageAndPrintLines(string deviceName, List<string> lines)
        {
            try
            {
                var context = Android.App.Application.Context;
                var pathToNewFolder = System.IO.Path.Combine(context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).ToString());
                Directory.CreateDirectory(pathToNewFolder);
                string pathToNewFile = System.IO.Path.Combine(pathToNewFolder, "elimportador.png");
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += (s, e) => {
                    using (BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter)
                    {
                        BluetoothDevice device = (from bd in bluetoothAdapter?.BondedDevices
                                                  where bd?.Name == deviceName
                                                  select bd).FirstOrDefault();
                        try
                        {
                            using (BluetoothSocket bluetoothSocket = device?.
                                CreateRfcommSocketToServiceRecord(
                                UUID.FromString("00001101-0000-1000-8000-00805f9b34fb")))
                            {
                                //byte[] cutCommand = { 29, 86, 1 };

                                List<byte> outputList = new List<byte>();
                                var imageBytes = new EscPrinter();
                                outputList.AddRange(imageBytes.GetImageBytes(pathToNewFile));
                                foreach (var txt in lines)
                                {
                                    outputList.AddRange(Encoding.UTF8.GetBytes(txt));
                                }
                                outputList.Add(0x0A);
                                outputList.Add(0x0A);
                                var buffer = outputList.ToArray();

                                bluetoothSocket?.Connect();
                                bluetoothSocket?.OutputStream.Write(buffer, 0, buffer.Length);
                                bluetoothSocket.Close();
                            }
                        }
                        catch (Exception exp)
                        {
                            throw exp;
                        }
                    }
                };
                webClient.DownloadFileAsync(new Uri("https://user-images.strikinglycdn.com/res/hrscywv4p/image/upload/c_limit,fl_lossy,h_300,w_300,f_auto,q_auto/407580/236110_7567.png"), pathToNewFile);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }
            await Task.CompletedTask;
        }

        private byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }
    }
}