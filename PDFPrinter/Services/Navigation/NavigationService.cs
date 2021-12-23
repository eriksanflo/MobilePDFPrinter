using PDFPrinter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;


namespace PDFPrinter.Services.Navigation
{
    public class NavigationService : INavigationService
    {
        protected readonly Dictionary<Type, Type> mappings;
        protected Application CurrentApplication => Application.Current;

        public NavigationService()
        {
            mappings = new Dictionary<Type, Type>();
            //CreatePageViewModelMappings();
        }

        //public async Task InitializeAsync() => await NavigateToAsync<PrinterManagerViewModel>();//LOGIN

        //public Task NavigateToAsync<TViewModel>() where TViewModel : ViewModel => InternalNavigateToAsync(typeof(TViewModel), null);

        //public Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : ViewModel => InternalNavigateToAsync(typeof(TViewModel), parameter);

        //public Task NavigateToAsync(Type viewModelType) => InternalNavigateToAsync(viewModelType, null);

        //public Task NavigateToAsync(Type viewModelType, object parameter) => InternalNavigateToAsync(viewModelType, parameter);

        //public async Task NavigateBackAsync()
        //{
        //    if (CurrentApplication.MainPage is MenuPrincipalView) //LOGIN
        //    {
        //        var mainPage = CurrentApplication.MainPage as MenuPrincipalView; //LOGIN
        //        await mainPage.Navigation.PopAsync();
        //    }
        //    else if (CurrentApplication.MainPage != null)
        //    {
        //        await CurrentApplication.MainPage.Navigation.PopAsync();
        //    }
        //}

        //public virtual Task RemoveLastFromBackStackAsync()
        //{
        //    if (CurrentApplication.MainPage is MenuPrincipalView mainPage) //LOGIN
        //    {
        //        mainPage.Navigation.RemovePage(
        //            mainPage.Navigation.NavigationStack[mainPage.Navigation.NavigationStack.Count - 2]);
        //    }

        //    return Task.FromResult(true);
        //}

        //public Task RemoveBackStackAsync()
        //{
        //    var mainPage = CurrentApplication.MainPage as CustomNavigationView;

        //    if (mainPage != null)
        //    {
        //        for (int i = 0; i < mainPage.Navigation.NavigationStack.Count - 1; i++)
        //        {
        //            var page = mainPage.Navigation.NavigationStack[i];
        //            mainPage.Navigation.RemovePage(page);
        //        }
        //    }

        //    return Task.FromResult(true);
        //}

        //public async Task GoToViewModel(string destination)
        //{
        //    var pages = Application.Current.MainPage.Navigation.NavigationStack.ToList();
        //    pages.Reverse();
        //    foreach (var page in pages)
        //    {
        //        var name = page.ToString();
        //        if (page.ToString() != $"Tuhandyman.Views.{destination}")
        //            Application.Current.MainPage.Navigation.RemovePage(page);
        //        else
        //            break;
        //    }
        //    await Application.Current.MainPage.Navigation.PopAsync();
        //}

        //public async Task BackToMenuPrincipal()
        //{
        //    await Application.Current.MainPage.Navigation.PopToRootAsync();
        //    //var pages = Application.Current.MainPage.Navigation.NavigationStack.ToList();
        //    //pages.Reverse();
        //    //if (pages.Any(x => x.ToString() == $"Tuhandyman.Views.MenuHamburguesaView"))
        //    //{
        //    //    foreach (var page in pages)
        //    //    {
        //    //        if (page.ToString() != $"Tuhandyman.Views.MenuHamburguesaView")
        //    //            Application.Current.MainPage.Navigation.RemovePage(page);
        //    //        else
        //    //            break;
        //    //    }
        //    //    await Application.Current.MainPage.Navigation.PopAsync();
        //    //}
        //}

        //protected virtual async Task InternalNavigateToAsync(Type viewModelType, object parameter)
        //{
        //    try
        //    {
        //        Page page = CreateAndBindPage(viewModelType, parameter);

        //        if (page is MenuPrincipalView || page is MenuPrincipalView) //LOGIN
        //        {
        //            try
        //            {
        //                CurrentApplication.MainPage = new CustomNavigationView(page);
        //            }
        //            catch (Exception ee)
        //            {
        //                throw new Exception($"Mapping type for {ee.Message} is not a page");
        //            }
        //        }
        //        else
        //        {
        //            var navigationPage = CurrentApplication.MainPage as CustomNavigationView;
        //            if (navigationPage != null)
        //            {
        //                await navigationPage.PushAsync(page);
        //            }
        //            else
        //            {
        //                CurrentApplication.MainPage = new CustomNavigationView(page);
        //            }
        //        }

