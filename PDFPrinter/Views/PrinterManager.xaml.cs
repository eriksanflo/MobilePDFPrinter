using PDFPrinter.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PDFPrinter.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PrinterManager : ContentPage
    {
        IDownloader downloader = DependencyService.Get<IDownloader>();
        public PrinterManager()
        {
            InitializeComponent();
            BindingContext = new PrinterManagerViewModel();
        }

        private void Downloader_OnFileDownloaded(object sender, DownloadEventArgs e)
        {
            if (e.FileSaved)
            {
                DisplayAlert("PDF Printer", "File Saved Successfully", "Aceptar");
            }
            else
            {
                DisplayAlert("PDF Printer", "Error while saving the file", "Aceptar");
            }
        }

        private void btnDownloadFile_Clicked(object sender, EventArgs e)
        {
            //http://192.168.1.64:45456/Report/PrintReport?idRep=0&renderType=4&folio=8665&tipo=0&sucursal=0&desde=2021-10-01&hasta=2021-10-24
            //https://cdn.pixabay.com/photo/2015/04/23/22/00/tree-736885_960_720.jpg
            downloader.DownloadFile("https://user-images.strikinglycdn.com/res/hrscywv4p/image/upload/c_limit,fl_lossy,h_300,w_300,f_auto,q_auto/407580/236110_7567.png");
        }

        private async void btnGoConfig_Clicked(object sender, EventArgs e)
        {
            var vm = BindingContext as PrinterManagerViewModel;
            var modalPage = new Configuration();
            modalPage.Disappearing += (sender2, e2) =>
            {
                var x = vm.Load();
            };
            await Navigation.PushModalAsync(modalPage);
        }
    }
}