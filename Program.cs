using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ScanerSimulator
{
    class Program
    {
        private static TcpListener tcpListener;
        private static Dictionary<string, TCPState> clients;
        private static int ratio = 3000;

        static void Main(string[] args)
        {
            if(args.Length>0)
            int.TryParse(args[0], out ratio);
            clients = new Dictionary<string, TCPState>();
            tcpListener = new TcpListener(IPAddress.Any,9014);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(connectCallback), tcpListener);
            Console.WriteLine("Service already started.");
            startSent();
            Console.ReadLine();
        }

        private static void startSent()
        {
            Thread thread = new Thread(() =>
              {
                  while (true)
                  {
                      Guid code = Guid.NewGuid();
                      byte[] buf = Encoding.ASCII.GetBytes(code.ToString().Replace("-", ""));
                      foreach (TCPState state in clients.Values)
                      {
                          try
                          {
                              state.Stream.BeginWrite(buf, 0, buf.Length, new AsyncCallback(sendCallback), state);
                              Thread.Sleep(1);
                          }catch(Exception ex)
                          {
                              Console.WriteLine($"Err : [{state.ID}] {ex.Message}");
                          }

                      }
                      Thread.Sleep(ratio);
                  }
              });
            thread.IsBackground = true;
            thread.Start();
        }

        private static void sendCallback(IAsyncResult ar)
        {
            TCPState state = (TCPState)ar.AsyncState;
            try
            {
                state.Stream.EndWrite(ar);
            }
            catch { }
        }

        private static void connectCallback(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(ar);
            listener.BeginAcceptTcpClient(new AsyncCallback(connectCallback), listener);
            if (client != null)
            {
                TCPState state = new TCPState(client);

                state.Stream.BeginRead(state.Buffer, 0, state.BufferSize, new AsyncCallback(receiveCallback), state);
                Console.WriteLine($"Client {state.ID} have already connect success，waiting for receive data.");
            }
        }

        private static void receiveCallback(IAsyncResult ar)
        {
            TCPState state = (TCPState)ar.AsyncState;
            int readbytes = 0;
            try
            {
                readbytes = state.Stream.EndRead(ar);
            }
            catch
            {
                readbytes = 0;
            }
            if (readbytes == 0)
            {                
                Console.WriteLine($"\r\nClient {state.ID} have already disconnect.");
                clients.Remove(state.ID);
                return;
            }
            byte[] buf = new byte[readbytes];
            Array.Copy(state.Buffer, 0, buf, 0, readbytes);            
            state.Stream.BeginRead(state.Buffer, 0, state.BufferSize, new AsyncCallback(receiveCallback), state);

            string cmd = Encoding.ASCII.GetString(buf);
            handle(cmd, state);
        }

        private static void handle(string cmd, TCPState state)
        {
            string key = state.ID;
            switch (cmd)
            {
                case "LON\r":
                    if(!clients.ContainsKey(key))
                    clients.Add(key, state);
                    break;
                case "LOFF\r":
                    clients.Remove(key);
                    break;
            }
        }
    }
}
