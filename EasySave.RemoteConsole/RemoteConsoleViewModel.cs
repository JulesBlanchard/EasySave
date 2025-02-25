using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

        public ICommand PauseCommand { get; }
        public ICommand ResumeCommand { get; }
        public ICommand StopCommand { get; }

        public RemoteConsoleViewModel()
        {
            PauseCommand = new RelayCommand(PauseSelected);
            ResumeCommand = new RelayCommand(ResumeSelected);
            StopCommand = new RelayCommand(StopSelected);

            ConnectToServer();
        }

        private async void ConnectToServer()
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync("192.168.187.152", 5000); // Adresse et port du serveur (à adapter)
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
                        var states = JsonSerializer.Deserialize<BackupState[]>(json);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            BackupStates.Clear();
                            foreach (var state in states)
                            {
                                // Initialiser IsSelected à false pour chaque sauvegarde
                                state.IsSelected = false;
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

        private void PauseSelected()
        {
            var selectedStates = BackupStates.Where(b => b.IsSelected).ToList();
            foreach (var state in selectedStates)
            {
                string command = $"COMMAND {state.Name} PAUSE";
                SendCommand(command);
            }
        }
        private void ResumeSelected()
        {
            var selectedStates = BackupStates.Where(b => b.IsSelected).ToList();
            foreach (var state in selectedStates)
            {
                string command = $"COMMAND {state.Name} RESUME";
                SendCommand(command);
            }
        }
        private void StopSelected()
        {
            var selectedStates = BackupStates.Where(b => b.IsSelected).ToList();
            foreach (var state in selectedStates)
            {
                string command = $"COMMAND {state.Name} STOP";
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
