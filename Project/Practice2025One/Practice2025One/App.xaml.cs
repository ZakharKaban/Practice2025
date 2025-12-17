using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Practice2025One
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Глобальная обработка необработанных исключений
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            ShowErrorDialog(e.Exception);
        }
        
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                ShowErrorDialog(ex);
            }
        }
        
        private void ShowErrorDialog(Exception ex)
        {
            string message = "Произошла непредвиденная ошибка.\n\n";
            message += $"Сообщение: {ex.Message}\n\n";
            
            if (ex.InnerException != null)
            {
                message += $"Внутренняя ошибка: {ex.InnerException.Message}\n\n";
            }
            
            message += "Приложение продолжит работу, но некоторые функции могут работать некорректно.";
            
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
