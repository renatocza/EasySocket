using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EasySocket
{
    public class Client
    {
        private TcpClient tcpClient = new TcpClient();
        private UdpClient udpClient = new UdpClient();
        private IPEndPoint? serverEndPoint;
        private bool _isRunningTcp = false;
        private bool _isRunningUdp = false;
        public async Task StartTcpClient(string serverIp, int port)
        {
            try
            {
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIp, port);
                Console.WriteLine($"Connecting to TCP {serverIp}:{port}");

                _isRunningTcp = true;

                NetworkStream stream = tcpClient.GetStream();

                byte[] data = new byte[1024];
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(data, 0, data.Length)) > 0)
                {
                    string responseData = Encoding.ASCII.GetString(data, 0, bytesRead);
                    Console.WriteLine($"TCP response: {responseData}");
                }

                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP Exception: {ex.Message}");
            }
        }

        public async Task SendTcpMessage(string message)
        {
            if(!_isRunningTcp)
                return;

            byte[] data = Encoding.ASCII.GetBytes(message);
            NetworkStream stream = tcpClient.GetStream();
            await stream.WriteAsync(data, 0, data.Length);
            Console.WriteLine($"Sending TCP: {message}");
        }

        public async Task DisconnectTcp()
        {
            _isRunningTcp = false;
            tcpClient.Close();
        }

        public async Task StartUdpClient(string serverIp, int port)
        {
            try
            {
                
                serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), port);
                udpClient = new UdpClient(1234);
                udpClient.Connect(serverEndPoint);
                _isRunningUdp = true;
                
                IPEndPoint localEndPoint = (IPEndPoint)udpClient.Client.LocalEndPoint;

                await StartReceiving();
                
                

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro UDP: {ex.Message}");
            }
        }

        private async Task StartReceiving()
        {
            while (_isRunningUdp)
            {
                UdpReceiveResult response = await udpClient.ReceiveAsync();
                string responseMessage = Encoding.UTF8.GetString(response.Buffer);
                Console.WriteLine("Received response from server: " + responseMessage);
            }
        }

        public async Task SendUdpMessage(string message)
        {
            if (!_isRunningUdp)
                return;

            byte[] data = Encoding.ASCII.GetBytes(message);
            await udpClient.SendAsync(data, data.Length);
            Console.WriteLine($"Sending UDP {message}");
        }

        public async Task DisconnectUdp()
        {
            _isRunningUdp = false;
            udpClient.Close();
        }
    }
}
