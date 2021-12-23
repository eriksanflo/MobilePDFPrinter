using System.Collections.Generic;
using System.Threading.Tasks;

namespace PDFPrinter.Core.SQLite
{
    public interface IDataStore<T>
    {
        Task CreateDataBaseAsync();
        Task AddAsync(T item);
        Task DeleteAsync(T item);
        Task<T> GetAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
    }
}
