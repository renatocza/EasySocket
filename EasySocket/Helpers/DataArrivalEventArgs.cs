using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySocket.Helpers
{
    public class DataArrivalEventArgs : EventArgs
    {
        public DataArrivalEventArgs(byte[] Dados, string IP, string SessionId, TimeSpan tsRecebimento)
        {
            this.Dados = Dados;
            this.IP = IP;
            this.SessionId = SessionId;
            this.tsRecebimento = tsRecebimento;
        }

        public string IP { get; set; }
        public string SessionId { get; set; }
        public byte[] Dados { get; set; }
        public TimeSpan tsRecebimento { get; set; }
        public StateObject Session { get; set; }
    }
}
