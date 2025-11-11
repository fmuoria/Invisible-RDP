using System;
using System.Windows;

namespace InvisibleRDP.Viewer
{
    public partial class ConnectWindow : Window
    {
        public ConnectWindow()
        {
            InitializeComponent();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var hostIp = HostIpTextBox.Text.Trim();
            var portText = PortTextBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(hostIp))
            {
                MessageBox.Show("Please enter the host IP address.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter the password.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(portText, out int port) || port <= 0 || port > 65535)
            {
                MessageBox.Show("Please enter a valid port number (1-65535).", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                StatusTextBlock.Text = "Connecting...";
                
                var rdpClient = new RdpClient(hostIp, port, password);
                var connected = await rdpClient.ConnectAsync();

                if (connected)
                {
                    StatusTextBlock.Text = "Connected successfully!";
                    
                    // Open viewer window
                    var viewerWindow = new ViewerWindow(rdpClient);
                    viewerWindow.Show();
                    Close();
                }
                else
                {
                    StatusTextBlock.Text = "Connection failed. Please check your credentials and try again.";
                    MessageBox.Show("Failed to connect to the remote host.", "Connection Failed", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error: {ex.Message}";
                MessageBox.Show($"Connection error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
