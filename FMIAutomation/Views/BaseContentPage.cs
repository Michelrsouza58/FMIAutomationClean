namespace FMIAutomation.Views
{
    public abstract class BaseContentPage : ContentPage
    {
        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Sessão removida - funcionalidade básica mantida
        }
        
        protected override bool OnBackButtonPressed()
        {
            // Sessão removida - funcionalidade básica mantida
            return base.OnBackButtonPressed();
        }
        
        protected void TrackUserActivity()
        {
            // Sessão removida - funcionalidade básica mantida
        }
    }
}