using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EasySocket.Helpers
{
    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 5120;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        public bool SessionServico = false;
        internal int MensagensEmFalha = 0;
        public enumConexaoSocket OrigemDoSocket = enumConexaoSocket.Desconhecido;

        public StateObject()
        {
            this.SessionID = Guid.NewGuid().ToString();
            DtUltimaMensagem = DateTime.Now;
        }

        public string SessionID { get; }
        public string IP { get; internal set; }
        internal DateTime DtUltimaMensagem { get; set; }
    }

    public enum enumConexaoSocket
    {
        Desconhecido,
        Ethernet,
        GPRS1,
        GPRS2
    }
}
