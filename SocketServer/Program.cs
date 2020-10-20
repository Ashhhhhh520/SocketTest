using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            int acceptNum = 0;
            var ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9001);
            var listenSocket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(ip);
            listenSocket.Listen(100);

            // 异步发送消息
            var sendArgs = new SocketAsyncEventArgs() {RemoteEndPoint=ip };
            sendArgs.Completed += (sender, e) =>
            {
                Console.WriteLine($"异步发送消息完成!");
            };
            sendArgs.SetBuffer(new byte[1024], 0, 1024);

            Stack<SocketAsyncEventArgs> ReceiveArgsPool = new Stack<SocketAsyncEventArgs>(1000);
            for(int i = 0; i < 1000; i++)
            {
                var NewArgs = new SocketAsyncEventArgs()
                {
                    AcceptSocket = listenSocket,
                    RemoteEndPoint=ip,
                    UserToken = listenSocket
                };
                NewArgs.SetBuffer(new byte[1024], 0, 1024);
                NewArgs.Completed += (object sender, SocketAsyncEventArgs e) =>
                  {

                      if(e.BytesTransferred>0&&e.SocketError== SocketError.Success)
                      {
                          // 接收消息异步
                          var text = Encoding.UTF8.GetString(e.Buffer);
                          Console.WriteLine($"ReceiveMsg:{text.Replace("\0","")}");
                          if (text.Contains("1"))
                          {
                              Console.WriteLine($"SendBack");
                              var msg = Encoding.UTF8.GetBytes("backMsg");
                              sendArgs.SetBuffer(msg, 0, msg.Length);
                              ((Socket)e.UserToken).SendAsync(sendArgs);
                          }
                          // 设置接收下一个消息
                          ((Socket)e.UserToken).ReceiveAsync(e);
                      }
                      else
                      {
                          Interlocked.Decrement(ref acceptNum);
                          Console.WriteLine($"一个socket已断开!当前共有{acceptNum} client!");
                      }
                  };
                ReceiveArgsPool.Push(NewArgs);
            }

            // client 异步连接
            var acceptArgs = new SocketAsyncEventArgs() { 
                    RemoteEndPoint=ip
            };
            acceptArgs.Completed += new EventHandler<SocketAsyncEventArgs>((object sender,SocketAsyncEventArgs e)=>
            {
                Interlocked.Increment(ref acceptNum);
                Console.WriteLine($"一个socket连接成功,当前共有{acceptNum} client!");
                // 从池子里拿一个出来
                var receiveArgs= ReceiveArgsPool.Pop();
                receiveArgs.UserToken = e.AcceptSocket;
                e.AcceptSocket.ReceiveAsync(receiveArgs);
                e.AcceptSocket = null;
                listenSocket.AcceptAsync(e);
            });
            
            listenSocket.AcceptAsync(acceptArgs);

            Console.WriteLine("按任意键关闭");
            Console.ReadKey();

        }

        static void IO_Complete(object sender,SocketAsyncEventArgs e)
        {
            if(e.LastOperation== SocketAsyncOperation.Send)
            {
                ReceiveMsg(e);
            }
            else if(e.LastOperation== SocketAsyncOperation.Receive)
            {
                SendMsg(e);
            }
        }

        static void ReceiveMsg(SocketAsyncEventArgs e)
        {
            Console.WriteLine("接收到新的消息");
        }

        static void SendMsg(SocketAsyncEventArgs e)
        {
            Console.WriteLine("发送一条消息");
        }
    }
}
