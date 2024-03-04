// See https://aka.ms/new-console-template for more information
using EasySocket;

Server socket = new Server();

Task.Run(() => socket.StartTcpServer(1733));
Task.Run(() => socket.StartUdpServer(1734, true));

while (true)
{
    Console.ReadLine();
    socket.StopUdpServer();
    socket.StopTcpServer();
    break;
}
