using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EasySave.Models;
using EasySave.Controllers;

namespace EasySave.GUI.Remote
{
    public class RemoteConsoleServer
    {
        private TcpListener listener;
        private bool isRunning;
        private List<TcpClient> clients = new List<TcpClient>();
        private CancellationTokenSource cts;
        
        public event Action<TcpClient> ClientConnected;

        public int Port { get; private set; } = 5000; // port configurable

        public RemoteConsoleServer(int port = 5000)
        {
            Port = port;
            listener = new TcpListener(IPAddress.Any, Port);
            cts = new CancellationTokenSource();
        }

        public void Start()
        {
            isRunning = true;
            listener.Start();
            Task.Run(() => AcceptClientsAsync(cts.Token));
            Task.Run(() => BroadcastStateLoop(cts.Token));
            Console.WriteLine($"[RemoteConsoleServer] Server started on port {Port}");
        }

        public void Stop()
        {
            isRunning = false;
            cts.Cancel();
            listener.Stop();
            lock (clients)
            {
                foreach (var client in clients)
                {
                    client.Close();
                }
                clients.Clear();
            }
        }

        private async Task AcceptClientsAsync(CancellationToken token)
        {
            while (isRunning)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    Console.WriteLine($"[RemoteConsoleServer] Client connected from {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                    ClientConnected?.Invoke(client);
            
                    lock (clients)
                    {
                        clients.Add(client);
                    }
                    Task.Run(() => HandleClientAsync(client, token));
                }
                catch (Exception ex)
                {
                    if (isRunning)
                        Console.WriteLine("[RemoteConsoleServer] Error accepting client: " + ex.Message);
                }
            }
        }


        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            while (isRunning && client.Connected)
            {
                try
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                        if (bytesRead > 0)
                        {
                            string command = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            ProcessCommand(command);
                        }
                    }
                    else
                    {
                        await Task.Delay(200, token);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[RemoteConsoleServer] Error handling client: " + ex.Message);
                    break;
                }
            }
            lock (clients)
            {
                clients.Remove(client);
            }
        }

        private void ProcessCommand(string command)
        {
            try
            {
                var parts = command.Trim().Split(' ');
                if (parts.Length >= 3 && parts[0].ToUpper() == "COMMAND")
                {
                    string backupName = parts[1];
                    string action = parts[2].ToUpper();
                    var backups = BackupController.Instance.GetBackups();
                    var backup = backups.Find(b => b.Name.Equals(backupName, StringComparison.OrdinalIgnoreCase));
                    if (backup != null)
                    {
                        switch (action)
                        {
                            case "PAUSE":
                                backup.JobControl.Pause(backup);
                                break;
                            case "RESUME":
                                backup.JobControl.Resume(backup);
                                break;
                            case "STOP":
                                backup.JobControl.Stop(backup);
                                break;
                        }
                        Console.WriteLine($"[RemoteConsoleServer] Command '{action}' appliquée à '{backupName}'.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[RemoteConsoleServer] Error processing command: " + ex.Message);
            }
        }

        private async Task BroadcastStateLoop(CancellationToken token)
        {
            while (isRunning)
            {
                try
                {
                    var states = StateManager.GetCurrentStates();
                    string json = JsonSerializer.Serialize(states);
                    byte[] data = Encoding.UTF8.GetBytes(json);
                    lock (clients)
                    {
                        foreach (var client in clients)
                        {
                            try
                            {
                                if (client.Connected)
                                {
                                    client.GetStream().Write(data, 0, data.Length);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("[RemoteConsoleServer] Error broadcasting to client: " + ex.Message);
                            }
                        }
                    }
                    await Task.Delay(1000, token); 
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    Console.WriteLine("[RemoteConsoleServer] Error in broadcast loop: " + ex.Message);
                }
            }
        }
    }
}
