using PDFPrinter.Droid;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidDownloader))]
namespace PDFPrinter.Droid
{
    public class AndroidDownloader : IDownloader
    {
        public event EventHandler<DownloadEventArgs> OnFileDownloaded;
        public string PathFile;
        public void DownloadFile(string url)
        {
            var context = Android.App.Application.Context;
            var pathToNewFolder = Path.Combine(context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).ToString());
            Directory.CreateDirectory(pathToNewFolder);
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                string pathToNewFile = Path.Combine(pathToNewFolder, "elimportador.png");
                webClient.DownloadFileAsync(new Uri(url), pathToNewFile);
            }
            catch (Exception ex)
            {
                if(OnFileDownloaded != null)
                {
                    OnFileDownloaded.Invoke(this, new DownloadEventArgs(false));
                }
            }
        }

        public void GetBitmapByUri(string url)
        {
            var context = Android.App.Application.Context;
            var pathToNewFolder = Path.Combine(context.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).ToString());
            Directory.CreateDirectory(pathToNewFolder);
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadDataCompleted += new DownloadDataCompletedEventHandler(CompletedData);
                webClient.DownloadDataAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                if (OnFileDownloaded != null)
                {
                    OnFileDownloaded.Invoke(this, new DownloadEventArgs(false));
                }
            }
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if(e.Error != null)
            {
                if (OnFileDownloaded != null)
                    OnFileDownloaded.Invoke(this, new DownloadEventArgs(false));
            }
            else
            {
                if (OnFileDownloaded != null)
                    OnFileDownloaded.Invoke(this, new DownloadEventArgs(true));
            }
        }

        private void CompletedData(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (OnFileDownloaded != null)
                    OnFileDownloaded.Invoke(this, new DownloadEventArgs(false));
            }
            else
            {
                Android.Graphics.Bitmap bm = Android.Graphics.BitmapFactory.DecodeByteArray(e.Result, 0, e.Result.Length);
            }
        }
    }
}