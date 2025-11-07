using Microsoft.Extensions.DependencyInjection;
using FMIAutomation.Services;

namespace FMIAutomation.Views
{
    public abstract class BaseContentPage : ContentPage
    {
        private ISessionService? _sessionService;

        protected ISessionService? SessionService 
        { 
            get 
            {
                if (_sessionService == null)
                {
                    try
                    {
                        var services = Handler?.MauiContext?.Services ?? Application.Current?.Handler?.MauiContext?.Services;
                        _sessionService = services?.GetService<ISessionService>();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[BaseContentPage] Erro ao obter SessionService: {ex.Message}");
                    }
                }
                return _sessionService;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Registra atividade quando a página aparece
            SessionService?.UpdateActivity();
            
            System.Diagnostics.Debug.WriteLine($"[BaseContentPage] Página {GetType().Name} apareceu - atividade registrada");
        }
        
        protected override bool OnBackButtonPressed()
        {
            // Registra atividade no botão voltar
            SessionService?.UpdateActivity();
            
            return base.OnBackButtonPressed();
        }
        
        protected void TrackUserActivity()
        {
            SessionService?.UpdateActivity();
        }

        protected void RegisterUserActivity()
        {
            SessionService?.UpdateActivity();
        }
    }
}