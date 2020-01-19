using EasySocket.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasySocket
{
    //-------------------------------------- NOTES ------------------------------------
    /*
     * 
     * THIS PROJECT IS A WORK IN PROGRESS EASY SOCKET SERVER THAT HANDLES ALL INCOMING MESSAGES TO A EVENT HANDLER
     * 
     * UDP IS STILL IN DEVELOPMENT
     * 
     * 
     * 
     */

    public class AsyncSocket
    {
        public event EventHandler<DataArrivalEventArgs> OnDataArrival;
        public event EventHandler<DisconnectionEventArgs> OnDisconnect;
        public List<StateObject> Sessions = new List<StateObject>();
        private Socket listener;
        private bool log = false;
        private int numTentativas = 30;

        //UDP
        private byte[] byteData = new byte[1024];
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);

        private AsyncCallback recv = null;
        private State state = new State();
        //UDP

        public bool rodando = false;


        public class State
        {
            public byte[] buffer = new byte[1024];
        }


        public AsyncSocket(bool log = false, int numTentativas = 3)
        {
            this.log = log;
            this.numTentativas = numTentativas;
        }

        public void Broadcast(byte[] data)
        {
            if (listener.ProtocolType == ProtocolType.Tcp)
                foreach (var item in Sessions)
                {
                    Send(item, data);
                }
        }

        public void Broadcast(string data)
        {
            Broadcast(Encoding.Default.GetBytes(data));
        }
        public void Send(StateObject session, string data)
        {
            Send(session, Encoding.Default.GetBytes(data));
        }

        public void Send(string sessionId, string data)
        {
            Send(sessionId, Encoding.Default.GetBytes(data));
        }



        private void AsynchronousSocketListener_SocketAccepted(object sender, SocketAcceptedEventHandler e)
        {
            StateObject state = new StateObject()
            {
                buffer = new byte[e.Session.ReceiveBufferSize],
                IP = e.Session.RemoteEndPoint.ToString(),
                SessionServico = false,
                workSocket = e.Session
            };
            Sessions.Add(state);
            e.Session.BeginReceive(state.buffer, 0, 0, SocketFlags.None, ReceiveCallback, state);
        }

        public void StartListening(string ip, int porta, ProtocolType type = ProtocolType.Tcp)
        {

            IPAddress ipAddress;
            IPEndPoint localEndPoint;
            try
            {
                ipAddress = IPAddress.Parse(ip);
                localEndPoint = new IPEndPoint(ipAddress, porta);
            }
            catch
            {
                ipAddress = Dns.GetHostAddresses(ip).First();
                localEndPoint = new IPEndPoint(ipAddress, porta);
            }

            if (type == ProtocolType.Tcp)
            {
                SocketAccepted -= AsynchronousSocketListener_SocketAccepted;
                SocketAccepted += AsynchronousSocketListener_SocketAccepted;
            }

            if (Sessions.Count > 0)
            {
                foreach (var item in Sessions)
                {
                    RemoveSocket(item);
                }
                Sessions.Clear();
            }
            // Create a TCP/IP socket.  
            if (type == ProtocolType.Tcp)
            {
                listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, type);
                listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                listener.SendTimeout = 30000;
            }
            //else
            //{
            //      Not implemented
            //    listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, type);
            //    listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            //}



            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                
                listener.Bind(localEndPoint);
                if (type == ProtocolType.Tcp)
                {
                    listener.Listen(0);
                    Console.WriteLine("Waiting connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                }
                else
                {
                    throw new NotImplementedException("TCP is the only protocol avaliable until now");
                    //Console.WriteLine("Waiting UDP Connection...");
                    //EndPoint newClientEP = new IPEndPoint(IPAddress.Any, 0);
                    ////this.listener.BeginReceiveFrom(this.byteData, 0, this.byteData.Length, SocketFlags.None, ref newClientEP, DoReceiveFrom, newClientEP);
                    //Receive();

                }
                rodando = true;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        private void Receive()
        {
            listener.BeginReceiveFrom(state.buffer, 0, state.buffer.Length, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = listener.EndReceiveFrom(ar, ref epFrom);
                listener.BeginReceiveFrom(so.buffer, 0, so.buffer.Length, SocketFlags.None, ref epFrom, recv, so);
                OnDataArrival(null, new DataArrivalEventArgs(so.buffer, listener.RemoteEndPoint.ToString(), "", DateTime.Now.TimeOfDay));
                //Console.WriteLine($"RECEIVED {Encoding.Default.GetString(so.buffer)}");
            }, state);
        }

        void AcceptCallback(IAsyncResult ar)
        {

            try
            {
                Socket s = this.listener.EndAccept(ar);

                if (SocketAccepted != null)
                {
                    SocketAccepted(this, new SocketAcceptedEventHandler(s));
                }

                this.listener.BeginAccept(AcceptCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public event EventHandler<SocketAcceptedEventHandler> SocketAccepted;


        void ReceiveCallback(IAsyncResult ar)
        {

            StateObject state = (StateObject)ar.AsyncState;
            try
            {
                state.workSocket.EndReceive(ar);

                int bytesRead = state.workSocket.Receive(state.buffer);

                if (bytesRead > 0)
                {
                    //Console.WriteLine(BitConverter.ToString(state.buffer.Take(bytesRead).ToArray()));

                    state.DtUltimaMensagem = DateTime.Now;
                    OnDataArrival(null, new DataArrivalEventArgs(state.buffer.Take(bytesRead).ToArray(), state.IP, state.SessionID, DateTime.Now.TimeOfDay) { Session = state });
                    //Console.WriteLine($"RECEIVED {Encoding.Default.GetString(state.buffer.Take(bytesRead).ToArray())}");
                }
                else
                    state.MensagensEmFalha++;

                if (state.MensagensEmFalha < numTentativas)
                    state.workSocket.BeginReceive(state.buffer, 0, 0, 0, ReceiveCallback, state);
                else
                {
                    if (state.MensagensEmFalha >= numTentativas)
                        RemoveSocket(state);
                }
            }
            catch (Exception ex)
            {
                if (state.workSocket != null)
                    RemoveSocket(state);
            }
        }

        internal void RemoveSocket(StateObject state, Exception exc = null)
        {
            try
            {

                if (state != null)
                {
                    Sessions.Remove(state);
                    

                    try
                    {
                        if (state.workSocket.Connected)
                            state.workSocket.Shutdown(SocketShutdown.Both);
                    }
                    catch (SocketException ex)
                    {
                        try
                        {
                            state.workSocket.Shutdown(SocketShutdown.Send);
                            exc = ex;
                        }
                        catch(Exception exs)
                        {
                            //log
                        }

                    }
                    finally
                    {
                        state.workSocket.Close();
                        state.workSocket.Dispose();
                        OnDisconnect(null, new DisconnectionEventArgs(state.SessionID, exc));

                    }

                }
            }
            catch (Exception ex)
            {
                //log
            }
        }


        public void Send(string sessionID, byte[] Data)
        {
            StateObject session = GetSessionByID(sessionID);
            try
            {

                if (session == null)
                    return;
                // Begin sending the data to the remote device.  
                session.workSocket.Send(Data);
            }
            catch (ObjectDisposedException ex)
            {
                RemoveSocket(session);
            }
            catch (SocketException ex)
            {
                RemoveSocket(session);
            }
            catch { }
        }

        public bool Send(StateObject session, byte[] Data)
        {
            try
            {
                // Begin sending the data to the remote device.  
                session.workSocket.Send(Data);
                Console.WriteLine("Sending to Client");
                return true;
            }
            catch (ObjectDisposedException ex)
            {
                RemoveSocket(session);
                return false;
            }
            catch (SocketException ex)
            {
                RemoveSocket(session);
                return false;
            }
            catch (Exception ex)
            {
                RemoveSocket(session);
                return false;
            }
        }



        internal StateObject GetSessionByID(string sessionConexao)
        {
            try
            {
                lock (Sessions)
                {
                    return Sessions.ToArray().FirstOrDefault(x => x.SessionID == sessionConexao);
                }
            }
            catch
            {
                return null;
            }
        }

        internal void Stop()
        {
            rodando = false;

            try
            {

                foreach (var item in Sessions.ToArray())
                {
                    RemoveSocket(item);
                }
                try
                {
                    try { listener.Shutdown(SocketShutdown.Both); }
                    catch { listener.Disconnect(true); }
                }
                catch
                {

                }
                listener.Close();
                listener.Dispose();

            }
            catch
            {

            }
            Sessions.Clear();
        }


        public void RestartListener()
        {
            var ipep = listener.LocalEndPoint.ToString();
            Stop();

            while (Sessions.Count != 0)
            {

            }

            Thread.Sleep(10 * 1000);

            StartListening(ipep.Split(':')[0], Convert.ToInt32(ipep.Split(':')[1]));

        }

    }

    public class SocketAcceptedEventHandler : EventArgs
    {
        public Socket Session
        {
            get;
            private set;
        }
        public SocketAcceptedEventHandler(Socket s)
        {
            Session = s;
        }
    }

}
