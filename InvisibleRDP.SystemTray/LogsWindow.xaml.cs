using System;
using System.IO;
using System.Windows;

namespace InvisibleRDP.SystemTray
{
    public partial class LogsWindow : Window
    {
        private readonly string _logPath;

        public LogsWindow()
        {
            InitializeComponent();
            _logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "InvisibleRDP", "Logs", "audit.log");
            
            LoadLogs();
        }

        private void LoadLogs()
        {
            try
            {
                if (File.Exists(_logPath))
                {
                    LogsTextBox.Text = File.ReadAllText(_logPath);
                    
                    // Scroll to bottom
                    LogsTextBox.ScrollToEnd();
                }
                else
                {
                    LogsTextBox.Text = "No logs found. The service may not have started yet.";
                }
            }
            catch (Exception ex)
            {
                LogsTextBox.Text = $"Error loading logs: {ex.Message}";
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
