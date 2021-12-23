using PDFPrinter.Core.SQLite;
using PDFPrinter.Views;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PDFPrinter
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            InitializeSqlite().Wait();
            MainPage = new PrinterManager();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        async Task InitializeSqlite()
        {
            await new ConfigurationRepository(SQLite.sqliteBase.ConectarAsync()).CreateDataBaseAsync().ConfigureAwait(false);
        }
    }
}
