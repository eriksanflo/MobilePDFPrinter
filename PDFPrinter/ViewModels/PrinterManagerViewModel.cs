using MvvmHelpers.Commands;
using Newtonsoft.Json;
using PDFPrinter.Model;
using PDFPrinter.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PDFPrinter.ViewModels
{
    public class PrinterManagerViewModel : BaseViewModel
    {
        #region Members
        private readonly IPrinter _blueToothService;
        private int _idPrintType;
        private int _minutes;
        private int _seconds;
        private bool _inExecute;
        private string _btnText;
        private string _lastExecution;
        private string _pointOfSale;
        private string _printer;
        private string _printType;
        private string _timer; 
        private string _cronometer;
        private string _printMessage;
        private string _selectedDevice;
        private string _urlBaseService;
        private DateTime _lastExecutionInternal;
        private ImageSource _imageTicket;
        private IList<string> _deviceList;
        #endregion

        #region Properties
        public ObservableCollection<PrinterJobDTO> Jobs { get; set; }
        public IList<string> DeviceList
        {
            get
            {
                if (_deviceList == null)
                    _deviceList = new ObservableCollection<string>();
                return _deviceList;
            }
            set
            {
                _deviceList = value;
            }
        }
        public string BtnText
        {
            get
            {
                return _btnText;
            }
            set
            {
                _btnText = value;
                OnPropertyChanged(nameof(BtnText));
            }
        }
        public string LastExecution
        {
            get
            {
                return _lastExecution;
            }
            set
            {
                _lastExecution = value;
                OnPropertyChanged(nameof(LastExecution));
            }
        }
        public string PointOfSale
        {
            get
            {
                return _pointOfSale;
            }
            set
            {
                _pointOfSale = value;
                OnPropertyChanged(nameof(PointOfSale));
            }
        }
        public string Printer
        {
            get
            {
                return _printer;
            }
            set
            {
                _printer = value;
                OnPropertyChanged(nameof(Printer));
            }
        }
        public string PrintType
        {
            get
            {
                return _printType;
            }
            set
            {
                _printType = value;
                OnPropertyChanged(nameof(PrintType));
            }
        }
        public string Timer
        {
            get
            {
                return _timer;
            }
            set
            {
                _timer = value;
                OnPropertyChanged(nameof(Timer));
            }
        }
        public string Cronometer
        {
            get
            {
                return _cronometer;
            }
            set
            {
                _cronometer = value;
                OnPropertyChanged(nameof(Cronometer));
            }
        }
        public string PrintMessage
        {
            get
            {
                return _printMessage;
            }
            set
            {
                _printMessage = value;
            }
        }
        public string SelectedDevice
        {
            get
            {
                return _selectedDevice;
            }
            set
            {
                _selectedDevice = value;
                OnPropertyChanged(nameof(SelectedDevice));
            }
        }
        public ImageSource ImageTicket
        {
            get
            {
                return _imageTicket;
            }
            set
            {
                _imageTicket = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands 
        public AsyncCommand OnOffCommand { get; set; }
        public AsyncCommand ResetCommand { get; set; }
        public AsyncCommand GetConfigurationCommand { get; set; }
        public AsyncCommand PrintTicketCommand { get; set; }
        public AsyncCommand DowloadTicketCommand { get; set; }
        public AsyncCommand DownloadPDFAndPrintCommand { get; set; }
        public AsyncCommand DownloadLogoAndPrintLinesAndPrintCommand { get; set; }
        public AsyncCommand DownloadPDFAndPrintBytesCommand { get; set; }
        #endregion

        #region Ctor
        public PrinterManagerViewModel()
        {
            OnOffCommand = new AsyncCommand(OnOffCommandOnclick);
            ResetCommand = new AsyncCommand(ResetCommandOnclick);
            GetConfigurationCommand = new AsyncCommand(GetConfigurationCommandOnClick);
            PrintTicketCommand = new AsyncCommand(PrintTicketCommandOnclick);
            DowloadTicketCommand = new AsyncCommand(DownloadTicketCommandOnclick);
            DownloadPDFAndPrintCommand = new AsyncCommand(DownloadPDFAndPrintOnClick);
            DownloadLogoAndPrintLinesAndPrintCommand = new AsyncCommand(DownloadLogoAndPrintLinesAndPrintCommandOnClick);
            DownloadPDFAndPrintBytesCommand = new AsyncCommand(DownloadPDFAndPrintBytesCommandOnClick);
            Jobs = new ObservableCollection<PrinterJobDTO>();
            _blueToothService = DependencyService.Get<IPrinter>();
            _inExecute = false;
            BtnText = "ON";
            Cronometer = $"0 Seg";
            _minutes = _seconds = 0;
            _blueToothService.CreateDirectory();
            Task.Run(async() => 
            {
                await Load();
            });
        }
        #endregion

        #region Printer Actions
        private async Task OnOffCommandOnclick()
        {
            var existsConfig = await ExistsAnyConfiguration();
            if (existsConfig && BtnText == "ON" && string.IsNullOrEmpty(SelectedDevice))
                await Load();

            if (existsConfig)
            {
                _inExecute = (BtnText == "ON");
                BtnText = _inExecute ? "OFF" : "ON";
                if (_inExecute)
                {
                    await GoCheckRemaining();
                    await LauchTimer();
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Print Manager", "Debe realizar primero una configuración para iniciar operaciones.", "Aceptar");
            }
        }
        private async Task ResetCommandOnclick()
        {
            var existsConfig = await ExistsAnyConfiguration();
            if (existsConfig)
            {
                await Load();
                await GoCheckRemaining();
                if (_inExecute)
                    _lastExecutionInternal = DateTime.Now;
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Print Manager", "Debe realizar primero una configuración para iniciar operaciones.", "Aceptar");
            }
        }
        private async Task GetConfigurationCommandOnClick()
        {
            await Load();
        }
        private async Task GoCheckRemaining()
        {
            _lastExecutionInternal = DateTime.Now;
            _blueToothService.ClearDirectory();
            var tickets = await GetRemainingTickets();
            LastExecution = _lastExecutionInternal.ToString("hh:mm:ss tt");
            foreach (var ticketData in tickets)
            {
                if (_idPrintType == 3) //ByLine
                    await TicketPrintByLine(ticketData.Content, ticketData.Logo);
                else if (_idPrintType == 4) // ByPDF
                    await TicketPrintByPDF(ticketData.Content);
                await MarkAsPrinted(ticketData.Id, ticketData.Content);
            }
        }
        private async Task LauchTimer()
        {
            Device.StartTimer(new TimeSpan(0, 0, 1), () =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (_inExecute)
                    {
                        var diff = DateTime.Now - _lastExecutionInternal;
                        var duration = new TimeSpan(0, _minutes, _seconds);
                        var remaining = (int)duration.TotalSeconds - (int)diff.TotalSeconds;
                        Cronometer = $"En {remaining} Seg";
                        if (remaining <= 0)
                        {
                            _lastExecutionInternal = DateTime.Now;
                            await GoCheckRemaining();
                        }
                    }
                    else
                    {
                        Cronometer = $"En 0 Seg";
                    }
                });
                return _inExecute;
            });
            await Task.CompletedTask;
        }
        private async Task<List<TicketObjectDTO>> GetRemainingTickets()
        {
            var response = new List<TicketObjectDTO>();
            try
            {
                var strUrl = string.Format(_urlBaseService, PointOfSale, _idPrintType);
                //var uri = new Uri($"{_urlBaseService}?port={PointOfSale}&type={_idPrintType}");
                var uri = new Uri(strUrl);
                using (var _client = new HttpClient())
                {
                    var httpResponse = await _client.GetAsync(uri);
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var respJson = await httpResponse.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<List<TicketObjectDTO>>(respJson);
                        response.AddRange(data.Where(x => x.Type == _idPrintType));
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return response;
        }
        private async Task MarkAsPrinted(int id, string folio)
        {
            try
            {
                var uri = new Uri($"http://tsoftware.ddns.net:8000/api/Ticket/ImpresionPrinted?Id={id}");
                using (var _client = new HttpClient())
                {
                    _client.Timeout = TimeSpan.FromSeconds(3);
                    var requestMsg = new HttpRequestMessage();
                    requestMsg.Method = HttpMethod.Put;
                    requestMsg.RequestUri = uri;
                    var httpResponse = await _client.SendAsync(requestMsg);
                    if (_idPrintType == 3)
                    {
                        Jobs.Add(new PrinterJobDTO
                        {
                            Folio = $"ID Ticket: {id}",
                            HoraImpresion = $"Impresión satisfactoria: {httpResponse.IsSuccessStatusCode}"
                        });
                    }
                    else
                    {
                        Jobs.Add(new PrinterJobDTO
                        {
                            Folio = $"Folio: {folio}",
                            HoraImpresion = $"Impresión satisfactoria: {httpResponse.IsSuccessStatusCode}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Jobs.Add(new PrinterJobDTO
                {
                    Folio = $"ID: {id}",
                    HoraImpresion = $"Error: {ex.Message}"
                });
            }
        }
        private async Task TicketPrintByLine(string content, string logo)
        {
            string[] stringSeparators = new string[] { "\r\n" };
            string[] lines = content.Split(stringSeparators, StringSplitOptions.None);
            await _blueToothService.Print(SelectedDevice, logo, lines.ToList());
        }
        private async Task TicketPrintByPDF(string content)
        {
            await _blueToothService.DownloadAndPrintPdfFile($"http://tsoftware.ddns.net:8002/api/Report/GetReport?Id=2&folio={content}");
            await Task.Delay(5000);
        }
        async Task<bool> ExistsAnyConfiguration()
        {
            var localConfigs = await new Core.SQLite.ConfigurationRepository(sqliteBase.ConectarAsync()).GetAllAsync();
            return localConfigs.Any();
        }
        public async Task Load()
        {
            var existConfig = await ExistsAnyConfiguration();
            if (existConfig)
            {
                var config = await new Core.SQLite.ConfigurationRepository(sqliteBase.ConectarAsync()).GetAsync(1);
                var minutes = $"0{config.Minutes}";
                var seconds = $"0{config.Seconds}";
                _idPrintType = DefinePrintType(config.PrintingType);
                _urlBaseService = config.ServicePath;
                _minutes = config.Minutes;
                _seconds = config.Seconds;
                PointOfSale = config.PoSName;
                PrintType = config.PrintingType;
                SelectedDevice = config.PrinterName;
                Timer = $"{minutes.Substring(minutes.Length -2, 2)}:{seconds.Substring(seconds.Length - 2, 2)}";
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Print Manager", "Debe realizar primero una configuración para iniciar operaciones.", "Aceptar");
            }
        }
        private int DefinePrintType(string strType)
        {
            if (strType == "Por Línea")
                return 3;
            if (strType == "Por PDF")
                return 4;
            return 0;
        }
        #endregion

        #region ActionHandler
        public async Task PrintTicketCommandOnclick()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedDevice))
                    await Application.Current.MainPage.DisplayAlert("Print Manager", "Debe seleccionar primero una impresroa Bluetooth", "Aceptar");
                else
                {

                
                //string directory = Path.Combine(Android.OS.Environment. ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                ////string file = Path.Combine(directory, "yourfile.txt");
                //var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //await Application.Current.MainPage.DisplayAlert("Print Manager", directory.ToString(), "Aceptar");
                //var webClient = new WebClient();

                //webClient.DownloadDataCompleted += (s, e) => {
                //    var data = e.Result;
                //    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                //    string localFilename = $"{_blueways}.pdf";
                //    File.WriteAllBytes(Path.Combine(documentsPath, localFilename), data);
                //    InvokeOnMainThread(() => {
                //        new UIAlertView("Done", "File downloaded and saved", null, "OK", null).Show();
                //    });
                //};
                //var url = new Uri("_blueway.PDF");
                //webClient.DownloadDataAsync(url);


                //string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                //string filePath = System.IO.Path.Combine(documentsPath, "Imagen.bmp");

                //using (HttpClient client = new HttpClient())
                //{
                //    HttpResponseMessage msg = await client.GetAsync($"http://192.168.1.64:45456/Report/PrintReport?idRep=0&renderType=5&folio=8665&tipo=0&sucursal=0&desde=2021-10-01&hasta=2021-10-24");

                //    if (msg.IsSuccessStatusCode)
                //    {
                //        using (var file = File.Create(filePath))
                //        {
                //            // create a new file to write to
                //            var contentStream = await msg.Content.ReadAsStreamAsync(); // get the actual content stream
                //            await contentStream.CopyToAsync(file); // copy that stream to the file stream
                //            await file.FlushAsync(); // flush back to disk before disposing
                //        }
                //    }
                //}
                //if (!File.Exists(filePath))
                //    await Application.Current.MainPage.DisplayAlert("Print Manager", "The Image doesn't exists", "Aceptar");
                //else
                //{
                //    await _blueToothService.PrintByImagePath(SelectedDevice, filePath);
                //}





                //using (var client = new WebClient())
                //{
                //    byte[] data = client.DownloadData($"http://192.168.1.64:45456/Report/PrintReport?idRep=0&renderType=5&folio=8665&tipo=0&sucursal=0&desde=2021-10-01&hasta=2021-10-24");
                //    await _blueToothService.Print(SelectedDevice, data);
                //}


                //using (HttpClient client = new HttpClient())
                //{
                //    HttpResponseMessage msg = await client.GetAsync($"http://192.168.1.64:45456/Report/PrintReport?idRep=0&renderType=5&folio=8665&tipo=0&sucursal=0&desde=2021-10-01&hasta=2021-10-24");

                //    if (msg.IsSuccessStatusCode)
                //    {
                //        var contentStream = await msg.Content.ReadAsStreamAsync(); // get the actual content stream
                //        Bitmap bitmap = BitmapFactory.DecodeStream(contentStream);
                //        MemoryStream stream = new MemoryStream();
                //        bitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
                //        byte[] bitmapData = stream.ToArray();
                //        await _blueToothService.Print(SelectedDevice, bitmapData);
                //    }
                //}




                //if(File.Exists(filePath))
                //    await Application.Current.MainPage.DisplayAlert("Print Manager", "The file PDF Exists", "Aceptar");
                //else
                //    await Application.Current.MainPage.DisplayAlert("Print Manager", "The file doesn't PDF Exists", "Aceptar");


                //PrintMessage += " Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";
                var lines = new List<string>();
                lines.Add("********************************");
                lines.Add("      EMPRESA DEMO SA DE CV     ");
                lines.Add("********************************");
                lines.Add(DateTime.Now.ToString("dd MMMM yyyy hh:mm tt"));
                lines.Add($"Folio: {DateTime.Now.ToString("hhmmss")}");
                lines.Add("Sucrsal: CENTRO CIUDAD");
                lines.Add("Cant.Producto      PreUnit Impor");
                lines.Add("---- ------------- ------- -----");
                lines.Add("1.00 CocaCola Lat $20.00  $20.00");
                lines.Add("2.00 Gall M Games $13.00  $26.00");
                lines.Add("3.00 Jugo Valle m $12.00  $36.00");
                lines.Add("1.00 Hambu. Mexic $75.00  $75.00");
                lines.Add("4.00 XX Lager 355 $20.00  $80.00");
                lines.Add("1.00 Fajitas poll$120.00 $120.00");
                lines.Add("2.00 Cheese Cake  $35.00  $70.00");
                lines.Add("--------------------------------");
                lines.Add("");
                lines.Add("Subotal                  $358.68");
                lines.Add("IVa                       $68.32");
                lines.Add("Total                    $427.00");
                await _blueToothService.Print(SelectedDevice, lines);



                    //string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    //string filePath = Path.Combine(documentsPath, "ItWorks.pdf");
                    //_blueToothService.PrintByPath(filePath, "ItWorks.pdf");

                    //string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    //string filePath = Path.Combine(documentsPath, "ItWorks.pdf");
                    //ceTe.DynamicPDF.Printing.PrintJob printJob = new ceTe.DynamicPDF.Printing.PrintJob(SelectedDevice, filePath);
                    //printJob.Print();

                    //var webClient = new WebClient();
                    //webClient.DownloadDataCompleted += (s, e) =>
                    //{
                    //    var data = e.Result;
                    //    _blueToothService.Print(SelectedDevice, data);
                    //};
                    //var url = new Uri("https://cdn.pixabay.com/photo/2015/04/23/22/00/tree-736885_960_720.jpg");
                    //webClient.DownloadDataAsync(url);

                    //_blueToothService.PrintErik(SelectedDevice, "http://storage.googleapis.com/www-paredro-com/uploads/2015/08/NEGRO-Y-BLANCO-LOGO-chanel-e1439176520766.png");
                }
            }
            catch (Exception exception)
            {
                var error = exception.ToString();
            }
        }
        public async Task DownloadTicketCommandOnclick()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var data = await client.GetByteArrayAsync($"http://192.168.1.64:45456/Report/PrintReport?idRep=0&renderType=4&folio=8665&tipo=0&sucursal=0&desde=2021-10-01&hasta=2021-10-24");
                    //Xamarin.Essentials.Preferences.Set("Imagen.png", Convert.ToBase64String(data));
                    ImageTicket = ImageSource.FromStream(() => new MemoryStream(data));
                }

                //HttpClient _client = new HttpClient();
                //var data = await _client.GetByteArrayAsync("http://192.168.1.64:45456/Report/PrintReport?idRep=0&renderType=5&folio=8665&tipo=0&sucursal=0&desde=2021-10-01&hasta=2021-10-24");
                //Xamarin.Essentials.Preferences.Set("Imagen.png", Convert.ToBase64String(data));
                //var imageAsBase64String = Xamarin.Essentials.Preferences.Get("Imagen.png", string.Empty);

                //ImageTicket.Source = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(imageAsBase64String)));

            }
            catch (Exception ex)
            {
                var error = ex.ToString();
            }
        }
        public async Task DownloadPDFAndPrintOnClick()
        {
            await _blueToothService.DownloadAndPrintPdfFile("https://blazorrdlcreport20211030135445.azurewebsites.net/api/Report/GetReport?Id=30");
        }
        public async Task DownloadPDFAndPrintBytesCommandOnClick()
        {
            if (string.IsNullOrEmpty(SelectedDevice))
                await Application.Current.MainPage.DisplayAlert("Print Manager", "Debe seleccionar primero una impresroa Bluetooth", "Aceptar");
            else
                await _blueToothService.DownloadAndPrintPdfFile(SelectedDevice, "https://blazorrdlcreport20211030135445.azurewebsites.net/api/Report/GetReport?Id=30");
        }
        public async Task DownloadLogoAndPrintLinesAndPrintCommandOnClick()
        {
            if (string.IsNullOrEmpty(SelectedDevice))
                await Application.Current.MainPage.DisplayAlert("Print Manager", "Debe seleccionar primero una impresroa Bluetooth", "Aceptar");
            else
            {
                //await _blueToothService.DownloadAndPrintPdfFile(SelectedDevice, "https://user-images.strikinglycdn.com/res/hrscywv4p/image/upload/c_limit,fl_lossy,h_300,w_300,f_auto,q_auto/407580/236110_7567.png");
                //await _blueToothService.PrintImageByteArray(SelectedDevice);
                var lines = new List<string>();
                lines.Add("********************************");
                lines.Add("      EMPRESA DEMO SA DE CV     ");
                lines.Add("********************************");
                lines.Add(DateTime.Now.ToString("dd MMMM yyyy hh:mm tt"));
                lines.Add($"Folio: {DateTime.Now.ToString("hhmmss")}");
                lines.Add("Sucrsal: CENTRO CIUDAD");
                lines.Add("Cant.Producto      PreUnit Impor");
                lines.Add("---- ------------- ------- -----");
                lines.Add("1.00 CocaCola Lat $20.00  $20.00");
                lines.Add("2.00 Gall M Games $13.00  $26.00");
                lines.Add("3.00 Jugo Valle m $12.00  $36.00");
                lines.Add("1.00 Hambu. Mexic $75.00  $75.00");
                lines.Add("4.00 XX Lager 355 $20.00  $80.00");
                lines.Add("1.00 Fajitas poll$120.00 $120.00");
                lines.Add("2.00 Cheese Cake  $35.00  $70.00");
                lines.Add("--------------------------------");
                lines.Add("");
                lines.Add("Subotal                  $358.68");
                lines.Add("IVa                       $68.32");
                lines.Add("Total                    $427.00");
                await _blueToothService.DownloadImageAndPrintLines(SelectedDevice, lines);
            }
        }
        #endregion

        #region Internal
        void BindDeviceList()
        {
            var list = _blueToothService.GetDeviceList();
            DeviceList.Clear();
            foreach (var item in list)
                DeviceList.Add(item);
        }
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        public async Task<ImageSource> DownloadImage()
        {
            using (HttpClient client = new HttpClient())
            {
                var data = await client.GetByteArrayAsync($"http://192.168.1.64:45456/Report/PrintReport?idRep=0&renderType=5&folio=8665&tipo=0&sucursal=0&desde=2021-10-01&hasta=2021-10-24");
                //Xamarin.Essentials.Preferences.Set("Imagen.png", Convert.ToBase64String(data));
                return ImageSource.FromStream(() => new MemoryStream(data));
            }
        }
        #endregion
    }
}