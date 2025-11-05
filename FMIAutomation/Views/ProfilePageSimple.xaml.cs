using Microsoft.Maui.Controls;

namespace FMIAutomation.Views
{
    public partial class ProfilePageSimple : ContentPage
    {
        public ProfilePageSimple()
        {
            InitializeComponent();
            LoadSimpleData();
        }

        private async void LoadSimpleData()
        {
            try
            {
                // Teste básico de carregamento
                await Task.Delay(100);
                
                var email = await Microsoft.Maui.Storage.SecureStorage.GetAsync("user_email");
                
                if (!string.IsNullOrEmpty(email))
                {
                    EmailLabelSimple.Text = email;
                    NameLabelSimple.Text = "Usuário Logado";
                }
                else
                {
                    EmailLabelSimple.Text = "Email não encontrado";
                    NameLabelSimple.Text = "Usuário não logado";
                }
            }
            catch (Exception ex)
            {
                EmailLabelSimple.Text = $"Erro: {ex.Message}";
                NameLabelSimple.Text = "Erro ao carregar";
            }
        }
    }
}