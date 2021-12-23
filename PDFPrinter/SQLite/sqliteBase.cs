using SQLite;
using Xamarin.Forms;

namespace PDFPrinter.SQLite
{
    public class sqliteBase
    {
        public static SQLiteConnection Conectar()
        {
            var platform = DependencyService.Get<ISQLitePlatform>();
            var db = platform.GetConnection();
            return db;
        }

        public static SQLiteAsyncConnection ConectarAsync()
        {
            var platform = DependencyService.Get<ISQLitePlatform>();
            var db = platform.GetConnectionAsync();
            return db;
        }
    }
}
