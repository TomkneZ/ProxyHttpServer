using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProxyHttpOnly
{
    class Program
    {
        static void Main(string[] args)
        {
            

            var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
            // запускаю ожидание входящих запросов на подключение
            Console.WriteLine("Proxy Server Started!");
            listener.Start();
            while (true)
            {
                //принимаю запрос на подключение и создаю клиента для обмена данными
                var client = listener.AcceptTcpClient();
                Thread thread = new Thread(() => RecvData(client));
                thread.Start();
            }
        }
        public static void RecvData(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buf = new byte[16000];            
            while (true)
            {
                if (!stream.CanRead)
                    return;
                stream.Read(buf, 0, buf.Length);
                HTTPserv(buf, client);
            }
        }
        public static void HTTPserv(byte[] buf, TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                string request = Encoding.UTF8.GetString(buf);
                string[] temp = Encoding.UTF8.GetString(buf).Trim().Split(new char[] { '\r', '\n' });

                string host = temp.FirstOrDefault(x => x.Contains("Host"));                
                host = host.Substring(host.IndexOf(" ") + 1);
                Console.WriteLine("_______"+host+"__________");
                //string[] endPoint = host.Trim().Split(new char[] { ':' });
                string[] endPoint = host.Trim().Split(':');
                string[] requestLine = temp[0].Trim().Split(' ');
                string newRequestLine = requestLine[1];
                if (requestLine[1].Contains(endPoint[0]))
                {
                    newRequestLine = requestLine[1].Remove(0, requestLine[1].IndexOf(endPoint[0]));
                    newRequestLine = newRequestLine.Remove(0, newRequestLine.IndexOf('/'));
                }
                request = request.Substring(0, request.IndexOf(requestLine[1])) + newRequestLine + " " + request.Substring(request.IndexOf("HTTP/1.1"));

                bool isClosed = IsBlocked(endPoint[0]);
                if (isClosed)
                {
                    string htmlBody = "<html><body><h1>OOPS!</h1><h2 style = \" color: crimson\">" + endPoint[0] + " is blocked</h2></body></html>";
                    byte[] errorBodyBytes = Encoding.ASCII.GetBytes(htmlBody);
                    stream.Write(errorBodyBytes, 0, errorBodyBytes.Length);
                    //Console.WriteLine("Host: " + endPoint[0] + " is blocked!");
                    return;
                }
                TcpClient server;
                if (endPoint.Length == 2)
                {
                    server = new TcpClient(endPoint[0], int.Parse(endPoint[1]));
                }
                else
                {
                    server = new TcpClient(endPoint[0], 80);
                }
                NetworkStream servStream = server.GetStream();
                buf = Encoding.ASCII.GetBytes(request);
                servStream.Write(buf, 0, buf.Length);

                var respBuf = new byte[32];
                servStream.Read(respBuf, 0, respBuf.Length);

                stream.Write(respBuf, 0, respBuf.Length);
                var head = Encoding.UTF8.GetString(respBuf).Split(new char[] { '\r', '\n' });
                string[] Response = head[0].Split(' ');
                Console.WriteLine($"\nHost: {host}  Status Code: {Response[1]} Status Message: {Response[2]}");
                servStream.CopyTo(stream);
            }
            catch
            {
                return;
            }
            finally
            {
                client.Dispose();
            }
        }
        private static bool IsBlocked(string host)
        {

            List<string> blacklist = XmlSettingsParsercs.GetBlockedWebsites();
            foreach (var key in blacklist)
            {
                if (host.Equals(key))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
