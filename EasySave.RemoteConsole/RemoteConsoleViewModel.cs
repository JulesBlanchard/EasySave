using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EasySave.GUI;
using EasySave.Models;
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;

namespace EasySave.RemoteConsole
{
    public class RemoteConsoleViewModel : INotifyPropertyChanged
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected;

        public ObservableCollection<BackupState> BackupStates { get; set; } = new ObservableCollection<BackupState>();

        // Properties for IP address and port
        private string serverIP = ""; // No default value for IP
        public string ServerIP
        {
            get => serverIP;
            set { serverIP = value; OnPropertyChanged(nameof(ServerIP)); }
        }

        private string serverPort = "5000"; // Default port
        public string ServerPort
        {
            get => serverPort;
            set { serverPort = value; OnPropertyChanged(nameof(ServerPort)); }
        }

        // Property to display connection status
        private string connectionStatus = "Déconnecté";
        public string ConnectionStatus
        {
            get => connectionStatus;
            set { connectionStatus = value; OnPropertyChanged(nameof(ConnectionStatus)); }
        }

        // Commands to act on a specific backup
        public ICommand PauseBackupCommand { get; }
        public ICommand ResumeBackupCommand { get; }
        public ICommand StopBackupCommand { get; }

        // Command to connect to the server
        public ICommand ConnectCommand { get; }

        public RemoteConsoleViewModel()
        {
            PauseBackupCommand = new RelayCommand<BackupState>(PauseBackup);
            ResumeBackupCommand = new RelayCommand<BackupState>(ResumeBackup);
            StopBackupCommand = new RelayCommand<BackupState>(StopBackup);
            ConnectCommand = new RelayCommand(ConnectToServer);
        }

        private async void ConnectToServer()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ServerIP))
                {
                    MessageBox.Show("Veuillez renseigner l'adresse IP du serveur.");
                    return;
                }
                if (!int.TryParse(ServerPort, out int port))
                {
                    MessageBox.Show("Port invalide. Veuillez entrer un nombre valide.");
                    return;
                }

                client = new TcpClient();
                await client.ConnectAsync(ServerIP, port);
                stream = client.GetStream();
                isConnected = true;
                ConnectionStatus = $"Connecté au serveur {ServerIP}:{ServerPort}";
                Console.WriteLine("Client connecté avec succès au serveur.");
                StartListening();
            }
            catch (Exception ex)
            {
                ConnectionStatus = "Connexion échouée";
                MessageBox.Show("Erreur lors de la connexion au serveur distant : " + ex.Message);
            }
        }

        private async void StartListening()
        {
            byte[] buffer = new byte[4096];
            while (isConnected)
            {
                try
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        var states = JsonSerializer.Deserialize<BackupState[]>(json);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            BackupStates.Clear();
                            foreach (var state in states)
                            {
                                BackupStates.Add(state);
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    ConnectionStatus = "Connexion perdue";
                    MessageBox.Show("Connexion perdue : " + ex.Message);
                    isConnected = false;
                }
            }
        }

        private void PauseBackup(BackupState backupState)
        {
            if (backupState != null)
            {
                string command = $"COMMAND {backupState.Name} PAUSE";
                SendCommand(command);
            }
        }

        private void ResumeBackup(BackupState backupState)
        {
            if (backupState != null)
            {
                string command = $"COMMAND {backupState.Name} RESUME";
                SendCommand(command);
            }
        }

        private void StopBackup(BackupState backupState)
        {
            if (backupState != null)
            {
                string command = $"COMMAND {backupState.Name} STOP";
                SendCommand(command);
            }
        }

        private async void SendCommand(string command)
        {
            if (isConnected && stream != null)
            {
                byte[] data = Encoding.UTF8.GetBytes(command);
                try
                {
                    await stream.WriteAsync(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur lors de l'envoi de la commande : " + ex.Message);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
