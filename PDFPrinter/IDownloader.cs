using Android.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDFPrinter
{
    public interface IDownloader
    {
        void DownloadFile(string url);
        void GetBitmapByUri(string url);
        event EventHandler<DownloadEventArgs> OnFileDownloaded;
    }

    public class DownloadEventArgs: EventArgs
    {
        public bool FileSaved = false;
        public DownloadEventArgs(bool fileSaved)
        {
            FileSaved = fileSaved;
        }
    }
}
