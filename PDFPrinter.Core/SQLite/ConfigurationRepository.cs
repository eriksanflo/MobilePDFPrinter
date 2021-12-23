using PDFPrinter.Models.SQLite;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PDFPrinter.Core.SQLite
{
    public class ConfigurationRepository : sqliteBase<Configuration>, IDataStore<Configuration>
    {
        public ConfigurationRepository(SQLiteAsyncConnection conn) : base(conn) { }

        public async Task CreateDataBaseAsync()
        {
            using (await Mutex.LockAsync().ConfigureAwait(false))
            {
                await connection.CreateTableAsync<Configuration>().ConfigureAwait(false);
            }
        }

        public async Task AddAsync(Configuration item)
        {
            using (await Mutex.LockAsync().ConfigureAwait(false))
            {
                var exists = await connection.Table<Configuration>()
                    .Where(x => x.IdConfiguration == item.IdConfiguration)
                    .FirstOrDefaultAsync();

                if (exists == null)
                    await connection.InsertAsync(item);
                else
                    await connection.UpdateAsync(item);
            }
        }

        public async Task DeleteAsync(Configuration item)
        {
            using (await Mutex.LockAsync().ConfigureAwait(false))
            {
                await connection.DeleteAsync(item);
            }
        }

        public async Task<IEnumerable<Configuration>> GetAllAsync()
        {
            var items = new List<Configuration>();
            using (await Mutex.LockAsync().ConfigureAwait(false))
            {
                items = await connection.Table<Configuration>().ToListAsync().ConfigureAwait(false);
            }
            return items;
        }

        public async Task<Configuration> GetAsync(int id)
        {
            var item = new Configuration();
            using (await Mutex.LockAsync().ConfigureAwait(false))
            {
                item = await connection.Table<Configuration>()
                    .FirstOrDefaultAsync(x => x.IdConfiguration == id);
            }
            return item;
        }
    }
}
