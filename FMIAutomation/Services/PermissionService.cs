namespace FMIAutomation.Services
{
    public interface IPermissionService
    {
        /// <summary>
        /// Verifica se todas as permissões de Bluetooth foram concedidas
        /// </summary>
        Task<bool> CheckBluetoothPermissionsAsync();
        
        /// <summary>
        /// Solicita todas as permissões necessárias para Bluetooth
        /// </summary>
        Task<bool> RequestBluetoothPermissionsAsync();
        
        /// <summary>
        /// Abre as configurações do app para o usuário conceder permissões manualmente
        /// </summary>
        Task OpenAppSettingsAsync();
    }
    
    public class PermissionService : IPermissionService
    {
        public async Task<bool> CheckBluetoothPermissionsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[PERMISSIONS] Verificando permissões de Bluetooth...");
                
                // Verificar permissão de localização (necessária para Bluetooth)
                var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                System.Diagnostics.Debug.WriteLine($"[PERMISSIONS] Status localização: {locationStatus}");
                
                if (locationStatus != PermissionStatus.Granted)
                {
                    System.Diagnostics.Debug.WriteLine("[PERMISSIONS] Permissão de localização não concedida");
                    return false;
                }
                
                System.Diagnostics.Debug.WriteLine("[PERMISSIONS] Todas as permissões verificadas com sucesso");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PERMISSIONS] Erro ao verificar permissões Bluetooth: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> RequestBluetoothPermissionsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[PERMISSIONS] Solicitando permissões de Bluetooth...");
                
                // Solicitar permissão de localização (necessária para Bluetooth no Android)
                var locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                System.Diagnostics.Debug.WriteLine($"[PERMISSIONS] Resultado permissão localização: {locationStatus}");
                
                if (locationStatus != PermissionStatus.Granted)
                {
                    System.Diagnostics.Debug.WriteLine("[PERMISSIONS] Usuário negou permissão de localização");
                    return false;
                }
                
                System.Diagnostics.Debug.WriteLine("[PERMISSIONS] Todas as permissões Bluetooth concedidas!");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PERMISSIONS] Erro ao solicitar permissões Bluetooth: {ex.Message}");
                return false;
            }
        }
        
        public async Task OpenAppSettingsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[PERMISSIONS] Abrindo configurações do app...");
                AppInfo.ShowSettingsUI();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PERMISSIONS] Erro ao abrir configurações: {ex.Message}");
            }
        }
    }
}