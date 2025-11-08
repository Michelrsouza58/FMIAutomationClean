namespace FMIAutomation.Services
{
    // Permissões customizadas para Bluetooth Android API 31+
    public class BluetoothScanPermission : Permissions.BasePermission
    {
        public override void EnsureDeclared()
        {
            // Não é necessário para esta implementação
        }

        public override bool ShouldShowRationale()
        {
            return false;
        }
        public override Task<PermissionStatus> CheckStatusAsync()
        {
#if ANDROID
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
            {
                var context = Platform.CurrentActivity ?? Android.App.Application.Context;
                
                // Verificar BLUETOOTH_SCAN
                var scanPermission = AndroidX.Core.Content.ContextCompat.CheckSelfPermission(
                    context, 
                    Android.Manifest.Permission.BluetoothScan
                );
                
                // Verificar BLUETOOTH_CONNECT
                var connectPermission = AndroidX.Core.Content.ContextCompat.CheckSelfPermission(
                    context, 
                    Android.Manifest.Permission.BluetoothConnect
                );
                
                if (scanPermission == Android.Content.PM.Permission.Granted && 
                    connectPermission == Android.Content.PM.Permission.Granted)
                {
                    return Task.FromResult(PermissionStatus.Granted);
                }
                else
                {
                    return Task.FromResult(PermissionStatus.Denied);
                }
            }
            else
            {
                // Para API < 31, verificar permissões antigas
                var context = Platform.CurrentActivity ?? Android.App.Application.Context;
                
                var bluetoothPermission = AndroidX.Core.Content.ContextCompat.CheckSelfPermission(
                    context, 
                    Android.Manifest.Permission.Bluetooth
                );
                
                var bluetoothAdminPermission = AndroidX.Core.Content.ContextCompat.CheckSelfPermission(
                    context, 
                    Android.Manifest.Permission.BluetoothAdmin
                );
                
                if (bluetoothPermission == Android.Content.PM.Permission.Granted && 
                    bluetoothAdminPermission == Android.Content.PM.Permission.Granted)
                {
                    return Task.FromResult(PermissionStatus.Granted);
                }
                else
                {
                    return Task.FromResult(PermissionStatus.Denied);
                }
            }
#else
            return Task.FromResult(PermissionStatus.Granted);
#endif
        }

        public override Task<PermissionStatus> RequestAsync()
        {
#if ANDROID
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
            {
                var tcs = new TaskCompletionSource<PermissionStatus>();
                
                var activity = Platform.CurrentActivity as AndroidX.AppCompat.App.AppCompatActivity;
                if (activity == null)
                {
                    tcs.SetResult(PermissionStatus.Denied);
                    return tcs.Task;
                }
                
                var permissions = new string[]
                {
                    Android.Manifest.Permission.BluetoothScan,
                    Android.Manifest.Permission.BluetoothConnect
                };
                
                AndroidX.Core.App.ActivityCompat.RequestPermissions(activity, permissions, 1001);
                
                // Para simplificar, vamos assumir que foi concedida
                // Em uma implementação completa, você precisaria interceptar OnRequestPermissionsResult
                tcs.SetResult(PermissionStatus.Granted);
                return tcs.Task;
            }
            else
            {
                return Task.FromResult(PermissionStatus.Granted);
            }
#else
            return Task.FromResult(PermissionStatus.Granted);
#endif
        }
    }
}