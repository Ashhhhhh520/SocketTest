using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketClient
{
    public class AsyncSocketToken
    {
        public Socket Socket { get; set; }
        public AsyncSocketToken():this(null)
        {

        }

        public AsyncSocketToken(Socket socket)
        {
            this.Socket = socket;
        }
    }
}
