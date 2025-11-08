using Android;
using Android.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace FMIAutomation.Platforms.Android
{
    public static class BluetoothPermissionHelper
    {
        private const int BLUETOOTH_PERMISSION_REQUEST_CODE = 1001;
        
        public static bool CheckBluetoothPermissions()
        {
            var activity = Platform.CurrentActivity;
            if (activity == null)
                return false;
                
            // Para Android 12+ (API 31+)
            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.S)
            {
                var scanPermission = ContextCompat.CheckSelfPermission(activity, Manifest.Permission.BluetoothScan);
                var connectPermission = ContextCompat.CheckSelfPermission(activity, Manifest.Permission.BluetoothConnect);
                
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-PERMISSIONS] API 31+ - Scan: {scanPermission}, Connect: {connectPermission}");
                
                return scanPermission == global::Android.Content.PM.Permission.Granted && 
                       connectPermission == global::Android.Content.PM.Permission.Granted;
            }
            else
            {
                // Para Android < 12 (API < 31)
                var bluetoothPermission = ContextCompat.CheckSelfPermission(activity, Manifest.Permission.Bluetooth);
                var bluetoothAdminPermission = ContextCompat.CheckSelfPermission(activity, Manifest.Permission.BluetoothAdmin);
                var locationPermission = ContextCompat.CheckSelfPermission(activity, Manifest.Permission.AccessFineLocation);
                
                System.Diagnostics.Debug.WriteLine($"[BLUETOOTH-PERMISSIONS] API <31 - Bluetooth: {bluetoothPermission}, Admin: {bluetoothAdminPermission}, Location: {locationPermission}");
                
                return bluetoothPermission == global::Android.Content.PM.Permission.Granted && 
                       bluetoothAdminPermission == global::Android.Content.PM.Permission.Granted &&
                       locationPermission == global::Android.Content.PM.Permission.Granted;
            }
        }
        
        public static void RequestBluetoothPermissions()
        {
            var activity = Platform.CurrentActivity as AndroidX.AppCompat.App.AppCompatActivity;
            if (activity == null)
            {
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH-PERMISSIONS] ❌ Não foi possível obter a Activity atual");
                return;
            }
            
            System.Diagnostics.Debug.WriteLine("[BLUETOOTH-PERMISSIONS] 📋 Solicitando permissões Bluetooth...");
            
            // Para Android 12+ (API 31+)
            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.S)
            {
                var permissions = new string[]
                {
                    Manifest.Permission.BluetoothScan,
                    Manifest.Permission.BluetoothConnect,
                    Manifest.Permission.AccessFineLocation
                };
                
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH-PERMISSIONS] Solicitando permissões API 31+...");
                ActivityCompat.RequestPermissions(activity, permissions, BLUETOOTH_PERMISSION_REQUEST_CODE);
            }
            else
            {
                // Para Android < 12 (API < 31)
                var permissions = new string[]
                {
                    Manifest.Permission.Bluetooth,
                    Manifest.Permission.BluetoothAdmin,
                    Manifest.Permission.AccessFineLocation,
                    Manifest.Permission.AccessCoarseLocation
                };
                
                System.Diagnostics.Debug.WriteLine("[BLUETOOTH-PERMISSIONS] Solicitando permissões API <31...");
                ActivityCompat.RequestPermissions(activity, permissions, BLUETOOTH_PERMISSION_REQUEST_CODE);
            }
        }
    }
}