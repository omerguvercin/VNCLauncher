using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using VNCLauncher.Models;
using VNCLauncher.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading;

namespace VNCLauncher;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // Mutex object
    private static Mutex? _mutex = null;
    private const string MutexName = "VNCLauncherSingleInstanceMutex"; // Uygulamanıza özel benzersiz bir isim

    protected override void OnStartup(StartupEventArgs e)
    {
        // Mutex oluşturmaya çalış
        bool createdNew;
        _mutex = new Mutex(true, MutexName, out createdNew);

        if (!createdNew)
        {
            // Başka bir örnek çalışıyor
            // Modern bildirim penceresini göster (Bu kısım kaldırıldı)
            // var notificationDialog = new SingleInstanceNotificationDialog("Uygulama zaten çalışıyor.");
            // notificationDialog.ShowDialog();

            // Standart MessageBox'ı göster
            MessageBox.Show("Uygulama zaten çalışıyor.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);

            Current.Shutdown(); // Bu örneği kapat
            return;
        }

        // İlk örnek çalışıyor, normal başlangıç
        base.OnStartup(e);
        
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        
        // Genel hata yakalama
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
    }
    
    private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);

        string exePath = Assembly.GetExecutingAssembly().Location;
        string? exeDir = Path.GetDirectoryName(exePath);

        if (string.IsNullOrEmpty(exeDir))
        {
            return null;
        }

        string assemblyPath = Path.Combine(exeDir, "Data", "Res", assemblyName.Name + ".dll");

        if (File.Exists(assemblyPath))
        {
            try
            {
                return Assembly.LoadFrom(assemblyPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Assembly yüklenemedi: {assemblyPath}. Hata: {ex.Message}");
                return null;
            }
        }

        return null;
    }
    
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            var exception = e.ExceptionObject as Exception;
            string errorMessage = exception?.Message ?? "Bilinmeyen bir hata oluştu.";
            
            MessageBox.Show($"Beklenmeyen bir hata oluştu: {errorMessage}\n\nUygulama kapatılacak.", 
                          "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch
        {
            // Hata gösterilirken hata oluşursa sessizce devam et
        }
        finally
        {
            Environment.Exit(1);
        }
    }
    
    private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            string errorMessage = e.Exception?.Message ?? "Bilinmeyen bir hata oluştu.";
            
            MessageBox.Show($"Beklenmeyen bir hata oluştu: {errorMessage}", 
                          "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            
            e.Handled = true;
        }
        catch
        {
            // Hata gösterilirken hata oluşursa sessizce devam et
        }
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        // Context menu açıldığında yapılacak işlemler
    }

    private void ContextMenuVncConnect_Click(object sender, RoutedEventArgs e)
    {
        if (Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.ConnectToSelectedVnc();
        }
    }

    private void ContextMenuCopyIp_Click(object sender, RoutedEventArgs e)
    {
        if (Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.CopySelectedIp();
        }
    }

    private void ContextMenuCopyHostname_Click(object sender, RoutedEventArgs e)
    {
        if (Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.CopySelectedHostname();
        }
    }

    private void ContextMenuCopyAll_Click(object sender, RoutedEventArgs e)
    {
        if (Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.CopyAllSelectedInfo();
        }
    }

    private async void ContextMenuAddToAddressBook_Click(object sender, RoutedEventArgs e)
    {
        if (Current.MainWindow is MainWindow mainWindow)
        {
            await mainWindow.AddSelectedToAddressBookAsync();
        }
    }

    private void ContextMenuClearSelection_Click(object sender, RoutedEventArgs e)
    {
        if (Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.ClearSelection();
        }
    }

    // OnExit metodunu override ederek Mutex'i serbest bırak
    protected override void OnExit(ExitEventArgs e)
    {
        if (_mutex != null)
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
        }
        base.OnExit(e);
    }
}
