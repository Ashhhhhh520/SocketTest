using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketClientThird
{
    class Program
    {
        static SocketAsyncEventArgs SendArgs;
        static AutoResetEvent sendReset = new AutoResetEvent(false);
        static byte[] sendBuff = new byte[1024];
        static void Main(string[] args)
        {
            var ipEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9001);
            Socket client = new Socket(ipEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // 异步发送
            SendArgs = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = ipEndpoint
            };
            SendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSend);
            SendArgs.SetBuffer(sendBuff, 0, sendBuff.Length);
            // 异步接收
            var ReceiveArgs = new SocketAsyncEventArgs()
            {
                UserToken = client,
            };
            ReceiveArgs.SetBuffer(new byte[1024], 0, 1024);
            ReceiveArgs.Completed += (object sender, SocketAsyncEventArgs e) =>
            {
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    Console.WriteLine($"receive complete");
                    var buffer = e.Buffer;
                    var text = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine($"receive :{text}");
                    // 重新设置异步接收消息
                    ((Socket)e.UserToken).ReceiveAsync(e);
                }
                else
                {
                    Console.WriteLine("服务器端断开连接!");
                }
            };

            // 异步连接
            var ConnectArgs = new SocketAsyncEventArgs()
            {
                UserToken = client,
                RemoteEndPoint = ipEndpoint,
            };
            ConnectArgs.Completed += (object sender, SocketAsyncEventArgs e) =>
            {
                Console.WriteLine($"connect complete!");
                e.ConnectSocket.ReceiveAsync(ReceiveArgs);
            };
            client.ConnectAsync(ConnectArgs);


            while (true)
            {
                Console.WriteLine("输入文本........");
                var text = Console.ReadLine();
                var msg = Encoding.UTF8.GetBytes(text);
                msg.CopyTo(sendBuff, 0);
                SendArgs.SetBuffer(0, sendBuff.Length);
                client.SendAsync(SendArgs);
            }
        }

        static void OnSend(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine("OnSend");
        }
    }
}
