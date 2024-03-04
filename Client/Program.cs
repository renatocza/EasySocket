// See https://aka.ms/new-console-template for more information

using EasySocket;

Client client = new Client();
bool _configuring = true;
Protocol protocol = Protocol.Undefined;


while (_configuring)
{
    Console.WriteLine("Start TCP or UDP client? (tcp/udp)");
    string input = Console.ReadLine();
    if (input.ToLower() == "tcp")
    {
        Task.Run(() => client.StartTcpClient("127.0.0.1", 1733));
        protocol = Protocol.Tcp;
        _configuring = false;
    }
    else if (input.ToLower() == "udp")
    {
        Task.Run(() => client.StartUdpClient("127.0.0.1", 1734));
        protocol = Protocol.Udp;
        _configuring = false;
    }
    else
    {
        Console.WriteLine("Invalid input.");
    }
}
Console.Write("Enter a message: ");
while (true)
{
    
    string message = Console.ReadLine();
    if (string.IsNullOrEmpty(message))
    {
        continue;
    }
    else if (message.ToLower() != "exit")
    {
        if (protocol == Protocol.Udp)
            await client.SendUdpMessage(message);
        else if (protocol == Protocol.Tcp)
            await client.SendTcpMessage(message);
    }
    else
    {
        if (protocol == Protocol.Udp)
            await client.DisconnectUdp();
        else if (protocol == Protocol.Tcp)
            await client.DisconnectTcp();
        break;
    }
}


public enum Protocol
{
    Undefined,
    Tcp,
    Udp
}