using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace WhiteBalanceCorrection
{
    public static class  LijingSocket
    {
        public static bool isConnected = false;
        public static void LijingSocketTcp(int port)
        {
           try
           {
                //点击开始监听时 在服务端创建一个负责监听IP和端口号的Socket
                Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Any;
                //创建对象端口
                IPEndPoint point = new IPEndPoint(ip, port/*Convert.ToInt32("50001")*/);
                socketWatch.Bind(point);//绑定端口号
                          //ShowMsg("监听成功!");
                socketWatch.Listen(10);//设置监听
                //创建监听线程
                Thread thread = new Thread(Listen);
                thread.IsBackground = true;
                thread.Start(socketWatch);
            }
          catch { }
        }

        static Socket socketSend;
        public static void Listen(object o)
        {
             try
             {
                Socket socketWatch = o as Socket;
                while (true)
               {
                    socketSend = socketWatch.Accept();//等待接收客户端连接
                    // ShowMsg(socketSend.RemoteEndPoint.ToString() + ":" + "连接成功!");
                    //开启一个新线程，执行接收消息方法

                    isConnected = true;
                    Thread r_thread = new Thread(Received);
                    r_thread.IsBackground = true;
                    r_thread.Start(socketSend);
               }
            }
            catch { }
       }
       public static string scanResult = "";   //扫码结果
       public static bool bStartTest = false;  //信号量
       public static void Received(object o)
       {
              try
              {
                  Socket socketSend = o as Socket;
                  while (isConnected)
                 {
                    
                     //客户端连接服务器成功后，服务器接收客户端发送的消息
                     if(scanResult =="")//确保不会遗漏
                    {
                        byte[] buffer = new byte[1024 * 1024 * 3];
                        //实际接收到的有效字节数
                        int len = socketSend.Receive(buffer);
                        if (len <= 0)
                        {
                            isConnected = false;
                            break;
                        }
                        string str = Encoding.UTF8.GetString(buffer, 0, len);
                        scanResult = str;
                        bStartTest = true;
                    }
                }
              }
              catch {
                isConnected = false;
            }
          }

        public static bool Send(string str)
        {
            if(str =="")
            {
                return false;
            }
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            int len = socketSend.Send(buffer);
            if(len <= 0)
            {
                isConnected = false;
                return false;
            }
            return true;
        }
        
    }
}
