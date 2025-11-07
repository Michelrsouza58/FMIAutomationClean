using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace FMIAutomation.Services
{
    public interface ISessionService
    {
        Task StartSessionAsync();
        void UpdateActivity();
        Task<bool> IsSessionValidAsync();
        Task EndSessionAsync();
        event EventHandler<SessionExpiredEventArgs>? SessionExpired;
    }

    public class SessionExpiredEventArgs : EventArgs
    {
        public string Reason { get; set; } = "";
    }

    public class SessionService : ISessionService
    {
        private const int SESSION_TIMEOUT_MINUTES = 15;
        private const string LAST_ACTIVITY_KEY = "last_activity";
        private const string SESSION_START_KEY = "session_start";
        
        private Timer? _sessionTimer;
        private DateTime _lastActivity;
        private readonly object _lockObject = new object();

        public event EventHandler<SessionExpiredEventArgs>? SessionExpired;

        public async Task StartSessionAsync()
        {
            try
            {
                _lastActivity = DateTime.UtcNow;
                
                // Salva o início da sessão
                await SecureStorage.SetAsync(SESSION_START_KEY, _lastActivity.ToBinary().ToString());
                await SecureStorage.SetAsync(LAST_ACTIVITY_KEY, _lastActivity.ToBinary().ToString());
                
                // Inicia o timer para verificar inatividade a cada minuto
                _sessionTimer = new Timer(CheckSessionTimeout, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
                
                System.Diagnostics.Debug.WriteLine($"[SessionService] Sessão iniciada às {_lastActivity:HH:mm:ss}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SessionService] Erro ao iniciar sessão: {ex.Message}");
            }
        }

        public void UpdateActivity()
        {
            try
            {
                lock (_lockObject)
                {
                    _lastActivity = DateTime.UtcNow;
                }
                
                // Salva a última atividade de forma assíncrona
                Task.Run(async () =>
                {
                    try
                    {
                        await SecureStorage.SetAsync(LAST_ACTIVITY_KEY, _lastActivity.ToBinary().ToString());
                        System.Diagnostics.Debug.WriteLine($"[SessionService] Atividade atualizada às {_lastActivity:HH:mm:ss}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SessionService] Erro ao salvar atividade: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SessionService] Erro ao atualizar atividade: {ex.Message}");
            }
        }

        public async Task<bool> IsSessionValidAsync()
        {
            try
            {
                var lastActivityStr = await SecureStorage.GetAsync(LAST_ACTIVITY_KEY);
                if (string.IsNullOrEmpty(lastActivityStr))
                {
                    System.Diagnostics.Debug.WriteLine("[SessionService] Nenhuma atividade registrada");
                    return false;
                }

                if (!long.TryParse(lastActivityStr, out var activityTicks))
                {
                    System.Diagnostics.Debug.WriteLine("[SessionService] Formato de data inválido");
                    return false;
                }

                var lastActivity = DateTime.FromBinary(activityTicks);
                var timeSinceActivity = DateTime.UtcNow - lastActivity;
                
                System.Diagnostics.Debug.WriteLine($"[SessionService] Tempo desde última atividade: {timeSinceActivity.TotalMinutes:F1} minutos");
                
                return timeSinceActivity.TotalMinutes <= SESSION_TIMEOUT_MINUTES;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SessionService] Erro ao verificar sessão: {ex.Message}");
                return false;
            }
        }

        public async Task EndSessionAsync()
        {
            try
            {
                // Para o timer
                _sessionTimer?.Dispose();
                _sessionTimer = null;
                
                // Remove dados da sessão
                SecureStorage.Remove(LAST_ACTIVITY_KEY);
                SecureStorage.Remove(SESSION_START_KEY);
                SecureStorage.Remove("login_time");
                SecureStorage.Remove("user_email");
                
                System.Diagnostics.Debug.WriteLine("[SessionService] Sessão finalizada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SessionService] Erro ao finalizar sessão: {ex.Message}");
            }
        }

        private async void CheckSessionTimeout(object? state)
        {
            try
            {
                var isValid = await IsSessionValidAsync();
                if (!isValid)
                {
                    System.Diagnostics.Debug.WriteLine("[SessionService] Sessão expirada por inatividade");
                    
                    // Para o timer
                    _sessionTimer?.Dispose();
                    _sessionTimer = null;
                    
                    // Notifica sobre a expiração
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        SessionExpired?.Invoke(this, new SessionExpiredEventArgs 
                        { 
                            Reason = $"Sessão expirada após {SESSION_TIMEOUT_MINUTES} minutos de inatividade" 
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SessionService] Erro no check de timeout: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _sessionTimer?.Dispose();
        }
    }
}