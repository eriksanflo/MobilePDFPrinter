using MvvmHelpers.Commands;
using PDFPrinter.SQLite;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PDFPrinter.ViewModels
{
    public class ConfigurationViewModel : BaseViewModel
    {
        #region Members
        private readonly IPrinter _printerService;
        private string _POSName;
        private string _SelectedDevice;
        private string _SelectedPrintMode;
        private string _SelectedMinute;
        private string _SelectedSecond;
        private string _UrlService;
        private IList<string> _DeviceList;
        private IList<string> _PrintModeList;
        private IList<string> _MinuteList;
        private IList<string> _SecondsList;
        private bool _CanSaveConfig;
        private Action action;
        #endregion

        #region Properties
        public string POSName
        {
            get
            {
                return _POSName;
            }
            set
            {
                _POSName = value;
                OnPropertyChanged(nameof(POSName));
                CheckCanSave();
            }
        }
        public string UrlService
        {
            get
            {
                return _UrlService;
            }
            set
            {
                _UrlService = value;
                OnPropertyChanged(nameof(UrlService));
                CheckCanSave();
            }
        }
        public string SelectedDevice
        {
            get
            {
                return _SelectedDevice;
            }
            set
            {
                _SelectedDevice = value;
                OnPropertyChanged(nameof(SelectedDevice));
                CheckCanSave();
            }
        }
        public string SelectedPrintMode
        {
            get
            {
                return _SelectedPrintMode;
            }
            set
            {
                _SelectedPrintMode = value;
                OnPropertyChanged(nameof(SelectedPrintMode));
                CheckCanSave();
            }
        }
        public string SelectedMinute
        {
            get
            {
                return _SelectedMinute;
            }
            set
            {
                _SelectedMinute = value;
                OnPropertyChanged(nameof(SelectedMinute));
                CheckCanSave();
            }
        }
        public string SelectedSecond
        {
            get
            {
                return _SelectedSecond;
            }
            set
            {
                _SelectedSecond = value;
                OnPropertyChanged(nameof(SelectedSecond));
                CheckCanSave();
            }
        }
        public IList<string> DeviceList
        {
            get
            {
                if (_DeviceList == null)
                    _DeviceList = new ObservableCollection<string>();
                return _DeviceList;
            }
            set
            {
                _DeviceList = value;
            }
        }
        public IList<string> PrintModeList
        {
            get
            {
                if (_PrintModeList == null)
                    _PrintModeList = new ObservableCollection<string>();
                return _PrintModeList;
            }
            set
            {
                _PrintModeList = value;
            }
        }
        public IList<string> MinuteList
        {
            get
            {
                if (_MinuteList == null)
                    _MinuteList = new ObservableCollection<string>();
                return _MinuteList;
            }
            set
            {
                _MinuteList = value;
            }
        }
        public IList<string> SecondsList
        {
            get
            {
                if (_SecondsList == null)
                    _SecondsList = new ObservableCollection<string>();
                return _SecondsList;
            }
            set
            {
                _SecondsList = value;
            }
        }
        public bool CanSaveConfig
        {
            get
            {
                return _CanSaveConfig;
            }
            set
            {
                _CanSaveConfig = value;
                OnPropertyChanged(nameof(CanSaveConfig));
            }
        }
        #endregion

        #region Commands
        public AsyncCommand SaveConfigurationCommand { get; set; }
        #endregion

        #region Ctor
        public ConfigurationViewModel()
        {
            //action = _action;
            _printerService = DependencyService.Get<IPrinter>();
            SaveConfigurationCommand = new AsyncCommand(SaveConfigurationOnclick);
            FillTimer();
            BindDeviceList();
            Task.Run(async() => {
                await Load();
            });
        }
        #endregion

        #region CanExecute
        private void CheckCanSave() => CanSaveConfig = !string.IsNullOrEmpty(POSName) && !string.IsNullOrEmpty(UrlService) && !string.IsNullOrEmpty(SelectedDevice) && !string.IsNullOrEmpty(SelectedPrintMode) && (SelectedMinute != "00" || SelectedSecond != "00");
        #endregion

        #region Internal
        private void FillTimer()
        {
            var number = string.Empty;
            for (int i = 0; i < 60; i++)
            {
                number = $"0{i}";
                MinuteList.Add(number.Substring(number.Length - 2, 2));
                SecondsList.Add(number.Substring(number.Length - 2, 2));
            }
            PrintModeList.Add("Por Línea");
            PrintModeList.Add("Por PDF");
        }
        private async Task Load()
        {
            var existConfig = await ExistsAnyConfiguration();
            if (existConfig)
            {
                var config = await new Core.SQLite.ConfigurationRepository(sqliteBase.ConectarAsync()).GetAsync(1);
                var minutes = $"0{config.Minutes}";
                var seconds = $"0{config.Seconds}";
                POSName = config.PoSName;
                UrlService = config.ServicePath;
                SelectedMinute = minutes.Substring(minutes.Length - 2, 2);
                SelectedSecond = seconds.Substring(seconds.Length - 2, 2);
                SelectedPrintMode = config.PrintingType;
                if (DeviceList.Any(x => x == config.PrinterName))
                    SelectedDevice = config.PrinterName;
            }
            else
            {
                SelectedSecond = "00";
                SelectedMinute = "00";
                UrlService = "http://tsoftware.ddns.net:8000/api/Ticket/ImpresionPendingToPrintByPortAndType?port={0}&type={1}";
            }
        }
        void BindDeviceList()
        {
            var list = _printerService.GetDeviceList();
            DeviceList.Clear();
            foreach (var item in list)
                DeviceList.Add(item);
        }
        async Task<bool> ExistsAnyConfiguration()
        {
            var localConfigs = await new Core.SQLite.ConfigurationRepository(sqliteBase.ConectarAsync()).GetAllAsync();
            return localConfigs.Any();
        }
        private async Task SaveConfigurationOnclick()
        {
            var model = new Models.SQLite.Configuration()
            {
                IdConfiguration = 1,
                PoSName = POSName,
                PrinterName = SelectedDevice,
                PrintingType = SelectedPrintMode,
                ServicePath = UrlService,
                Minutes = int.Parse(SelectedMinute),
                Seconds = int.Parse(SelectedSecond),
            };
            await new Core.SQLite.ConfigurationRepository(sqliteBase.ConectarAsync()).AddAsync(model);
            
            var exists = await ExistsAnyConfiguration();
            if (exists)
            {
                await Application.Current.MainPage.DisplayAlert("Print Manager", "La configuración se guardo con éxito", "Aceptar");
                await Application.Current.MainPage.Navigation.PopModalAsync();
            }
            else
                await Application.Current.MainPage.DisplayAlert("Print Manager", "Ocurrio un erro al guardar la configuración de impresión", "Aceptar");
        }
        #endregion
    }
}