using PDFPrinter.Core.Helper;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDFPrinter.Core.SQLite
{
    public class sqliteBase<T>
    {
        protected SQLiteAsyncConnection connection;
        protected static readonly AsyncLock Mutex = new AsyncLock();

        public sqliteBase(SQLiteAsyncConnection conn)
        {
            if (conn != null)
                connection = conn;
            else
                throw new Exception("The connection can not be null");
        }
    }
}
