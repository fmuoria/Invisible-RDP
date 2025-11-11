using System.Windows;

namespace InvisibleRDP.SystemTray
{
    public partial class App : Application
    {
        private System.Windows.Forms.NotifyIcon? _notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Hide main window on startup
            MainWindow = new MainWindow();
            MainWindow.Hide();
            
            // Create system tray icon
            InitializeSystemTray();
        }

        private void InitializeSystemTray()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Shield,
                Visible = true,
                Text = "InvisibleRDP Host - Remote desktop service running"
            };

            // Create context menu
            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            
            contextMenu.Items.Add("View Consent", null, (s, e) => ShowConsentDialog());
            contextMenu.Items.Add("View Logs", null, (s, e) => ShowLogs());
            contextMenu.Items.Add("Status", null, (s, e) => ShowStatus());
            contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => ShowStatus();

            // Show initial notification
            _notifyIcon.ShowBalloonTip(3000, "InvisibleRDP Host", 
                "Remote desktop service is running. Right-click icon for options.", 
                System.Windows.Forms.ToolTipIcon.Info);
        }

        private void ShowConsentDialog()
        {
            var consentWindow = new ConsentWindow();
            consentWindow.ShowDialog();
        }

        private void ShowLogs()
        {
            var logsWindow = new LogsWindow();
            logsWindow.Show();
        }

        private void ShowStatus()
        {
            var statusWindow = new StatusWindow();
            statusWindow.Show();
        }

        private void ExitApplication()
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit? Remote desktop access will be disabled.",
                "Exit InvisibleRDP",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _notifyIcon?.Dispose();
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}
