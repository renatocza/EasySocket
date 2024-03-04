# EasySocket

## Server

The `Server.cs` file in the EasySocket library provides functionality for creating and managing a TCP and UDP server.

### TCP Server

To start a TCP server, you can use the `StartTcpServer` method. By default, it listens on port 1733, but you can specify a different port if needed.
To stop the TCP server, you can use the `StopTcpServer` method.
### UDP Server

To start a UDP server, you can use the `StartUdpServer` method. By default, it listens on port 1734, but you can specify a different port if needed.
To stop the UDP server, you can use the `StopUdpServer` method.
You can also enable the echo functionality, which sends back the received message to the client, by passing `true` as the second parameter to the `StartUdpServer` method.
To send a broadcast message to all connected UDP clients, you can use the `SendBroadcast` method.
## Client

The `Client.cs` file in the EasySocket library provides functionality for creating and managing a TCP and UDP client.

### TCP Client

To start a TCP client, you can use the `StartTcpClient` method. You need to provide the server IP address and port number as parameters.
To send a message to the TCP server, you can use the `SendTcpMessage` method.
To disconnect from the TCP server, you can use the `DisconnectTcp` method.
### UDP Client

To start a UDP client, you can use the `StartUdpClient` method. You need to provide the server IP address and port number as parameters.
To send a message to the UDP server, you can use the `SendUdpMessage` method.
To disconnect from the UDP server, you can use the `DisconnectUdp` method.


# Future Work:
	- Add support for gRPC
    - Add support for WebSockets
	- Better error handling
	- Separation of TCP and UDP server and client classes