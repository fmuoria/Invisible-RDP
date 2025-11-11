using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InvisibleRDP.Viewer
{
    public class RdpClient : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _password;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private string? _sessionId;
        private int _screenWidth;
        private int _screenHeight;

        public string? SessionId => _sessionId;
        public int ScreenWidth => _screenWidth;
        public int ScreenHeight => _screenHeight;
        public bool IsConnected => _client?.Connected ?? false;

        public RdpClient(string host, int port, string password)
        {
            _host = host;
            _port = port;
            _password = password;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_host, _port);
                _stream = _client.GetStream();

                // Send authentication
                var authRequest = new
                {
                    Password = _password,
                    Username = Environment.UserName
                };

                var authJson = JsonSerializer.Serialize(authRequest);
                var authBytes = Encoding.UTF8.GetBytes(authJson);
                await _stream.WriteAsync(authBytes);
                await _stream.FlushAsync();

                // Read response
                var buffer = new byte[4096];
                var bytesRead = await _stream.ReadAsync(buffer);
                var responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                
                var response = JsonSerializer.Deserialize<AuthResponse>(responseJson);

                if (response?.success == true)
                {
                    _sessionId = response.sessionId;
                    _screenWidth = response.screenWidth;
                    _screenHeight = response.screenHeight;
                    return true;
                }

                return false;
            }
            catch
            {
                Dispose();
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_stream != null && IsConnected)
            {
                try
                {
                    var disconnectBytes = Encoding.UTF8.GetBytes("DISCONNECT");
                    await _stream.WriteAsync(disconnectBytes);
                    await _stream.FlushAsync();
                }
                catch
                {
                    // Ignore errors during disconnect
                }
            }
            
            Dispose();
        }

        public async Task SendInputAsync(string inputType, object data)
        {
            if (_stream == null || !IsConnected)
                return;

            try
            {
                var message = new
                {
                    type = inputType,
                    data = data
                };

                var json = JsonSerializer.Serialize(message);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _stream.WriteAsync(bytes);
                await _stream.FlushAsync();
            }
            catch
            {
                // Connection lost
                Dispose();
            }
        }

        public void Dispose()
        {
            _stream?.Close();
            _stream?.Dispose();
            _client?.Close();
            _client?.Dispose();
        }

        private class AuthResponse
        {
            public bool success { get; set; }
            public string? error { get; set; }
            public string? sessionId { get; set; }
            public int screenWidth { get; set; }
            public int screenHeight { get; set; }
        }
    }
}
