using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EasySocket
{
    public class Server
    {
        private List<Socket> _tcpClients = new List<Socket>();
        private List<IPEndPoint> _udpClients = new List<IPEndPoint>();
        private UdpClient udpServer;
        private TcpListener tcpListener;
        private IPEndPoint broadcastAddress;
        private const int TcpPort = 1733;
        private const int UdpPort = 1734;
        private const int BufferSize = 1024;
        private bool _isRunningTcp = false;
        private bool _isRunningUdp = false;

        public async Task StartTcpServer(int port = TcpPort)
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            Console.WriteLine($"TCP Server started @ {port}.");

            try
            {
                _isRunningTcp = true;
                while (_isRunningTcp)
                {
                    Socket tcpClientSocket = await tcpListener.AcceptSocketAsync();
                    _tcpClients.Add(tcpClientSocket);

                    Console.WriteLine($"Connected: {tcpClientSocket.RemoteEndPoint}");

                    Task.Run(() => HandleTcpClientAsync(tcpClientSocket));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP exception: {ex.Message}");
            }
            finally
            {
                foreach (var client in _tcpClients)
                {
                    client.Close();
                }
                tcpListener.Stop();
            }
        }

        public async Task StopTcpServer()
        {
            _isRunningTcp = false;
            await Task.Delay(1000);
        }

        private async Task HandleTcpClientAsync(Socket client)
        {
            try
            {
                byte[] buffer = new byte[BufferSize];

                while (true)
                {
                    int bytesRead = await client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

                    if (bytesRead == 0)
                    {
                        Console.WriteLine($"TCP Client {client.RemoteEndPoint} disconnected.");
                        _tcpClients.Remove(client);
                        break;
                    }

                    string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"TCP message from {client.RemoteEndPoint}: {receivedData}");

                    // Echo the message back to the client
                    byte[] responseBuffer = Encoding.ASCII.GetBytes($"Echo TCP: {receivedData}");
                    await client.SendAsync(new ArraySegment<byte>(responseBuffer), SocketFlags.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP Exception: {ex.Message}");
            }
        }

        public async Task StartUdpServer(int port = UdpPort, bool sendEcho = false, int broadcastPort = 0)
        {
            udpServer = new UdpClient(port);

            if (broadcastPort != 0)
            {
                broadcastAddress = new IPEndPoint(IPAddress.Broadcast, broadcastPort);
                udpServer.EnableBroadcast = true;
            }
            Console.WriteLine($"UDP Server started @ {port}.");

            try
            {
                _isRunningUdp = true;
                while (_isRunningUdp)
                {
                    UdpReceiveResult result = await udpServer.ReceiveAsync();
                    IPEndPoint clientEndPoint = result.RemoteEndPoint;

                    if (!_udpClients.Contains(clientEndPoint))
                    {
                        _udpClients.Add(clientEndPoint);
                        Console.WriteLine($"Connected: {clientEndPoint}");
                    }

                    string receivedData = Encoding.ASCII.GetString(result.Buffer);
                    Console.WriteLine($"Echo {clientEndPoint}: {receivedData}");
                    if (sendEcho)
                    {
                        byte[] responseBuffer = Encoding.ASCII.GetBytes(receivedData);

                        await udpServer.SendAsync(responseBuffer, responseBuffer.Length, clientEndPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP Exception: {ex.Message}");
            }
            finally
            {
                udpServer.Close();
            }
        }


        public void SendBroadcast(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            udpServer.Send(data, data.Length, broadcastAddress);
        }

        public async Task StopUdpServer()
        {
            _isRunningUdp = false;
            await Task.Delay(1000);
        }

    }
}
