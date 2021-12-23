using Android.Content;
using Android.OS;
using Android.Print;
using Android.Util;
using Java.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDFPrinter.Common
{
    public class PrintPDFAdapter : PrintDocumentAdapter
    {
        Context context;
        string path;
        string fileName;

        public PrintPDFAdapter(Context _context, string _path, string _fileName)
        {
            context = _context;
            path = _path;
            fileName = _fileName;
        }

        public override void OnLayout(PrintAttributes oldAttributes, PrintAttributes newAttributes, CancellationSignal cancellationSignal, LayoutResultCallback callback, Bundle extras)
        {
            if (cancellationSignal.IsCanceled)
                callback.OnLayoutCancelled();
            else
            {
                PrintDocumentInfo.Builder builder = new PrintDocumentInfo.Builder(fileName);
                builder.SetContentType(PrintContentType.Document).Build();
                callback.OnLayoutFinished(builder.Build(), !newAttributes.Equals(oldAttributes));
            }
        }

        public override void OnWrite(PageRange[] pages, ParcelFileDescriptor destination, CancellationSignal cancellationSignal, WriteResultCallback callback)
        {
            InputStream input = null;
            OutputStream output = null;
            try
            {
                File file = new File(path);
                input = new FileInputStream(file);
                output = new FileOutputStream(destination.FileDescriptor);

                byte[] buff = new byte[8 * 1024];
                int lenght;
                while ((lenght = input.Read(buff)) >= 0 && !cancellationSignal.IsCanceled)
                    output.Write(buff, 0, lenght);
                if (cancellationSignal.IsCanceled)
                    callback.OnWriteCancelled();
                else
                    callback.OnWriteFinished(new PageRange[] { PageRange.AllPages });
            }
            catch (Exception ex)
            {
                callback.OnWriteFailed(ex.Message);
            }
            finally
            {
                try
                {
                    input.Close();
                    output.Close();
                }
                catch (IOException ioex)
                {
                    Log.Error("SmartToBusiness", "" + ioex.Message);
                }
            }
        }
    }
}
