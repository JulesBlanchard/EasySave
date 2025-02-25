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

        // Commandes pour agir sur une sauvegarde donnée (commande paramétrée)
        public ICommand PauseBackupCommand { get; }
        public ICommand ResumeBackupCommand { get; }
        public ICommand StopBackupCommand { get; }

        public RemoteConsoleViewModel()
        {
            // Instanciation des commandes avec un paramètre de type BackupState
            PauseBackupCommand = new RelayCommand<BackupState>(PauseBackup);
            ResumeBackupCommand = new RelayCommand<BackupState>(ResumeBackup);
            StopBackupCommand = new RelayCommand<BackupState>(StopBackup);

            ConnectToServer();
        }

        private async void ConnectToServer()
        {
            try
            {
                client = new TcpClient();
                // Assurez-vous que l'adresse IP correspond bien à celle du serveur distant
                await client.ConnectAsync("10.131.130.100", 5000);
                stream = client.GetStream();
                isConnected = true;
                Console.WriteLine("Client connected successfully to server.");
                StartListening();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to remote server: " + ex.Message);
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
                        // Désérialisation des états (attendu sous forme de BackupState[])
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
                    MessageBox.Show("Connection lost: " + ex.Message);
                    isConnected = false;
                }
            }
        }

        // Méthode exécutée quand l'utilisateur clique sur "Pause" dans la ligne de sauvegarde
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
                    MessageBox.Show("Error sending command: " + ex.Message);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
