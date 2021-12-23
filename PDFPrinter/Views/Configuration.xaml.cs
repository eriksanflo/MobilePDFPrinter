using PDFPrinter.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PDFPrinter.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Configuration : ContentPage
    {
        public Configuration()
        {
            InitializeComponent();
            BindingContext = new ConfigurationViewModel();
        }
    }
}