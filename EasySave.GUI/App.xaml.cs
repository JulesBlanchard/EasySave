using System;
using System.Windows;
using System.Windows.Threading;
using EasySave.GUI.Remote;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace EasySave.GUI
{
    public partial class App : Application
    {
        private RemoteConsoleServer remoteServer;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Gestionnaire pour les exceptions sur le thread UI
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            // Gestionnaire pour les exceptions sur d’autres threads
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            // Démarrage de la console déportée sur le port 5000
            remoteServer = new RemoteConsoleServer(5000);
            remoteServer.Start();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Arrêt de la console déportée lors de la fermeture de l'application
            remoteServer?.Stop();
            base.OnExit(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Une exception non gérée s'est produite : " + e.Exception.ToString(),
                "Erreur non gérée", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show("Une exception non gérée (domain) s'est produite : " + ex?.Message,
                "Erreur non gérée", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}