        //        await (page.BindingContext as ViewModel).InitializeAsync(parameter);
        //    }
        //    catch (Exception ex)
        //    {
        //        var error = ex.ToString();
        //        await Task.Delay(1);
        //        throw new Exception($"Mapping type for {ex.Message}");
        //    }
        //}

        protected Type GetPageTypeForViewModel(Type viewModelType)
        {
            if (!mappings.ContainsKey(viewModelType))
            {
                throw new KeyNotFoundException($"No map for ${viewModelType} was found on navigation mappings");
            }

            return mappings[viewModelType];
        }

        //protected Page CreateAndBindPage(Type viewModelType, object parameter)
        //{
        //    try
        //    {
        //        var pageType = GetPageTypeForViewModel(viewModelType);

        //        if (pageType == null)
        //        {
        //            throw new Exception($"Mapping type for {viewModelType} is not a page");
        //        }

        //        var page = Activator.CreateInstance(pageType) as Page;

        //        var viewModel = Locator.Instance.Resolve(viewModelType) as ViewModel;
        //        page.BindingContext = viewModel;
        //        return page;
        //    }
        //    catch (Exception ex)
        //    {
        //        var error = ex.ToString();
        //        throw new Exception($"Mapping type for {ex.Message}");
        //    }
        //}

        //private void CreatePageViewModelMappings()
        //{
        //    //Mapeo de paginas
        //    mappings.Add(typeof(MainPageViewModel), typeof(MainPage));
        //    mappings.Add(typeof(RegistrarseViewModel), typeof(RegistrarseView));// Module Sign up
        //    mappings.Add(typeof(FormaDeRegistrarseViewModel), typeof(FormaDeRegistrarseView));
        //    mappings.Add(typeof(MenuHamburguesaViewModel), typeof(MenuHamburguesaView));
        //    mappings.Add(typeof(MenuPrincipalViewModel), typeof(MenuPrincipalView));
        //    mappings.Add(typeof(MiInformacionViewModel), typeof(MiInformacionView));
        //    mappings.Add(typeof(AyudaOSoporteViewModel), typeof(AyudaOSoporteView));
        //    mappings.Add(typeof(ConfiguracionViewModel), typeof(ConfiguracionView));
        //    mappings.Add(typeof(CanceleriaViewModel), typeof(CanceleriaView));
        //    mappings.Add(typeof(ReparacionDeElectrodomesticosViewModel), typeof(ReparacionDeElectrodomesticosView));
        //    mappings.Add(typeof(RefrigeradorViewModel), typeof(RefrigeradorView));
        //    mappings.Add(typeof(EstufaViewModel), typeof(EstufaView));
        //    mappings.Add(typeof(CalentadorViewModel), typeof(CalentadorView));
        //    mappings.Add(typeof(SanitizacionViewModel), typeof(SanitizacionView));
        //    mappings.Add(typeof(SanitizacionDepartamentoViewModel), typeof(SanitizacionDepartamentoView));
        //    mappings.Add(typeof(SanitizacionOficinaViewModel), typeof(SanitizacionOficinaView));
        //    mappings.Add(typeof(SanitizacionCasaViewModel), typeof(SanitizacionCasaView));
        //    mappings.Add(typeof(PlomeriaViewModel), typeof(PlomeriaView));
        //    mappings.Add(typeof(PlomeriaBombaViewModel), typeof(PlomeriaBombaView));
        //    mappings.Add(typeof(PlomeriaLavaboViewModel), typeof(PlomeriaLavaboView));
        //    mappings.Add(typeof(PlomeriaTarjaViewModel), typeof(PlomeriaTarjaView));
        //    mappings.Add(typeof(PlomeriaRegaderaViewModel), typeof(PlomeriaRegaderaView));
        //    mappings.Add(typeof(LavadoDeCisternasViewModel), typeof(LavadoDeCisternasView));
        //    mappings.Add(typeof(PinturaViewModel), typeof(PinturaView));
        //    mappings.Add(typeof(PisosViewModel), typeof(PisosView));
        //    mappings.Add(typeof(ReparacionDeVentanaViewModel), typeof(ReparacionDeVentanaView));
        //    mappings.Add(typeof(ColocacionDeVentanaNuevaViewModel), typeof(ColocacionDeVentanaNuevaView));
        //    mappings.Add(typeof(PisoColocacionDeCeramicaViewModel), typeof(PisoColocacionDeCeramicaView));
        //    mappings.Add(typeof(CanceleriaAditamentosViewModel), typeof(CanceleriaAditamentosView));
        //    mappings.Add(typeof(MantenimientoDePinturaViewModel), typeof(MantenimientoDePinturaView));
        //    mappings.Add(typeof(PintadoRetiroDeHumedadViewModel), typeof(PintadoRetiroDeHumedadView));
        //    mappings.Add(typeof(PinturaImpermeabilizacionViewModel), typeof(PinturaImpermeabilizacionView));
        //    mappings.Add(typeof(PisoLaminadoViewModel), typeof(PisoLaminadoView));
        //    mappings.Add(typeof(ReparacionDePisoViewModel), typeof(ReparacionDePisoView));
        //    mappings.Add(typeof(LavadoLimpiezaCisternaViewModel), typeof(LavadoLimpiezaCisternaView));
        //    mappings.Add(typeof(LavadoLimpiezaTinacoViewModel), typeof(LavadoLimpiezaTinacoView));
        //    mappings.Add(typeof(CerrajeriaViewModel), typeof(CerrajeriaView));
        //    mappings.Add(typeof(CerrajeriaChapaViewModel), typeof(CerrajeriaChapaView));
        //    mappings.Add(typeof(CerrajeriaAutomovilViewModel), typeof(CerrajeriaAutomovilView));
        //    mappings.Add(typeof(ProcesoDePagoViewModel), typeof(ProcesoDePagoView));
        //    mappings.Add(typeof(MisCitasViewModel), typeof(MisCitasView));
        //    mappings.Add(typeof(AgendarEventoViewModel), typeof(AgendarEventoView));
        //    mappings.Add(typeof(ProyectosEspecialesViewModel), typeof(ProyectosEspecialesView));
        //    mappings.Add(typeof(PergolaViewModel), typeof(PergolaView));
        //    mappings.Add(typeof(SubmenuTechoViewModel), typeof(SubmenuTechoView));
        //    mappings.Add(typeof(LosaViewModel), typeof(LosaView));
        //    mappings.Add(typeof(MuroViewModel), typeof(MuroView));
        //    mappings.Add(typeof(SubmenuRemodelacionViewModel), typeof(SubmenuRemodelacionView));
        //    mappings.Add(typeof(BanoViewModel), typeof(BanoView));
        //    mappings.Add(typeof(CocinaViewModel), typeof(CocinaView));
        //    mappings.Add(typeof(RecamaraViewModel), typeof(RecamaraView));
        //    mappings.Add(typeof(SalaViewModel), typeof(SalaView));
        //    mappings.Add(typeof(SubmenuProyectosViewModel), typeof(SubmenuProyectosView));
        //    mappings.Add(typeof(DisenoEspacioViewModel), typeof(DisenoEspacioView));
        //    mappings.Add(typeof(ProyectoNuevoViewModel), typeof(ProyectoNuevoView));
        //    mappings.Add(typeof(OtrosViewModel), typeof(OtrosView));
        //    mappings.Add(typeof(TerminosViewModel), typeof(TerminosView));
        //    mappings.Add(typeof(AgregarTarjetaViewModel), typeof(AgregarTarjetaView));
        //    mappings.Add(typeof(InternetViewModel), typeof(InternetView));
        //    mappings.Add(typeof(RegistrarHandyManViewModel), typeof(RegistrarHandyManView));
        //    mappings.Add(typeof(HandymanServicesListViewModel), typeof(HandymanServicesList));
        //    mappings.Add(typeof(SanitizacionPopUpViewModel), typeof(SanitizacionPopUpView));
        //    mappings.Add(typeof(ConfirmScheduleServiceViewModel), typeof(ConfirmScheduleService));
        //    #region Fumigacion
        //    mappings.Add(typeof(FumigacionViewModel), typeof(FumigacionView));
        //    mappings.Add(typeof(FumigacionDepartamentoViewModel), typeof(FumigacionDepartamentoView));
        //    mappings.Add(typeof(FumigacionCasaViewModel), typeof(FumigacionCasaView));
        //    mappings.Add(typeof(FumigacionDeOficinaViewModel), typeof(FumigacionDeOficinaView));
        //    #endregion

        //}


    }
}
