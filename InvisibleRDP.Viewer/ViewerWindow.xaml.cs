using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace InvisibleRDP.Viewer
{
    public partial class ViewerWindow : Window
    {
        private readonly RdpClient _client;

        public ViewerWindow(RdpClient client)
        {
            InitializeComponent();
            _client = client;
            
            HostInfoText.Text = $"Session: {_client.SessionId}";
            StatusText.Text = "● Connected";
            
            // Focus canvas for keyboard input
            ScreenCanvas.Focus();
        }

        private void ScreenCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(ScreenCanvas);
            
            // Send mouse move to remote host
            _ = _client.SendInputAsync("MouseMove", new
            {
                x = (int)position.X,
                y = (int)position.Y
            });
        }

        private void ScreenCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(ScreenCanvas);
            var button = e.ChangedButton switch
            {
                MouseButton.Left => "Left",
                MouseButton.Right => "Right",
                MouseButton.Middle => "Middle",
                _ => "Unknown"
            };

            // Send mouse down to remote host
            _ = _client.SendInputAsync("MouseDown", new
            {
                x = (int)position.X,
                y = (int)position.Y,
                button = button
            });
        }

        private void ScreenCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(ScreenCanvas);
            var button = e.ChangedButton switch
            {
                MouseButton.Left => "Left",
                MouseButton.Right => "Right",
                MouseButton.Middle => "Middle",
                _ => "Unknown"
            };

            // Send mouse up to remote host
            _ = _client.SendInputAsync("MouseUp", new
            {
                x = (int)position.X,
                y = (int)position.Y,
                button = button
            });
        }

        private void ScreenCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            // Send key down to remote host
            _ = _client.SendInputAsync("KeyDown", new
            {
                key = e.Key.ToString(),
                systemKey = e.SystemKey.ToString()
            });
        }

        private void ScreenCanvas_KeyUp(object sender, KeyEventArgs e)
        {
            // Send key up to remote host
            _ = _client.SendInputAsync("KeyUp", new
            {
                key = e.Key.ToString(),
                systemKey = e.SystemKey.ToString()
            });
        }

        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            await DisconnectAsync();
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            await DisconnectAsync();
        }

        private async System.Threading.Tasks.Task DisconnectAsync()
        {
            StatusText.Text = "● Disconnecting...";
            await _client.DisconnectAsync();
            Close();
        }
    }
}
