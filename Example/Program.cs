using EasySocket;
using EasySocket.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example
{
    class Program
    {

        public static List<StateObject> sessions = new List<StateObject>();

        static void Main(string[] args)
        {
            AsyncSocket asyncSocket = new AsyncSocket(numTentativas: 2);

            asyncSocket.OnDataArrival += AsyncSocket_OnPrecisaTratarDados;
            asyncSocket.OnDisconnect += AsyncSocket_OnSinalizaDesconexao;

            asyncSocket.StartListening("127.0.0.1", 1733);

            bool quit = false;

            while(!quit)
            {
                string x = Console.ReadLine();
                if (!string.IsNullOrEmpty(x) && x != "quit")
                    //send to all conected devices
                    asyncSocket.Broadcast(x);
                else
                    quit = true;
            }
        }

        private static void AsyncSocket_OnSinalizaDesconexao(object sender, DisconnectionEventArgs e)
        {
            Console.WriteLine($"Client {e.SessionID} has disconnected");
            if (e.Exception!=null)
            Console.WriteLine($"Reason: {e.Exception.Message}");
            
               

            var session = sessions.FirstOrDefault(x => x.SessionID == e.SessionID);

            if (session!=null)
            {
                sessions.Remove(session);
            }

        }

        private static void AsyncSocket_OnPrecisaTratarDados(object sender, EasySocket.Helpers.DataArrivalEventArgs e)
        {
            Console.WriteLine($"Received {Encoding.Default.GetString(e.Dados)} from {e.SessionId}");

            if (!sessions.Exists(x => x.SessionID == e.SessionId))
                sessions.Add(e.Session);

        }
    }
}
