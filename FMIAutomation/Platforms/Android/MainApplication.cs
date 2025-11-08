using Android.App;
using Android.Runtime;
using System;

namespace FMIAutomation;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override MauiApp CreateMauiApp()
	{
		try
		{
			return MauiProgram.CreateMauiApp();
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[MainApplication] Erro ao criar MAUI App: {ex}");
			throw;
		}
	}

	public override void OnCreate()
	{
		try
		{
			base.OnCreate();
			System.Diagnostics.Debug.WriteLine("[MainApplication] OnCreate executado com sucesso");
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"[MainApplication] Erro no OnCreate: {ex}");
			throw;
		}
	}
}
