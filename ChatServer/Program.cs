using System.Net;
using System.Net.Quic;
using System.Net.Sockets;
using System.Reflection.Metadata;

using ChatAbastraction;

using ChatServer.Server.Services;

namespace ChatServer.Server;

public class Program {
    static void Main(string[] args) {
        IPAddress addr = IPAddress.Any;

        TcpListener chat_listener = new TcpListener(addr, 7001);
        TcpListener comm_listener = new TcpListener(addr, 7000);
        comm_listener.Start();
        chat_listener.Start();

        Thread comm_thread = StartCommandServer(comm_listener);
        comm_thread.IsBackground = false;
        Thread chat_thread = StartChatServer(chat_listener);
        chat_thread.IsBackground = false;

        Console.WriteLine("Server has Started....");

        bool quit = false;

        while (!quit) {
            char t = (char)Console.Read();

            if (t == 'q')
                quit = true;
        }

        Console.WriteLine("Server is Stopping....");

        comm_thread.Interrupt();
        chat_thread.Interrupt();

        comm_listener.Stop();
        chat_listener.Stop();

        return;
    }

    public static Thread StartCommandServer(TcpListener listener) {
        Thread comm_server_thead = new Thread(() => {
            while (true) {
                Thread thread = new Thread(async () => {
                    try {
                        using Socket socket = await listener.AcceptSocketAsync();
                        IPEndPoint? conn = socket.RemoteEndPoint as IPEndPoint;
                        Console.WriteLine($"[COMM] Accepted connection from: {conn}");
                        CommandConnectionHandler handler = new CommandConnectionHandler(socket);
                        handler.Handle();
                        Console.WriteLine($"[COMM] Closing connection to: {conn}");
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.Message);
                    }
                });
                thread.Start();
            }
        });

        comm_server_thead.Start();

        return comm_server_thead;
    }

    public static Thread StartChatServer(TcpListener listener) {

        Thread chat_server_thread = new Thread(() => {
            ChatServerService chat = new ChatServerService(listener);
            chat.Handle();
        });

        chat_server_thread.Start();

        return chat_server_thread;

    }

}
