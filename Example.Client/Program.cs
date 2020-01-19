using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Example.Client
{
    class Program
    {
        public static int Main(String[] args)
        {
            List<SynchronousSocketClient> sockets = new List<SynchronousSocketClient>();
            Console.WriteLine("Number of sockets: ");
            string n = Console.ReadLine();

            try
            {
                for (int i = 0; i < Convert.ToInt32(n); i++)
                {
                    var item = new SynchronousSocketClient();
                    sockets.Add(item);
                    item.StartClient();
                    Thread.Sleep(100);
                }
                Console.WriteLine("Sockets running");
                Console.WriteLine("Type N - TEXT to send a text to a specific socket on the list");
                Console.WriteLine("Example: 3 - Hello World");
                Console.WriteLine("Type B - TEXT to broadcast a message");
                Console.WriteLine("Type QUIT to shutdown");

                
                while (true)
                {
                    try
                    {
                        var x = Console.ReadLine().Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                        int sn = -1;

                        if(int.TryParse(x[0].Replace(" ",""), out sn) && sn>=0)
                        {
                            sockets[sn].Send(x[1]);
                        }
                        else
                        {
                            if (x[0].ToUpper().Replace(" ","") == "QUIT")
                            {
                                break;
                            }
                            else
                            {
                                if(x[0].ToUpper() == "B")
                                {
                                    foreach (var item in sockets)
                                    {
                                        item.Send(x[1]);
                                    }
                                }
                            }
                        }


                    }
                    catch
                    {

                    }
                }
                foreach (var item in sockets)
                {
                    item.Stop();
                }

            }
            catch
            {

            }

            
            return 0;
        }

    }
    public class SynchronousSocketClient
    {
        Socket sender;
        public void StartClient()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                var ipAddress = IPAddress.Parse("127.0.0.1");
                var remoteEP = new IPEndPoint(ipAddress, 1733);

                // Create a TCP/IP  socket.  
                sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    //while (true)
                    //{
                    //    // Receive the response from the remote device.  
                    //    int bytesRec = sender.Receive(bytes);
                    //    Console.WriteLine("Echoed test = {0}",
                    //        Encoding.ASCII.GetString(bytes, 0, bytesRec));
                    //}


                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Stop()
        {
            // Release the socket.  
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        internal void Send(string v)
        {
            sender.Send(Encoding.Default.GetBytes(v));
        }
    }
}
