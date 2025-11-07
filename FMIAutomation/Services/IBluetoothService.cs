using FMIAutomation.Models;
using System.Collections.ObjectModel;

namespace FMIAutomation.Services
{
    public interface IBluetoothService
    {
        /// <summary>
        /// Verifica se as permissões de Bluetooth foram concedidas
        /// </summary>
        Task<bool> CheckPermissionsAsync();
        
        /// <summary>
        /// Solicita permissões de Bluetooth ao usuário
        /// </summary>
        Task<bool> RequestPermissionsAsync();
        
        /// <summary>
        /// Verifica se o Bluetooth está habilitado no dispositivo
        /// </summary>
        Task<bool> IsBluetoothEnabledAsync();
        
        /// <summary>
        /// Habilita o Bluetooth (solicita ao usuário)
        /// </summary>
        Task<bool> EnableBluetoothAsync();
        
        /// <summary>
        /// Inicia o scan por dispositivos Bluetooth
        /// </summary>
        Task<bool> StartScanAsync();
        
        /// <summary>
        /// Para o scan por dispositivos Bluetooth
        /// </summary>
        Task StopScanAsync();
        
        /// <summary>
        /// Obtém a lista de dispositivos descobertos
        /// </summary>
        Task<ObservableCollection<BluetoothDevice>> GetDiscoveredDevicesAsync();
        
        /// <summary>
        /// Obtém a lista de dispositivos pareados
        /// </summary>
        Task<ObservableCollection<BluetoothDevice>> GetPairedDevicesAsync();
        
        /// <summary>
        /// Tenta parear com um dispositivo
        /// </summary>
        Task<bool> PairDeviceAsync(BluetoothDevice device);
        
        /// <summary>
        /// Conecta com um dispositivo
        /// </summary>
        Task<bool> ConnectDeviceAsync(BluetoothDevice device);
        
        /// <summary>
        /// Desconecta de um dispositivo
        /// </summary>
        Task<bool> DisconnectDeviceAsync(BluetoothDevice device);
        
        /// <summary>
        /// Remove o pareamento com um dispositivo
        /// </summary>
        Task<bool> UnpairDeviceAsync(BluetoothDevice device);
        
        /// <summary>
        /// Evento disparado quando um novo dispositivo é descoberto
        /// </summary>
        event EventHandler<BluetoothDevice>? DeviceDiscovered;
        
        /// <summary>
        /// Evento disparado quando o status do scan muda
        /// </summary>
        event EventHandler<bool>? ScanStatusChanged;
        
        /// <summary>
        /// Evento disparado quando o status de conexão de um dispositivo muda
        /// </summary>
        event EventHandler<BluetoothDevice>? DeviceConnectionChanged;
    }
}