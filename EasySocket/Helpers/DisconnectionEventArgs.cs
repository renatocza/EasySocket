using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySocket.Helpers
{
    public class DisconnectionEventArgs : EventArgs
    {
        public DisconnectionEventArgs(string sessionID, Exception ex = null)
        {
            SessionID = sessionID;
        }

        public string SessionID { get; set; }
        public Exception Exception { get; set; }
    }
}
