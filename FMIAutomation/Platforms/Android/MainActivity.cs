using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;

namespace FMIAutomation;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[MainActivity] OnCreate iniciado");
            
            // Configuração para evitar problemas de inicialização
            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;
            
            System.Diagnostics.Debug.WriteLine("[MainActivity] Chamando base.OnCreate");
            base.OnCreate(savedInstanceState);
            
            System.Diagnostics.Debug.WriteLine("[MainActivity] OnCreate concluído com sucesso");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainActivity] ERRO em OnCreate: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"[MainActivity] Mensagem: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[MainActivity] StackTrace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"[MainActivity] InnerException: {ex.InnerException.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"[MainActivity] InnerException Message: {ex.InnerException.Message}");
                System.Diagnostics.Debug.WriteLine($"[MainActivity] InnerException StackTrace: {ex.InnerException.StackTrace}");
            }
            
            throw;
        }
    }
}
