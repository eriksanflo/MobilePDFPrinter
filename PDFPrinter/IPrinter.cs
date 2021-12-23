using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PDFPrinter
{
    public interface IPrinter
    {
        IList<string> GetDeviceList();
        void ClearDirectory();
        void CreateDirectory();
        Task Print(string deviceName, string text);
        Task Print(string deviceName, List<string> lines);
        Task Print(string deviceName, string logoName, List<string> lines);
        Task Print(string deviceName, byte[] buffer);
        Task PrintByImagePath(string deviceName, string imagePath);
        void Print(Stream inputStream, string fileName);
        void PrintByPath(string path, string fileName);
        void PrintErik(string deviceName, string uri);
        Task DownloadAndPrintPdfFile(string uri);
        Task DownloadAndPrintPdfFile(string deviceName, string uri);
        Task PrintImageByteArray(string deviceName);
        Task DownloadImageAndPrintLines(string deviceName, List<string> lines);
    }
}
