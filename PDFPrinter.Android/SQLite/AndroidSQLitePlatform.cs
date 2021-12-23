using PDFPrinter.Droid.SQLite;
using PDFPrinter.SQLite;
using SQLite;
using System;
using System.IO;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidSQLitePlatform))]
namespace PDFPrinter.Droid.SQLite
{
    public class AndroidSQLitePlatform : ISQLitePlatform
    {
        protected string dbname = "printer.db";
        protected string dbpath;
        protected SQLiteConnection db;

        private string GetPath()
        {
            dbpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), dbname);
            return dbpath;
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(GetPath());
        }

        public SQLiteAsyncConnection GetConnectionAsync()
        {
            return new SQLiteAsyncConnection(GetPath());
        }
    }
}