using System;
using System.Windows;
using EasySave.GUI.Remote;


namespace EasySave.RemoteConsole
{
    public partial class App : Application
    {
        private RemoteConsoleServer remoteServer;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Démarrer le serveur sur le port configuré (ici 5000)
            remoteServer = new RemoteConsoleServer(5000);
            remoteServer.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            remoteServer?.Stop();
            base.OnExit(e);
        }
    }
}